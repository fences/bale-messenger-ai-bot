

using BaleAiBot.Helpers;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace YourNamespace
{

    public class CustomTelegramBot
    {
        private readonly TelegramBotClient _botClient;
        private CancellationTokenSource? _cts;

        public event Func<Exception, HandleErrorSource, Task>? OnError;
        public event Func<ITelegramBotClient, Update, CancellationToken, Task>? OnUpdate;
        public event Func<Telegram.Bot.Types.Message, UpdateType?, Task>? OnMessage;
        private readonly string _baseApiUrl;
        private readonly string _baseFileUrl;


        public CustomTelegramBot(
            string token,
            string baseApiUrl = "",
            string baseFileUrl = "",
            HttpClient? httpClient = null)
        {
            var options = new TelegramBotClientOptions(token, baseApiUrl);
            _baseApiUrl = baseApiUrl;
            _baseFileUrl = baseFileUrl;

            if (!string.IsNullOrEmpty(baseFileUrl))
            {
                var field = typeof(TelegramBotClientOptions).GetField(
                    "<BaseFileUrl>k__BackingField",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                field?.SetValue(options, baseFileUrl);
            }

            _botClient = httpClient != null
                ? new TelegramBotClient(options, httpClient)
                : new TelegramBotClient(options);
        }

        public void StartReceiving(ReceiverOptions? receiverOptions = null)
        {
            _cts = new CancellationTokenSource();
            _botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                errorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions ?? new ReceiverOptions(),
                cancellationToken: _cts.Token
            );
        }


        public async Task<byte[]> DownloadFile(string? fileId)
        {
            byte[] fileBytes;
            using (var http = new HttpClient())
            {
                string downloadUrl = $"{_baseFileUrl}{_botClient.Token}/{fileId}";
                var response = await http.GetAsync(downloadUrl);
                response.EnsureSuccessStatusCode();
                fileBytes = await response.Content.ReadAsByteArrayAsync();
            }
            return fileBytes;
        }


        public void StopReceiving()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken ct)
        {
            if (OnUpdate != null)
                await OnUpdate.Invoke(bot, update, ct);

            if (update.Message is { } message && OnMessage != null)
                await OnMessage.Invoke(message, update.Type);
        }

        private async Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, HandleErrorSource source, CancellationToken ct)
        {
            if (OnError != null)
                await OnError.Invoke(ex, source);
            else
                Console.WriteLine($"Error: {ex.Message}"); 
        }


        public TelegramBotClient Client => _botClient;
    }
}