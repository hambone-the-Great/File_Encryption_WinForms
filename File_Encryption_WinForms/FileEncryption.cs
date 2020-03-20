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

namespace File_Encryption_WinForms
{
    public partial class FileEncryption : Form
    {

        private bool HasErrors = false;
        private string SuccessLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "File_Encryption_Success.log");
        private string ErrorLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "File_Encryption_Error.log");
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
                w.WriteLine("File Encryption Error Log");
                w.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "\r\n\r\n");
            }

            using (StreamWriter w = File.AppendText(SuccessLog))
            {
                w.WriteLine("File Encryption Success Log");
                w.WriteLine(DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() + "\r\n\r\n");
            }

        }

        private void ProcessTarget(string path, Task task)
        {

            if (path == null)
            {
                MessageBox.Show("Error! Path of target was null. Try again.");                
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
                    w.WriteLine("\n\rJob Completed at: " + DateTime.Now.ToLongTimeString() + " " + DateTime.Now.ToLongDateString());
                    w.WriteLine("--------------------------------------------------------------------------------------------\n\r\n\r");
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
                w.WriteLine(errorText);
            }
        }

        private void LogSuccess(string action, string filePath)
        {

            using (StreamWriter w = File.AppendText(SuccessLog))
            {
                w.WriteLine("Succesfully " + action + " file: " + filePath);
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
            pBar.Value = e.ProgressPercentage;
        }

        private void BgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Process.Start(SuccessLog);
            if (HasErrors) Process.Start(ErrorLog);            
            pBar.SendToBack();
            txtTarget.Text = string.Empty;
            btnDecrypt.Enabled = false;
            btnEncrypt.Enabled = false; 
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Directory.GetCurrentDirectory();
            var url = new Uri(Path.Combine(path, "resources/about.htm"));
            Process.Start(url.ToString());

        }
    }
}
