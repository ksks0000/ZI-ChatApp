using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient.Crypto
{
    class RC6_OFB
    {
        private int w;          // register length in bits
        private int log_w;      // log2(W)
        private int r;          // number of rounds
        private uint[] s;       // W-bit round keys S[0,...,2r + 3]  

        private uint[] l;
        private uint pw;
        private uint qw;

        private byte[] IV;
        private byte[] KEY;
        private int B;          // key length in bytes

        public RC6_OFB(string key = null)
        {
            w = 32;
            r = 20;
            B = 24;
            log_w = (int)Math.Log2(w);
            s = new uint[2 * r + 4];

            pw = 0xB7E15163;
            qw = 0x9E3779B9;

            KEY = new byte[B];
            KEY = key != null ? Encoding.UTF8.GetBytes(key) : Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["RC6CryptoKey"]);

            GenerateRoundKeys(KEY);

            if (l == null) l = new uint[10];

            IV = new byte[w * 4 / 8];
            IV = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["RC6InitializationVector"]);
        }

        private void GenerateRoundKeys(byte[] key)
        {
            int sLength = 2 * r + 4;
            int c = key.Length / (w / 8);
            l = new uint[c];

            byte[] extendedKey = new byte[c * 4];
            Buffer.BlockCopy(key, 0, extendedKey, 0, key.Length);
            Buffer.BlockCopy(extendedKey, 0, l, 0, c * 4);

            s[0] = pw;

            for (int x = 1; x < sLength; x++)
            {
                s[x] = s[x - 1] + qw;
            }

            uint A = 0;
            uint B = 0;
            int i = 0;
            int j = 0;
            int v = 3 * (c > sLength ? c : sLength);

            for (int s = 1; s <= v; s++)
            {
                A = this.s[i] = (this.s[i] + A + B) << 3;
                B = l[j] = (l[j] + A + B) << (int)(A + B);
                i = (i + 1) % sLength;
                j = (j + 1) % c;
            }

        }

        public uint[] Encrypt(uint[] blockData)
        {
            if (blockData.Length != 4) Console.WriteLine("RC6 Encryption: Invalid block size.");
            uint[] encryptedBlock = new uint[4];
            uint A = blockData[0];
            uint B = blockData[1];
            uint C = blockData[2];
            uint D = blockData[3];

            uint t, u, temp;

            B += s[0];
            D += s[1];

            for (int i = 1; i <= r; i++)
            {
                t = (B * (2 * B + 1)) << log_w;
                u = (D * (2 * D + 1)) << log_w;

                A = ((A ^ t) << (int)u) + s[2 * i];
                C = ((C ^ u) << (int)t) + s[2 * i + 1];

                temp = A;
                A = B;
                B = C;
                C = D;
                D = temp;
            }

            A += s[2 * r + 2];
            C += s[2 * r + 3];

            encryptedBlock[0] = A;
            encryptedBlock[1] = B;
            encryptedBlock[2] = C;
            encryptedBlock[3] = D;

            return encryptedBlock;
        }

        public uint[] Decrypt(uint[] blockData)
        {
            if (blockData.Length != 4) Console.WriteLine("RC6 Decryption: Invalid block size.");
            uint[] decryptedBlock = new uint[4];
            uint A = blockData[0];
            uint B = blockData[1];
            uint C = blockData[2];
            uint D = blockData[3];

            uint t, u, temp;

            C -= s[2 * r + 3];
            A -= s[2 * r + 2];

            for (int i = r; i >= 1; i--)
            {
                temp = D;
                D = C;
                C = B;
                B = A;
                A = temp;

                u = (D * (2 * D + 1)) << log_w;
                t = (B * (2 * B + 1)) << log_w;

                C = ((C - s[2 * i + 1]) >> (int)t) ^ u;
                A = ((A - s[2 * i]) >> (int)u) ^ t;
            }

            D -= s[1];
            B -= s[0];

            decryptedBlock[0] = A;
            decryptedBlock[1] = B;
            decryptedBlock[2] = C;
            decryptedBlock[3] = D;

            return decryptedBlock;
        }

        public byte[] EncryptByteArray(byte[] input)
        {
            int BLOCK_LENGHT_IN_BYTES = 4 * w / 8;

            int padding = BLOCK_LENGHT_IN_BYTES - (input.Length % BLOCK_LENGHT_IN_BYTES);
            int extendedInpuntLength = input.Length + padding;
            byte[] extendedInput = new byte[extendedInpuntLength];
            Buffer.BlockCopy(input, 0, extendedInput, 0, input.Length);

            byte[] encrypted = new byte[extendedInpuntLength];
            uint[] blockData = new uint[4];

            for (int i = 0; i < extendedInpuntLength / BLOCK_LENGHT_IN_BYTES; i++)
            {
                Buffer.BlockCopy(extendedInput, i * BLOCK_LENGHT_IN_BYTES, blockData, 0, BLOCK_LENGHT_IN_BYTES);
                uint[] encryptedBlock = Encrypt(blockData);
                Buffer.BlockCopy(encryptedBlock, 0, encrypted, i * BLOCK_LENGHT_IN_BYTES, BLOCK_LENGHT_IN_BYTES);
            }

            return encrypted;
        }

        public byte[] DecryptByteArray(byte[] input)
        {
            int BLOCK_LENGHT_IN_BYTES = 4 * w / 8;

            byte[] decrypted = new byte[input.Length];
            uint[] blockData = new uint[4];

            for (int i = 0; i < input.Length / BLOCK_LENGHT_IN_BYTES; i++)
            {
                Buffer.BlockCopy(input, i * BLOCK_LENGHT_IN_BYTES, blockData, 0, BLOCK_LENGHT_IN_BYTES);
                uint[] decryptedBlock = Decrypt(blockData);
                Buffer.BlockCopy(decryptedBlock, 0, decrypted, i * BLOCK_LENGHT_IN_BYTES, BLOCK_LENGHT_IN_BYTES);
            }

            return decrypted;
        }

        public byte[] EncryptByteArrayOFB(byte[] input)
        {
            int BLOCK_LENGHT_IN_BYTES = 4 * w / 8;

            int padding = BLOCK_LENGHT_IN_BYTES - (input.Length % BLOCK_LENGHT_IN_BYTES);
            int extendedInpuntLength = input.Length + padding;
            byte[] extendedInput = new byte[extendedInpuntLength];
            Buffer.BlockCopy(input, 0, extendedInput, 0, input.Length);

            byte[] encrypted = new byte[extendedInpuntLength];
            uint[] blockData = new uint[4];

            uint[] currentIV = new uint[4];
            Buffer.BlockCopy(IV, 0, currentIV, 0, BLOCK_LENGHT_IN_BYTES);

            for (int i = 0; i < extendedInpuntLength / BLOCK_LENGHT_IN_BYTES; i++)
            {
                Buffer.BlockCopy(currentIV, 0, blockData, 0, BLOCK_LENGHT_IN_BYTES);
                uint[] encryptedIV = Encrypt(blockData);
                Buffer.BlockCopy(extendedInput, i * BLOCK_LENGHT_IN_BYTES, blockData, 0, BLOCK_LENGHT_IN_BYTES);
                for (int j = 0; j < 4; j++)
                {
                    currentIV[j] = encryptedIV[j];
                    encryptedIV[j] = encryptedIV[j] ^ blockData[j];
                }
                Buffer.BlockCopy(encryptedIV, 0, encrypted, i * BLOCK_LENGHT_IN_BYTES, BLOCK_LENGHT_IN_BYTES);
            }

            return encrypted;
        }

        public byte[] DecryptByteArrayOFB(byte[] input)
        {
            int BLOCK_LENGHT_IN_BYTES = 4 * w / 8;

            byte[] decrypted = new byte[input.Length];
            uint[] blockData = new uint[4];

            uint[] currentIV = new uint[4];
            Buffer.BlockCopy(IV, 0, currentIV, 0, BLOCK_LENGHT_IN_BYTES);

            for (int i = 0; i < input.Length / BLOCK_LENGHT_IN_BYTES; i++)
            {
                Buffer.BlockCopy(currentIV, 0, blockData, 0, BLOCK_LENGHT_IN_BYTES);
                uint[] encryptedIV = Encrypt(blockData);
                Buffer.BlockCopy(input, i * BLOCK_LENGHT_IN_BYTES, blockData, 0, BLOCK_LENGHT_IN_BYTES);
                for (int j = 0; j < 4; j++)
                {
                    currentIV[j] = encryptedIV[j];
                    encryptedIV[j] = encryptedIV[j] ^ blockData[j];
                }
                Buffer.BlockCopy(encryptedIV, 0, decrypted, i * BLOCK_LENGHT_IN_BYTES, BLOCK_LENGHT_IN_BYTES);
            }

            return decrypted;
        }
    }
}
