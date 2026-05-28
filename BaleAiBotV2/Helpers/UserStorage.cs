using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BaleAiBot.Helpers
{
    public static class UserStorage
    {
        private static readonly string BasePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Users");

        private static readonly ConcurrentDictionary<long, object> _userLocks = new();
        private static readonly byte[] Key = Encoding.UTF8.GetBytes("BaleBot1234567890123456789012345");
        private static readonly byte[] IV = Encoding.UTF8.GetBytes("BaleBot123456789");

        private static string GetFilePath(long uid) =>
            Path.Combine(BasePath, $"{uid}.dat");

        public static UserData Load(long uid)
        {
            var lockObj = _userLocks.GetOrAdd(uid, _ => new object());
            lock (lockObj)
            {
                string path = GetFilePath(uid);
                if (!File.Exists(path))
                    return CreateDefault(uid);

                try
                {
                    string encryptedBase64 = File.ReadAllText(path, Encoding.UTF8);
                    string json = Decrypt(encryptedBase64);

                    var data = JsonSerializer.Deserialize<UserData>(json);

                    if (data == null)
                        return CreateDefault(uid);

                    data.ChatId = uid;
                    data.Settings ??= new UserSettings();
                    data.History ??= new List<ChatMessage>();
                    data.RecentFiles ??= new List<UserFile>();

                    if (string.IsNullOrWhiteSpace(data.Settings.SystemPrompt))
                        data.Settings.SystemPrompt = "تو یک دستیار مفید و مختصر هستی. پاسخ‌ها را تا جای ممکن کوتاه بده.";

                    return data;
                }
                catch
                {
                    return CreateDefault(uid);
                }
            }
        }

        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = false,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static void Save(UserData data)
        {
            var lockObj = _userLocks.GetOrAdd(data.ChatId, _ => new object());
            lock (lockObj)
            {
                string json = JsonSerializer.Serialize(data, _jsonOptions);
                string encryptedBase64 = Encrypt(json);

                Directory.CreateDirectory(BasePath);
                File.WriteAllText(GetFilePath(data.ChatId), encryptedBase64, new UTF8Encoding(false));
            }
        }

        private static UserData CreateDefault(long uid)
        {
            return new UserData
            {
                ChatId = uid,
                Settings = new UserSettings
                {
                    Model = BotConfig.DEFAULT_MODEL,
                    Temperature = 1.0,
                    MaxTokens = 10000,
                    SystemPrompt = "تو یک دستیار مفید و مختصر هستی. پاسخ‌ها را تا جای ممکن کوتاه بده."
                },
                History = new List<ChatMessage>(),
                RecentFiles = new List<UserFile>(),
                State = null
            };
        }

        #region Encryption Methods

        private static string Encrypt(string plainText)
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream();
            using CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using (StreamWriter sw = new StreamWriter(cs))
            {
                sw.Write(plainText);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private static string Decrypt(string cipherText)
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;

            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream ms = new MemoryStream(buffer);
            using CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using StreamReader sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        #endregion
    }

    public class UserData
    {
        public long ChatId { get; set; }
        public UserSettings Settings { get; set; } = new();
        public List<ChatMessage> History { get; set; } = new();
        public string? State { get; set; }
        public List<UserFile> RecentFiles { get; set; } = new();
    }

    public class UserFile
    {
        public string? FileType { get; set; }
        public byte[]? ImageBytes { get; set; }
        public string? ExtractedText { get; set; }
        public string? Caption { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

    public class UserSettings
    {
        public string Model { get; set; } = string.Empty;
        public double Temperature { get; set; } = 1.0;
        public int MaxTokens { get; set; } = 10000;
        public string SystemPrompt { get; set; } = string.Empty;
    }

    public class ChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<ContentPart>? ContentParts { get; set; }
    }

    public class ContentPart
    {
        public string? Type { get; set; }
        public string? Text { get; set; }
        public ImageUrl? ImageUrl { get; set; }
    }

    public class ImageUrl
    {
        public string? Url { get; set; }
    }
}