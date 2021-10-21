using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace MyRealFrame
{
    
    [Serializable]
    public class MD5Manager:Singleton<MD5Manager>
    {
        public void SaveMd5(string filePath, string md5SavePath=null)
        {
            string md5 = BuildFileMd5(filePath);
            string name = filePath + "_md5.dat";
            if (File.Exists(name))
            {
                File.Delete(name);
            }

            StreamWriter sw = new StreamWriter(name, false, Encoding.UTF8);
            if (sw != null)
            {
                sw.Write(md5);
                sw.Flush();
                sw.Close();
            }

        }



        public string GetMd5(string path)
        {
            string name = path + "_md5.dat";
            try
            {
                using (StreamReader sr = new StreamReader(name, Encoding.UTF8))
                {
                    string content = sr.ReadToEnd();
                    return content;
                }

            }
            catch
            {
                return "";
            }
        }
        
        public string BuildFileMd5(string filePath)
        {
            string filemd5 = null;
            try
            {
                using (var fs =  File.OpenRead(filePath))
                {
                    var md5 = MD5.Create();
                    var fileMD5Bytes = md5.ComputeHash(fs);
                    filemd5 = FormatMD5(fileMD5Bytes);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            return filemd5;
        }

        public string FormatMD5(Byte[] data)
        {
            return System.BitConverter.ToString(data).Replace("-", "").ToLower();//将byte[]装换成字符串
        }
    }
}