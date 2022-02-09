using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

namespace FontEditor
{
    public class ArtFontEditorWindow : EditorWindow
    {
        private enum PageIndex
        {
            File = 0,
            Setting = 1,
        }

        private int pageIndex = 0;
        private string[] titles = new string[] { "文件", "设置" };
        private List<FontTextureData> fontTextureDataList;
        private Vector2 texScrollPos = new Vector2();
        private int[] sizeArr = new int[] { 64, 128, 256, 512, 1024 };
        private string[] sizeStrArr = new string[] { "64", "128", "256", "512", "1024" };
        private int maxWidth = 256;
        private int maxHeight = 256;
        private string fontName = "";
        private Texture2D curAtlas;
        private string fontPath = "";
        private int spacing = 0;
        private int fontSpace = 0;
        private int curX = 0;
        private int curY = 0;
        private int MaxY = 0;

        [MenuItem("Tools/UI/美术字体编辑器", false, 40002)]
        [MenuItem("Assets/美术字体编辑器", false, 40002)]
        static void Init()
        {
            ArtFontEditorWindow THIS = (ArtFontEditorWindow)EditorWindow.GetWindow(typeof(ArtFontEditorWindow));
            THIS.titleContent = new GUIContent("美术字体编辑器");
        }

        void OnGUI()
        {
            pageIndex = GUILayout.SelectionGrid(pageIndex, titles, 5);
            switch ((PageIndex)pageIndex)
            {
                case PageIndex.File:
                    DrawImageFilePickerGUI();
                    break;
                case PageIndex.Setting:
                    DrawFontSettingGUI();
                    break;
            }
        }

        /// <summary>
        /// 绘制图片文件选择界面
        /// </summary>
        private void DrawImageFilePickerGUI()
        {
            if (GUILayout.Button("加载选中文件夹内所有图片"))
            {
                AddTextures();
            }

            if (fontTextureDataList != null)
            {
                texScrollPos = EditorGUILayout.BeginScrollView(texScrollPos, GUILayout.Width(300), GUILayout.Height(400));
                for (int i = 0; i < fontTextureDataList.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Box(fontTextureDataList[i].tex, GUILayout.Width(20), GUILayout.Height(20));
                    fontTextureDataList[i].text = GUILayout.TextField(fontTextureDataList[i].text);
                    if (GUILayout.Button("删除"))
                    {
                        RemoveTexture(fontTextureDataList[i].tex);
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndScrollView();
            }
        }

        /// <summary>
        /// 绘制自定义字体设置界面
        /// </summary>
        private void DrawFontSettingGUI()
        {
            GUILayout.Label("请输入字体名称");
            fontName = EditorGUILayout.TextField(fontName);
            GUILayout.Label("图集设置");
            GUILayout.Label("最大宽度");
            maxWidth = EditorGUILayout.IntPopup(maxWidth, sizeStrArr, sizeArr);
            GUILayout.Label("最大高度");
            maxHeight = EditorGUILayout.IntPopup(maxHeight, sizeStrArr, sizeArr);
            GUILayout.Label("图集间距");
            spacing = EditorGUILayout.IntField(spacing);
            GUILayout.Label("字体间距");
            fontSpace = EditorGUILayout.IntField(fontSpace);
            if (GUILayout.Button("保存"))
            {
                SaveArtFont();
            }
        }

        /// <summary>
        /// 添加图片
        /// </summary>
        private void AddTextures()
        {
            if (fontTextureDataList == null)
            {
                fontTextureDataList = new List<FontTextureData>();
            }

            LoadTextures();
        }

        /// <summary>
        /// 加载图片
        /// </summary>
        private void LoadTextures()
        {
            Object[] objxs = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);
            if (objxs.Length > 0)
            {
                fontName = objxs[0].name;
            }

            Object[] objs = Selection.GetFiltered(typeof(Texture2D), SelectionMode.DeepAssets);
            if (objs != null && objs.Length > 0)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    string path = AssetDatabase.GetAssetPath(objs[i]);
                    if (!string.IsNullOrEmpty(path))
                    {
                        TextureImporter ti = (TextureImporter)AssetImporter.GetAtPath(path);
                        if (ti != null)
                        {
                            // ti.textureFormat = TextureImporterFormat.AutomaticTruecolor;
                            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                            Add2TextureLib(tex);
                        }
                    }
                }
            }
            else
            {
                EditorUtility.DisplayDialog("提示", "请先选中有美术字图片的文件夹", "确定");
            }
        }

