using BaleAiBot.Helpers;
using BaleAiBotV2.Forms;
using BaleAiBotV2.Helpers;
using ConfigEditor;
using Microsoft.VisualBasic.ApplicationServices;
using OpenAI.Chat;
using System.Collections.Concurrent;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using YourNamespace;
using Color = System.Drawing.Color;
using Message = Telegram.Bot.Types.Message;

namespace BaleAiBotV2
{
    public partial class MainForm : Form
    {
        private CustomTelegramBot? _bot;
        private StreamingChatService? _aiService;
        private UserManager _userManager = new UserManager();
        private readonly ConcurrentDictionary<long, CancellationTokenSource> _activeRequests = new();
        private NotifyIcon? trayIcon;
        private ContextMenuStrip? trayMenu;

        public MainForm()
        {
            InitializeComponent();
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            trayMenu = new ContextMenuStrip();
            var showItem = new ToolStripMenuItem("Show");
            var exitItem = new ToolStripMenuItem("Exit");

            showItem.Click += (s, e) => ShowFormFromTray();
            exitItem.Click += (s, e) => ExitApplication();

            trayMenu.Items.Add(showItem);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add(exitItem);




            trayIcon = new NotifyIcon
            {
                Icon = this.Icon,
                Text = "Bale AI Bot",
                ContextMenuStrip = trayMenu,
                Visible = true
            };

            trayIcon.DoubleClick += (s, e) => ShowFormFromTray();
        }

        private void ShowFormFromTray()
        {
            if (WindowState == FormWindowState.Minimized)
                WindowState = FormWindowState.Normal;

            Show();
            BringToFront();
            Focus();
        }



