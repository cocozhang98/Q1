using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;


/* Q1 for Software Assessment. 
 * - My hardcoded folder is @"C:\test". This appears 3 times in form.cs - please change if needed. 
 * - SQL database is built online using Azure MySQL; the current connection strings automatically connects to the database. 
 *     Database can be accessed with server pulsenicssa.mysql.database.azure.com; port 3306; uid = coco@pulsenicssa; pwd = Pulsenics.2020.
 * - Upon start of application, all files and corresponding assignments which are non-existent in the current folder are deleted. 
 * - Changes in the directory are always synchronized - if you want messages for debugging, please uncomment the messagebox lines. 
 * - temp files are not included in the database because they result in errors with filesystemwatcher D: (will need to poll if want to include them, 
 *       instead of using filesystemwatcher).
 */

namespace pulsenics
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //Initiate left(file) Listbox
            DirectoryInfo dir = new DirectoryInfo(@"C:\test");
            listBox1.Items.Clear();
            foreach (var file in dir.GetFiles())
            {
                if (!file.Name.StartsWith("~$") & !file.Name.EndsWith(".tmp"))
                {
                    listBox1.Items.Add(file.Name);
                }
            }
            using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
            {
                conn.Open();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;

                    //Delete SQL rows on non-existent files
                    cmd.CommandText = "SELECT * FROM files";
                    List<string> non_exist_files = new List<string> { };
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while(reader.Read())
                    {
                        if(!File.Exists(@"C:\test\" + reader["filename"].ToString()))
                        {
                            non_exist_files.Add(reader["fileid"].ToString());
                        }
                    }
                    reader.Close();
                    foreach (var fileid in non_exist_files)
                    {
                        cmd.CommandText = "DELETE FROM assignments WHERE fileid = '" + fileid + "'";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM files WHERE fileid = '" + fileid + "'";
                        cmd.ExecuteNonQuery();
                    }

                    //Add SQL rows on files without record; update all file details
                    foreach (var file in dir.GetFiles())
                    {
                        cmd.CommandText = "INSERT IGNORE INTO files (filename) " + "Values('"+file.Name+"')";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = string.Format("UPDATE files set fileAttribute = '{0}', lastAccessTime = '{1}', lastWriteTime = '{2}', creationTime = '{3}' WHERE filename = '{4}'", File.GetAttributes(file.FullName).ToString(), File.GetLastAccessTime(file.FullName).ToString(), File.GetLastWriteTime(file.FullName).ToString(), File.GetCreationTime(file.FullName).ToString(), file.Name);
                        cmd.ExecuteNonQuery();
                    }
                }
                conn.Close();
            }

            //Assign Users button only enabled when a file is chosen on the left listbox
            button1.Enabled = false;

            //Initiate comboBox (dropbox to select user)
            RefreshDropBox();
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            refreshFileList();
        }


        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        //When a file is selected, show its assigned users on the user listbox
        {
            RefreshAssignedUsers();
        }


        private void button2_Click(object sender, EventArgs e)
        //Create User button: initiate form2 (create-user form)
        {
            Form2 form2 = new Form2(this);
            form2.ShowDialog();
        }


        private void button3_Click(object sender, EventArgs e)
        //Update User button: Initiate update_user_form
        {
            Update_User update_user_form = new Update_User(this);
            update_user_form.ShowDialog();
        }


        private void button1_Click(object sender, EventArgs e)
        //Handler for user assignment button; insert corresponding assignment into database, and add the user in the userassignment listbox
        {
            using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
            {
                conn.Open();
                using (var cmd = new MySqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = "INSERT INTO assignments (fileid, userid) SELECT files.fileid, users.userid from files, users where files.filename = '" + listBox1.SelectedItem.ToString() + "' AND users.userid =" + comboBox1.SelectedItem.ToString().Substring(0, comboBox1.SelectedItem.ToString().IndexOf(":"));
                    cmd.ExecuteNonQuery();
                }
                conn.Close();
            }
            listBox2.Items.Add(comboBox1.SelectedItem.ToString());
            button1.Enabled = false;
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        //Enables user-assignment button when a file AND a user is selected 
        {
            if (listBox2.Items.Contains(comboBox1.SelectedItem.ToString()) == false & listBox1.SelectedIndex > -1)
            {
                button1.Enabled = true;
            }
            else button1.Enabled = false;
        }


        //Handler for File change outside app; updates file details in database
        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            //MessageBox.Show(string.Format("Changed: {0}. Database updated", e.Name));
            if (!e.Name.StartsWith("~$") & !e.Name.EndsWith(".tmp"))
            {
                using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = string.Format("UPDATE files set fileAttribute = '{0}', lastAccessTime = '{1}', lastWriteTime = '{2}', creationTime = '{3}' WHERE filename = '{4}'", File.GetAttributes(e.FullPath).ToString(), File.GetLastAccessTime(e.FullPath).ToString(), File.GetLastWriteTime(e.FullPath).ToString(), File.GetCreationTime(e.FullPath).ToString(), e.Name);
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            }
        }


        //Handler for file creation outside app; adds file to database and refreshes file listbox
        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            //MessageBox.Show(string.Format("Created: {0}. Database updated", e.Name));
            if (!e.Name.StartsWith("~$") & !e.Name.EndsWith(".tmp"))
            {
                using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = string.Format("INSERT INTO files (filename, fileAttribute, lastAccessTime, lastWriteTime, creationTime) " + "Values('{0}', '{1}', '{2}', '{3}', '{4}')", e.Name, File.GetAttributes(e.FullPath).ToString(), File.GetLastAccessTime(e.FullPath).ToString(), File.GetLastWriteTime(e.FullPath).ToString(), File.GetCreationTime(e.FullPath).ToString());
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                refreshFileList();
            }
        }


        //Handler for file deletion outside app; deletes file and corresponding assignments in database.
        private void fileSystemWatcher1_Deleted(object sender, FileSystemEventArgs e)
        {
            //MessageBox.Show(string.Format("Deleted: {0}. Corresponding rows in database deleted.", e.Name));
            if (!e.Name.StartsWith("~$") & !e.Name.EndsWith(".tmp"))
            {
                using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "DELETE assignments FROM assignments LEFT JOIN files ON assignments.fileid = files.fileid WHERE files.filename='" + e.Name + "'";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "DELETE FROM files WHERE filename = '" + e.Name + "'";
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
                refreshFileList();
            }
        }


        //Handler for file renaming; updates filename in database and refreshes listboxs.
        private void fileSystemWatcher1_Renamed(object sender, RenamedEventArgs e)
        {
            //MessageBox.Show(string.Format("Renamed: {0} to {1}", e.OldName, e.Name));
            if (!e.Name.StartsWith("~$") & !e.Name.EndsWith(".tmp"))
            {
                using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        //cmd.CommandText = string.Format("UPDATE files SET filename = '{0}', fileAttribute = '{1}', lastAccessTime = '{2}', lastWriteTime = '{3}', creationTime = '{4}' WHERE filename = '{5}'", e.Name, File.GetAttributes(e.FullPath).ToString(), File.GetLastAccessTime(e.FullPath).ToString(), File.GetLastWriteTime(e.FullPath).ToString(), File.GetCreationTime(e.FullPath).ToString(), e.OldName);
                        cmd.CommandText = "UPDATE files SET filename = '" + e.Name.ToString() + "', fileAttribute = '" + File.GetAttributes(e.FullPath).ToString() + "', lastAccessTime = '" + File.GetLastAccessTime(e.FullPath).ToString() + "', lastWriteTime = '" + File.GetLastAccessTime(e.FullPath).ToString() + "', creationTime = '" + File.GetCreationTime(e.FullPath).ToString() + "' WHERE filename = '" + e.OldName.ToString() + "'";
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }
            refreshFileList();
            RefreshAssignedUsers();
            }
        }


        //Below are helper functions to avoid code repeitiiton and to connect different forms
        public void RefreshAssignedUsers()
        //Search for assigned user(s) for selected file in assignments table, and list them in the right listbox
        {
            listBox2.Items.Clear();
            if (listBox1.SelectedIndex > -1)
            {
                using (var conn = new MySqlConnection("server=pulsenicssa.mysql.database.azure.com;port=3306;database=pulsenics;uid=coco@pulsenicssa;pwd=Pulsenics.2020;"))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand())
                    {
                        cmd.Connection = conn;
                        cmd.CommandText = "SELECT users.userid, users.name FROM assignments LEFT JOIN users ON assignments.userid = users.userid LEFT JOIN files ON assignments.fileid = files.fileid WHERE files.filename='" + listBox1.SelectedItem.ToString() + "'";
                        MySqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            listBox2.Items.Add(reader["userid"].ToString() + ": " + reader["name"].ToString());
                        }
                        reader.Close();
                    }
                    conn.Close();
                }
                if (comboBox1.SelectedIndex > -1)
                {
                    if (listBox2.Items.Contains(comboBox1.SelectedItem.ToString()) == false) button1.Enabled = true;
                }
                else button1.Enabled = false;
            }
        }

        public void RefreshDropBox()
        //Refreshes the comboBox (dropbox) where all users are listed.
        {
            comboBox1.Items.Clear();
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
                        comboBox1.Items.Add(reader["userid"].ToString() + ": " + reader["name"].ToString());
                    }
                    reader.Close();
                }
                conn.Close();
            }
        }


        //This function is called when another form is closed. Since the two other forms are for user management, only the 
        //right (user) listbox and the dropbox need to be refreshed.
        public void RefreshForm()
        {
            RefreshDropBox();
            RefreshAssignedUsers();
        }


        public void refreshFileList()
        //refreshes the file listbox (on the left) according to the search string in textbox
        {
            DirectoryInfo dir = new DirectoryInfo(@"C:\test");
            listBox1.Items.Clear();
            foreach (var file in dir.GetFiles("*" + textBox1.Text + "*.*"))
            {
                if (!file.Name.StartsWith("~$") & !file.Name.EndsWith(".tmp"))
                {
                    listBox1.Items.Add(file.Name);
                }
            }
        }
    }
}
