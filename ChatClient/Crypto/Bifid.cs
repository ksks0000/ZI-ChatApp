using System;
using System.Collections.Generic;
using System.Configuration;

namespace ChatClient.Crypto
{
    class Bifid
    {
        private int SQUARE_SIZE = 5;
        private char[,] _polybiousSquare;
        private Dictionary<char, (int, int)> _charToCoordMap;

        private string Key { get; set; }

        public Bifid(string key = null)
        {
            if (key != null) Key = key;
            Key = ConfigurationManager.AppSettings["BifidCryptoKey"];
            Key = Key.ToUpper();
            _polybiousSquare = new char[SQUARE_SIZE, SQUARE_SIZE];
            BuildKeyMatrix();

            _charToCoordMap = new Dictionary<char, (int, int)>();

            for (int i = 0; i < SQUARE_SIZE; i++)
            {
                for (int j = 0; j < SQUARE_SIZE; j++)
                {
                    _charToCoordMap.Add(_polybiousSquare[i, j], (i, j));
                }
            }
        }
        private void BuildKeyMatrix()
        {
            int keyIndex = 0;
            Dictionary<char, bool> usedChars = new Dictionary<char, bool>();

            for (int i = 0; i < Key.Length; i++)
            {
                char currentKeyChar = Key[keyIndex++];
                if (!usedChars.ContainsKey(currentKeyChar))
                {
                    if (currentKeyChar == 'J')
                    {
                        currentKeyChar = 'I';
                    }
                    _polybiousSquare[i / SQUARE_SIZE, i % SQUARE_SIZE] = currentKeyChar;
                    usedChars[currentKeyChar] = true;
                }
                else
                {
                    i--;
                }
            }

            char alphabetChar = 'A';
            for (int i = 0 /*Key.Length*/; i < SQUARE_SIZE * SQUARE_SIZE; i++)
            {
                if (_polybiousSquare[i / SQUARE_SIZE, i % SQUARE_SIZE] == '\0')
                {
                    while (usedChars.ContainsKey(alphabetChar) || alphabetChar == 'J')
                    {
                        alphabetChar++;
                    }
                    _polybiousSquare[i / SQUARE_SIZE, i % SQUARE_SIZE] = alphabetChar++;
                }
            }
        }
        public string Encrypt(string word)
        {
            if (word == "") return "";

            word = word.ToUpper();
            int WORD_LENGHT = word.Length;
            int[] rowCoords = new int[WORD_LENGHT];
            int[] columnCoords = new int[WORD_LENGHT];

            string encryptedWord = "";

            for (int i = 0; i < WORD_LENGHT; i++)
            {
                char ch = word[i];
                if (ch < 'A' || ch > 'Z')
                {
                    continue;
                }
                if (ch == 'J')
                {
                    ch = 'I';
                }
                var pair = _charToCoordMap[ch];
                rowCoords[i] = pair.Item1;
                columnCoords[i] = pair.Item2;
            }

            int PERIOD = WORD_LENGHT;
            int[] coordsArray = new int[2 * WORD_LENGHT];

            for (int i = 0; i < WORD_LENGHT; i += PERIOD)
            {
                Buffer.BlockCopy(rowCoords, i, coordsArray, i, PERIOD * sizeof(int));
                Buffer.BlockCopy(columnCoords, i, coordsArray, i + PERIOD * sizeof(int), PERIOD * sizeof(int));
            }


            for (int i = 0; i < 2 * WORD_LENGHT; i += 2)
            {
                encryptedWord += _polybiousSquare[coordsArray[i], coordsArray[i + 1]];
            }

            return encryptedWord;
        }

        /* public string Decrypt(string word)
        {
            int WORD_LENGHT = word.Length;
            int[] rowCoords = new int[WORD_LENGHT];
            int[] columnCoords = new int[WORD_LENGHT];

            int[] coordsArray = new int[2 * WORD_LENGHT];

            for (int i = 0; i < WORD_LENGHT; i++)
            {
                char ch = word[i];
                if (ch < 'A' || ch > 'Z')
                {
                    continue;
                }
                var pair = _charToCoordMap[ch];
                coordsArray[2 * i] = pair.Item1;
                coordsArray[2 * i + 1] = pair.Item2;
            }

            int PERIOD = WORD_LENGHT;
            for (int i = 0; i < WORD_LENGHT; i += PERIOD)
            {
                Buffer.BlockCopy(coordsArray, i, rowCoords, i, PERIOD * sizeof(int));
                Buffer.BlockCopy(coordsArray, i + PERIOD, columnCoords, i, PERIOD * sizeof(int));
            }

            string decryptedWord = "";
            for (int i = 0; i < WORD_LENGHT; i++)
            {
                decryptedWord += _polybiousSquare[rowCoords[i], columnCoords[i]];
            }

            return decryptedWord;
        } */

        public string Decrypt(string word)
        {
            if (word == "") return "";

            int WORD_LENGHT = word.Length;
            int[] rowCoords = new int[WORD_LENGHT];
            int[] columnCoords = new int[WORD_LENGHT];

            int[] coordsArray = new int[2 * WORD_LENGHT];

            int size = 0;
            for (int i = 0; i < WORD_LENGHT; i++)
            {
                char ch = word[i];
                if (ch < 'A' || ch > 'Z')
                {
                    continue;
                }
                var pair = _charToCoordMap[ch];
                coordsArray[size++] = pair.Item1;
                coordsArray[size++] = pair.Item2;
            }

            Buffer.BlockCopy(coordsArray, 0, rowCoords, 0, size * sizeof(int) / 2);
            Buffer.BlockCopy(coordsArray, size * sizeof(int) / 2, columnCoords, 0, size * sizeof(int) / 2);

            string dectryptedWord = "";

            for (int i = 0; i < size / 2; i++)
            {
                dectryptedWord += _polybiousSquare[rowCoords[i], columnCoords[i]];
            }

            return dectryptedWord;
        }
    }
}
