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
        private string SuccessLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "File_Encryption_Success.log");
        private string ErrorLog = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "File_Encryption_Error.log");
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
        

        private void Encrypt(string path)
        {
            FileAttributes attr = File.GetAttributes(path);
            
            int i = 0;

            if (attr.HasFlag(FileAttributes.Directory))
            {

                foreach (string file in Directory.GetFiles(path))
                {
                    if (File.Exists(file))
                    {
                        try
                        {
                            File.Encrypt(file);
                            Console.WriteLine("Encrypting: " + file);
                            LogSuccess("encrypted", file);                            
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
                                    File.Encrypt(file);
                                    Console.WriteLine("Encrypting: " + file);
                                    LogSuccess("encrypted", file);
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
                        File.Encrypt(path);
                        Console.WriteLine("Encrypting: " + path);
                        LogSuccess("encrypted", path);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex.Message);
                        Console.WriteLine("Error: " + ex.Message);
                    }
                    UpdateProgress(i++);
                }
            }

            //percentage = (i + 1) * 100 / FileCount;
            //bgWorker.ReportProgress(percentage);
        }


        private void Decrypt(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            int i = 0;

            if (attr.HasFlag(FileAttributes.Directory))
            {

                foreach (string file in Directory.GetFiles(path))
                {
                    if (File.Exists(file))
                    {
                        try
                        {
                            File.Decrypt(file);
                            Console.WriteLine("Decrypting: " + file);
                            LogSuccess("decrypted", file);
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
                                    File.Decrypt(file);
                                    Console.WriteLine("Decrypting: " + file);
                                    LogSuccess("decrypted", file);
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
                        File.Decrypt(path);
                        Console.WriteLine("Decrypting: " + path);
                        LogSuccess("decrypted", path);
                        

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

            if (Directory.Exists(txtTarget.Text) || File.Exists(txtTarget.Text))
            {
                FileCount = Directory.GetFiles(txtTarget.Text, "*.*", SearchOption.AllDirectories).Length;
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
            if (CurrentTask == Task.Encrypt)
            {
                Encrypt(txtTarget.Text);
            }
            else if (CurrentTask == Task.Decrypt)
            {
                Decrypt(txtTarget.Text);
            }
            else
            {
                MessageBox.Show("Someting went wrong.");
            }
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
        }
    }
}
