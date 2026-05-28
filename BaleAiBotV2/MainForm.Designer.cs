namespace BaleAiBotV2
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            lvLog = new ListView();
            chTime = new ColumnHeader();
            chStatus = new ColumnHeader();
            chUser = new ColumnHeader();
            chMessage = new ColumnHeader();
            toolStrip1 = new ToolStrip();
            btnStart = new ToolStripButton();
            btnStop = new ToolStripButton();
            toolStripSeparator1 = new ToolStripSeparator();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            btnAddUser = new ToolStripMenuItem();
            btnDeleteUser = new ToolStripMenuItem();
            btnUserOptions = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            btnOptions = new ToolStripButton();
            tabControl1 = new TabControl();
            tabPage1 = new TabPage();
            tabPage2 = new TabPage();
            lvUsers = new ListView();
            ChatID = new ColumnHeader();
            FullName = new ColumnHeader();
            Username = new ColumnHeader();
            Active = new ColumnHeader();
            CanSendFile = new ColumnHeader();
            HasMenuAccess = new ColumnHeader();
            CanChangeModel = new ColumnHeader();
            CanChangeSystemPrompt = new ColumnHeader();
            CanChangeToken = new ColumnHeader();
            statusStrip1 = new StatusStrip();
            toolStrip1.SuspendLayout();
            tabControl1.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            SuspendLayout();
            // 
            // lvLog
            // 
            lvLog.Columns.AddRange(new ColumnHeader[] { chTime, chStatus, chUser, chMessage });
            lvLog.Dock = DockStyle.Fill;
            lvLog.FullRowSelect = true;
            lvLog.GridLines = true;
            lvLog.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            lvLog.Location = new Point(3, 3);
            lvLog.Name = "lvLog";
            lvLog.Size = new Size(1323, 624);
            lvLog.TabIndex = 2;
            lvLog.UseCompatibleStateImageBehavior = false;
            lvLog.View = View.Details;
            // 
            // chTime
            // 
            chTime.Text = "Time";
            chTime.Width = 70;
            // 
            // chStatus
            // 
            chStatus.Text = "Condition";
            chStatus.Width = 70;
            // 
            // chUser
            // 
            chUser.Text = "User";
            chUser.Width = 120;
            // 
            // chMessage
            // 
            chMessage.Text = "Message";
            chMessage.Width = 800;
            // 
            // toolStrip1
            // 
            toolStrip1.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
            toolStrip1.ImageScalingSize = new Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { btnStart, btnStop, toolStripSeparator1, toolStripDropDownButton1, toolStripSeparator2, btnOptions });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(1337, 31);
            toolStrip1.TabIndex = 3;
            toolStrip1.Text = "toolStrip1";
            // 
            // btnStart
            // 
            btnStart.Image = Properties.Resources._7025;
            btnStart.ImageTransparentColor = Color.Magenta;
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(63, 28);
            btnStart.Text = "Start";
            btnStart.Click += btnStart_Click;
            // 
            // btnStop
            // 
            btnStop.Image = Properties.Resources._7024;
            btnStop.ImageTransparentColor = Color.Magenta;
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(61, 28);
            btnStop.Text = "Stop";
            btnStop.Click += btnStop_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(6, 31);
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { btnAddUser, btnDeleteUser, btnUserOptions });
            toolStripDropDownButton1.Image = Properties.Resources._0492;
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(122, 28);
            toolStripDropDownButton1.Text = "User Manager";
            // 
            // btnAddUser
            // 
            btnAddUser.Image = Properties.Resources._1104;
            btnAddUser.Name = "btnAddUser";
            btnAddUser.Size = new Size(154, 30);
            btnAddUser.Text = "Add User";
            btnAddUser.Click += btnAddUser_Click;
            // 
            // btnDeleteUser
            // 
            btnDeleteUser.Image = Properties.Resources._1106;
            btnDeleteUser.Name = "btnDeleteUser";
            btnDeleteUser.Size = new Size(154, 30);
            btnDeleteUser.Text = "Delete User";
            btnDeleteUser.Click += btnDeleteUser_Click;
            // 
            // btnUserOptions
            // 
            btnUserOptions.Image = Properties.Resources._1103;
            btnUserOptions.Name = "btnUserOptions";
            btnUserOptions.Size = new Size(154, 30);
            btnUserOptions.Text = "User Options";
            btnUserOptions.Click += btnUserOptions_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(6, 31);
            // 
            // btnOptions
            // 
            btnOptions.Image = Properties.Resources._4150;
            btnOptions.ImageTransparentColor = Color.Magenta;
            btnOptions.Name = "btnOptions";
            btnOptions.Size = new Size(78, 28);
            btnOptions.Text = "Options";
            btnOptions.Click += btnOptions_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(tabPage1);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Dock = DockStyle.Fill;
            tabControl1.Location = new Point(0, 31);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1337, 658);
            tabControl1.TabIndex = 4;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(lvLog);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1329, 630);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Robot Status";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(lvUsers);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1329, 630);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Users";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // lvUsers
            // 
            lvUsers.Columns.AddRange(new ColumnHeader[] { ChatID, FullName, Username, Active, CanSendFile, HasMenuAccess, CanChangeModel, CanChangeSystemPrompt, CanChangeToken });
            lvUsers.Dock = DockStyle.Fill;
            lvUsers.FullRowSelect = true;
            lvUsers.GridLines = true;
            lvUsers.Location = new Point(3, 3);
            lvUsers.Name = "lvUsers";
            lvUsers.Size = new Size(1323, 624);
            lvUsers.TabIndex = 0;
            lvUsers.UseCompatibleStateImageBehavior = false;
            lvUsers.View = View.Details;
            // 
            // ChatID
            // 
            ChatID.Text = "Chat ID";
            ChatID.Width = 120;
            // 
            // FullName
            // 
            FullName.Text = "Full Name";
            FullName.Width = 140;
            // 
            // Username
            // 
            Username.Text = "Username";
            Username.Width = 120;
            // 
            // Active
            // 
            Active.Text = "Active";
            Active.Width = 80;
            // 
            // CanSendFile
            // 
            CanSendFile.Text = "Can Send File";
            CanSendFile.Width = 100;
            // 
            // HasMenuAccess
            // 
            HasMenuAccess.Text = "Has Menu Access";
            HasMenuAccess.Width = 120;
            // 
            // CanChangeModel
            // 
            CanChangeModel.Text = "Can Change Model";
            CanChangeModel.Width = 120;
            // 
            // CanChangeSystemPrompt
            // 
            CanChangeSystemPrompt.Text = "Can Change System Prompt";
            CanChangeSystemPrompt.Width = 140;
            // 
            // CanChangeToken
            // 
            CanChangeToken.Text = "Can Change Token";
            CanChangeToken.Width = 120;
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 667);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1337, 22);
            statusStrip1.TabIndex = 5;
            statusStrip1.Text = "statusStrip1";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1337, 689);
            Controls.Add(statusStrip1);
            Controls.Add(tabControl1);
            Controls.Add(toolStrip1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Bale Ai Bot";
            FormClosing += MainForm_FormClosing;
            Load += MainForm_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            tabControl1.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion


        private ListView lvLog;
        private ColumnHeader chTime;
        private ColumnHeader chStatus;
        private ColumnHeader chUser;
        private ColumnHeader chMessage;
        private ToolStrip toolStrip1;
        private ToolStripButton btnStart;
        private ToolStripButton btnStop;
        private TabControl tabControl1;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private ListView lvUsers;
        private ToolStripSeparator toolStripSeparator1;
        private ColumnHeader ChatID;
        private ColumnHeader FullName;
        private ColumnHeader Username;
        private ColumnHeader Active;
        private ColumnHeader CanSendFile;
        private ColumnHeader HasMenuAccess;
        private ColumnHeader CanChangeModel;
        private ColumnHeader CanChangeSystemPrompt;
        private ColumnHeader CanChangeToken;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem btnAddUser;
        private ToolStripMenuItem btnDeleteUser;
        private ToolStripMenuItem btnUserOptions;
        private StatusStrip statusStrip1;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripButton btnOptions;
    }
}
