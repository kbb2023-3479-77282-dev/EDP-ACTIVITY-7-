#nullable disable
using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MyInformationSystem {
    public class UserManagementForm : Form {
        private DataGridView dgv;
        public UserManagementForm() {
            this.Text = "User Management"; this.Size = new System.Drawing.Size(700, 500);
            dgv = new DataGridView { Dock = DockStyle.Top, Height = 350, SelectionMode = DataGridViewSelectionMode.FullRowSelect };
            Button btnAdd = new Button { Text = "Add/Edit Profile", Top = 370, Left = 20 };
            btnAdd.Click += (s, e) => { new UserEditDialog().ShowDialog(); LoadUsers(); };
            Button btnToggle = new Button { Text = "Toggle Status", Top = 370, Left = 150 };
            btnToggle.Click += (s, e) => Toggle();
            this.Controls.AddRange(new Control[] { dgv, btnAdd, btnToggle });
            LoadUsers();
        }
        private void LoadUsers() {
            using var conn = DatabaseConnection.GetConnection();
            var da = new MySqlDataAdapter("SELECT UserID, FullName, Username, Status FROM users", conn);
            var dt = new DataTable(); da.Fill(dt); dgv.DataSource = dt;
        }
        private void Toggle() {
            if (dgv.SelectedRows.Count == 0) return;
            string id = dgv.SelectedRows[0].Cells[0].Value.ToString();
            string status = dgv.SelectedRows[0].Cells[3].Value.ToString() == "Active" ? "Inactive" : "Active";
            using var conn = DatabaseConnection.GetConnection(); conn.Open();
            new MySqlCommand($"UPDATE users SET Status='{status}' WHERE UserID={id}", conn).ExecuteNonQuery();
            LoadUsers();
        }
    }
}