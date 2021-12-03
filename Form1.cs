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
                dtPegawai = new DataTable();
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

        private void tbxPenghasilan_TextChanged(object sender, EventArgs e) {
            if (!string.IsNullOrEmpty(tbxPenghasilan.Text)) {
                long penghasilan = Convert.ToInt64(tbxPenghasilan.Text);
                double komisi = 0;

                for (int i = 0; i < komisiDatas.Count; i++) {
                    if (penghasilan >= komisiDatas[i].MinPenghasilan && penghasilan <= komisiDatas[i].MaxPenghasilan) {
                        if (i == 0) {
                            komisi = penghasilan * komisiDatas[i].PersenKomisi;
                        } else {
                            long sisa = penghasilan - komisiDatas[i - 1].MaxPenghasilan;
                            if (sisa > 0) {
                                komisi += komisiDatas[0].MaxPenghasilan * komisiDatas[0].PersenKomisi;
                                for (int j = 1; j < i; j++) {
                                    komisi += (komisiDatas[j].MaxPenghasilan - komisiDatas[j - 1].MaxPenghasilan) * komisiDatas[j].PersenKomisi;
                                }
                                komisi += sisa * komisiDatas[i].PersenKomisi;
                            }
                        }

                    }
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

                dgvPegawai.Columns["nip"].HeaderText = "NIP";
                dgvPegawai.Columns["nama"].HeaderText = "Nama Pengemudi";
                dgvPegawai.Columns["tanggal"].HeaderText = "Tanggal SIO";
                dgvPegawai.Columns["no_mobil"].HeaderText = "No Mobil";
                dgvPegawai.Columns["penghasilan"].HeaderText = "Penghasilan (Rp)";
                dgvPegawai.Columns["komisi"].HeaderText = "Komisi (Rp)";

                dgvPegawai.Sort(dgvPegawai.Columns["created_at"], ListSortDirection.Descending);
            } catch (Exception) {
                return;
            }
        }

        List<KomisiData> komisiDatas = new List<KomisiData> { };

        private void showTableKomisi(DataTable dataTable) {
            try {
                DataView dataView = dataTable.DefaultView;
                dataView.Sort = "min_penghasilan ASC";

                dgvKomisi.DataSource = dataTable;

                dgvKomisi.Columns["created_at"].Visible = false;
                dgvKomisi.Columns["updated_at"].Visible = false;
                dgvKomisi.Columns["id"].Visible = false;

                StringBuilder deskripsis = new StringBuilder();

                foreach (DataRow dRow in dataTable.Rows) {

                    int komisi = Convert.ToInt32(dRow["komisi"]);
                    string deskripsi = (string)dRow["deskripsi"];
                    long maxPenghasilan = Convert.ToInt32(dRow["max_penghasilan"]);
                    long minPenghasilan = Convert.ToInt32(dRow["min_penghasilan"]);
                    double persen = (double)komisi / 100;

                    KomisiData komisiData = new KomisiData(maxPenghasilan, minPenghasilan, persen, deskripsi);

                    komisiDatas.Add(komisiData);

                    deskripsis.AppendLine($"* Penghasilan = {minPenghasilan} s/d {maxPenghasilan} => Komisi {komisi}%");
                }

                lblDeskripsi.Text = deskripsis.ToString();

                dgvKomisi.Columns["max_penghasilan"].HeaderText = "Maksimal Penghasilan (Rp)";
                dgvKomisi.Columns["min_penghasilan"].HeaderText = "Mininal Penghasilan (Rp)";
                dgvKomisi.Columns["komisi"].HeaderText = "Persen Komisi (%)";
                dgvKomisi.Columns["deskripsi"].HeaderText = "Deskripsi";


                dgvKomisi.Sort(dgvPegawai.Columns["created_at"], ListSortDirection.Descending);
            } catch (Exception) {
                return;
            }
        }
    }

    class Pegawai {
        public string Nama, NIP, Tanggal, NoMobil;
        public string TransaksiId;
        public long Penghasilan, Komisi;

        public Pegawai(string nip, string nama, string tanggal, string noMobil, string transaksiId, long penghasilan, long komisi) {
            NIP = nip;
            Nama = nama;
            Tanggal = tanggal;
            NoMobil = noMobil;
            TransaksiId = transaksiId;
            Penghasilan = penghasilan;
            Komisi = komisi;
        }
    }

    public class KomisiData {
        public long MaxPenghasilan, MinPenghasilan;
        public double PersenKomisi;
        public string Deskripsi;

        public KomisiData(long maxPenghasilan, long minPenghasilan, double persenKomisi, string deskripsi) {
            MaxPenghasilan = maxPenghasilan;
            MinPenghasilan = minPenghasilan;
            PersenKomisi = persenKomisi;
            Deskripsi = deskripsi;
        }

    }
}
