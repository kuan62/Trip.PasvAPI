using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Trip.PasvAPI.AppCode
{
    public class TripAesCryptHelper
    {
        public static string Encrypt(string data, string key, string iv)
        {
            try
            {
                byte[] keyArray = Encoding.UTF8.GetBytes(key);
                byte[] ivArray = Encoding.UTF8.GetBytes(iv);
                byte[] toEncryptArray = Encoding.UTF8.GetBytes(data);
                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.IV = ivArray;
                rDel.Mode = CipherMode.CBC;
                rDel.Padding = PaddingMode.PKCS7;
                ICryptoTransform cTransform = rDel.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length); 
                var encode = encodeBytes(resultArray);
                return encode;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
     
        public static string Decrypt(string data, string key, string iv)
        {
            try
            {
                byte[] keyArray = Encoding.UTF8.GetBytes(key);
                byte[] ivArray = Encoding.UTF8.GetBytes(iv);
                byte[] toEncryptArray = decodeBytes(data);

                RijndaelManaged rDel = new RijndaelManaged();
                rDel.Key = keyArray;
                rDel.IV = ivArray;
                rDel.Mode = CipherMode.CBC;
                rDel.Padding = PaddingMode.PKCS7;

                ICryptoTransform cTransform = rDel.CreateDecryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Encoding.UTF8.GetString(resultArray);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string encodeBytes(byte[] bytes)
        {
            var strBuf = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                strBuf.Append((char)(((bytes[i] >> 4) & 0xF) + ((int)'a')));
                strBuf.Append((char)(((bytes[i]) & 0xF) + ((int)'a')));
            }
            return strBuf.ToString();
        }

        public static byte[] decodeBytes(String str)
        {
            byte[] bytes = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i += 2)
            {
                char c = str[i];
                bytes[i / 2] = (byte)((c - 'a') << 4);
                c = str[i + 1];
                bytes[i / 2] += (byte)(c - 'a');
            }
            return bytes;
        }
    }
}
