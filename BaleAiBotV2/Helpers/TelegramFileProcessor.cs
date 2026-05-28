using BaleAiBot.Helpers;
using BaleAiBotV2.Helpers;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Message = Telegram.Bot.Types.Message;

namespace YourNamespace
{
    public class TelegramFileProcessor
    {
        private readonly CustomTelegramBot _bot;
        private readonly StreamingChatService _aiService;
        private readonly Action<string> _logError;

        public TelegramFileProcessor(CustomTelegramBot bot, StreamingChatService aiService, Action<string> logError)
        {
            _bot = bot;
            _aiService = aiService;
            _logError = logError;
        }

        private async Task EditProcessingMessage(long chatId, int messageId, string text)
        {
            var cancelButton = new InlineKeyboardButton("❌ لغو") { CallbackData = "cancel_operation" };
            var keyboard = new InlineKeyboardMarkup(cancelButton);
            await _bot.Client.EditMessageText(chatId, messageId, text, replyMarkup: keyboard);
        }

        private async Task FinalizeMessage(long chatId, int messageId, string text, bool cancelled)
        {
            if (cancelled)
                await _bot.Client.EditMessageText(chatId, messageId, text, replyMarkup: null);
            else
                await _bot.Client.EditMessageText(chatId, messageId, text);
        }

        public async Task ProcessMessageAsync(
            Message message,
            long chatId,
            Message processingMessage,
            UserData userData,
            CancellationToken cancellationToken)
        {
            try
            {
                string fileId = string.Empty;
                string mimeType = string.Empty;
                string fileCategory = string.Empty;

                if (message.Photo != null && message.Photo.Length > 0)
                {
                    fileId = message.Photo[^1].FileId;
                    mimeType = "image/jpeg";
                    fileCategory = "Image";
                }
                else if (message.Voice != null)
                {
                    fileId = message.Voice.FileId;
                    mimeType = message.Voice.MimeType ?? "audio/ogg";
                    fileCategory = "Voice";
                }
                else if (message.Audio != null)
                {
                    fileId = message.Audio.FileId;
                    mimeType = message.Audio.MimeType ?? "audio/mpeg";
                    fileCategory = "Audio";
                }
                else if (message.Video != null)
                {
                    fileId = message.Video.FileId;
                    mimeType = message.Video.MimeType ?? "video/mp4";
                    fileCategory = "Video";
                }
                else if (message.Document != null)
                {
                    fileId = message.Document.FileId;
                    mimeType = message.Document.MimeType ?? "application/octet-stream";
                    fileCategory = mimeType.StartsWith("image/") ? "Image" : "Document";
                }
                else
                {
                    return;
                }

                var file = await _bot.Client.GetFile(fileId, cancellationToken);
                if (file?.FilePath == null)
                {
                    await _bot.Client.EditMessageText(chatId, processingMessage.MessageId, "⚠️ فایل در سرور یافت نشد.");
                    return;
                }

                var fileBytes = await _bot.DownloadFile(file.FileId);

                switch (fileCategory)
                {
                    case "Image":
                        await ProcessImageAsync(chatId, processingMessage.MessageId, fileBytes, message.Caption, userData, cancellationToken);
                        break;
                    case "Voice":
                    case "Audio":
                            await ProcessAudioAsync(chatId, processingMessage.MessageId, fileBytes, mimeType, message.Caption, userData, cancellationToken);
                        break;
                    case "Document":
                        await ProcessDocumentAsync(chatId, processingMessage.MessageId, fileBytes, message.Document?.FileName, message.Caption, userData, cancellationToken);
                        break;
                    default:
                        await _bot.Client.EditMessageText(chatId, processingMessage.MessageId, "⚠️ پردازش این فرمت فایل در حال حاضر پشتیبانی نمی‌شود.");
                        break;
                }
            }
            catch (OperationCanceledException)
            {
                await FinalizeMessage(chatId, processingMessage.MessageId, "✅ پردازش فایل لغو شد.", cancelled: true);
            }
            catch (Exception ex)
            {
                _logError($"Failed Process File Message: {ex.Message}");
                await _bot.Client.EditMessageText(chatId, processingMessage.MessageId, $"⚠️ خطا در پردازش فایل: {ex.Message}");
            }
        }


