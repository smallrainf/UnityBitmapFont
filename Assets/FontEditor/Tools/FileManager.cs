using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FontEditor
{
    public class FileManager
    {
        /// <summary>
        /// 检查某文件夹路径是否存在，如不存在，创建
        /// </summary>
        /// <param name="path"></param>
        public static void CheckDirection(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// 单纯检查某个文件夹路径是否存在
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsDirectoryExists(string path)
        {
            if (Directory.Exists(path))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        public static void SaveFile(string path, string content, bool needUtf8 = false)
        {
            CheckFileSavePath(path);
            if (needUtf8)
            {
                UTF8Encoding code = new UTF8Encoding(false);
                File.WriteAllText(path, content, code);
            }
            else
            {
                File.WriteAllText(path, content, Encoding.Default);
            }
        }

        /// <summary>
        /// 保存bytes
        /// </summary>
        /// <param name="path"></param>
        /// <param name="bytes"></param>
        public static void SaveBytes(string path, byte[] bytes)
        {
            CheckFileSavePath(path);
            File.WriteAllBytes(path, bytes);
        }

        public static void CheckFileSavePath(string path)
        {
            string realPath = path;
            int ind = path.LastIndexOf("/");
            if (ind >= 0)
            {
                realPath = path.Substring(0, ind);
            }
            else
            {
                ind = path.LastIndexOf("\\");
                if (ind >= 0)
                {
                    realPath = path.Substring(0, ind);
                }
            }

            CheckDirection(realPath);
        }
    }
}