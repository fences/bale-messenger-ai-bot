using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BaleAiBotV2.Helpers
{
    public class UserManager
    {
        private readonly string _filePath = Application.StartupPath + @"\ValidUsers\users.json";
        private ConcurrentDictionary<long, BotUser> _users = new();

        public UserManager()
        {
            LoadUsers();
        }

        public void LoadUsers()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    var list = JsonSerializer.Deserialize<List<BotUser>>(json);
                    if (list != null)
                    {
                        _users = new ConcurrentDictionary<long, BotUser>(list.ToDictionary(u => u.ChatId));
                        return; 
                    }
                }
            }

           
        }

        public void SaveUsers()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_users.Values.ToList(), options);
            File.WriteAllText(_filePath, json);
        }

        public void AddOrUpdateUser(BotUser user)
        {
            _users[user.ChatId] = user;
            SaveUsers();
        }

        public void RemoveUser(long chatId)
        {
            if (_users.TryRemove(chatId, out _))
            {
                SaveUsers();
            }
        }

        public List<BotUser> GetAllUsers() => _users.Values.ToList();

        // بررسی‌های دسترسی
        public bool IsAllowed(long chatId) => _users.TryGetValue(chatId, out var user) && user.IsActive;
        public bool CanSendFile(long chatId) => _users.TryGetValue(chatId, out var user) && user.CanSendFiles;
        public bool HasMenuAccess(long chatId) => _users.TryGetValue(chatId, out var user) && user.HasMenuAccess;
        public bool CanChangeModel(long chatId) => _users.TryGetValue(chatId, out var user) && user.CanChangeModel;
        public bool CanChangeSystemPrompt(long chatId) => _users.TryGetValue(chatId, out var user) && user.CanChangeSystemPrompt;
        public bool CanChangeToken(long chatId) => _users.TryGetValue(chatId, out var user) && user.CanChangeToken;

    }

    public class BotUser
    {
        public long ChatId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;

        // دسترسی‌ها
        public bool IsActive { get; set; } = true;         
        public bool CanSendFiles { get; set; } = false;    
        public bool HasMenuAccess { get; set; } = false;
        public bool CanChangeModel { get; set; } = false;
        public bool CanChangeSystemPrompt { get; set; } = false;
        public bool CanChangeToken { get; set; } = false;
    }
}
