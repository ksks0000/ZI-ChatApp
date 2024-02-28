using ChatClient.Crypto;
using ChatClient.MVVM.Core;
using ChatClient.MVVM.Model;
using ChatClient.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


enum CipherType
{
    BifidCipher,
    RC6Cipher
}

namespace ChatClient.MVVM.ViewModel
{
    class MainViewModel : INotifyPropertyChanged
    {
        Bifid _bifidCipher;
        RC6_OFB _rc6OFBCipher;
        SHA1 _sha1;
        public ObservableCollection<UserModel> Users { get; set; }
        public ObservableCollection<MessageModel> Messages { get; set; }
        public RelayCommand ConnectToServerCommand { get; set; }
        public RelayCommand SendMessageCommand { get; set; }
        public RelayCommand ChooseEncryptionCipherCommand { get; set; }
        public string Username { get; set; }
        public CipherType ChoosenCipher
        {
            get { return _server.ChoosenCipher; }
            set
            {
                if (_server.ChoosenCipher != value)
                {
                    _server.ChoosenCipher = value;
                    OnPropertyChanged(nameof(ChoosenCipher));
                }
            }
        }
        public string Message 
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged(nameof(Message));
                }
            }
        }
        private string _message; 
        private Server _server;

        public MainViewModel()
        {
            Users = new ObservableCollection<UserModel>();
            Messages = new ObservableCollection<MessageModel>();
            _bifidCipher = new Bifid();
            _rc6OFBCipher = new RC6_OFB();
            _sha1 = new SHA1();
            _server = new Server();
            _server.connectedEvent += UserConnected;
            _server.msgReceivedEvent += MessageReceived;
            _server.userDisconnectedEvent += RemoveUser;
            ConnectToServerCommand = new RelayCommand(obj => _server.ConnectToServer(Username),
                obj => !string.IsNullOrEmpty(Username));
            SendMessageCommand = new RelayCommand(
                obj =>
                {
                    string currentMessage = Message;
                    Message = string.Empty;
                    _server.SendMessageToServer(currentMessage);
                },
                obj => !string.IsNullOrEmpty(Message));
            ChooseEncryptionCipherCommand = new RelayCommand(obj =>
            {
                ChoosenCipher = ChoosenCipher == CipherType.BifidCipher ? CipherType.RC6Cipher : CipherType.BifidCipher;
                _server.ChoosenCipher = ChoosenCipher; 
            });
        }

        private void UserConnected()
        {

            UserModel user = new UserModel
            {
                Username = _server.PacketReader.ReadMessage(),
                UID = _server.PacketReader.ReadMessage(),
            };

            if (!Users.Any(x => x.UID == user.UID))
            {
                Application.Current.Dispatcher.Invoke(() => Users.Add(user));
            }
        }

        private void MessageReceived()
        {
            string msg = _server.PacketReader.ReadMessage();
            MessageModel mm = SplitAndDecryptMessage(msg);
            Application.Current.Dispatcher.Invoke(() => Messages.Add(mm));
        }

        private void RemoveUser()
        {
            string uid = _server.PacketReader.ReadMessage();
            UserModel user = Users.Where(x => x.UID == uid).FirstOrDefault();
            Application.Current.Dispatcher.Invoke(()=>Users.Remove(user));
        }

        private MessageModel SplitAndDecryptMessage(string msg)
        {
            string[] splitedMsg = msg.Split("$");
            MessageModel mm = new MessageModel();

            mm.Message = msg;
            mm.Time = DateTime.Now.ToString();
            mm.Username = "";

            if (splitedMsg.Length > 1)
            {
                string message = splitedMsg[3];
                string opcode = splitedMsg[0];
                string decryptedMessage = "";

                if (opcode == "5")
                    decryptedMessage = BifidDecrypt(message);
                if (opcode == "6")
                    decryptedMessage = RC6OFBDecrypt(message);

                mm.Time = splitedMsg[1];
                mm.Username = splitedMsg[2];
                //mm.Message = decryptedMessage;
                mm.Message = CalculateMessageHashCode(decryptedMessage);
                mm.MessageEncrypted = message;
            }
            return mm;
        }

        public event PropertyChangedEventHandler PropertyChanged; 

        protected virtual void OnPropertyChanged(string propertyName) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string CalculateMessageHashCode(string hashCodePlusMessage)
        {
            string hashCodeFromMessage = hashCodePlusMessage.Substring(0, 28);
            string decryptedMessage = hashCodePlusMessage.Substring(28);
            string hashCodeCalculated = _sha1.GetMessageHashCode_String(decryptedMessage);

            hashCodeFromMessage = hashCodeFromMessage.ToUpper(); // zbog bifid algoritma
            hashCodeCalculated = hashCodeCalculated.ToUpper();

            if (!hashCodeFromMessage.Equals(hashCodeCalculated))
                Console.WriteLine("Hash code of recieved message and calculated hash code missmatch.");

            return decryptedMessage;
        }

        private string BifidDecrypt(string message)
        {
            string decryptedMessage = "";
            string word = "";
            for (int i = 0; i < message.Length; i++)
            {
                char ch = message[i];
                if ((ch <= 'Z' && ch >= 'A') || (ch <= 'z' && ch >= 'a'))
                {
                    word += ch;
                }
                else
                {
                    decryptedMessage += _bifidCipher.Decrypt(word);
                    decryptedMessage += ch;
                    word = "";
                }
            }
            decryptedMessage += _bifidCipher.Decrypt(word);
            decryptedMessage = decryptedMessage.ToLower();

            return decryptedMessage;
        }

        private string RC6OFBDecrypt(string message)
        {
            byte[] messageBytes = Convert.FromBase64String(message);
            byte[] decryptedBytes = _rc6OFBCipher.DecryptByteArrayOFB(messageBytes);
            string decryptedText = Encoding.UTF8.GetString(decryptedBytes);

            return decryptedText;
        }
    }
}
