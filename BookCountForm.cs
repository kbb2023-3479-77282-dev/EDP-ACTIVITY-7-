#nullable disable
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MyInformationSystem {
    public class BookCountForm : Form {
        private ComboBox cbBooks;
        private Label lblBookDetail;

        public BookCountForm() {
            this.Text = "LibApp | Physical Inventory Count";
            this.Size = new Size(550, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblDesc = new Label { 
                Text = "INSTRUCTION: Select the book below to verify its physical presence. This registers a count entry in the system history.",
                Top = 20, Left = 40, Width = 450, Height = 50, Font = new Font("Segoe UI", 9, FontStyle.Italic), ForeColor = Color.Gray 
            };

            cbBooks = new ComboBox { Top = 100, Left = 40, Width = 450, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 11) };
            cbBooks.SelectedIndexChanged += (s, e) => UpdateUI();

            lblBookDetail = new Label { 
                Top = 150, Left = 40, Width = 450, Height = 100, 
                BackColor = Color.FromArgb(242, 242, 242), Padding = new Padding(10), Font = new Font("Segoe UI", 10) 
            };

            Button btnConfirm = new Button { 
                Text = "CONFIRM PHYSICAL COUNT", Top = 280, Left = 40, Width = 450, Height = 55,
                BackColor = Color.FromArgb(0, 179, 89), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Bold", 11)
            };
            btnConfirm.Click += (s, e) => ProcessPhysicalCount();

            this.Controls.AddRange(new Control[] { lblDesc, cbBooks, lblBookDetail, btnConfirm });
            LoadBooks();
        }

        private void LoadBooks() {
            try {
                using var conn = DatabaseConnection.GetConnection();
                var da = new MySqlDataAdapter("SELECT BookID, Title, Price, Quantity FROM books", conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                cbBooks.DataSource = dt;
                cbBooks.DisplayMember = "Title";
                cbBooks.ValueMember = "BookID";
            } catch (Exception ex) { MessageBox.Show("Load Error: " + ex.Message); }
        }

        private void UpdateUI() {
            if (cbBooks.SelectedItem is DataRowView row) {
                lblBookDetail.Text = $"Book Name: {row["Title"]}\nPrice: ₱{Convert.ToDecimal(row["Price"]):N2}\nTotal Supply: {row["Quantity"]}";
            }
        }

        private void ProcessPhysicalCount() {
            try {
                using var conn = DatabaseConnection.GetConnection();
                conn.Open();
                // We log the 'Count' action for the record section
                string sql = "INSERT INTO transactions (BookID, TransType, TransDate, UserID) VALUES (@bid, 'Count', @dt, @uid)";
                var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@bid", cbBooks.SelectedValue);
                cmd.Parameters.AddWithValue("@dt", DateTime.Now);
                cmd.Parameters.AddWithValue("@uid", AppSession.UserID);
                cmd.ExecuteNonQuery();
                
                MessageBox.Show("Physical count for '" + cbBooks.Text + "' recorded successfully!");
                this.Close();
            } catch (Exception ex) { MessageBox.Show("Failed to record count: " + ex.Message); }
        }
    }
}