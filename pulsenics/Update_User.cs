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
    public partial class Update_User : Form
    {
        Form1 form1;
        public Update_User(Form1 form1)
        //Initialization - reads userids and corresponding names from database and generates listbox
        {
            this.form1 = form1;
            InitializeComponent();
            listBox1.Items.Clear();
            using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
            {
                conn.Open();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT * FROM users";
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        listBox1.Items.Add(reader["userid"].ToString() + ": " + reader["name"].ToString());
                    }
                }
                conn.Close();
            }
            button1.Enabled = false;
            button2.Enabled = false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        //Upon user selection, show user info in corresponding textboxs 
        {
            if (listBox1.SelectedIndex > -1)
            {
                using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT * FROM users WHERE userid=" + listBox1.SelectedItem.ToString().Substring(0, listBox1.SelectedItem.ToString().IndexOf(":"));
                        MySqlDataReader reader = cmd.ExecuteReader();
                        reader.Read();
                        textBox1.Text = reader["name"].ToString();
                        textBox2.Text = reader["email"].ToString();
                        textBox3.Text = reader["phone"].ToString();
                        reader.Close();
                    }
                    conn.Close();
                }
                button1.Enabled = true;
                button2.Enabled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        //Update User button: save input user info in database, and update the information in all forms
        {
            using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
            {

                conn.Open();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "UPDATE users SET name = '" + textBox1.Text + "', email = '" + textBox2.Text + "', phone = '" + textBox3.Text + "' WHERE userid = " + listBox1.SelectedItem.ToString().Substring(0, listBox1.SelectedItem.ToString().IndexOf(":"));
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                if (textBox1.Text != listBox1.SelectedItem.ToString())
                {
                    refreshPage();
                }
            }

        }

        private void button2_Click(object sender, EventArgs e)
        //Delete user button: delete user from both users and assignment tables, and update info in all forms
        {
            using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
            {

                conn.Open();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "DELETE FROM assignments WHERE userid = " + listBox1.SelectedItem.ToString().Substring(0, listBox1.SelectedItem.ToString().IndexOf(":"));
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "DELETE FROM users WHERE userid = " + listBox1.SelectedItem.ToString().Substring(0, listBox1.SelectedItem.ToString().IndexOf(":"));
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
                refreshPage();
            }
        }

        public void refreshPage()
        //refreshes the listbox and textboxs; called when username is changed or when user is deleted
        {
            using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
            {
                listBox1.Items.Clear();
                conn.Open();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "SELECT * FROM users";
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        listBox1.Items.Add(reader["userid"].ToString() + ": " + reader["name"].ToString());
                    }
                }
                conn.Close();
                textBox1.Text = "";
                textBox2.Text = "";
                textBox3.Text = "";
                button1.Enabled = false;
                button2.Enabled = false;
                form1.RefreshForm();
            }
        }
    }
}

