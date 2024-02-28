using ChatClient.Crypto;
using ChatClient.Net.IO;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Net
{
    class Server
    {
        Bifid _bifidCipher;
        RC6_OFB _rc6OFBCipher;
        SHA1 _sha1;
        TcpClient _client;
        public PacketReader PacketReader;

        public event Action connectedEvent;
        public event Action msgReceivedEvent;
        public event Action userDisconnectedEvent;

        public CipherType ChoosenCipher { get; set; }

        public Server()
        {
            _client = new TcpClient();
            _sha1 = new SHA1();
            _bifidCipher = new Bifid();
            _rc6OFBCipher = new RC6_OFB();
            ChoosenCipher = CipherType.BifidCipher;
        }

        public void ConnectToServer(string username)
        {
            if (!_client.Connected)
            {
                _client.Connect("127.0.0.1", 7777);
                PacketReader = new PacketReader(_client.GetStream());

                if (!string.IsNullOrEmpty(username))
                {
                    PacketBuilder connectPacket = new PacketBuilder();
                    connectPacket.WriteOpCode(0);
                    connectPacket.WriteMessage(username);
                    _client.Client.Send(connectPacket.GetPacketBytes());
                }
                ReadPackets();
            }
        }

        private void ReadPackets()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    byte opcode = PacketReader.ReadByte();
                    switch (opcode)
                    {
                        case 1:
                            connectedEvent?.Invoke();
                            break;
                        case 5:
                            msgReceivedEvent?.Invoke();
                            break;
                        case 6:
                            msgReceivedEvent?.Invoke();
                            break;
                        case 10:
                            userDisconnectedEvent?.Invoke();
                            break;
                        default:
                            break;
                    }
                }
            });
        }

        public void SendMessageToServer(string message)
        {
            PacketBuilder messagePacket = new PacketBuilder();
            string encryptedMessage = "";

            string hashCodePlusMessage = _sha1.GetMessageHashCode_String(message);
            hashCodePlusMessage += message;

            switch (ChoosenCipher)
            {
                case CipherType.BifidCipher:
                    messagePacket.WriteOpCode(5);
                    encryptedMessage = BifidEncrypt(hashCodePlusMessage);
                    break;
                case CipherType.RC6Cipher:
                    messagePacket.WriteOpCode(6);
                    encryptedMessage = RC6OFBEncrypt(hashCodePlusMessage);
                    break;
                default:
                    break;
            }

            messagePacket.WriteMessage(encryptedMessage);
            _client.Client.Send(messagePacket.GetPacketBytes());
        }

        private string BifidEncrypt(string text)
        {
            if (ConfigurationManager.AppSettings["BifidCryptoKey"] == "")
                return text;

            string encryptedText = "";
            string word = "";
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if ((ch <= 'Z' && ch >= 'A') || (ch <= 'z' && ch >= 'a'))
                {
                    word += ch;
                }
                else
                {
                    encryptedText += _bifidCipher.Encrypt(word);
                    encryptedText += ch;
                    word = "";
                }
            }
            encryptedText += _bifidCipher.Encrypt(word);
            return encryptedText;
        }

        private string RC6OFBEncrypt(string text)
        {
            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            byte[] encryptedBytes = _rc6OFBCipher.EncryptByteArrayOFB(textBytes);
            string encryptedText = Convert.ToBase64String(encryptedBytes);

            return encryptedText;
        }
    }
}