        private void Add2TextureLib(Texture2D tex)
        {
            if (tex == null)
            {
                return;
            }

            if (isTextureExist(tex))
            {
                return;
            }

            fontTextureDataList.Add(new FontTextureData(tex));
        }

        private bool isTextureExist(Texture2D tex)
        {
            bool isExist = false;
            for (int i = 0; i < fontTextureDataList.Count; i++)
            {
                if (fontTextureDataList[i].tex == tex)
                {
                    isExist = true;
                    break;
                }
            }

            return isExist;
        }

        /// <summary>
        /// 移除图片
        /// </summary>
        /// <param name="tex"></param>
        private void RemoveTexture(Texture2D tex)
        {
            int removeIndex = -1;
            for (int i = 0; i < fontTextureDataList.Count; i++)
            {
                if (fontTextureDataList[i].tex == tex)
                {
                    removeIndex = i;
                    break;
                }
            }

            if (removeIndex >= 0)
            {
                fontTextureDataList.RemoveAt(removeIndex);
            }
        }

        private void SaveArtFont()
        {
            if (fontTextureDataList == null || fontTextureDataList.Count == 0)
            {
                EditorUtility.DisplayDialog("提示", "请先选择美术字图片", "确定");
                return;
            }

            if (CheckSavePath())
            {
                if (EditorUtility.DisplayDialog("提示", "字体文件已存在，是否覆盖？", "确定", "取消") == false)
                {
                    return;
                }
            }

            FileManager.CheckDirection(fontPath);
            string r = CreateKeyCode();
            if (!string.IsNullOrEmpty(r))
            {
                EditorUtility.DisplayDialog("提示", "ASCII编码转换错误:" + r, "确定");
                return;
            }

            r = CreateAtlas();
            if (!string.IsNullOrEmpty(r))
            {
                EditorUtility.DisplayDialog("提示", "图集转换错误：" + r, "确定");
                return;
            }

            SavePNG2File();
            CreateArtFont();
            EditorUtility.DisplayDialog("提示", "创建字体成功，路径：" + fontPath, "确定");
        }

