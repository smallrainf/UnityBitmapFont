using UnityEditor;
using UnityEngine;

namespace FontEditor
{
    public class FontTextureData
    {
        public string path = "";
        public string text = "";
        public int charIndex; // Unicode字符索引
        public int x = 0;
        public int y = 0;
        public int width = 0;
        public int height = 0;
        public int offsetX = 0;
        public int offsetY = 0;
        public Texture2D tex;

        public FontTextureData(Texture2D tex)
        {
            path = AssetDatabase.GetAssetPath(tex);
            this.tex = tex;
            width = tex.width;
            height = tex.height;
            string texName = tex.name.ToLower();
            if (texName.IndexOf("space") >= 0)
            {
                text = " ";
            }
            else
            {
                text = tex.name;
            }
        }
    }
}
