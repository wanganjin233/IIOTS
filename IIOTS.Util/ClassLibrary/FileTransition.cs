using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace IIOTS.Util
{
    public static class FileTransition
    { /// <summary>
      /// 将文件转化位byte数组
      /// </summary>
      /// <param name="Path">文件地址</param>
      /// <returns>转换为的byte数组</returns>
        public static byte[] FileBytes(string Path)
        {
            if (!File.Exists(Path))
            {
                return Array.Empty<byte>();
            }
            FileInfo fi = new FileInfo(Path);
            byte[] buff = new byte[fi.Length];
            FileStream fs = fi.OpenRead();
            fs.Read(buff, 0, Convert.ToInt32(fs.Length));
            fs.Close();
            return buff;
        }
        /// <summary>
        /// 将byte数组转换为文件并保存到指定地址
        /// </summary>
        /// <param name="buff">byte数组</param>
        /// <param name="savepath">保存地址</param>
        public static void BytesFile(byte[] buff, string savepath)
        {
            if (File.Exists(savepath))
            {
                File.Delete(savepath);
            }
            FileStream fs = new FileStream(savepath, FileMode.CreateNew);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(buff, 0, buff.Length);
            fs.Close();
        }
        /// <summary>
        /// 将指定目录压缩为Zip文件
        /// </summary>
        /// <param name="folderPath">文件夹地址 D:/1/ </param>
        /// <param name="zipPath">zip地址 D:/1.zip </param>
        public static void CompressDirectoryZip(string folderPath, string zipPath)
        {
            DirectoryInfo directoryInfo = new(zipPath);

            if (directoryInfo.Parent != null)
            {
                directoryInfo = directoryInfo.Parent;
            }

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            }

            ZipFile.CreateFromDirectory(folderPath, zipPath, CompressionLevel.Optimal, false);
        }
        /// <summary>
        /// 将指定文件压缩为Zip文件
        /// </summary>
        /// <param name="filePath">文件地址 D:/1.txt </param>
        /// <param name="zipPath">zip地址 D:/1.zip </param>
        public static void CompressFileZip(string filePath, string zipPath)
        {

            FileInfo fileInfo = new FileInfo(filePath);
            string dirPath = fileInfo.DirectoryName?.Replace("\\", "/") + "/";
            string tempPath = dirPath + Guid.NewGuid() + "_temp/";
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }
            fileInfo.CopyTo(tempPath + fileInfo.Name);
            CompressDirectoryZip(tempPath, zipPath);
            DirectoryInfo directory = new(tempPath);
            if (directory.Exists)
            {
                //将文件夹属性设置为普通,如：只读文件夹设置为普通
                directory.Attributes = FileAttributes.Normal;

                directory.Delete(true);
            }
        }
        /// <summary>
        /// 解压Zip文件到指定目录
        /// </summary>
        /// <param name="zipPath">zip地址 D:/1.zip</param>
        /// <param name="folderPath">文件夹地址 D:/1/</param>
        public static void DecompressZip(string zipPath, string folderPath)
        {
            DirectoryInfo directoryInfo = new(folderPath);

            if (!directoryInfo.Exists)
            {
                directoryInfo.Create();
            } 
            ZipFile.ExtractToDirectory(zipPath, folderPath,true);
        }


        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }
    }
}


