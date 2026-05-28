using BaleAiBot.Helpers;
using OpenAI;
using OpenAI.Audio;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ChatMessage = BaleAiBot.Helpers.ChatMessage;

namespace BaleAiBotV2.Helpers
{
    public class StreamingChatService : IDisposable
    {
        private readonly OpenAIClient _openAiClient;

        public StreamingChatService(string apiKey, string baseUrl)
        {
            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(baseUrl)
            };
            _openAiClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), options);
        }

        public async IAsyncEnumerable<string> StreamTextResponseAsync(
            UserData userData,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var history = userData.History ?? new List<ChatMessage>();
            var settings = userData.Settings;

            var apiMessages = new List<OpenAI.Chat.ChatMessage>();

            if (!string.IsNullOrWhiteSpace(settings.SystemPrompt))
                apiMessages.Add(new SystemChatMessage(settings.SystemPrompt));

            foreach (var msg in history)
            {
                if (msg.Role == "user")
                {
                    if (msg.ContentParts != null && msg.ContentParts.Count > 0)
                    {
                        var openAIParts = new List<ChatMessageContentPart>();
                        foreach (var part in msg.ContentParts)
                        {
                            if (part.Type == "text" && !string.IsNullOrEmpty(part.Text))
                                openAIParts.Add(ChatMessageContentPart.CreateTextPart(part.Text));
                            else if (part.Type == "image_url" && part.ImageUrl != null)
                            {
                                var dataUrl = part.ImageUrl.Url!;
                                var base64 = dataUrl.Substring(dataUrl.IndexOf(',') + 1);
                                var imageBytes = Convert.FromBase64String(base64);
                                var imageData = BinaryData.FromBytes(imageBytes);
                                openAIParts.Add(ChatMessageContentPart.CreateImagePart(imageData, "image/jpeg"));
                            }
                        }
                        apiMessages.Add(new UserChatMessage(openAIParts));
                    }
                    else if (!string.IsNullOrEmpty(msg.Content))
                    {
                        apiMessages.Add(new UserChatMessage(msg.Content));
                    }
                }
                else if (msg.Role == "assistant")
                {
                    if (!string.IsNullOrEmpty(msg.Content))
                        apiMessages.Add(new AssistantChatMessage(msg.Content));
                }
            }

            var chatClient = _openAiClient.GetChatClient(settings.Model);

            var updates = chatClient.CompleteChatStreamingAsync(
                apiMessages,
                new ChatCompletionOptions
                {
                    Temperature = (float)settings.Temperature,
                    MaxOutputTokenCount = settings.MaxTokens
                },
                cancellationToken);

            var fullResponse = new System.Text.StringBuilder();

            await foreach (var update in updates.WithCancellation(cancellationToken))
            {
                foreach (var part in update.ContentUpdate)
                {
                    if (!string.IsNullOrEmpty(part.Text))
                    {
                        fullResponse.Append(part.Text);
                        yield return part.Text;
                    }
                }
            }

            history.Add(new ChatMessage { Role = "assistant", Content = fullResponse.ToString() });

            int maxHistory = BotConfig.MAX_HISTORY;
            if (history.Count > maxHistory)
                history.RemoveRange(0, history.Count - maxHistory);

            userData.History = history;
        }


        public async IAsyncEnumerable<string> StreamImageResponseAsync(
            UserData userData,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var token in StreamTextResponseAsync(userData, cancellationToken))
                yield return token;
        }

        public async Task<string> TranscribeAudioAsync(
            byte[] audioBytes,
            string fileName,
            string? model = null,
            string? language = null,
            CancellationToken cancellationToken = default)
        {
            var audioClient = _openAiClient.GetAudioClient(
                model ?? BotConfig.AUDIO_ANALYSIS_MODEL);

            using var audioStream = new MemoryStream(audioBytes);
            var options = new AudioTranscriptionOptions
            {
                Language = language ?? BotConfig.AUDIO_LANGUAGE,
                ResponseFormat = AudioTranscriptionFormat.Text
            };

            AudioTranscription transcription = await audioClient.TranscribeAudioAsync(
                audioStream, fileName, options, cancellationToken);

            return transcription.Text;
        }

        public void Dispose() { }
    }
}