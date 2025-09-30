using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LibraryApp
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            cmbCategory.Items.AddRange(new string[] { "Студент", "Аспирант", "Преподаватель" });
        }



        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string category = cmbCategory.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(category))
            {
                lblError.Text = "Заполните все поля";
                return;
            }

            User user = LibraryDB.GetUser(username, password, category);
            if (user == null)
            {
                lblError.Text = "Неверный логин или пароль";
                return;
            }

            this.Hide();
            MainForm mainForm = new MainForm(user);
            mainForm.Show();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}
