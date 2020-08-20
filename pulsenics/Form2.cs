using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;

namespace pulsenics
{
    public partial class Form2 : Form
    {
        Form1 form1;
        public Form2(Form1 form1)
        {
            this.form1 = form1;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
            {
                conn.Open();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO users (name, email, phone)" + "Values('" + textBox1.Text + "', '" + textBox2.Text + "', '" + textBox3.Text + "')";
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                form1.RefreshForm();
                this.Close();
            }
            
        }
    }
}
