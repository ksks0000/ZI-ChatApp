using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Crypto
{
    class SHA1
    {
        private uint h0, h1, h2, h3, h4;
        private uint numOfBlocks;
        private readonly byte BLOCK_SIZE;

        public SHA1()
        {
            h0 = 0x67452301;
            h1 = 0xEFCDAB89;
            h2 = 0x98BADCFE;
            h3 = 0x10325476;
            h4 = 0xC3D2E1F0;

            numOfBlocks = 0;
            BLOCK_SIZE = 64;
        }

        private byte[] Preprocess(byte[] message)
        {
            byte[] msgLength = BitConverter.GetBytes((ulong)message.Length);

            numOfBlocks = (uint)message.Length / BLOCK_SIZE;
            uint paddingInBytes = (uint)message.Length - numOfBlocks * BLOCK_SIZE;
            paddingInBytes = BLOCK_SIZE - paddingInBytes;
            if (paddingInBytes > 0) numOfBlocks++;

            paddingInBytes -= sizeof(ulong);
            byte[] extended = new byte[numOfBlocks * BLOCK_SIZE];
           
            if (paddingInBytes > 0)
            {
                byte[] bits = new byte[paddingInBytes];

                bits[0] = 128;  // 128 = 10000000 u bitovima
                for (int i = 1; i < paddingInBytes; bits[i++] = 0)

                Buffer.BlockCopy(message, 0, extended, 0, message.Length);
                Buffer.BlockCopy(bits, 0, extended, message.Length, bits.Length);
                Buffer.BlockCopy(msgLength, 0, extended, message.Length + bits.Length, msgLength.Length);
            } 
            else
            {
                Buffer.BlockCopy(message, 0, extended, 0, message.Length);
                Buffer.BlockCopy(msgLength, 0, extended, message.Length, msgLength.Length);
            }

            return extended;
        }

        public string GetMessageHashCode_String(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] hashBytes = GetMessageHashCode_Byte(messageBytes);
            string hashCode = Convert.ToBase64String(hashBytes);

            return hashCode;
        }

        public byte[] GetMessageHashCode_Byte(byte[] message)
        {
            byte[] hash = new byte[20];

            byte[] extendedMessage = Preprocess(message);
            byte[] block = new byte[BLOCK_SIZE];

            for (int i = 0; i < numOfBlocks; i++)
            {
                Buffer.BlockCopy(extendedMessage, i * BLOCK_SIZE, block, 0, BLOCK_SIZE);
                GetBlockHashCode(block);
            }

            byte[] hBytes = BitConverter.GetBytes(h0);
            Buffer.BlockCopy(hBytes, 0, hash, 0, 4);
            hBytes = BitConverter.GetBytes(h1);
            Buffer.BlockCopy(hBytes, 0, hash, 4, 4);
            hBytes = BitConverter.GetBytes(h2);
            Buffer.BlockCopy(hBytes, 0, hash, 8, 4);
            hBytes = BitConverter.GetBytes(h3);
            Buffer.BlockCopy(hBytes, 0, hash, 12, 4);
            hBytes = BitConverter.GetBytes(h4);
            Buffer.BlockCopy(hBytes, 0, hash, 16, 4);

            return hash;
        }

        public void GetBlockHashCode(byte[] block)
        {
            if (block.Length != BLOCK_SIZE) Console.WriteLine("SHA1 error: Invalid block size.");

            uint[] w = new uint[80];

            Buffer.BlockCopy(block, 0, w, 0, BLOCK_SIZE);

            for (int i = 16; i < 79; i++)
                w[i] = (w[i - 3] ^ w[i - 8] ^ w[i - 14] ^ w[i - 16]) << 1;

            uint a = h0;
            uint b = h1;
            uint c = h2;
            uint d = h3;
            uint e = h4;

            uint f, k, temp;

            for (int i = 0; i < 19; i++)
            {
                f = (b & c) | (~b & c);
                k = 0x5A827999;

                temp = (a << 5) + f + e + k + w[i];
                e = d;
                d = c;
                c = b << 30;
                b = a;
                a = temp;
            }

            for (int i = 20; i < 39; i++)
            {
                f = b ^ c ^ d;
                k = 0x6ED9EBA1;

                temp = (a << 5) + f + e + k + w[i];
                e = d;
                d = c;
                c = b << 30;
                b = a;
                a = temp;
            }

            for (int i = 40; i < 59; i++)
            {
                f = (b & c) | (b & d) | (c & d);
                k = 0x8F1BBCDC;

                temp = (a << 5) + f + e + k + w[i];
                e = d;
                d = c;
                c = b << 30;
                b = a;
                a = temp;
            }

            for (int i = 60; i < 79; i++)
            {
                f = b ^ c ^ d;
                k = 0xCA62C1D6;

                temp = (a << 5) + f + e + k + w[i];
                e = d;
                d = c;
                c = b << 30;
                b = a;
                a = temp;
            }

            h0 += a;
            h1 += b;
            h2 += c;
            h3 += d;
            h4 += e;          
        }
    }
}
