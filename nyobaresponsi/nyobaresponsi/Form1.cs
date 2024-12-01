using System.Data;
using Npgsql;

namespace nyobaresponsi
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private NpgsqlConnection conn;
        string connstring = "Host=localhost; Port=5432; Username=postgres; Password=nblpd1503; database=nyobaresponsi";
        public DataTable dt;
        public static NpgsqlCommand cmd;
        private string sql = null;
        private DataGridViewRow r;
        private void Form1_Load(object sender, EventArgs e)
        {
            conn = new NpgsqlConnection(connstring);
            LoadData();
        }

        private void LoadData()
        {
            conn = new NpgsqlConnection(connstring);
            try
            {
                conn.Open();
                sql = "select * from karyawan, departemen WHERE karyawan.id_dep = departemen.id_dep;";
                cmd = new NpgsqlCommand(sql, conn);
                dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                dataGridView1.DataSource = dt;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
            finally
            {
                conn.Close();
            }
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(cbDep.Text) || cbDep.SelectedIndex == -1 || string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Silakan lengkapi nama dan departemen terlebih dahulu.");
                return;
            }
            string nama = textBox1.Text;
            string firstName = nama.Split(' ')[0];
            string id_dep = cbDep.Text;
            string id_karyawan = firstName + "_" + id_dep;
            try
            {
                conn = new NpgsqlConnection(connstring);
                conn.Open();
                sql = "SELECT * FROM add_karyawan(@in_id_karyawan, @in_nama, @in_id_dep)";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@in_id_karyawan", id_karyawan);
                cmd.Parameters.AddWithValue("@in_nama", nama);
                cmd.Parameters.AddWithValue("@in_id_dep", id_dep);
                int statusCode = (int)cmd.ExecuteScalar();
                if (statusCode == 201)
                {
                    MessageBox.Show("Insert Succes");
                    LoadData();
                    return;
                }
                if (statusCode == 409)
                {
                    throw new Exception("User already exist");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
                return;
            }
            finally { conn.Close(); }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            try
            {
                string nama = textBox1
                    .Text;
                if (r == null)
                {
                    MessageBox.Show("Silakan pilih karyawan terlebih dahulu.");
                    return;
                }
                string id_dep = cbDep.Text;

                conn = new NpgsqlConnection(connstring);
                conn.Open();
                sql = "SELECT * FROM edit_karyawan(@in_id_karyawan, @in_nama, @in_id_dep)";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@in_id_karyawan", r.Cells["id_karyawan"].Value);
                cmd.Parameters.AddWithValue("@in_nama", nama);
                cmd.Parameters.AddWithValue("@in_id_dep", id_dep);
                int statusCode = (int)cmd.ExecuteScalar();

                if (statusCode == 200)
                {
                    MessageBox.Show("Edit success", "Success");
                    LoadData();
                    return;
                }
                if (statusCode == 404)
                {
                    throw new Exception("karyawan tidak ditemukan");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
            }
            finally { conn.Close(); }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (r == null)
            {
                MessageBox.Show("Silakan pilih karyawan terlebih dahulu.");
                return;
            }
            try
            {
                conn = new NpgsqlConnection(connstring);
                conn.Open();
                sql = "SELECT * FROM delete_karyawan(@in_id_karyawan)";
                cmd = new NpgsqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@in_id_karyawan", r.Cells["id_karyawan"].Value);
                int statusCode = (int)cmd.ExecuteScalar();

                if (statusCode == 204)
                {
                    MessageBox.Show("Delete success", "Success");
                    LoadData();
                    return;
                }
                if (statusCode == 404)
                {
                    throw new Exception("Karyawan tidak ditemukan");
                }
            }
            catch (Exception er)
            {
                MessageBox.Show(er.Message);
            }
            finally
            { conn.Close(); }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                r = dataGridView1.Rows[e.RowIndex];
                textBox1.Text = r.Cells["nama"].Value.ToString();
                cbDep.SelectedItem = r.Cells["id_dep"].Value.ToString();
            }
        }
    }
}
