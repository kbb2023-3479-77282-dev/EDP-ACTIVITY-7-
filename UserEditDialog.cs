#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MyInformationSystem
{
    public class UserEditDialog : Form
    {
        private TextBox txtName, txtUser, txtPass, txtEmail;
        private ComboBox cbRole;
        private string _userId = ""; // Empty for "Add", contains ID for "Edit"

        // Constructor for ADDING a user
        public UserEditDialog() { BuildUI("Add New User"); }

        // Constructor for EDITING a user
        public UserEditDialog(DataGridViewRow selectedRow)
        {
            BuildUI("Update User Profile");
            _userId = selectedRow.Cells["UserID"].Value.ToString();
            txtName.Text = selectedRow.Cells["FullName"].Value.ToString();
            txtUser.Text = selectedRow.Cells["Username"].Value.ToString();
            cbRole.Text = selectedRow.Cells["Role"].Value.ToString();
            // Password and Email would be loaded here if they were in the Grid
        }

        private void BuildUI(string title)
        {
            this.Text = title;
            this.Size = new Size(400, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            txtName = CreateInput("Full Name:", 30);
            txtUser = CreateInput("Username:", 90);
            txtPass = CreateInput("Password:", 150, true);
            txtEmail = CreateInput("Email Address:", 210);

            Label lblRole = new Label { Text = "Role:", Top = 270, Left = 40, AutoSize = true };
            cbRole = new ComboBox { Top = 290, Left = 40, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cbRole.Items.AddRange(new string[] { "Admin", "Student", "Faculty" });
            cbRole.SelectedIndex = 1;

            Button btnSave = new Button { 
                Text = "SAVE CHANGES", Top = 350, Left = 40, Width = 300, Height = 45,
                BackColor = Color.SeaGreen, ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Bold", 10)
            };
            btnSave.Click += (s, e) => SaveData();

            this.Controls.AddRange(new Control[] { lblRole, cbRole, btnSave });
        }

        private TextBox CreateInput(string label, int top, bool isPassword = false)
        {
            this.Controls.Add(new Label { Text = label, Top = top, Left = 40, AutoSize = true });
            TextBox tb = new TextBox { Top = top + 20, Left = 40, Width = 300, Font = new Font("Segoe UI", 10), UseSystemPasswordChar = isPassword };
            this.Controls.Add(tb);
            return tb;
        }

        private void SaveData()
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtUser.Text))
            {
                MessageBox.Show("Name and Username are required.");
                return;
            }

            try
            {
                using var conn = DatabaseConnection.GetConnection();
                conn.Open();
                string sql;

                if (string.IsNullOrEmpty(_userId)) // INSERT logic
                {
                    sql = "INSERT INTO users (FullName, Username, Password, Email, Role, Status) VALUES (@n, @u, @p, @e, @r, 'Active')";
                }
                else // UPDATE logic
                {
                    sql = "UPDATE users SET FullName=@n, Username=@u, Email=@e, Role=@r WHERE UserID=@id";
                }

                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@n", txtName.Text.Trim());
                cmd.Parameters.AddWithValue("@u", txtUser.Text.Trim());
                cmd.Parameters.AddWithValue("@e", txtEmail.Text.Trim());
                cmd.Parameters.AddWithValue("@r", cbRole.Text);
                if (string.IsNullOrEmpty(_userId)) cmd.Parameters.AddWithValue("@p", txtPass.Text);
                else cmd.Parameters.AddWithValue("@id", _userId);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Account saved successfully!", "Success");
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message);
            }
        }
    }
}