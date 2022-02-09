using System;

namespace FontEditor
{
    public class StringTools
    {
        /// <summary>
        /// 把字符串拆分再分别获取unicode码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int[] String2Unicodes(string str)
        {
            int[] res = new int[str.Length];
            for (int i = 0; i < str.Length; i++)
            {
                string subStr = str.Substring(i, 1);
                res[i] = String2Unicode(subStr);
            }

            return res;
        }

        /// <summary>
        /// 从字符串获取unicode码
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int String2Unicode(string str)
        {
            System.Text.UnicodeEncoding unicodeEncodeing = new System.Text.UnicodeEncoding();
            byte[] bs = unicodeEncodeing.GetBytes(str);
            int ts = 0;
            for (int i = 0; i < bs.Length; i++)
            {
                int sub = bs[i];
                int addSub = (int)Math.Pow(256, i) * sub;
                ts += addSub;
            }

            return ts;
        }
    }
}