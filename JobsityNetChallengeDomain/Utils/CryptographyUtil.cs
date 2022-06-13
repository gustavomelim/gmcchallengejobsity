using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JobsityNetChallenge.Domain.Utils
{
    public static class CryptographyUtil
    {
        public static string Encrypt(string param) 
        {
            MD5 md5Hasher = MD5.Create();
            byte[] cryptBuffer = md5Hasher.ComputeHash(Encoding.Default.GetBytes(param));
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < cryptBuffer.Length; i++)
            {
                strBuilder.Append(cryptBuffer[i].ToString("x2"));
            }
            return strBuilder.ToString();
        }
    }
}
