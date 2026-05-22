#nullable disable
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using ClosedXML.Excel; 
using System.IO;
using System.Linq;

namespace MyInformationSystem {
    public class ReportExportForm : Form {
        private DataGridView dgvAct;

        public ReportExportForm() {
            this.Text = "LibApp | Professional Records & Analytics";
            this.Size = new Size(1150, 800);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            Label lblHeader = new Label { 
                Text = "Official Activity & Return History", 
                Top = 20, Left = 30, Font = new Font("Segoe UI Bold", 14), AutoSize = true 
            };

            dgvAct = new DataGridView { 
                Top = 70, Left = 30, Width = 1080, Height = 550,
                BackgroundColor = Color.WhiteSmoke, AllowUserToAddRows = false, 
                RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, 
                BorderStyle = BorderStyle.None
            };
            dgvAct.Columns.Add(new DataGridViewCheckBoxColumn { Name = "Select", HeaderText = "✔", Width = 50 });

            Button btn = new Button { 
                Text = "📥 GENERATE EXCEL REPORT", Top = 640, Left = 30, Width = 380, Height = 60, 
                BackColor = Color.FromArgb(0, 179, 89), ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI Bold", 12),
                Cursor = Cursors.Hand
            };
            btn.Click += (s, e) => ExportReport();

            this.Controls.AddRange(new Control[] { lblHeader, dgvAct, btn });
            LoadData();
        }

        private void LoadData() {
            try {
                using var conn = DatabaseConnection.GetConnection();
                conn.Open();
                
                string query = "SELECT TransactionID, Type, ItemName, ProcessedBy, Quantity, Amount, Status, TransDate FROM v_transactionreport WHERE Type IN ('Borrow', 'Purchase', 'Return') ORDER BY TransDate DESC";
                var da = new MySqlDataAdapter(query, conn);
                DataTable dt = new DataTable(); 
                da.Fill(dt); 
                dgvAct.DataSource = dt;
                if (dgvAct.Columns.Contains("Select")) dgvAct.Columns["Select"].DisplayIndex = 0;
            } catch (Exception ex) { 
                MessageBox.Show("Error: " + ex.Message); 
            }
        }

private void ExportReport() {
    var selected = dgvAct.Rows.Cast<DataGridViewRow>()
        .Where(r => r.Cells["Select"].Value != null && Convert.ToBoolean(r.Cells["Select"].Value))
        .ToList();

    if (!selected.Any()) {
        MessageBox.Show("Please select records to export.");
        return;
    }

    try {
        SaveFileDialog sfd = new SaveFileDialog { 
            Filter = "Excel Workbook|*.xlsx", 
            FileName = "LibApp_Official_Report" 
        };

        if (sfd.ShowDialog() == DialogResult.OK) {
            using (var wb = new XLWorkbook()) {
                // --- SHEET 1: HISTORY ---
                var ws = wb.Worksheets.Add("History");
                if (File.Exists("LibApp.png")) ws.AddPicture("LibApp.png").MoveTo(ws.Cell(1, 1)).Scale(0.15);
                
                var title = ws.Cell(10, 1);
                title.Value = "LibApp - Library Information System";
                title.Style.Font.Bold = true;
                title.Style.Font.FontSize = 16;
                title.Style.Font.FontColor = XLColor.FromArgb(0, 179, 89);

                DataTable dt = ((DataTable)dgvAct.DataSource).Clone();
                foreach (var r in selected) dt.ImportRow(((DataRowView)r.DataBoundItem).Row);
                if (dt.Columns.Contains("Select")) dt.Columns.Remove("Select");
                ws.Cell(14, 1).InsertTable(dt).Theme = XLTableTheme.TableStyleLight8;
                ws.Columns().AdjustToContents();

                // Signatures
                int fRow = 14 + dt.Rows.Count + 6;
                ws.Cell(fRow, 1).Value = "Prepared By:";
                ws.Cell(fRow + 2, 1).Value = "___________________________";
                ws.Cell(fRow + 4, 1).Value = "System Administrator";

                int ownerCol = Math.Max(4, dt.Columns.Count);
                ws.Cell(fRow, ownerCol).Value = "Approved By:";
                ws.Cell(fRow + 2, ownerCol).Value = "___________________________";
                ws.Cell(fRow + 3, ownerCol).Value = "OWNER / STORE MANAGER";

                // --- SHEET 2: VISUAL ANALYTICS 
                var ws2 = wb.Worksheets.Add("Visual Analytics");
                ws2.Cell(1, 1).Value = "Activity Distribution Summary";
                ws2.Cell(1, 1).Style.Font.Bold = true;
                ws2.Cell(1, 1).Style.Font.FontSize = 14;

                var summary = dt.AsEnumerable()
                    .GroupBy(row => row.Field<string>("Type"))
                    .Select(g => new { Type = g.Key, Count = g.Count() }).ToList();

                ws2.Cell(3, 1).Value = "Transaction Type";
                ws2.Cell(3, 2).Value = "Frequency";
                ws2.Cell(3, 3).Value = "Visual Progress"; 
                ws2.Range(3, 1, 3, 3).Style.Fill.BackgroundColor = XLColor.FromArgb(33, 37, 41);
                ws2.Range(3, 1, 3, 3).Style.Font.FontColor = XLColor.White;

                int rowIdx = 4;
                foreach (var item in summary) {
                    ws2.Cell(rowIdx, 1).Value = item.Type;
                    ws2.Cell(rowIdx, 2).Value = item.Count;
                    

                    string bar = "";
                    for(int i = 0; i < item.Count; i++) bar += "████"; 
                    
                    var barCell = ws2.Cell(rowIdx, 3);
                    barCell.Value = bar;
                    barCell.Style.Font.FontColor = XLColor.FromArgb(0, 179, 89);
                    
                    rowIdx++;
                }
                
                ws2.Columns().AdjustToContents();
                wb.SaveAs(sfd.FileName);
            }
            MessageBox.Show("Final Report with Analytics generated! Triumph!", "Success");
        }
    } catch (Exception ex) { 
        MessageBox.Show("Export Error: " + ex.Message); 
    }
}
    }
}