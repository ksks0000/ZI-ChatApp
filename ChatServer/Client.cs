using ChatServer.Net.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatServer
{
    class Client
    {
        public string Username { get; set; }
        public Guid UID { get; set; }
        public TcpClient ClientSocket { get; set; }

        PacketReader _packetReader;

        public Client(TcpClient client)
        {
            ClientSocket = client;
            UID = Guid.NewGuid();
            _packetReader = new PacketReader(ClientSocket.GetStream());

            byte opcode = _packetReader.ReadByte();
            Username = _packetReader.ReadMessage();

            Console.WriteLine($"[{DateTime.Now}]: Clinet has connected with the username: {Username}");

            Task.Run(() => ProcessPackets());
        }

        void ProcessPackets()
        {
            while(true)
            {
                try
                {
                    byte opcode = _packetReader.ReadByte();
                    string msg;
                    switch (opcode)
                    {
                        case 5:
                            msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message recived {msg}");
                            Program.BroadcastMessage($"{opcode}${DateTime.Now}${Username}${msg}", opcode);
                            break;
                        case 6:
                            msg = _packetReader.ReadMessage();
                            Console.WriteLine($"[{DateTime.Now}]: Message recived {msg}");
                            Program.BroadcastMessage($"{opcode}${DateTime.Now}${Username}${msg}", opcode);
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[{UID}]: Disconnected");
                    Program.BroadcastDiscconect(UID.ToString());
                    ClientSocket.Close();
                    break;
                }
            }        
        }
    }
}
