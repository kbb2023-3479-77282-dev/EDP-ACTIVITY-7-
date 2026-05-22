//ACTIVITY 7: Modification in the app name for better visibility
// Final review and alignment complete.
#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;

namespace MyInformationSystem {
    public class LoginForm : Form {
        private TextBox txtUser, txtPass;
        private PictureBox pbLogo;

        public LoginForm() {
            this.Text = "LibApp | Login";
            this.Size = new Size(450, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            pbLogo = new PictureBox {
                Size = new Size(120, 120),
                Top = 40,
                Left = (this.ClientSize.Width - 120) / 2,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            
            if (File.Exists("LibApp.png")) {
                pbLogo.Image = Image.FromFile("LibApp.png");
            } else {
                pbLogo.BackColor = Color.FromArgb(0, 179, 89); 
            }

            Label lblAppName = new Label {
                Text = "LibApp",
                Top = 170,
                Width = 450,
                Height = 60,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Black", 32, FontStyle.Bold), // ACTIVITY 7 MODIFICATION: Increased text size
                ForeColor = Color.FromArgb(0, 179, 89) 
            };

            txtUser = new TextBox { 
                Top = 260, Left = 60, Width = 330, Height = 45, 
                PlaceholderText = "Username", 
                Font = new Font("Segoe UI", 14), // ACTIVITY 7 MODIFICATION: Increased font size
                BorderStyle = BorderStyle.FixedSingle
            };

            txtPass = new TextBox { 
                Top = 330, Left = 60, Width = 330, Height = 45, 
                PlaceholderText = "Password", 
                UseSystemPasswordChar = true, 
                Font = new Font("Segoe UI", 14), // ACTIVITY 7 MODIFICATION: Increased font size
                BorderStyle = BorderStyle.FixedSingle
            };

            Button btnLogin = new Button { 
                Text = "LOGIN", 
                Top = 405, Left = 60, Width = 330, Height = 55, 
                BackColor = Color.FromArgb(0, 179, 89), ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, 
                Font = new Font("Segoe UI Black", 14, FontStyle.Bold), // ACTIVITY 7 MODIFICATION: Updated font family and size
                Cursor = Cursors.Hand 
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += btnLogin_Click;

            LinkLabel lnkForgot = new LinkLabel {
                Text = "Forgot Password?",
                Top = 480,
                Width = 450,
                Height = 30,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11), // ACTIVITY 7 MODIFICATION: Increased font size
                LinkColor = Color.Gray,
                ActiveLinkColor = Color.FromArgb(0, 179, 89),
                Cursor = Cursors.Hand
            };
            lnkForgot.LinkClicked += (s, e) => {
                MessageBox.Show("Please coordinate with the System Administrator (Kenneth Borjal) for password recovery.", "System Notice");
            };

            this.Controls.AddRange(new Control[] { pbLogo, lblAppName, txtUser, txtPass, btnLogin, lnkForgot });
        }

        private void btnLogin_Click(object sender, EventArgs e) {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text)) {
                MessageBox.Show("Authentication fields cannot be empty.", "Error");
                return;
            }

            try {
                using var conn = DatabaseConnection.GetConnection();
                conn.Open();
                
                string query = "SELECT UserID, FullName FROM users WHERE Username = @u AND Password = @p AND Status = 'Active'";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());
                cmd.Parameters.AddWithValue("@p", txtPass.Text.Trim());

                using var reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    AppSession.UserID = Convert.ToInt32(reader["UserID"]);
                    AppSession.FullName = reader["FullName"].ToString();

                    this.Hide();
                    new DashboardForm().Show();
                } else {
                    MessageBox.Show("Incorrect username or password.", "Login Denied");
                }
            } catch (Exception ex) {
                MessageBox.Show("Database Failure: " + ex.Message);
            }
        }
    }
}