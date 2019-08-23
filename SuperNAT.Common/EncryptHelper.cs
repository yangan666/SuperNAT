using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SuperNAT.Common
{
    public class EncryptHelper
    {
        public static string CreateGuid()
        {
            return Guid.NewGuid().ToString("N");
        }

        #region MD5加密

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="encryptString">待加密的密文</param>
        /// <returns></returns>
        public static string MD5Encrypt(string encryptString)
        {
            string returnValue;
            try
            {
                encryptString = "#*f-`" + encryptString + "6^)!mL";
                MD5 md5 = new MD5CryptoServiceProvider();
                returnValue = BitConverter.ToString(md5.ComputeHash(Encoding.Default.GetBytes(encryptString))).Replace("-", "");
                md5.Clear();
            }
            catch (Exception)
            {
                throw;
            }
            return returnValue;
        }

        public static string MD5(string encryptString)
        {
            string returnValue;
            try
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                returnValue = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(encryptString))).Replace("-", "").ToLower();
                md5.Clear();
            }
            catch (Exception)
            {
                throw;
            }
            return returnValue;
        }

        #endregion MD5加密

        #region DES加密解密

        /// <summary>
        /// DES加密
        /// </summary>
        /// <param name="encryptString">待加密的密文</param>
        /// <param name="encryptKey">密匙（8位）</param>
        /// <returns></returns>
        public static string DESEncrypt(string encryptString, string encryptKey)
        {
            string returnValue;
            try
            {
                byte[] temp = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
                DESCryptoServiceProvider dES = new DESCryptoServiceProvider();
                byte[] byteEncrypt = Encoding.Default.GetBytes(encryptString);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, dES.CreateEncryptor(Encoding.Default.GetBytes(encryptKey), temp), CryptoStreamMode.Write);
                cryptoStream.Write(byteEncrypt, 0, byteEncrypt.Length);
                cryptoStream.FlushFinalBlock();
                returnValue = Convert.ToBase64String(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// DES解密
        /// </summary>
        /// <param name="decryptString">密文</param>
        /// <param name="decryptKey">密匙（8位）</param>
        /// <returns></returns>
        public static string DESDecrypt(string decryptString, string decryptKey)
        {
            string returnValue;
            try
            {
                byte[] temp = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
                DESCryptoServiceProvider dES = new DESCryptoServiceProvider();
                byte[] byteDecryptString = Convert.FromBase64String(decryptString);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, dES.CreateDecryptor(Encoding.Default.GetBytes(decryptKey), temp), CryptoStreamMode.Write);

                cryptoStream.Write(byteDecryptString, 0, byteDecryptString.Length);

                cryptoStream.FlushFinalBlock();

                returnValue = Encoding.Default.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        #endregion DES加密解密

        #region RC2加密解密

        /// <summary>
        /// RC2加密
        /// </summary>
        /// <param name="encryptString">待加密的密文</param>
        /// <param name="encryptKey">密匙(必须为5-16位)</param>
        /// <returns></returns>
        public static string RC2Encrypt(string encryptString, string encryptKey)
        {
            string returnValue;
            try
            {
                byte[] temp = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
                RC2CryptoServiceProvider rC2 = new RC2CryptoServiceProvider();
                byte[] byteEncryptString = Encoding.Default.GetBytes(encryptString);
                MemoryStream memorystream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memorystream, rC2.CreateEncryptor(Encoding.Default.GetBytes(encryptKey), temp), CryptoStreamMode.Write);
                cryptoStream.Write(byteEncryptString, 0, byteEncryptString.Length);
                cryptoStream.FlushFinalBlock();
                returnValue = Convert.ToBase64String(memorystream.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// RC2解密
        /// </summary>
        /// <param name="decryptString">密文</param>
        /// <param name="decryptKey">密匙(必须为5-16位)</param>
        /// <returns></returns>
        public static string RC2Decrypt(string decryptString, string decryptKey)
        {
            string returnValue;
            try
            {
                byte[] temp = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
                RC2CryptoServiceProvider rC2 = new RC2CryptoServiceProvider();
                byte[] byteDecrytString = Convert.FromBase64String(decryptString);
                MemoryStream memoryStream = new MemoryStream();
                CryptoStream cryptoStream = new CryptoStream(memoryStream, rC2.CreateDecryptor(Encoding.Default.GetBytes(decryptKey), temp), CryptoStreamMode.Write);
                cryptoStream.Write(byteDecrytString, 0, byteDecrytString.Length);
                cryptoStream.FlushFinalBlock();
                returnValue = Encoding.Default.GetString(memoryStream.ToArray());
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        #endregion RC2加密解密

        #region 3DES加密解密

        /// <summary>
        /// 3DES 加密
        /// </summary>
        /// <param name="encryptString">待加密密文</param>
        /// <param name="encryptKey1">密匙1(长度必须为8位)</param>
        /// <param name="encryptKey2">密匙2(长度必须为8位)</param>
        /// <param name="encryptKey3">密匙3(长度必须为8位)</param>
        /// <returns></returns>
        public static string DES3Encrypt(string encryptString, string encryptKey1, string encryptKey2, string encryptKey3)
        {
            string returnValue;
            try
            {
                returnValue = DESEncrypt(encryptString, encryptKey3);
                returnValue = DESEncrypt(returnValue, encryptKey2);
                returnValue = DESEncrypt(returnValue, encryptKey1);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        /// <summary>
        /// 3DES 解密
        /// </summary>
        /// <param name="decryptString">待解密密文</param>
        /// <param name="decryptKey1">密匙1(长度必须为8位)</param>
        /// <param name="decryptKey2">密匙2(长度必须为8位)</param>
        /// <param name="decryptKey3">密匙3(长度必须为8位)</param>
        /// <returns></returns>
        public static string DES3Decrypt(string decryptString, string decryptKey1, string decryptKey2, string decryptKey3)
        {
            string returnValue;
            try
            {
                returnValue = DESDecrypt(decryptString, decryptKey1);
                returnValue = DESDecrypt(returnValue, decryptKey2);
                returnValue = DESDecrypt(returnValue, decryptKey3);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return returnValue;
        }

        #endregion 3DES加密解密

        #region AES加密解密

        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="encryptString">待加密的密文</param>
        /// <param name="encryptKey">加密密钥,要求为8位</param>
        /// <returns></returns>
        public static string AESEncrypt(string encryptString, string encryptKey)
        {
            MemoryStream mStream = new MemoryStream();
            RijndaelManaged aes = new RijndaelManaged();

            byte[] plainBytes = Encoding.UTF8.GetBytes(encryptString);
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(encryptKey.PadRight(bKey.Length)), bKey, bKey.Length);

            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            //aes.Key = _key;
            aes.Key = bKey;
            //aes.IV = _iV;
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateEncryptor(), CryptoStreamMode.Write);
            try
            {
                cryptoStream.Write(plainBytes, 0, plainBytes.Length);
                cryptoStream.FlushFinalBlock();
                return Convert.ToBase64String(mStream.ToArray());
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }

        /// <summary>
        ///AES 解密
        /// </summary>
        /// <param name="decryptString">待解密密文</param>
        /// <param name="decryptKey">解密密钥,要求为8位 </param>
        /// <returns></returns>
        public static string AESDecrypt(string decryptString, string decryptKey)
        {
            Byte[] encryptedBytes = Convert.FromBase64String(decryptString);
            Byte[] bKey = new Byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(decryptKey.PadRight(bKey.Length)), bKey, bKey.Length);

            MemoryStream mStream = new MemoryStream(encryptedBytes);
            //mStream.Write( encryptedBytes, 0, encryptedBytes.Length );
            //mStream.Seek( 0, SeekOrigin.Begin );
            RijndaelManaged aes = new RijndaelManaged();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.PKCS7;
            aes.KeySize = 128;
            aes.Key = bKey;
            //aes.IV = _iV;
            CryptoStream cryptoStream = new CryptoStream(mStream, aes.CreateDecryptor(), CryptoStreamMode.Read);
            try
            {
                byte[] tmp = new byte[encryptedBytes.Length + 32];
                int len = cryptoStream.Read(tmp, 0, encryptedBytes.Length + 32);
                byte[] ret = new byte[len];
                Array.Copy(tmp, 0, ret, 0, len);
                return Encoding.UTF8.GetString(ret);
            }
            finally
            {
                cryptoStream.Close();
                mStream.Close();
                aes.Clear();
            }
        }

        #endregion AES加密解密

        #region HMACSHA1算法
        public static string HMACSHA1Base64(string value, string keyStr)
        {
            Encoding encode = Encoding.UTF8;
            byte[] byteData = encode.GetBytes(value);
            byte[] byteKey = encode.GetBytes(keyStr);
            HMACSHA1 hmac = new HMACSHA1(byteKey);
            CryptoStream cs = new CryptoStream(Stream.Null, hmac, CryptoStreamMode.Write);
            cs.Write(byteData, 0, byteData.Length);
            cs.Close();
            return Convert.ToBase64String(hmac.Hash);
        }
        #endregion
    }
}