using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.MVVM.Model
{
    class MessageModel
    {
        public string Username { get; set; }
        public string UsernameColor { get; set; }
        public string Message { get; set; }
        public string MessageEncrypted { get; set; }
        public string Time { get; set; }
        public string Opcode { get; set; }
    }
}
