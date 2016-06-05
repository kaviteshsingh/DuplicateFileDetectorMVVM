using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace DuplicateFileDetectorMVVM.Hashing
{
    class MD5Hash
    {
        public static string GetMD5HashFromFile(string fileName)
        {
            using(var md5 = MD5.Create())
            {
                try
                {
                    using(var stream = File.OpenRead(fileName))
                    {
                        byte[] HashArray;
                        StringBuilder sb = new StringBuilder(256);
                        int i = 0;
                        sb.Append("0x");

                        HashArray = md5.ComputeHash(stream);

                        for(i = 0; i < HashArray.Length; i++)
                        {
                            sb = sb.Append(String.Format("{0:X2}", HashArray[i]));
                        }
                        return sb.ToString();
                    }
                }
                catch(Exception)
                {
                    return null;
                }
            }
        }
    }
}
