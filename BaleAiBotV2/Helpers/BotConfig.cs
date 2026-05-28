using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public static class BotConfig
{
    public static string BOT_TOKEN { get; private set; } = null!;
    public static string BASE_URL { get; private set; } = null!;
    public static string AVAL_API_KEY { get; private set; } = null!;
    public static string AVAL_BASE_URL { get; private set; } = null!;
    public static string AVAL_BASE_CREDIT { get; private set; } = null!;
    public static string AVALAI_BASE_AUDIO_URL { get; private set; } = null!;
    public static string BALE_FILE_URL { get; private set; } = null!;
    public static string DEFAULT_MODEL { get; private set; } = null!;
    public static Dictionary<string, string> MODELS { get; private set; } = null!;
    public static string IMAGE_ANALYSIS_MODEL { get; private set; } = null!;
    public static string AUDIO_ANALYSIS_MODEL { get; private set; } = null!;
    public static string AUDIO_LANGUAGE { get; private set; } = null!;
    public static int MAX_HISTORY { get; private set; }
    public static int IMAGE_MAX_SIZE_COMPRESS { get; private set; }
    public static int MAX_DOCTEXTSIZE { get; private set; }
    public static int VECTOR_DIM { get; private set; }
    public static bool VECTOR_MEMORY_ENABLED { get; private set; }
    public static double STREAM_EDIT_INTERVAL { get; private set; }
    public static int STREAM_MIN_CHARS { get; private set; }

    static BotConfig()
    {
        LoadConfig();
    }

    private static void LoadConfig()
    {
        const string configPath = "botconfig.json";
        if (!File.Exists(configPath))
            throw new FileNotFoundException($"فایل پیکربندی یافت نشد: {configPath}");

        var json = File.ReadAllText(configPath);
        var data = JsonSerializer.Deserialize<BotConfigData>(json);
        if (data == null)
            throw new InvalidOperationException("خطا در دسرالایز کردن فایل JSON.");

        BOT_TOKEN = data.BOT_TOKEN ?? throw new InvalidOperationException("BOT_TOKEN در فایل JSON ضروری است.");
        BASE_URL = data.BASE_URL ?? "https://tapi.bale.ai/bot";
        AVAL_API_KEY = data.AVAL_API_KEY ?? throw new InvalidOperationException("AVAL_API_KEY در فایل JSON ضروری است.");
        AVAL_BASE_URL = data.AVAL_BASE_URL ?? "https://api.avalapis.ir/v1";
        AVAL_BASE_CREDIT = data.AVAL_BASE_CREDIT ?? "https://api.avalapis.ir/user/v1/credit";
        AVALAI_BASE_AUDIO_URL = data.AVALAI_BASE_AUDIO_URL ?? "https://api.avalapis.ir/v1/audio/transcriptions";
        BALE_FILE_URL = data.BALE_FILE_URL ?? "https://tapi.bale.ai/file/bot";
        DEFAULT_MODEL = data.DEFAULT_MODEL ?? "gpt-5.4-nano";
        MODELS = data.MODELS ?? new Dictionary<string, string>
        {
            { "gpt-5.4-nano", "⚡ GPT-5.4 Nano — سریع و سبک" },
            { "gpt-5.4-mini", "🚀 GPT-5.4 Mini — تعادل سرعت و کیفیت" },
            { "gpt-5.4", "🧠 GPT-5.4 — قدرتمند" },
            { "gpt-4o", "👁 GPT-4o — پشتیبانی تصویر" },
            { "gemini-2.5-pro", "💎 Gemini 2.5 Pro — گوگل" },
            { "claude-sonnet-4-5", "🎭 Claude Sonnet 4.5 — آنتروپیک" }
        };
        MAX_HISTORY = data.MAX_HISTORY ?? 250;
        IMAGE_MAX_SIZE_COMPRESS = data.IMAGE_MAX_SIZE_COMPRESS ?? 51200;
        MAX_DOCTEXTSIZE = data.MAX_DOCTEXTSIZE ?? 15000;
        VECTOR_DIM = data.VECTOR_DIM ?? 384;
        VECTOR_MEMORY_ENABLED = data.VECTOR_MEMORY_ENABLED ?? false;
        IMAGE_ANALYSIS_MODEL = data.IMAGE_ANALYSIS_MODEL ?? "gpt-4o";
        AUDIO_ANALYSIS_MODEL = data.AUDIO_ANALYSIS_MODEL ?? "gpt-4o-transcribe";
        STREAM_EDIT_INTERVAL = data.STREAM_EDIT_INTERVAL ?? 0.5;
        STREAM_MIN_CHARS = data.STREAM_MIN_CHARS ?? 50;
        AUDIO_LANGUAGE = data.AUDIO_LANGUAGE ?? "fa";
    }

    private class BotConfigData
    {
        public string? BOT_TOKEN { get; set; }
        public string? BASE_URL { get; set; }
        public string? AVAL_API_KEY { get; set; }
        public string? AVAL_BASE_URL { get; set; }
        public string? AVAL_BASE_CREDIT { get;  set; }
        public string? AVALAI_BASE_AUDIO_URL { get; set; }
        public string? BALE_FILE_URL { get; set; }
        public string? DEFAULT_MODEL { get; set; }
        public Dictionary<string, string>? MODELS { get; set; }
        public int? MAX_HISTORY { get; set; }
        public int? IMAGE_MAX_SIZE_COMPRESS { get; set; }
        public int? MAX_DOCTEXTSIZE { get; set; }
        public int? VECTOR_DIM { get; set; }
        public bool? VECTOR_MEMORY_ENABLED { get; set; }
        public string? IMAGE_ANALYSIS_MODEL { get; set; }
        public string? AUDIO_ANALYSIS_MODEL { get; set; }
        public double? STREAM_EDIT_INTERVAL { get; set; }
        public int? STREAM_MIN_CHARS { get; set; }
        public string? AUDIO_LANGUAGE { get; set; }
    }
}