using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace BaleAiBot.Helpers
{
    public class VectorMemory : IDisposable
    {
        private readonly long _uid;
        private bool _enabled;

        private InferenceSession? _onnxSession;
        private int _vectorDim;

        private List<float[]> _vectors;       
        private List<string> _texts;

        private string _indexPath;
        private string _textsPath;

        // توکنایزر
        private WordPieceTokenizer? _tokenizer;

        public VectorMemory(long uid)
        {
            _uid = uid;
            _enabled = BotConfig.VECTOR_MEMORY_ENABLED;
            _vectors = new List<float[]>();
            _texts = new List<string>();
            _vectorDim = BotConfig.VECTOR_DIM;
            _indexPath = $"data/users/{uid}.vecs";        
            _textsPath = $"data/users/{uid}_texts.json";

            if (!_enabled)
            {
                _onnxSession = null;
                _tokenizer = null;
                return;
            }

            // بارگذاری مدل ONNX
            try
            {
                string modelPath = "models/all-MiniLM-L6-v2.onnx";
                if (!File.Exists(modelPath))
                    throw new FileNotFoundException($"مدل ONNX یافت نشد: {modelPath}");

                _onnxSession = new InferenceSession(modelPath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"خطا در بارگذاری مدل embedding: {ex.Message}");
                _enabled = false;
                _onnxSession = null;
                _tokenizer = null;
                _vectors = new List<float[]>();
                _texts = new List<string>();
                return;
            }

            string vocabPath = "models/vocab.txt";
            if (!File.Exists(vocabPath))
            {
                System.Diagnostics.Debug.WriteLine("فایل vocab.txt پیدا نشد. حافظه برداری غیرفعال شد.");
                _enabled = false;
                _onnxSession?.Dispose();
                _onnxSession = null;
                return;
            }

            _tokenizer = new WordPieceTokenizer(vocabPath);

            if (File.Exists(_indexPath))
            {
                _vectors = LoadVectors(_indexPath, _vectorDim);
            }

            // بارگذاری متن‌ها
            if (File.Exists(_textsPath))
            {
                string json = File.ReadAllText(_textsPath, Encoding.UTF8);
                _texts = JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
        }

        private static void SaveVectors(string path, List<float[]> vectors)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);
            bw.Write(vectors.Count);
            if (vectors.Count > 0)
                bw.Write(vectors[0].Length);   
            foreach (var vec in vectors)
            {
                foreach (var v in vec)
                    bw.Write(v);
            }
        }

        private static List<float[]> LoadVectors(string path, int expectedDim)
        {
            var result = new List<float[]>();
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            using var br = new BinaryReader(fs);
            int count = br.ReadInt32();
            int dim = br.ReadInt32();
            if (dim != expectedDim)
                throw new InvalidOperationException($"بُعد ناسازگار: {dim} (مورد انتظار {expectedDim})");

            for (int i = 0; i < count; i++)
            {
                float[] vec = new float[dim];
                for (int j = 0; j < dim; j++)
                    vec[j] = br.ReadSingle();
                result.Add(vec);
            }
            return result;
        }

        private float[] Encode(string text)
        {
            if (_onnxSession == null || _tokenizer == null)
                throw new InvalidOperationException("مدل embedding یا توکنایزر بارگذاری نشده است.");

            // توکنایز کردن متن
            var (inputIds, attentionMask, tokenTypeIds) = _tokenizer.Tokenize(text, maxLength: 128);

            // ساخت ورودی‌های مدل ONNX
            var inputIdTensor = new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length });
            var attentionMaskTensor = new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length });
            var tokenTypeIdTensor = new DenseTensor<long>(tokenTypeIds, new[] { 1, tokenTypeIds.Length });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input_ids", inputIdTensor),
                NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
                NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdTensor)
            };

            // اجرای مدل (نام خروجی معمولاً "sentence_embedding")
            using var results = _onnxSession.Run(inputs);
            var embeddingTensor = results.First().AsTensor<float>();
            return embeddingTensor.ToArray();
        }

        private float[,] EncodeBatch(string[] texts)
        {
            float[,] embeddings = new float[texts.Length, _vectorDim];
            for (int i = 0; i < texts.Length; i++)
            {
                float[] vec = Encode(texts[i]);
                for (int j = 0; j < _vectorDim; j++)
                    embeddings[i, j] = vec[j];
            }
            return embeddings;
        }

        public void Add(string text)
        {
            if (!_enabled || string.IsNullOrWhiteSpace(text) || _onnxSession == null)
                return;

            float[] vector = Encode(text);
            _vectors.Add(vector);
            _texts.Add(text);

            // ذخیره‌سازی روی دیسک
            SaveVectors(_indexPath, _vectors);
            string json = JsonSerializer.Serialize(_texts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_textsPath, json, Encoding.UTF8);
        }

        public List<string> Search(string query, int k = 3)
        {
            if (!_enabled || _vectors.Count == 0 || _onnxSession == null)
                return new List<string>();

            float[] queryVec = Encode(query);

            // محاسبه فاصله‌ی L2 برای همه‌ی بردارها
            var distances = new List<(float distance, int index)>();
            for (int i = 0; i < _vectors.Count; i++)
            {
                float dist = L2Distance(queryVec, _vectors[i]);
                distances.Add((dist, i));
            }

            // انتخاب k تا با کمترین فاصله
            var topK = distances
                        .OrderBy(d => d.distance)
                        .Take(k)
                        .Select(d => d.index)
                        .ToList();

            // بازگرداندن متن‌های متناظر
            return topK.Where(idx => idx >= 0 && idx < _texts.Count)
                       .Select(idx => _texts[idx])
                       .ToList();
        }

        private static float L2Distance(float[] a, float[] b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("طول بردارها برابر نیست");
            float sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                float diff = a[i] - b[i];
                sum += diff * diff;
            }
            return (float)Math.Sqrt(sum);
        }

        public void Dispose()
        {
            _onnxSession?.Dispose();
        }
    }

    // -------------------- WordPiece Tokenizer (حداقلی) --------------------
    public class WordPieceTokenizer
    {
        private readonly Dictionary<string, int> _vocab;
        private readonly Dictionary<int, string> _idToToken;
        private const int CLS_ID = 101;   // [CLS]
        private const int SEP_ID = 102;   // [SEP]
        private const int PAD_ID = 0;     // [PAD]
        private const int UNK_ID = 100;   // [UNK]
        private const int MAX_LEN = 512;

        public WordPieceTokenizer(string vocabPath)
        {
            _vocab = new Dictionary<string, int>();
            _idToToken = new Dictionary<int, string>();
            foreach (var line in File.ReadLines(vocabPath, Encoding.UTF8))
            {
                string token = line.Trim();
                if (string.IsNullOrEmpty(token)) continue;
                int id = _vocab.Count;
                _vocab[token] = id;
                _idToToken[id] = token;
            }
        }

        public (long[] inputIds, long[] attentionMask, long[] tokenTypeIds) Tokenize(string text, int maxLength = 128)
        {
            // نرمال‌سازی اولیه
            text = text.ToLower().Trim();
            var tokens = new List<string> { "[CLS]" };
            foreach (char ch in text)
            {
                if (char.IsWhiteSpace(ch))
                    continue;
                string token = ch.ToString();
                if (_vocab.ContainsKey(token))
                {
                    tokens.Add(token);
                }
                else
                {
                    tokens.Add("[UNK]");
                }
            }
            tokens.Add("[SEP]");

            // کوتاه کردن به maxLength
            if (tokens.Count > maxLength)
                tokens = tokens.Take(maxLength - 1).ToList();
            tokens.Add("[SEP]");   // انتها همیشه [SEP]

            // تبدیل به شناسه
            var inputIds = tokens.Select(t => _vocab.TryGetValue(t, out int id) ? id : UNK_ID).ToArray();
            int len = inputIds.Length;

            // padding
            var paddedIds = new long[maxLength];
            var attentionMask = new long[maxLength];
            var tokenTypeIds = new long[maxLength];
            for (int i = 0; i < maxLength; i++)
            {
                if (i < len)
                {
                    paddedIds[i] = inputIds[i];
                    attentionMask[i] = 1;
                }
                else
                {
                    paddedIds[i] = PAD_ID;
                    attentionMask[i] = 0;
                }
                tokenTypeIds[i] = 0;   // برای تک‌جمله همیشه 0
            }
            return (paddedIds, attentionMask, tokenTypeIds);
        }
    }
}