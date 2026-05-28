using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Windows.Forms;

namespace ConfigEditor
{
    public class ConfigEditorForm : Form
    {
        private const string ConfigFileName = "botconfig.json";
        private Panel _scrollContainer;
        private TableLayoutPanel _contentTable;
        private Dictionary<string, Control> _controlMap = new Dictionary<string, Control>();
        private DataGridView _dgvModels;
        private Button _btnSave;

        public ConfigEditorForm()
        {
            InitializeComponent();
            LoadConfig();
        }

        private void InitializeComponent()
        {
            this.Text = "Configuration Editor - botconfig.json";
            this.Size = new Size(850, 650);
            this.MinimumSize = new Size(700, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.Font = new Font("Segoe UI", 9.0F, FontStyle.Regular);

            // Main TableLayout (top: toolbar, bottom: content)
            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // Toolbar
            Panel toolbar = new Panel { Height = 40, Dock = DockStyle.Fill };
            _btnSave = new Button
            {
                Text = "💾 Save to botconfig.json",
                Size = new Size(180, 35),
                BackColor = Color.FromArgb(0, 122, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            _btnSave.FlatAppearance.BorderSize = 0;
            _btnSave.Click += BtnSave_Click;
            toolbar.Controls.Add(_btnSave);
            _btnSave.Location = new Point(10, 5);

            // Scrollable content area
            _scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            _contentTable = new TableLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                ColumnCount = 2,
                Dock = DockStyle.Top,
                Padding = new Padding(10),
                BackColor = Color.White
            };
            _contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            _contentTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65));

            _scrollContainer.Controls.Add(_contentTable);
            mainLayout.Controls.Add(toolbar, 0, 0);
            mainLayout.Controls.Add(_scrollContainer, 0, 1);
            this.Controls.Add(mainLayout);
        }

