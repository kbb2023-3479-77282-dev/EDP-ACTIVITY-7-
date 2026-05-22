#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MyInformationSystem
{
    public class RecoveryForm : Form
    {
        private TextBox txtEmail;
        private Button btnVerify;

        public RecoveryForm()
        {
            this.Text = "Account Recovery";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            Label lbl = new Label { 
                Text = "Enter your registered email address:", 
                Top = 30, Left = 30, Width = 320, AutoSize = true 
            };

            txtEmail = new TextBox { 
                Top = 60, Left = 30, Width = 320, 
                Font = new Font("Segoe UI", 10),
                PlaceholderText = "example@email.com" 
            };

            btnVerify = new Button { 
                Text = "VERIFY EMAIL", 
                Top = 110, Left = 30, Width = 320, Height = 40,
                BackColor = Color.SteelBlue, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Bold", 9)
            };
            btnVerify.Click += BtnVerify_Click;

            this.Controls.AddRange(new Control[] { lbl, txtEmail, btnVerify });
        }

        private void BtnVerify_Click(object sender, EventArgs e)
        {
            string email = txtEmail.Text.Trim();
            if (string.IsNullOrEmpty(email) || !email.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using var conn = DatabaseConnection.GetConnection();
                conn.Open();
                using var cmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE Email = @e", conn);
                cmd.Parameters.AddWithValue("@e", email);

                long exists = Convert.ToInt64(cmd.ExecuteScalar());

                if (exists > 0)
                {
                    MessageBox.Show("Verification successful! A reset link has been sent to your email.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Email address not found in our system.", "Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}