namespace BaleAiBotV2.Forms
{
    partial class UserOptionsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UserOptionsForm));
            btnSave = new Button();
            btnCancel = new Button();
            chkIsActive = new CheckBox();
            chkCanChangeToken = new CheckBox();
            chkCanChangeSystemPrompt = new CheckBox();
            chkCanChangeModel = new CheckBox();
            chkHasMenuAccess = new CheckBox();
            chkCanSendFiles = new CheckBox();
            groupBox1 = new GroupBox();
            groupBox1.SuspendLayout();
            SuspendLayout();
            // 
            // btnSave
            // 
            btnSave.Location = new Point(274, 132);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(75, 23);
            btnSave.TabIndex = 0;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = true;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.Location = new Point(193, 132);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // chkIsActive
            // 
            chkIsActive.AutoSize = true;
            chkIsActive.Location = new Point(20, 25);
            chkIsActive.Name = "chkIsActive";
            chkIsActive.Size = new Size(73, 19);
            chkIsActive.TabIndex = 2;
            chkIsActive.Text = "Is Active ";
            chkIsActive.UseVisualStyleBackColor = true;
            // 
            // chkCanChangeToken
            // 
            chkCanChangeToken.AutoSize = true;
            chkCanChangeToken.Location = new Point(150, 50);
            chkCanChangeToken.Name = "chkCanChangeToken";
            chkCanChangeToken.Size = new Size(125, 19);
            chkCanChangeToken.TabIndex = 3;
            chkCanChangeToken.Text = "Can Change Token";
            chkCanChangeToken.UseVisualStyleBackColor = true;
            // 
            // chkCanChangeSystemPrompt
            // 
            chkCanChangeSystemPrompt.AutoSize = true;
            chkCanChangeSystemPrompt.Location = new Point(150, 25);
            chkCanChangeSystemPrompt.Name = "chkCanChangeSystemPrompt";
            chkCanChangeSystemPrompt.Size = new Size(175, 19);
            chkCanChangeSystemPrompt.TabIndex = 4;
            chkCanChangeSystemPrompt.Text = "Can Change System Prompt";
            chkCanChangeSystemPrompt.UseVisualStyleBackColor = true;
            // 
            // chkCanChangeModel
            // 
            chkCanChangeModel.AutoSize = true;
            chkCanChangeModel.Location = new Point(150, 75);
            chkCanChangeModel.Name = "chkCanChangeModel";
            chkCanChangeModel.Size = new Size(128, 19);
            chkCanChangeModel.TabIndex = 5;
            chkCanChangeModel.Text = "Can Change Model";
            chkCanChangeModel.UseVisualStyleBackColor = true;
            // 
            // chkHasMenuAccess
            // 
            chkHasMenuAccess.AutoSize = true;
            chkHasMenuAccess.Location = new Point(20, 75);
            chkHasMenuAccess.Name = "chkHasMenuAccess";
            chkHasMenuAccess.Size = new Size(119, 19);
            chkHasMenuAccess.TabIndex = 6;
            chkHasMenuAccess.Text = "Has MenuA ccess";
            chkHasMenuAccess.UseVisualStyleBackColor = true;
            // 
            // chkCanSendFiles
            // 
            chkCanSendFiles.AutoSize = true;
            chkCanSendFiles.Location = new Point(20, 50);
            chkCanSendFiles.Name = "chkCanSendFiles";
            chkCanSendFiles.Size = new Size(102, 19);
            chkCanSendFiles.TabIndex = 7;
            chkCanSendFiles.Text = "Can Send Files";
            chkCanSendFiles.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(chkHasMenuAccess);
            groupBox1.Controls.Add(chkCanSendFiles);
            groupBox1.Controls.Add(chkIsActive);
            groupBox1.Controls.Add(chkCanChangeToken);
            groupBox1.Controls.Add(chkCanChangeModel);
            groupBox1.Controls.Add(chkCanChangeSystemPrompt);
            groupBox1.Location = new Point(12, 12);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(337, 114);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "User Options";
            // 
            // UserOptionsForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(366, 163);
            Controls.Add(groupBox1);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UserOptionsForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "UserOptionsForm";
            Load += UserOptionsForm_Load;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Button btnSave;
        private Button btnCancel;
        private CheckBox chkIsActive;
        private CheckBox chkCanChangeToken;
        private CheckBox chkCanChangeSystemPrompt;
        private CheckBox chkCanChangeModel;
        private CheckBox chkHasMenuAccess;
        private CheckBox chkCanSendFiles;
        private GroupBox groupBox1;
    }
}