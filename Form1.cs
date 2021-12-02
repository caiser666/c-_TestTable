using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TableTest {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        int intiDB = -1;
        DataTable dtPegawai, dtKomisi;

        string tbPegawai = Properties.Settings.Default.tb_pegawai, 
            tbKomisi = Properties.Settings.Default.tb_komisi;

        private void Form1_Load(object sender, EventArgs e) {
            string server = Properties.Settings.Default.server;
            string port = Properties.Settings.Default.port;
            string databasename = Properties.Settings.Default.database_name;
            string username = Properties.Settings.Default.username;
            string password = Properties.Settings.Default.password;

            DatabaseConfig dbConfig = new DatabaseConfig(server, port, databasename, username, password);
            string refMessage = string.Empty;
            intiDB = Database.Connect(dbConfig, ref refMessage);

            if (intiDB == 0) {
                dtPegawai = new DataTable();
                dtKomisi = new DataTable();
                string tb = tbPegawai;

                int ret = Database.Read(tbPegawai, ref dtPegawai, ref refMessage);

                if (ret == 0) {
                    showTablePegawai(dtPegawai);
                    tb = tbKomisi;
                    ret = Database.Read(tbKomisi, ref dtKomisi, ref refMessage);
                    if ( ret == 0) {
                        showTableKomisi(dtKomisi);
                    } 
                } 

                if (ret != 0) MessageBox.Show(refMessage, $"Read Table {tb}");
                
            } else {
                MessageBox.Show(refMessage, "Initial Database");
            }


        }

        string formatDate = "yyyy-MM-dd";
        string formatTransactionDate = "yyyyMMdd";

        private void btnSimpan_Click(object sender, EventArgs e) {
            string nip = tbxNIP.Text;
            string nama = tbxNama.Text;
            string noMobil = tbxNoMobil.Text;
            DateTime date = dtpTanggal.Value;
            string tanggal = date.ToString(formatDate);
            long komisi =  Convert.ToInt64(tbxKomisi.Text);
            long penghasilan = Convert.ToInt64(tbxPenghasilan.Text);

            string transaksiId = $"{nip}{date.ToString(formatTransactionDate)}";

            Pegawai pegawai = new Pegawai(nip, nama, tanggal, noMobil, transaksiId, penghasilan, komisi);

            string refMessage = string.Empty;

            int ret = Database.Create(tbPegawai, pegawai, ref refMessage);

            if (ret == 0) {
                ret = Database.Read(tbPegawai, ref dtPegawai, ref refMessage);

                if (ret == 0) {
                    showTablePegawai(dtPegawai);
                } else {
                    MessageBox.Show(refMessage, $"Read Table {tbPegawai}");
                }
            } else {
                MessageBox.Show(refMessage, "Create");
            }
        }

        List<double> persens = new List<double> { };

        private void tbxPenghasilan_TextChanged(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(tbxPenghasilan.Text)) {
                long penghasilan = Convert.ToInt64(tbxPenghasilan.Text);
                double komisi = 0;

                if (penghasilan <= 300000) {
                    komisi = 0;
                    komisi = penghasilan * persens[0];
                } else if (penghasilan > 300000 && penghasilan <= 500000) {
                    long sisa = penghasilan - 300000;
                    komisi = 0;
                    komisi = 300000 * persens[0];
                    komisi += sisa * persens[1];
                } else {
                    long sisa = penghasilan - 500000;
                    komisi = 0;
                    komisi = 300000 * persens[0];
                    komisi += 200000 * persens[1];
                    komisi += sisa * persens[2];
                }


                tbxKomisi.Text = komisi.ToString();
            } else {
                tbxKomisi.Text = string.Empty;
            }
        }

        private void showTablePegawai(DataTable dataTable) {
            try {
                dgvPegawai.DataSource = dataTable;

                dgvPegawai.Columns["trx_id"].Visible = false;
                dgvPegawai.Columns["created_at"].Visible = false;

                dgvPegawai.Sort(dgvPegawai.Columns["created_at"], ListSortDirection.Descending);
            } catch (Exception) {
                return;
            }
        }

        private void showTableKomisi(DataTable dataTable) {
            try {
                dgvKomisi.DataSource = dataTable;

                dgvKomisi.Columns["created_at"].Visible = false;
                dgvKomisi.Columns["updated_at"].Visible = false;
                dgvKomisi.Columns["id"].Visible = false;

                foreach (DataRow dRow in dataTable.Rows) {

                    int komisi = Convert.ToInt32(dRow["komisi"]);
                    double persen = (double)komisi / 100;

                    persens.Add(persen);
                }

                dgvKomisi.Columns["komisi"].HeaderText = "Komisi (%)";


                dgvKomisi.Sort(dgvPegawai.Columns["created_at"], ListSortDirection.Descending);
            } catch (Exception) {
                return;
            }
        }
    }
}
