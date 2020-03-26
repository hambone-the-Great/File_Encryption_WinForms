using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.AccessControl;
using System.Diagnostics;
using System.Security.Permissions;
using CustomMessageBox;

namespace File_Encryption_WinForms
{
    public partial class FileEncryption : Form
    {

        private bool HasErrors = false;
        private string SuccessLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "File_Encryption_Success.htm");
        private string ErrorLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "File_Encryption_Error.htm");
        int FileCount = 0; 

        private Task CurrentTask; 
        private enum Task
        {
            Encrypt = 1, 
            Decrypt = 2
        }

        public FileEncryption()
        {
            InitializeComponent();

            btnDecrypt.Enabled = false;
            btnEncrypt.Enabled = false;
            pBar.Visible = true;
            pBar.Minimum = 1;
            pBar.Value = 1;
            pBar.Step = 1;
            pBar.Maximum = 100;
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;

        }
        
        private void PrepLogs()
        {

            File.WriteAllText(SuccessLog, "");
            File.WriteAllText(ErrorLog, "");

            using (StreamWriter w = File.AppendText(ErrorLog))
            {
                w.WriteLine("<h1>File Encryption Error Log</h1>");
                w.WriteLine("<h3>" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "</h3><br /><br />");
            }

            using (StreamWriter w = File.AppendText(SuccessLog))
            {
                w.WriteLine("<h1>File Encryption Success Log</h1>");
                w.WriteLine("<h3>" + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "</h3><br /><br />");
            }

        }

        private void ProcessTarget(string path, Task task)
        {

            if (path == null)
            {
                MessageBox.Show("Error! Path of target was null. Try again.");
                LogError("Target path was null.");
            }
            else
            { 
                FileAttributes attr = File.GetAttributes(path);

                PrepLogs();

                int i = 0;

                if (attr.HasFlag(FileAttributes.Directory))
                {

                    foreach (string file in Directory.GetFiles(path))
                    {
                        if (File.Exists(file))
                        {
                            try
                            {
                                if (task == Task.Encrypt) File.Encrypt(file);
                                else if (task == Task.Decrypt) File.Decrypt(file);
                                Console.WriteLine(task.ToString() + "ing: " + file);
                                LogSuccess(task.ToString() + "ed", file);
                            }
                            catch (Exception ex)
                            {
                                LogError(ex.Message);
                                Console.WriteLine("Error: " + ex.Message);
                            }
                            UpdateProgress(i++);
                        }
                    }

                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        if (Directory.Exists(subDir))
                        {
                            foreach (string file in Directory.GetFiles(subDir))
                            {
                                if (File.Exists(file))
                                {
                                    try
                                    {
                                        if (task == Task.Encrypt) File.Encrypt(file);
                                        else if (task == Task.Decrypt) File.Decrypt(file);
                                        Console.WriteLine(task.ToString() + "ing: " + file);
                                        LogSuccess(task.ToString() + "ed", file);
                                    }
                                    catch (Exception ex)
                                    {
                                        LogError(ex.Message);
                                        Console.WriteLine("Error: " + ex.Message);
                                    }
                                    UpdateProgress(i++);
                                }
                            }
                        }
                    }

                }
                else
                {
                    if (File.Exists(path))
                    {
                        try
                        {
                            if (task == Task.Encrypt) File.Encrypt(path);
                            else if (task == Task.Decrypt) File.Decrypt(path);
                            Console.WriteLine(task.ToString() + "ing: " + path);
                            LogSuccess(task.ToString() + "ed", path);
                        }
                        catch (Exception ex)
                        {
                            LogError(ex.Message);
                            Console.WriteLine("Error: " + ex.Message);
                        }
                        UpdateProgress(i++);
                    }
                }

                using (StreamWriter w = File.AppendText(SuccessLog))
                {
                    w.WriteLine("<p>Job Completed at: " + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString() + "</p>");
                    w.WriteLine("<p>--------------------------------------------------------------------------------------------</p>");
                }
            }


        }

      
        private void UpdateProgress(int count)
        {            
            double fraction = ((double)(count + 1) / (double)FileCount);
            int percentage = (int)Math.Round(fraction * 100);
            bgWorker.ReportProgress(percentage);
        }

        private void LogError(string errorText)
        {
            HasErrors = true;

            using (StreamWriter w = File.AppendText(ErrorLog))
            {
                w.WriteLine("<p>" + errorText + "</p>");
            }
        }

        private void LogSuccess(string action, string filePath)
        {

            using (StreamWriter w = File.AppendText(SuccessLog))
            {
                w.WriteLine("<p>Succesfully " + action + " file: " + filePath + "</p>");
            }
        }

        private void FileEncryption_DragEnter(object sender, DragEventArgs e)
        {
            Console.Write("It is working.");
            e.Effect = DragDropEffects.Link;
            
        }

        private void FileEncryption_DragDrop(object sender, DragEventArgs e)
        {
            string[] fileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            txtTarget.Text = fileList[0];

            if (Directory.Exists(txtTarget.Text))
            {
                FileCount = Directory.GetFiles(txtTarget.Text, "*.*", SearchOption.AllDirectories).Length;
            }
            else if (File.Exists(txtTarget.Text))
            {
                FileCount = 1; 
            }
        }

        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            pBar.BringToFront();            
            CurrentTask = Task.Encrypt;            
            bgWorker.RunWorkerAsync();
        }

        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            pBar.BringToFront();            
            CurrentTask = Task.Decrypt;
            bgWorker.RunWorkerAsync();
        }

        private void TxtTarget_TextChanged(object sender, EventArgs e)
        {
            if (txtTarget.Text.Length > 1)
            {
                btnEncrypt.Enabled = true;
                btnDecrypt.Enabled = true;
            }
        }

        private void BgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ProcessTarget(txtTarget.Text, CurrentTask);
        }

        private void BgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage > 0)
            {
                pBar.Value = e.ProgressPercentage;
            }
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            pBar.Value = 100;

            MsgBx msgbx = new MsgBx(SuccessLog);
            msgbx.Show();

            if (HasErrors)
            {
                MsgBx msgbx2 = new MsgBx(ErrorLog);
                msgbx2.Show();                
            }
            
            txtTarget.Text = string.Empty;
            btnDecrypt.Enabled = false;
            btnEncrypt.Enabled = false;
            pBar.SendToBack();
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory();
            var url = new Uri(Path.Combine(path, "resources/about.htm"));
            //Process.Start(url.ToString());
            MsgBx msgbx = new MsgBx(url.ToString());
            msgbx.Show();

        }
    }
}