        private void LoadConfig()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);
            if (!File.Exists(configPath))
            {
                MessageBox.Show($"File '{ConfigFileName}' not found. Creating default configuration.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CreateDefaultConfigFile(configPath);
            }

            try
            {
                string jsonString = File.ReadAllText(configPath);
                var jsonObj = JsonSerializer.Deserialize<JsonObject>(jsonString);
                if (jsonObj == null) throw new InvalidDataException("Invalid JSON format.");
                BuildControlsFromJson(jsonObj);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CreateDefaultConfigFile(string path)
        {
            var defaultJson = new JsonObject
            {
                ["BOT_TOKEN"] = "",
                ["BASE_URL"] = "https://tapi.bale.ai/bot",
                ["AVAL_API_KEY"] = "",
                ["AVAL_BASE_URL"] = "https://api.avalapis.ir/v1",
                ["AVAL_BASE_CREDIT"] = "https://api.avalapis.ir/user/v1/credit",
                ["AVALAI_BASE_AUDIO_URL"] = "https://api.avalapis.ir/v1/audio/transcriptions",
                ["BALE_FILE_URL"] = "https://tapi.bale.ai/file/bot",
                ["DEFAULT_MODEL"] = "gpt-5.4-nano",
                ["MODELS"] = new JsonObject
                {
                    ["gpt-5.4-nano"] = "⚡ GPT-5.4 Nano — سریع و سبک",
                    ["gpt-5.4-mini"] = "🚀 GPT-5.4 Mini — تعادل سرعت و کیفیت",
                    ["gpt-5.4"] = "🧠 GPT-5.4 — قدرتمند",
                    ["gpt-4o"] = "👁 GPT-4o — پشتیبانی تصویر",
                    ["gemini-2.5-pro"] = "💎 Gemini 2.5 Pro — گوگل",
                    ["claude-sonnet-4-5"] = "🎭 Claude Sonnet 4.5 — آنتروپیک"
                },
                ["MAX_HISTORY"] = 250,
                ["IMAGE_MAX_SIZE_COMPRESS"] = 30720,
                ["MAX_DOCTEXTSIZE"] = 15000,
                ["VECTOR_DIM"] = 384,
                ["VECTOR_MEMORY_ENABLED"] = false,
                ["IMAGE_ANALYSIS_MODEL"] = "gpt-4o",
                ["AUDIO_ANALYSIS_MODEL"] = "gpt-4o-transcribe",
                ["STREAM_EDIT_INTERVAL"] = 0.5,
                ["STREAM_MIN_CHARS"] = 50,
                ["AUDIO_LANGUAGE"] = "fa"
            };
            string jsonOutput = JsonSerializer.Serialize(defaultJson, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, jsonOutput);
        }

        private TextBox CreateTextBox(string defaultValue, bool isPassword = false)
        {
            TextBox txt = new TextBox { Text = defaultValue, Anchor = AnchorStyles.Left | AnchorStyles.Right };
            if (isPassword)
            {
                txt.UseSystemPasswordChar = true;
            }
            return txt;
        }


        private void BuildControlsFromJson(JsonObject root)
        {
            _contentTable.Controls.Clear();
            _controlMap.Clear();
            _dgvModels = null;

            int row = 0;
            foreach (var prop in root)
            {
                string key = prop.Key;
                JsonNode? value = prop.Value;

                // Label
                Label lbl = new Label
                {
                    Text = ToTitleCase(key),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(40, 40, 60),
                    Margin = new Padding(5, 12, 5, 5),
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Control editorControl = null;

                if (key == "MODELS" && value is JsonObject modelsObj)
                {
                    editorControl = CreateModelsDataGridView(modelsObj);
                }
                else
                {
                    // Extract raw value
                    object? rawVal = GetRawValue(value);
                    if (rawVal is bool b)
                        editorControl = CreateCheckBox(b);
                    else if (rawVal is int i)
                        editorControl = CreateNumericUpDown(i, Math.Min(0, i - 100), Math.Max(i + 100, 1000), 0);
                    else if (rawVal is double d)
                        editorControl = CreateNumericUpDown((decimal)d, -1000000M, 1000000M, 2);
                    else if (rawVal is string s)
                    {
                        bool isPassword = (key == "BOT_TOKEN" || key == "AVAL_API_KEY");
                        editorControl = CreateTextBox(s, isPassword);
                    }
                    else
                    {
                        editorControl = CreateTextBox(rawVal?.ToString() ?? "");
                    }

                }

                if (editorControl != null)
                {
                    editorControl.Margin = new Padding(5, 5, 15, 5);
                    editorControl.Dock = DockStyle.Fill;

                    _contentTable.RowCount = row + 1;
                    _contentTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    _contentTable.Controls.Add(lbl, 0, row);
                    _contentTable.Controls.Add(editorControl, 1, row);
                    _controlMap[key] = editorControl;
                    row++;
                }
            }

            _contentTable.PerformLayout();
            _scrollContainer.PerformLayout();
        }

        private object? GetRawValue(JsonNode? node)
        {
            if (node is JsonValue jVal)
            {
                if (jVal.TryGetValue(out int intVal)) return intVal;
                if (jVal.TryGetValue(out long longVal)) return (int)longVal;
                if (jVal.TryGetValue(out double dblVal)) return dblVal;
                if (jVal.TryGetValue(out bool boolVal)) return boolVal;
                if (jVal.TryGetValue(out string strVal)) return strVal;
            }
            return node?.ToString();
        }

        private DataGridView CreateModelsDataGridView(JsonObject modelsObj)
        {
            DataGridView dgv = new DataGridView
            {
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                RowHeadersWidth = 30,
                Height = 150,
                Width = 350,
                ReadOnly = false
            };
            dgv.Columns.Add("Key", "Model Key");
            dgv.Columns.Add("Value", "Description");
            dgv.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;

            foreach (var kvp in modelsObj)
                dgv.Rows.Add(kvp.Key, kvp.Value?.ToString());

            return dgv;
        }

        private CheckBox CreateCheckBox(bool defaultValue)
        {
            return new CheckBox { Checked = defaultValue, AutoSize = true };
        }

        private NumericUpDown CreateNumericUpDown(decimal defaultValue, decimal min, decimal max, int decimalPlaces)
        {
            NumericUpDown num = new NumericUpDown
            {
               
                Minimum = min,
                Maximum = max,
                Value = Math.Clamp(defaultValue, min, max),
                DecimalPlaces = decimalPlaces,
                ThousandsSeparator = true,
                Increment = decimalPlaces > 0 ? 0.1m : 1
            };
            return num;
        }

        private TextBox CreateTextBox(string defaultValue)
        {
            return new TextBox { Text = defaultValue, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        }

        private string ToTitleCase(string key)
        {
            return string.Join(" ", key.Split('_').Select(p => char.ToUpper(p[0]) + p.Substring(1).ToLower()));
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            SaveConfig();
        }

        private void SaveConfig()
        {
            try
            {
                JsonObject root = new JsonObject();

                foreach (var kvp in _controlMap)
                {
                    string key = kvp.Key;
                    Control ctrl = kvp.Value;

                    if (key == "MODELS" && ctrl is DataGridView dgv)
                    {
                        JsonObject modelsObj = new JsonObject();
                        foreach (DataGridViewRow row in dgv.Rows)
                        {
                            if (row.IsNewRow) continue;
                            string modelKey = row.Cells[0]?.Value?.ToString();
                            string modelDesc = row.Cells[1]?.Value?.ToString();
                            if (!string.IsNullOrEmpty(modelKey))
                                modelsObj[modelKey] = modelDesc ?? "";
                        }
                        root[key] = modelsObj;
                    }
                    else if (ctrl is CheckBox chk)
                        root[key] = chk.Checked;
                    else if (ctrl is NumericUpDown num)
                    {
                        if (num.DecimalPlaces > 0)
                            root[key] = (double)num.Value;
                        else
                            root[key] = (int)num.Value;
                    }
                    else if (ctrl is TextBox txt)
                        root[key] = txt.Text;
                }

                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };

                string jsonOutput = JsonSerializer.Serialize(root, options);
                File.WriteAllText(configPath, jsonOutput);
                MessageBox.Show($"Configuration saved to {ConfigFileName}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving config: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}