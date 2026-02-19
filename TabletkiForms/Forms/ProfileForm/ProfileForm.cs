using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CapDefectDetector.Authorization;

namespace CapDefectDetector.Forms
{
    public partial class ProfileForm : Form
    {
        public ProfileForm()
        {
            InitializeComponent();
            profileCmB.SelectedIndex = 1; // Оператор по умолчанию
            profilePasswordTb.Enabled = false;

            profileCmB.SelectedIndexChanged += ProfileCmB_SelectedIndexChanged;
        }

        private void ProfileCmB_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool admin = profileCmB.SelectedItem?.ToString() == "Администратор";
            profilePasswordTb.Enabled = admin;
            if (!admin)
            {
                profilePasswordTb.Enabled = admin;
                profilePasswordTb.Text = "";
            }
        }

        private void okProfileBt_Click(object sender, EventArgs e)
        {
            string selected = profileCmB.SelectedItem?.ToString();

            if (selected == "Оператор")
            {
                AuthManager.Instance.SetRole(Role.Operator);
                this.Close();
                return;
            }

            if (selected == "Администратор")
            {
                if (!AuthManager.Instance.ValidateAdminPassword(profilePasswordTb.Text))
                {
                    MessageBox.Show("Неверный пароль администратора!");
                    return;
                }

                AuthManager.Instance.SetRole(Role.Admin);
                this.Close();
            }
        }

        private void cancelProfileFormBt_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

}
