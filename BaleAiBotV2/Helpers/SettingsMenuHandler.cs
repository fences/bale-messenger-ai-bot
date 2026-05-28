using BaleAiBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BaleAiBotV2.Helpers
{
    public static class SettingsMenuHandler
    {
        public static async Task ShowSettings(ITelegramBotClient client, long chatId, UserData userData, UserManager? userManager)
        {
            if (userManager != null && !userManager.HasMenuAccess(chatId))
            {
                await client.SendMessage(chatId, "⛔ شما دسترسی به منوی تنظیمات ندارید.");
                return;
            }

            await client.SendMessage(
                chatId,
                "📋 از منوی زیر انتخاب کنید:",
                replyMarkup: new ReplyKeyboardRemove()
            );

            var s = userData.Settings;

            string currentModelDesc = BotConfig.MODELS.ContainsKey(s.Model)
                ? BotConfig.MODELS[s.Model]
                : s.Model;

            string text = $"⚙️ **تنظیمات فعلی شما**\n\n" +
                          $"🧠 مدل: {currentModelDesc}\n" +
                          $"🌡️ دما: `{s.Temperature:F2}`\n" +
                          $"📏 حداکثر توکن: `{s.MaxTokens}`\n" +
                          $"📝 پرامپت سیستم:\n_{s.SystemPrompt}_";

            var keyboardButtons = new List<List<InlineKeyboardButton>>();

            if (userManager == null || userManager.CanChangeModel(chatId))
                keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("🔄 تغییر مدل", "set_model") }.ToList());

            if (userManager == null || userManager.HasMenuAccess(chatId))
                keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("🌡️ تغییر دما", "set_temperature") }.ToList());

            if (userManager == null || userManager.CanChangeToken(chatId))
                keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("📏 تغییر توکن", "set_max_tokens") }.ToList());

            if (userManager == null || userManager.CanChangeSystemPrompt(chatId))
                keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("📝 تغییر پرامپت", "set_prompt") }.ToList());

            keyboardButtons.Add(new[] { InlineKeyboardButton.WithCallbackData("❌ بستن", "close_settings") }.ToList());

            var keyboard = new InlineKeyboardMarkup(keyboardButtons);

            await client.SendMessage(
                chatId,
                text,
                parseMode: ParseMode.Markdown,
                replyMarkup: keyboard
            );
        }

        public static async Task HandleCallback(ITelegramBotClient client, CallbackQuery query, UserData userData, UserManager? userManager = null)
        {
            string data = query.Data;
            long chatId = query.Message!.Chat.Id;

            await client.AnswerCallbackQuery(query.Id);

            switch (data)
            {
                case "set_model":
                    if (userManager != null && !userManager.CanChangeModel(chatId))
                    {
                        await client.EditMessageText(chatId, query.Message.MessageId, "⛔ شما مجوز تغییر مدل را ندارید.");
                        return;
                    }
                    await ShowModelSelection(client, chatId, query.Message.MessageId);
                    break;

                case "set_temperature":
                    if (userManager != null && !userManager.HasMenuAccess(chatId))
                    {
                        await client.EditMessageText(chatId, query.Message.MessageId, "⛔ شما مجوز تغییر دما را ندارید.");
                        return;
                    }
                    userData.State = "setting_temperature";
                    UserStorage.Save(userData);  
                    await PromptWithCancel(client, chatId, "🌡️ لطفاً دمای جدید (مثلاً 0.7) را وارد کنید:", "setting_temperature");
                    break;

                case "set_max_tokens":
                    if (userManager != null && !userManager.CanChangeToken(chatId))
                    {
                        await client.EditMessageText(chatId, query.Message.MessageId, "⛔ شما مجوز تغییر حداکثر توکن را ندارید.");
                        return;
                    }
                    userData.State = "setting_max_tokens";
                    UserStorage.Save(userData);   
                    await PromptWithCancel(client, chatId, "📏 لطفاً حداکثر توکن جدید را وارد کنید:", "setting_max_tokens");
                    break;

                case "set_prompt":
                    if (userManager != null && !userManager.CanChangeSystemPrompt(chatId))
                    {
                        await client.EditMessageText(chatId, query.Message.MessageId, "⛔ شما مجوز تغییر پرامپت سیستم را ندارید.");
                        return;
                    }
                    userData.State = "setting_prompt";
                    UserStorage.Save(userData);   
                    await PromptWithCancel(client, chatId, "📝 لطفاً پرامپت سیستم جدید را وارد کنید:", "setting_prompt");
                    break;

                case "close_settings":
                    await client.EditMessageReplyMarkup(chatId, query.Message.MessageId, null);
                    await ReturnMainKeyboard(client, chatId);
                    break;

                case var d when d.StartsWith("model_"):
                    if (userManager != null && !userManager.CanChangeModel(chatId))
                    {
                        await client.EditMessageText(chatId, query.Message.MessageId, "⛔ شما مجوز تغییر مدل را ندارید.");
                        return;
                    }
                    string modelKey = d.Substring(6);
                    if (BotConfig.MODELS.ContainsKey(modelKey))
                    {
                        userData.Settings.Model = modelKey;
                        UserStorage.Save(userData);
                        await client.EditMessageText(
                            chatId,
                            query.Message.MessageId,
                            $"✅ مدل به **{BotConfig.MODELS[modelKey]}** تغییر یافت.",
                            parseMode: ParseMode.Markdown
                        );
                        await Task.Delay(1000);
                        await ShowSettings(client, chatId, userData, userManager);
                    }
                    break;




                case "cancel_setting":
                    userData.State = null;
                    UserStorage.Save(userData);   
                    await client.EditMessageText(
                        chatId,
                        query.Message.MessageId,
                        "❌ عملیات لغو شد."
                    );
                    await ReturnMainKeyboard(client, chatId);
                    break;
            }
        }

        private static async Task ShowModelSelection(ITelegramBotClient client, long chatId, int messageId)
        {
            var buttons = BotConfig.MODELS
                .Select(kvp => new[]
                {
                    InlineKeyboardButton.WithCallbackData(kvp.Value, $"model_{kvp.Key}")
                })
                .ToList();

            buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("❌ انصراف", "cancel_setting") });

            var keyboard = new InlineKeyboardMarkup(buttons);

            await client.EditMessageText(
                chatId,
                messageId,
                "🧠 **مدل مورد نظر خود را انتخاب کنید:**",
                parseMode: ParseMode.Markdown,
                replyMarkup: keyboard
            );
        }

        private static async Task PromptWithCancel(ITelegramBotClient client, long chatId, string text, string stateContext)
        {
            var cancelKeyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton("❌ انصراف")
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await client.SendMessage(
                chatId,
                text,
                replyMarkup: cancelKeyboard
            );
        }


        public static async Task ClearHistory(ITelegramBotClient client, long chatId, UserData userData, UserManager? userManager = null)
        {
            if (userManager != null && !userManager.HasMenuAccess(chatId))
            {
                await client.SendMessage(chatId, "⛔ شما دسترسی به پاک کردن تاریخچه ندارید.");
                return;
            }

            userData.History = new List<ChatMessage>();
            userData.RecentFiles = new List<UserFile>();
            UserStorage.Save(userData);
            await Task.Delay(2000);
            await client.SendMessage(chatId, "✅ تاریخچه مکالمات شما با موفقیت پاک شد.");

        }

        public static async Task ReturnMainKeyboard(ITelegramBotClient client, long chatId)
        {
                var mainKeyboard = new ReplyKeyboardMarkup(new[]
                {
                    new KeyboardButton("💰 بررسی اعتبار"),  
                    new KeyboardButton("⚙️ تنظیمات"),
                    new KeyboardButton("✨ گفتگوی جدید")
                })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = false
                };

                await client.SendMessage(
                    chatId,
                    "✅ می‌توانید به گفتگو ادامه دهید.",
                    replyMarkup: mainKeyboard
            );
        }

        public static async Task HandleStateInput(ITelegramBotClient client, long chatId, UserData userData, string input, UserManager? userManager = null)
        {
            if (input == "❌ انصراف" || input == "/cancel")
            {
                userData.State = null;
                UserStorage.Save(userData);   
                await client.SendMessage(
                    chatId,
                    "❌ عملیات لغو شد.",
                    replyMarkup: new ReplyKeyboardMarkup(new[] { new KeyboardButton("⚙️ تنظیمات") })
                    {
                        ResizeKeyboard = true,
                        OneTimeKeyboard = false
                    }
                );
                return;
            }

            string state = userData.State;
            if (string.IsNullOrEmpty(state)) return;

            switch (state)
            {
                case "setting_temperature":
                    if (userManager != null && !userManager.HasMenuAccess(chatId))
                    {
                        await client.SendMessage(chatId, "⛔ شما مجوز تغییر دما را ندارید.");
                        userData.State = null;
                        UserStorage.Save(userData);   
                        return;
                    }
                    if (double.TryParse(input, out double temp) && temp >= 0 && temp <= 2)
                    {
                        userData.Settings.Temperature = temp;
                        userData.State = null;           
                        UserStorage.Save(userData);      
                        await client.SendMessage(chatId, "✅ دما با موفقیت ذخیره شد.");
                        await ReturnMainKeyboard(client, chatId);
                        await ShowSettings(client, chatId, userData, userManager);
                    }
                    else
                    {
                        await client.SendMessage(chatId, "⚠️ مقدار دما باید عددی بین 0 تا 2 باشد. دوباره تلاش کنید یا «❌ انصراف» را بزنید.");
                    }
                    break;

                case "setting_max_tokens":
                    if (userManager != null && !userManager.CanChangeToken(chatId))
                    {
                        await client.SendMessage(chatId, "⛔ شما مجوز تغییر حداکثر توکن را ندارید.");
                        userData.State = null;
                        UserStorage.Save(userData);
                        return;
                    }
                    if (int.TryParse(input, out int tokens) && tokens > 0)
                    {
                        userData.Settings.MaxTokens = tokens;
                        userData.State = null;            
                        UserStorage.Save(userData);      
                        await client.SendMessage(chatId, "✅ حداکثر توکن با موفقیت ذخیره شد.");
                        await ReturnMainKeyboard(client, chatId);
                        await ShowSettings(client, chatId, userData, userManager);
                    }
                    else
                    {
                        await client.SendMessage(chatId, "⚠️ تعداد توکن باید یک عدد صحیح مثبت باشد. دوباره تلاش کنید یا «❌ انصراف» را بزنید.");
                    }
                    break;

                case "setting_prompt":
                    if (userManager != null && !userManager.CanChangeSystemPrompt(chatId))
                    {
                        await client.SendMessage(chatId, "⛔ شما مجوز تغییر پرامپت سیستم را ندارید.");
                        userData.State = null;
                        UserStorage.Save(userData);
                        return;
                    }
                    userData.Settings.SystemPrompt = input.Trim();
                    userData.State = null;               
                    UserStorage.Save(userData);         
                    await client.SendMessage(chatId, "✅ پرامپت سیستم با موفقیت ذخیره شد.");
                    await ReturnMainKeyboard(client, chatId);
                    await ShowSettings(client, chatId, userData, userManager);
                    break;



                default:
                    userData.State = null;
                    UserStorage.Save(userData);
                    await client.SendMessage(chatId, "⚠️ وضعیت نامعتبر. لطفاً دوباره تلاش کنید.");
                    break;

            }
        }
    }
}