        private async Task ProcessImageAsync(
            long chatId,
            int messageId,
            byte[] fileBytes,
            string? caption,
            UserData userData,
            CancellationToken cancellationToken)
        {
            fileBytes = ImageCompressor.CompressToMaxSize(fileBytes, BotConfig.IMAGE_MAX_SIZE_COMPRESS);

            var userFile = new UserFile
            {
                FileType = "image",
                ImageBytes = fileBytes,
                Caption = caption
            };

            userData.RecentFiles.Add(userFile);
            while (userData.RecentFiles.Count > 3)
                userData.RecentFiles.RemoveAt(0);   

            string imagePrompt = caption ?? "تصویر ارسال شد.";
            var imageMessage = new ChatMessage
            {
                Role = "user",
                ContentParts = new List<ContentPart>
                {
                    new() { Type = "text", Text = imagePrompt },
                    new()
                    {
                        Type = "image_url",
                        ImageUrl = new ImageUrl
                        {
                            Url = $"data:image/jpeg;base64,{Convert.ToBase64String(fileBytes)}"
                        }
                    }
                }
            };
            userData.History.Add(imageMessage);

            var buffer = new StringBuilder();
            var lastEdit = DateTime.MinValue;

            try
            {
                await foreach (var token in _aiService.StreamImageResponseAsync(userData, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    buffer.Append(token);
                    var now = DateTime.UtcNow;
                    if ((now - lastEdit).TotalSeconds >= BotConfig.STREAM_EDIT_INTERVAL &&
                        buffer.Length >= BotConfig.STREAM_MIN_CHARS)
                    {
                        await EditProcessingMessage(chatId, messageId, buffer.ToString());
                        lastEdit = now;
                    }
                }

                userData.History.Add(new ChatMessage
                {
                    Role = "assistant",
                    Content = buffer.ToString()
                });

                UserStorage.Save(userData);

                await FinalizeMessage(chatId, messageId, buffer.ToString(), cancelled: false);
            }
            catch (OperationCanceledException)
            {
                await FinalizeMessage(chatId, messageId, "✅ تحلیل تصویر لغو شد.", cancelled: true);
                UserStorage.Save(userData);
            }
            catch (Exception ex)
            {
                await FinalizeMessage(chatId, messageId, $"⚠️ خطا در تحلیل تصویر: {ex.Message}", cancelled: true);
                _logError($"خطا در تحلیل تصویر: {ex.Message}");
                UserStorage.Save(userData);
            }
        }

        private async Task ProcessDocumentAsync(
                long chatId,
                int messageId,
                byte[] fileBytes,
                string? fileName,
                string? caption,
                UserData userData,
                CancellationToken cancellationToken)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            string extractedText = DocumentTextExtractor.ExtractText(fileBytes, fileName);
            if (string.IsNullOrWhiteSpace(extractedText) ||
                extractedText.StartsWith("فرمت فایل پشتیبانی نمی‌شود"))
            {
                await _bot.Client.EditMessageText(chatId, messageId,
                    "⚠️ امکان استخراج متن از این فایل وجود ندارد یا فرمت آن پشتیبانی نمی‌شود.");
                return;
            }

            int maxChars = BotConfig.MAX_DOCTEXTSIZE;
            if (extractedText.Length > maxChars)
                extractedText = extractedText[..maxChars] +
                                "\n\n[بخش‌های بعدی فایل به دلیل محدودیت حجم حذف شدند...]";

            var userFile = new UserFile
            {
                FileType = "document",
                ExtractedText = extractedText,
                Caption = caption
            };
            userData.RecentFiles.Add(userFile);
            while (userData.RecentFiles.Count > 3)
                userData.RecentFiles.RemoveAt(0);

            string finalPrompt = string.IsNullOrWhiteSpace(caption)
                ? $"لطفاً این متن را تحلیل کن:\n{extractedText}"
                : $"{caption}\n\nمتن فایل:\n{extractedText}";

            var userMessage = new ChatMessage
            {
                Role = "user",
                Content = finalPrompt
            };
            userData.History.Add(userMessage);

            var buffer = new StringBuilder();
            var lastEdit = DateTime.MinValue;

            try
            {
                await foreach (var token in _aiService.StreamTextResponseAsync(userData, cancellationToken))

                {
                    cancellationToken.ThrowIfCancellationRequested();
                    buffer.Append(token);
                    var now = DateTime.UtcNow;
                    if ((now - lastEdit).TotalSeconds >= BotConfig.STREAM_EDIT_INTERVAL &&
                        buffer.Length >= BotConfig.STREAM_MIN_CHARS)
                    {
                        await EditProcessingMessage(chatId, messageId, buffer.ToString());
                        lastEdit = now;
                    }
                }

                userData.History.Add(new ChatMessage
                {
                    Role = "assistant",
                    Content = buffer.ToString()
                });
                UserStorage.Save(userData);

                await FinalizeMessage(chatId, messageId, buffer.ToString(), cancelled: false);
            }
            catch (OperationCanceledException)
            {
                await FinalizeMessage(chatId, messageId, "✅ پردازش سند لغو شد.", cancelled: true);
                UserStorage.Save(userData);
            }
            catch (Exception ex)
            {
                await _bot.Client.EditMessageText(chatId, messageId, $"⚠️ خطا در پردازش: {ex.Message}");
                _logError($"خطا در پردازش سند: {ex.Message}");
                UserStorage.Save(userData);
            }
        }