        private bool CheckSavePath()
        {
            fontPath = Application.dataPath + "/_OutputFont/" + fontName + "/";
            if (FileManager.IsDirectoryExists(fontPath))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private string CreateKeyCode()
        {
            string errorCode = "";
            List<int> checkList = new List<int>();

            if (fontTextureDataList == null)
            {
                return "没有美术字图片";
            }

            for (int i = 0; i < fontTextureDataList.Count; i++)
            {
                if (string.IsNullOrEmpty(fontTextureDataList[i].text))
                {
                    errorCode = "有些美术字没有输入对应的文字";
                    break;
                }

                try
                {
                    int charIndex = StringTools.String2Unicodes(fontTextureDataList[i].text)[0];
                    if (checkList.IndexOf(charIndex) < 0)
                    {
                        checkList.Add(charIndex);
                    }
                    else
                    {
                        errorCode = "有些美术字输入的对应文字重复";
                        break;
                    }

                    if (string.IsNullOrEmpty(errorCode))
                    {
                        fontTextureDataList[i].charIndex = charIndex;
                    }
                    else
                    {
                        break;
                    }
                }
                catch (System.Exception e)
                {
                    errorCode = e.Message;
                    break;
                }
            }

            return errorCode;
        }

        private string CreateAtlas()
        {
            InitAtlasData();
            string errorCode = "";
            for (int i = 0; i < fontTextureDataList.Count; i++)
            {
                if (CheckCanWriteAtlas(fontTextureDataList[i].tex) == false)
                {
                    errorCode = "设置的图集宽高不足以容纳所有字体，请调大一点";
                    break;
                }

                Write2Atlas(fontTextureDataList[i]);
            }

            return errorCode;
        }

        private void InitAtlasData()
        {
            curX = 0;
            curY = 0;
            MaxY = 0;
            curAtlas = CreateEmptyTexture(maxWidth, maxHeight);
        }

        private Texture2D CreateEmptyTexture(int w, int h)
        {
            Texture2D tex = new Texture2D(w, h);
            Color32[] cols = new Color32[w * h];
            int ind = 0;
            for (int i = 0; i < h; i++)
            {
                for (int j = 0; j < w; j++)
                {
                    cols[ind] = new Color32(0, 0, 0, 0);
                    ind++;
                }
            }

            tex.SetPixels32(cols);
            tex.Apply();
            return tex;
        }

        private bool CheckCanWriteAtlas(Texture2D tex)
        {
            if (curY + spacing + tex.height > maxHeight)
            {
                return false;
            }

            if (curX + spacing + tex.width > maxWidth)
            {
                curX = 0;
                curY = MaxY;
                return CheckCanWriteAtlas(tex);
            }

            return true;
        }

        private void Write2Atlas(FontTextureData data)
        {
            Texture2D tex = data.tex;
            data.x = curX + spacing;
            data.y = curY + spacing;
            data.width = tex.width;
            data.height = tex.height;
            data.offsetY = -1 * tex.height;
            List<Color32> cols = new List<Color32>();
            for (int i = 0; i < tex.height; i++)
            {
                for (int j = 0; j < tex.width; j++)
                {
                    cols.Add(tex.GetPixel(j, tex.height - i));
                }
            }

            int ind = 0;
            for (int i = curY + spacing; i < curY + spacing + tex.height; i++)
            {
                for (int j = curX + spacing; j < curX + spacing + tex.width; j++)
                {
                    curAtlas.SetPixel(j, maxHeight - i, cols[ind]);
                    ind++;
                }
            }

            //curAtlas.SetPixels32(curX + spacing, curY+spacing, tex.width, tex.height, cols);
            curX = curX + spacing + tex.width;
            MaxY = curY + spacing + tex.height;
            curAtlas.Apply();
        }

        /// <summary>
        /// 保存字体贴图
        /// </summary>
        private void SavePNG2File()
        {
            string pngPath = fontPath + fontName + ".png";
            byte[] bs = curAtlas.EncodeToPNG();
            FileManager.SaveBytes(pngPath, bs);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CreateArtFont()
        {
            string path = (string)fontPath.Clone();
            int ind = path.IndexOf("Assets");
            if (ind >= 0)
            {
                path = path.Substring(ind, path.Length - ind);
            }

            bool isCreateFont = false;
            string fontAssetPath = path + fontName + ".fontsettings";
            Font font = AssetDatabase.LoadAssetAtPath<Font>(fontAssetPath);
            if (font == null)
            {
                font = new Font();
                isCreateFont = true;
            }

            int characterNum = fontTextureDataList.Count;
            CharacterInfo[] cs = new CharacterInfo[characterNum];
            int csInd = 0;
            int lineHeight = 0;
            for (int i = 0; i < fontTextureDataList.Count; i++)
            {
                FontTextureData ftd = fontTextureDataList[i];
                CharacterInfo ci = GetCharacterInfo(ftd, ftd.charIndex);
                cs[csInd] = ci;
                if (ftd.height > lineHeight)
                {
                    lineHeight = ftd.height;
                }

                csInd++;
            }

            font.characterInfo = cs;
            font.name = fontName;

            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path + fontName + "mat.mat");
            if (mat == null)
            {
                mat = new Material(Shader.Find("UI/Default"));
                AssetDatabase.CreateAsset(mat, path + fontName + "mat.mat");
            }

            Texture tex = AssetDatabase.LoadAssetAtPath<Texture>(path + fontName + ".png");
            mat.SetTexture("_MainTex", tex);
            font.material = mat;
            if (isCreateFont == true)
            {
                AssetDatabase.CreateAsset(font, path + fontName + ".fontsettings");
            }
            else
            {
                EditorUtility.SetDirty(font);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private CharacterInfo GetCharacterInfo(FontTextureData ftd, int index)
        {
            CharacterInfo ci = new CharacterInfo();
            ci.index = index;
            ci.width = ftd.width;
            ci.vert = new Rect(ftd.offsetX, ftd.offsetY, ftd.width, ftd.height);
            float uvx = (float)ftd.x / (float)maxWidth;
            float uvy = 1 - (float)(ftd.y) / (float)maxHeight;
            float uvw = (float)ftd.width / (float)maxWidth;
            float uvh = -1 * (float)ftd.height / (float)maxHeight;
            ci.uv = new Rect(uvx, uvy, uvw, uvh);
            ci.advance = ftd.width + fontSpace;
            return ci;
        }
    }
}