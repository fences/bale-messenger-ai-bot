using BaleAiBotV2.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BaleAiBotV2.Forms
{
    public partial class UserOptionsForm : Form
    {
        public UserOptionsForm()
        {
            InitializeComponent();
        }

        public UserManager? Users { get; set; }
        public BotUser? CurrentUser { get; set; }  

        private void UserOptionsForm_Load(object sender, EventArgs e)
        {

            if (Users == null || CurrentUser == null)
            {
                MessageBox.Show("User Info Not Found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            chkIsActive.Checked = CurrentUser.IsActive;
            chkCanSendFiles.Checked = CurrentUser.CanSendFiles;
            chkHasMenuAccess.Checked = CurrentUser.HasMenuAccess;
            chkCanChangeModel.Checked = CurrentUser.CanChangeModel;
            chkCanChangeSystemPrompt.Checked = CurrentUser.CanChangeSystemPrompt;
            chkCanChangeToken.Checked = CurrentUser.CanChangeToken;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (CurrentUser == null || Users == null)
            {
                MessageBox.Show("Can Not Save Info", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            CurrentUser.IsActive = chkIsActive.Checked;
            CurrentUser.CanSendFiles = chkCanSendFiles.Checked;
            CurrentUser.HasMenuAccess = chkHasMenuAccess.Checked;
            CurrentUser.CanChangeModel = chkCanChangeModel.Checked;
            CurrentUser.CanChangeSystemPrompt = chkCanChangeSystemPrompt.Checked;
            CurrentUser.CanChangeToken = chkCanChangeToken.Checked;
            Users.AddOrUpdateUser(CurrentUser);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
