#nullable disable
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MyInformationSystem {
    public class TransactionForm : Form {
        private ComboBox cbType, cbBooks;
        private TextBox txtQty;

        public TransactionForm() {
            this.Text = "LibApp | Process Transaction";
            this.Size = new Size(500, 550);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblInfo = new Label { 
                Text = "Logged in as: " + AppSession.FullName, 
                Top = 15, Left = 50, ForeColor = Color.FromArgb(0, 179, 89), Font = new Font("Segoe UI Bold", 9) 
            };

            cbType = new ComboBox { Top = 80, Left = 50, Width = 380, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            cbType.Items.AddRange(new string[] { "Borrow", "Purchase", "Return" });

            cbBooks = new ComboBox { Top = 140, Left = 50, Width = 380, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            
            txtQty = new TextBox { Top = 200, Left = 50, Width = 150, PlaceholderText = "Qty", Font = new Font("Segoe UI", 11) };

            Button btnProcess = new Button { 
                Text = "CONFIRM TRANSACTION", Top = 300, Left = 50, Width = 380, Height = 60,
                BackColor = Color.FromArgb(0, 179, 89), ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Bold", 12), Cursor = Cursors.Hand
            };
            btnProcess.Click += (s, e) => ProcessTransaction();

            this.Controls.AddRange(new Control[] { lblInfo, cbType, cbBooks, txtQty, btnProcess });
            LoadBooks();
        }

        private void LoadBooks() {
            using var conn = DatabaseConnection.GetConnection();
            var da = new MySqlDataAdapter("SELECT BookID, Title FROM books", conn);
            DataTable dt = new DataTable(); 
            da.Fill(dt);
            cbBooks.DataSource = dt; 
            cbBooks.DisplayMember = "Title"; 
            cbBooks.ValueMember = "BookID";
        }

        private void ProcessTransaction() {
            if (cbType.SelectedIndex == -1 || string.IsNullOrEmpty(txtQty.Text) || cbBooks.SelectedValue == null) {
                MessageBox.Show("Please complete all fields.");
                return;
            }

            int bid = (int)cbBooks.SelectedValue;
            string type = cbType.Text;
            int qty = int.Parse(txtQty.Text);

            try {
                using var conn = DatabaseConnection.GetConnection();
                conn.Open();
                using var trans = conn.BeginTransaction();

                // FIX: Ang @uid ay galing na sa AppSession.UserID (HINDI NA 0)
                string sql = "INSERT INTO transactions (BookID, Type, Quantity, TransDate, UserID, Status) " +
                             "VALUES (@bid, @type, @qty, NOW(), @uid, 'Active')";
                
                MySqlCommand cmd = new MySqlCommand(sql, conn, trans);
                cmd.Parameters.AddWithValue("@bid", bid);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@qty", qty);
                cmd.Parameters.AddWithValue("@uid", AppSession.UserID); 
                cmd.ExecuteNonQuery();

                // Update books stock
                string op = (type == "Return") ? "+" : "-";
                new MySqlCommand($"UPDATE books SET Quantity = Quantity {op} {qty} WHERE BookID = {bid}", conn, trans).ExecuteNonQuery();

                trans.Commit();
                MessageBox.Show($"{type} successful for user: {AppSession.FullName}");
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}