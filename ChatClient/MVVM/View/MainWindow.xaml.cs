using ChatClient.Crypto;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ZI_ChatApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ListViewEncryptedMessages.Visibility = (ListViewEncryptedMessages.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            ListViewDecryptedMessages.Visibility = (ListViewDecryptedMessages.Visibility == Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible;
            ToggleButton.Content = (ListViewDecryptedMessages.Visibility == Visibility.Visible) ? "Show encrypted messages" : "Show decrypted messages";
        }

        private void ChooseEncryptionButton_Click(object sender, RoutedEventArgs e)
        {
            ChooseEncryptionButton.Content = ChooseEncryptionButton.Content.ToString() == "Encrypt by RC6 OFB Cipher" ? "Encrypt by Bifid Cipher" : "Encrypt by RC6 OFB Cipher";
        }

        private void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();

            bool? response = openFileDialog.ShowDialog();

            //  SHA1 sha1 = new SHA1();

            if (response == true)
            {
                string filePath = openFileDialog.FileName;
                try
                {
                    //string fileContentString = File.ReadAllText(filePath);

                      byte[] fileContent = File.ReadAllBytes(filePath);
                    //  string fileContentString = Convert.ToBase64String(fileContent);
                    string fileContentString = Encoding.UTF8.GetString(fileContent);  // cita samo utf8 txt fajlove lepo

                    //  byte[] hashBytes = sha1.GetMessageHashCode_Byte(fileContent);
                    //  string fileHashCode = Convert.ToBase64String(hashBytes);

                    /*string fileHashCode = sha1.GetMessageHashCode(fileContent);
                    string message = fileHashCode + fileContentString;*/

                    //  string message = fileHashCode + filePath;

                    if (MessageTextBox != null)
                    {
                        MessageTextBox.Text = /*filePath*/fileContentString;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading file: {ex.Message}");
                }
            }
        }
    }
}
