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


        public FileEncryption()
        {
            InitializeComponent();
        }




        private void Encrypt(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            if (attr.HasFlag(FileAttributes.Directory))
            {

                foreach (string file in Directory.GetFiles(path))
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
                }

                foreach (string subDir in Directory.GetDirectories(path))
                {
                    foreach (string file in Directory.GetFiles(subDir))
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
                    }
                }

            }
            else
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
            }
        }


        private void Decrypt(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            if (attr.HasFlag(FileAttributes.Directory))
            {

                foreach (string file in Directory.GetFiles(path))
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
                }

                foreach (string subDir in Directory.GetDirectories(path))
                {
                    foreach (string file in Directory.GetFiles(subDir))
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
                    }
                }

            }
            else
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
            }
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


    }
}
