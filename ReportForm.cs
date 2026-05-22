#nullable disable
using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace MyInformationSystem
{
    public partial class ReportForm : Form
    {
        private DataGridView dgv;
        public ReportForm() { BuildUI(); LoadData(); }
        private void BuildUI()
        {
            dgv = new DataGridView { Dock = DockStyle.Fill };
            this.Controls.Add(dgv);
        }
        private void LoadData()
        {
            using var conn = DatabaseConnection.GetConnection();
            var da = new MySqlDataAdapter("SELECT * FROM v_transactionreport", conn);
            var dt = new DataTable();
            da.Fill(dt);
            dgv.DataSource = dt;
        }
    }
}