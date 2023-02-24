using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace EncryptDecrypt;

public partial class MainWindow : Window
{
    public string password;
    CancellationTokenSource cancellation;
    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        password = Passwordtxtbox.Text;
    }

    private void FileOpenBtn_Click(object sender, RoutedEventArgs e)
    {
        FileDialog dialog = new OpenFileDialog();
        dialog.Filter = "txt files |*.txt";

        if (dialog.ShowDialog() == true)
            FilePathtxtbox.Text = dialog.FileName;
    }

    private void EncryptDecryptBtn_Click(object sender, RoutedEventArgs e)
    {
        if (Passwordtxtbox.Text is null)
            return;
        cancellation= new CancellationTokenSource();
        var text = File.ReadAllText(FilePathtxtbox.Text);
        var writeText = Encrypt(text, password);
        if ((bool)Encryptrbtn.IsChecked)
        {
            ThreadPool.QueueUserWorkItem(o =>
            {
                using var fs = new FileStream(FilePathtxtbox.Text, FileMode.Open);

                for (int i = 0; i < writeText.Length; i++)
                {
                    if (i % 32 == 0)
                    {
                        if (cancellation.IsCancellationRequested)
                        {
                            fs.Dispose();
                            Dispatcher.Invoke(() => File.WriteAllText(FilePathtxtbox.Text, text));
                            Dispatcher.Invoke(() => Progressbar.Value = 0);
                            return;
                        }

                        Thread.Sleep(500);
                        if (i != 0)
                            Dispatcher.Invoke(() => Progressbar.Value = 100 * i / writeText.Length);
                    }
                    fs.WriteByte((byte)writeText[i]);
                }

                fs.Seek(0, SeekOrigin.Begin);

                Dispatcher.Invoke(() => Progressbar.Value = 100);
            });
        }

        else if ((bool)Decryptrbtn.IsChecked)
        {
            writeText = Decrypt(text, password);
            ThreadPool.QueueUserWorkItem(o =>
            {
                using var fs = new FileStream(FilePathtxtbox.Text, FileMode.Truncate);

                for (int i = 0; i < writeText.Length; i++)
                {
                    if (i % 32 == 0)
                    {
                        if (cancellation.IsCancellationRequested)
                        {
                            fs.Dispose();
                            Dispatcher.Invoke(() => File.WriteAllText(FilePathtxtbox.Text, text));
                            Dispatcher.Invoke(() => Progressbar.Value = 0);
                            return;
                        }

                        Thread.Sleep(500);
                        if (i != 0)
                            Dispatcher.Invoke(() => Progressbar.Value = 100 * i / writeText.Length);
                    }
                    fs.WriteByte((byte)writeText[i]);
                }

                fs.Seek(0, SeekOrigin.Begin);

                Dispatcher.Invoke(() => Progressbar.Value = 100);
            });
        }

    }

    public static string Encrypt(string clearText,string password)
    {
        string EncryptionKey = password;
        byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
        using (Aes encryptor = Aes.Create())
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
            encryptor.Key = pdb.GetBytes(32);
            encryptor.IV = pdb.GetBytes(16);
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearBytes, 0, clearBytes.Length);
                    cs.Close();
                }
                clearText = Convert.ToBase64String(ms.ToArray());
            }
        }
        return clearText;
    }

public static string Decrypt(string cipherText,string password)
    {
        string EncryptionKey = password;
        cipherText = cipherText.Replace(" ", "+");
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        return cipherText;
    }

    private void DecryptBtn_Click(object sender, RoutedEventArgs e)
    {

    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        cancellation.Cancel();
    }



}