        private void ExitApplication()
        {
            _bot?.StopReceiving();
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }
            Environment.Exit(0); 
        }



        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                WindowState = FormWindowState.Minimized;
                return;
            }

            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
            }

            base.OnFormClosing(e);
        }

        private bool IsProcessing(long chatId) => _activeRequests.ContainsKey(chatId);

        private async Task<Message> SendProcessingMessage(long chatId, string text)
        {
            if (_bot == null)
                throw new InvalidOperationException("کلاینت ربات مقداردهی نشده است.");

            var cancelButton = new InlineKeyboardButton("❌ لغو") { CallbackData = "cancel_operation" };
            var keyboard = new InlineKeyboardMarkup(cancelButton);
            return await _bot.Client.SendMessage(chatId, text, replyMarkup: keyboard);
        }

        private async Task EditProcessingMessage(long chatId, int messageId, string text)
        {
            if (_bot == null)
                throw new InvalidOperationException("کلاینت ربات مقداردهی نشده است.");

            var cancelButton = new InlineKeyboardButton("❌ لغو") { CallbackData = "cancel_operation" };
            var keyboard = new InlineKeyboardMarkup(cancelButton);
            await _bot.Client.EditMessageText(chatId, messageId, text, replyMarkup: keyboard);
        }

        private async Task FinalizeMessage(long chatId, int messageId, string text, bool cancelled)
        {
            if (_bot == null)
                throw new InvalidOperationException("کلاینت ربات مقداردهی نشده است.");

            if (cancelled)
                await _bot.Client.EditMessageText(chatId, messageId, text, replyMarkup: null);
            else
                await _bot.Client.EditMessageText(chatId, messageId, text);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            RefreshUserList();
        }


        private void btnStart_Click(object sender, EventArgs e)
        {
            _aiService = new StreamingChatService(BotConfig.AVAL_API_KEY, BotConfig.AVAL_BASE_URL);

            if (_bot == null)
            {
                _bot = new CustomTelegramBot(
                    token: BotConfig.BOT_TOKEN,
                    baseApiUrl: BotConfig.BASE_URL,
                    baseFileUrl: BotConfig.BALE_FILE_URL
                );

                _bot.OnError += async (ex, source) =>
                {
                    AppendLog(ex.Message, LogStatus.Error);
                    await Task.CompletedTask;
                };

                _bot.OnMessage += async (message, type) =>
                {
                    if (_aiService == null) return;
                    var chatId = message.Chat.Id;
                    var username = message.Chat.Username ?? "بدون نام کاربری";
                    var fullName = $"{message.Chat.FirstName} {message.Chat.LastName}".Trim();

                    if (_userManager.IsAllowed(chatId))
                    {
                        var user = _userManager.GetAllUsers().FirstOrDefault(u => u.ChatId == chatId);
                        if (user != null && (user.Username != username || user.FullName != fullName))
                        {
                            user.Username = username;
                            user.FullName = fullName;
                            _userManager.AddOrUpdateUser(user);
                            Invoke(new Action(RefreshUserList));
                        }
                    }

                    if (!_userManager.IsAllowed(chatId))
                    {
                        AppendLog("⛔ Reject User", LogStatus.Warning, $"{chatId} (@{username})");
                        try { await _bot.Client.SendMessage(chatId, "⛔ شما مجاز به استفاده از این ربات نیستید."); } catch { }
                        return;
                    }

                    if (IsProcessing(chatId))
                    {
                        await _bot.Client.SendMessage(chatId, "⚠️ درخواست قبلی هنوز در حال پردازش است. لطفاً با دکمه «لغو» آن را متوقف کنید یا صبر کنید.");
                        return;
                    }

                    var userData = UserStorage.Load(chatId);
                    bool isFileMessage = message.Photo != null ||
                                         message.Document != null ||
                                         message.Voice != null ||
                                         message.Audio != null ||
                                         message.Video != null;

                    if (isFileMessage)
                    {
                        if (_userManager.CanSendFile(chatId))
                        {
                            var processingMsg = await SendProcessingMessage(chatId, "🔍 در حال دریافت و تحلیل فایل...");
                            var cts = new CancellationTokenSource();
                            _activeRequests[chatId] = cts;

                            var fileProcessor = new TelegramFileProcessor(_bot, _aiService, err => AppendLog(err, LogStatus.Error));

                            await fileProcessor.ProcessMessageAsync(message, chatId, processingMsg, userData, cts.Token);

                            _activeRequests.TryRemove(chatId, out _);
                        }
                        else
                        {
                            await _bot.Client.SendMessage(chatId, "⛔ شما مجاز به ارسال فایل نیستید.");
                        }
                        return;
                    }

                    if (string.IsNullOrEmpty(message.Text)) return;

                    if (message.Text.StartsWith("/start"))
                    {
                        userData.RecentFiles.Clear();
                        userData.History.Clear();
                        userData.State = null;
                        UserStorage.Save(userData);

                        var replyKeyboard = new ReplyKeyboardMarkup(new[]
                        {
                                new KeyboardButton("💰 بررسی اعتبار"),
                                new KeyboardButton("⚙️ تنظیمات"),
                                new KeyboardButton("✨ گفتگوی جدید")
                            })
                        {
                            ResizeKeyboard = true,
                            OneTimeKeyboard = false
                        };

                        await _bot.Client.SendMessage(
                            chatId,
                            "👋 سلام! به ربات هوش مصنوعی خوش آمدید. چطور می‌توانم کمک کنم؟",
                            replyMarkup: replyKeyboard,
                            replyParameters: new ReplyParameters { MessageId = message.MessageId }
                        );
                        return;
                    }

                    if (message.Text == "⚙️ تنظیمات" || message.Text.StartsWith("/settings"))
                    {
                        await SettingsMenuHandler.ShowSettings(_bot.Client, chatId, userData, _userManager);
                        return;
                    }

                    if (message.Text == "✨ گفتگوی جدید")
                    {
                        await SettingsMenuHandler.ClearHistory(_bot.Client, chatId, userData, _userManager);
                        await SettingsMenuHandler.ReturnMainKeyboard(_bot.Client, chatId);
                        return;
                    }

                    if (message.Text == "💰 بررسی اعتبار")
                    {
                        var creditText = await CreditService.GetCreditDisplayTextAsync();
                        await _bot.Client.SendMessage(chatId, creditText, parseMode: ParseMode.Markdown);
                        return;
                    }

                    if (!string.IsNullOrEmpty(userData.State))
                    {
                        await SettingsMenuHandler.HandleStateInput(_bot.Client, chatId, userData, message.Text, _userManager);
                        return;
                    }


                    userData.History.Add(new BaleAiBot.Helpers.ChatMessage { Role = "user", Content = message.Text });
                    UserStorage.Save(userData);

                    var textProcessingMsg = await SendProcessingMessage(chatId, "🤔 در حال فکر کردن...");
                    var ctsText = new CancellationTokenSource();
                    _activeRequests[chatId] = ctsText;
                    var tokenText = ctsText.Token;

                    var chatBuffer = new StringBuilder();
                    var chatLastEdit = DateTime.MinValue;

                    try
                    {
                        await foreach (var token in _aiService.StreamTextResponseAsync(userData, tokenText))
                        {
                            tokenText.ThrowIfCancellationRequested();
                            chatBuffer.Append(token);
                            var now = DateTime.UtcNow;
                            if ((now - chatLastEdit).TotalSeconds >= BotConfig.STREAM_EDIT_INTERVAL &&
                                chatBuffer.Length >= BotConfig.STREAM_MIN_CHARS)
                            {
                                await EditProcessingMessage(chatId, textProcessingMsg.MessageId, chatBuffer.ToString());
                                chatLastEdit = now;
                            }
                        }
                        await FinalizeMessage(chatId, textProcessingMsg.MessageId, chatBuffer.ToString(), cancelled: false);
                    }
                    catch (OperationCanceledException)
                    {
                        await FinalizeMessage(chatId, textProcessingMsg.MessageId, "✅ عملیات لغو شد.", cancelled: true);
                    }
                    catch (Exception ex)
                    {
                        await FinalizeMessage(chatId, textProcessingMsg.MessageId, $"⚠️ خطا: {ex.Message}", cancelled: true);
                        AppendLog(ex.Message, LogStatus.Error);
                    }
                    finally
                    {
                        UserStorage.Save(userData);
                        _activeRequests.TryRemove(chatId, out _);
                    }
                };

                _bot.OnUpdate += async (client, update, ct) =>
                {
                    if (update.CallbackQuery is { } query)
                    {
                        var chatId = query.Message!.Chat.Id;
                        if (!_userManager.IsAllowed(chatId)) return;

                        if (query.Data == "cancel_operation")
                        {
                            if (_activeRequests.TryGetValue(chatId, out var cts))
                            {
                                cts.Cancel();
                                await _bot.Client.AnswerCallbackQuery(query.Id, "درخواست لغو شد.");
                            }
                            else
                            {
                                await _bot.Client.AnswerCallbackQuery(query.Id, "هیچ عملیات فعالی یافت نشد.");
                            }
                            return;
                        }

                        var userData = UserStorage.Load(chatId);
                        await SettingsMenuHandler.HandleCallback(client, query, userData, _userManager);
                    }
                };
            }

            try
            {
                _bot.StartReceiving();
                AppendLog("Bot Start Successfuly...", LogStatus.Success);
                btnStart.Enabled = false;
                btnStop.Enabled = true;
            }
            catch (Exception ex)
            {
                AppendLog($"Failed to start bot: {ex.Message}", LogStatus.Error);
                MessageBox.Show($"Failed to start bot: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void btnStop_Click(object sender, EventArgs e)
        {
            _bot?.StopReceiving();
            AppendLog("Bot stopped.");
            btnStart.Enabled = true;
            btnStop.Enabled = false;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _bot?.StopReceiving();
        }

        private enum LogStatus { Info, Success, Error, Warning }

        private void AppendLog(string message, LogStatus status = LogStatus.Info, string userInfo = "")
        {
            if (lvLog.InvokeRequired)
            {
                lvLog.Invoke(new Action<string, LogStatus, string>(AppendLog), message, status, userInfo);
                return;
            }

            ListViewItem item = new ListViewItem(DateTime.Now.ToString("HH:mm:ss"));

            item.SubItems.Add(GetStatusText(status));
            item.SubItems.Add(userInfo);
            item.SubItems.Add(message);

            switch (status)
            {
                case LogStatus.Success:
                    item.BackColor = Color.FromArgb(230, 255, 230);
                    item.ForeColor = Color.DarkGreen;
                    break;
                case LogStatus.Error:
                    item.BackColor = Color.FromArgb(255, 230, 230);
                    item.ForeColor = Color.DarkRed;
                    break;
                case LogStatus.Warning:
                    item.BackColor = Color.FromArgb(255, 255, 230);
                    item.ForeColor = Color.DarkGoldenrod;
                    break;
                default: // Info
                    item.BackColor = Color.White;
                    item.ForeColor = Color.Black;
                    break;
            }

            lvLog.Items.Insert(0, item);

            while (lvLog.Items.Count > 500)
                lvLog.Items.RemoveAt(lvLog.Items.Count - 1);
        }

        private string GetStatusText(LogStatus status)
        {
            return status switch
            {
                LogStatus.Success => "✅ Success",
                LogStatus.Error => "❌ Error",
                LogStatus.Warning => "⚠️ Warning",
                _ => "ℹ️ Info"
            };
        }




        private void RefreshUserList()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(RefreshUserList));
                return;
            }

            lvUsers.Items.Clear();
            foreach (var user in _userManager.GetAllUsers())
            {
                var item = new ListViewItem(user.ChatId.ToString());
                item.SubItems.Add(user.FullName);
                item.SubItems.Add(user.Username);
                item.SubItems.Add(user.IsActive ? "✅" : "❌");
                item.SubItems.Add(user.CanSendFiles ? "✅" : "❌");
                item.SubItems.Add(user.HasMenuAccess ? "✅" : "❌");
                item.SubItems.Add(user.CanChangeModel ? "✅" : "❌");
                item.SubItems.Add(user.CanChangeSystemPrompt ? "✅" : "❌");
                item.SubItems.Add(user.CanChangeToken ? "✅" : "❌");

                item.Tag = user;

                lvUsers.Items.Add(item);
            }
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            if (lvLog.SelectedItems.Count == 0) return;

            ListViewItem selectedItem = lvLog.SelectedItems[0];

            string userInfo = selectedItem.SubItems[2].Text;

            if (string.IsNullOrWhiteSpace(userInfo))
            {

                AppendLog("There is no info about User in this Record.", LogStatus.Error);
                return;
            }

            string idString = userInfo.Split(' ')[0];

            if (long.TryParse(idString, out long chatId))
            {
                string username = "";
                if (userInfo.Contains("(@"))
                {
                    username = userInfo.Split(new[] { "(@" }, StringSplitOptions.None)[1].TrimEnd(')');
                }

                if (_userManager.IsAllowed(chatId))
                {

                    AppendLog("this User Added To List Before", LogStatus.Warning);
                    return;
                }

                _userManager.AddOrUpdateUser(new BotUser
                {
                    ChatId = chatId,
                    Username = username,
                    FullName = "Unknown",
                    IsActive = true,
                    CanSendFiles = true
                });

                _userManager.SaveUsers();
                RefreshUserList();

                AppendLog($"User Added To List Successfuly {chatId}", LogStatus.Success);

            }
            else
            {
                AppendLog("User Id Format Error", LogStatus.Error);
            }
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            if (lvUsers.SelectedItems.Count > 0)
            {

                var tag = lvUsers.SelectedItems[0].Tag;
                if (tag == null)
                    return;

                var user = (BotUser)tag;
                if (user == null)
                    return;
                _userManager.RemoveUser(user.ChatId);
                RefreshUserList();

            }
        }

        private void btnUserOptions_Click(object sender, EventArgs e)
        {
            UserOptionsForm userOptionsForm = new UserOptionsForm();

            var tag = lvUsers.SelectedItems[0].Tag;
            if (tag == null)
                return;

            var user = (BotUser)tag;
            userOptionsForm.CurrentUser = user;
            userOptionsForm.Users = _userManager;
            userOptionsForm.ShowDialog();
            RefreshUserList();
        }

        private void btnOptions_Click(object sender, EventArgs e)
        {
            ConfigEditorForm form =  new ConfigEditorForm();
            form.ShowDialog();

        }
    }

}