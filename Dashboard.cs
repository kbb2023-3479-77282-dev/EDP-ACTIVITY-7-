#nullable disable
using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MyInformationSystem {
    public class DashboardForm : Form {
        private Label lblBooks, lblTrans;
        private Panel sidebar;

        public DashboardForm() {
            
            this.Text = "LibApp | Dashboard";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(245, 245, 250);
            
            BuildSidebar();
            BuildMainContent();
            LoadStats();
        }

        private void BuildSidebar() {
            sidebar = new Panel { 
                Dock = DockStyle.Left, 
                Width = 240, 
                BackColor = Color.FromArgb(33, 37, 41) 
            };

            
            string[] menus = { "📊 Dashboard", "👥 Users", "📚 Transactions", "📄 Reports", "🚪 Logout" };
            
            for (int i = 0; i < menus.Length; i++) {
                Button btn = new Button {
                    Text = menus[i], 
                    Top = 100 + (i * 60), 
                    Width = 240, 
                    Height = 55,
                    FlatStyle = FlatStyle.Flat, 
                    ForeColor = Color.White,
                    TextAlign = ContentAlignment.MiddleLeft, 
                    Padding = new Padding(25, 0, 0, 0),
                    Font = new Font("Segoe UI", 11),
                    Cursor = Cursors.Hand
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 179, 89);
                
                string target = menus[i];
                btn.Click += (s, e) => Navigate(target);
                sidebar.Controls.Add(btn);
            }
            this.Controls.Add(sidebar);
        }

        private void BuildMainContent() {
            // App Branding
            Label title = new Label { 
                Text = "LibApp", 
                Top = 50, 
                Left = 280, 
                Font = new Font("Segoe UI Black", 48), 
                ForeColor = Color.FromArgb(0, 179, 89), 
                AutoSize = true 
            };

            // Physical Inventory Stats
            lblBooks = new Label { 
                Text = "Total Physical Books: 0", 
                Top = 180, 
                Left = 280, 
                Font = new Font("Segoe UI Semibold", 26), 
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            // System Activity Stats
            lblTrans = new Label { 
                Text = "Activity Records: 0", 
                Top = 250, 
                Left = 280, 
                Font = new Font("Segoe UI Semibold", 26), 
                AutoSize = true,
                ForeColor = Color.FromArgb(50, 50, 50)
            };

            this.Controls.AddRange(new Control[] { title, lblBooks, lblTrans });
        }

        private void Navigate(string target) {
            if (target.Contains("Dashboard")) LoadStats();
            else if (target.Contains("Users")) new UserManagementForm().ShowDialog();
            else if (target.Contains("Transactions")) new TransactionForm().ShowDialog();
            else if (target.Contains("Reports")) new ReportExportForm().ShowDialog();
            else if (target.Contains("Logout")) this.Close();

            // Refresh stats after returning from any module
            LoadStats();
        }

        public void LoadStats() {
            try {
                using var conn = DatabaseConnection.GetConnection(); 
                conn.Open();

                // 1. Calculate Total Physical Stock (Sum of Quantity in 'books' table)
                var bookStock = new MySqlCommand("SELECT COALESCE(SUM(Quantity), 0) FROM books", conn).ExecuteScalar();
                
                // 2. Count Total Activities using the correct column name 'Type'
                // This ensures we ignore 'Book Count' and match the Report Section
                string activitySql = "SELECT COUNT(*) FROM transactions WHERE Type IN ('Borrow', 'Purchase', 'Return')";
                var activityCount = new MySqlCommand(activitySql, conn).ExecuteScalar();

                lblBooks.Text = $"Total Physical Books: {bookStock}";
                lblTrans.Text = $"Activity Records: {activityCount}";
            } catch {
                lblBooks.Text = "Total Physical Books: --";
                lblTrans.Text = "Activity Records: --";
            }
        }
    }
}