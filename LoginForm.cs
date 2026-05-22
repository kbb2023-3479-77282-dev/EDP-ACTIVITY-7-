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
            // Form Setup
            this.Text = "LibApp | Login";
            this.Size = new Size(450, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 1. Logo Branding
            pbLogo = new PictureBox {
                Size = new Size(120, 120),
                Top = 40,
                Left = (this.ClientSize.Width - 120) / 2,
                SizeMode = PictureBoxSizeMode.Zoom
            };
            
            // Hanapin ang LibApp.png sa bin folder
            if (File.Exists("LibApp.png")) {
                pbLogo.Image = Image.FromFile("LibApp.png");
            } else {
                // Background color kung sakaling nawawala ang image file
                pbLogo.BackColor = Color.FromArgb(0, 179, 89); 
            }

            // 2. App Name Display: "LibApp"
            Label lblAppName = new Label {
                Text = "LibApp",
                Top = 170,
                Width = 450,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Black", 28, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 179, 89) // Custom Green
            };

            // 3. User Credentials Fields
            txtUser = new TextBox { 
                Top = 260, Left = 60, Width = 330, Height = 40, 
                PlaceholderText = "Username", 
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle
            };

            txtPass = new TextBox { 
                Top = 320, Left = 60, Width = 330, Height = 40, 
                PlaceholderText = "Password", 
                UseSystemPasswordChar = true, 
                Font = new Font("Segoe UI", 12),
                BorderStyle = BorderStyle.FixedSingle
            };

            // 4. Login Action Button
            Button btnLogin = new Button { 
                Text = "LOGIN", 
                Top = 390, Left = 60, Width = 330, Height = 55, 
                BackColor = Color.FromArgb(0, 179, 89), ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Bold", 12), 
                Cursor = Cursors.Hand 
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += btnLogin_Click;

            // 5. Forget Password Link
            LinkLabel lnkForgot = new LinkLabel {
                Text = "Forgot Password?",
                Top = 460,
                Width = 450,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 10),
                LinkColor = Color.Gray,
                ActiveLinkColor = Color.FromArgb(0, 179, 89),
                Cursor = Cursors.Hand
            };
            lnkForgot.LinkClicked += (s, e) => {
                MessageBox.Show("Please coordinate with the System Administrator (Kenneth Borjal) for password recovery.", "System Notice");
            };

            // Add all components to the UI
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
                
                // Query to fetch UserID and Name
                string query = "SELECT UserID, FullName FROM users WHERE Username = @u AND Password = @p AND Status = 'Active'";
                using var cmd = new MySqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());
                cmd.Parameters.AddWithValue("@p", txtPass.Text.Trim());

                using var reader = cmd.ExecuteReader();
                if (reader.Read()) {
                    // CRITICAL FIX: I-save ang UserID sa session para hindi mag-0 sa transaction
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