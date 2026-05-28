using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BaleAiBotV2.Helpers
{

    public class CreditSourcePackage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("title")]
        public string Title { get; set; } = "";

        [JsonPropertyName("remaining_unit")]
        public double RemainingUnit { get; set; }
    }

    public class CreditSources
    {
        [JsonPropertyName("grants")]
        public List<object> Grants { get; set; } = new();

        [JsonPropertyName("packages")]
        public List<CreditSourcePackage> Packages { get; set; } = new();
    }

    public class CreditInfo
    {
        [JsonPropertyName("limit")]
        public double Limit { get; set; }

        [JsonPropertyName("remaining_irt")]
        public double RemainingIrt { get; set; }

        [JsonPropertyName("remaining_unit")]
        public double RemainingUnit { get; set; }

        [JsonPropertyName("total_unit")]
        public double TotalUnit { get; set; }

        [JsonPropertyName("exchange_rate")]
        public double ExchangeRate { get; set; }

        [JsonPropertyName("account_tier")]
        public int AccountTier { get; set; }

        [JsonPropertyName("credit_sources")]
        public CreditSources CreditSources { get; set; } = new();
    }


    public static class CreditService
    {
        private static readonly HttpClient _http = new HttpClient();

        public static async Task<CreditInfo?> GetCreditAsync()
        {
            try
            {
                string url = BotConfig.AVAL_BASE_CREDIT; 
                using var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("Authorization", $"Bearer {BotConfig.AVAL_API_KEY}");

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<CreditInfo>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<string> GetCreditDisplayTextAsync()
        {
            var credit = await GetCreditAsync();
            if (credit == null)
                return "❌ خطا در دریافت اطلاعات اعتبار. لطفاً بعداً تلاش کنید.";

            string ProtectNumber(string numberStr)
            {
                if (string.IsNullOrEmpty(numberStr)) return numberStr;
                return string.Join("‌", numberStr.ToCharArray());
            }

            string remIrt = credit.RemainingIrt.ToString("N2");
            string remUnit = credit.RemainingUnit.ToString("N4");
            string totalUnit = credit.TotalUnit.ToString("N4");
            string exchangeRate = credit.ExchangeRate.ToString("N2");
            string limit = credit.Limit.ToString("N0");

            return $"💰 **اعتبار باقی‌مانده**\n" +
                   $"• تومان: {ProtectNumber(remIrt)} تومان\n" +
                   $"• واحد (تتر): {ProtectNumber(remUnit)}\n" +
                   $"• مجموع واحدها: {ProtectNumber(totalUnit)}\n" +
                   $"• نرخ ارز: {ProtectNumber(exchangeRate)} تومان\n" +
                   $"• سقف: {ProtectNumber(limit)}\n" +
                   $"• سطح حساب: {credit.AccountTier}";
        }
    }
}