        private async Task ProcessAudioAsync(
            long chatId,
            int messageId,
            byte[] audioBytes,
            string mimeType,
            string? caption,
            UserData userData,
            CancellationToken cancellationToken)
        {
            await EditProcessingMessage(chatId, messageId, "🎤 در حال تبدیل گفتار به متن...");

            string extension = mimeType switch
            {
                "audio/ogg" => ".ogg",
                "audio/mpeg" => ".mp3",
                "audio/mp4" => ".m4a",
                "audio/wav" => ".wav",
                _ => ".ogg"
            };
            string fileName = $"audio{extension}";

            string transcription;
            try
            {
                transcription = await _aiService.TranscribeAudioAsync(audioBytes, fileName, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logError($"Audio transcription failed: {ex.Message}");
                await _bot.Client.EditMessageText(chatId, messageId, $"⚠️ خطا در تبدیل گفتار به متن: {ex.Message}");
                return;
            }

            if (string.IsNullOrWhiteSpace(transcription))
            {
                await _bot.Client.EditMessageText(chatId, messageId, "⚠️ متنی از فایل صوتی استخراج نشد.");
                return;
            }

            await EditProcessingMessage(chatId, messageId,
                $"📝 **متن پیام صوتی شما:**\n{transcription}\n\n🤔 در حال پردازش...");

            string userPrompt = string.IsNullOrEmpty(caption)
                ? transcription
                : $"{caption}\n\n{transcription}";

            userData.History.Add(new ChatMessage
            {
                Role = "user",
                Content = userPrompt
            });

            var buffer = new StringBuilder();
            var lastEdit = DateTime.MinValue;

            try
            {
                await foreach (var token in _aiService.StreamTextResponseAsync(userData, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    buffer.Append(token);
                    var now = DateTime.UtcNow;
                    if ((now - lastEdit).TotalSeconds >= BotConfig.STREAM_EDIT_INTERVAL
                        && buffer.Length >= BotConfig.STREAM_MIN_CHARS)
                    {
                        await EditProcessingMessage(chatId, messageId,
                            $"📝 **متن پیام صوتی:**\n{transcription}\n\n🤖 **پاسخ:**\n{buffer}");
                        lastEdit = now;
                    }
                }

                string finalText = $"📝 **متن پیام صوتی شما:**\n{transcription}\n\n🤖 **پاسخ:**\n{buffer}";
                await FinalizeMessage(chatId, messageId, finalText, cancelled: false);

                userData.History.Add(new ChatMessage
                {
                    Role = "assistant",
                    Content = buffer.ToString()
                });
                UserStorage.Save(userData);
            }
            catch (OperationCanceledException)
            {
                await FinalizeMessage(chatId, messageId,
                    $"📝 **متن پیام صوتی شما:**\n{transcription}\n\n✅ پردازش لغو شد.", cancelled: true);
                UserStorage.Save(userData);
            }
            catch (Exception ex)
            {
                await _bot.Client.EditMessageText(chatId, messageId,
                    $"📝 **متن پیام صوتی:**\n{transcription}\n\n⚠️ خطا در پاسخ: {ex.Message}");
                _logError($"AI response failed for audio: {ex.Message}");
                UserStorage.Save(userData);
            }
        }








    }
}