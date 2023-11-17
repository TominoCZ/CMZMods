// Decompiled with JetBrains decompiler
// Type: DNA.CastleMinerZ.UI.PackInfo
// Assembly: CastleMinerZ, Version=1.9.8.0, Culture=neutral, PublicKeyToken=null
// MVID: BC9414ED-22F4-4D68-BF63-CB3255ED4BF4
// Assembly location: C:\Users\tom\Downloads\CMZTL_v1.0.0-beta\CastleMinerZ.exe

using DNA.CastleMinerZ;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.Drawing;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace ResourcePacks
{
    public class PackInfo
    {
        public static Pack[] validPacks = new Pack[0];
        private static int toLoad;
        public static int currentPack;
        private static Dictionary<string, Sprite> _cache = new Dictionary<string, Sprite>();

        private static TexturesTab _texturesTab;

        public static void Init(OptionsScreen optionsScreen)
        {
            if (_texturesTab != null)
                return;

            _texturesTab = new TexturesTab();

            var tabControl = (TabControl)typeof(OptionsScreen).GetField("tabControl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.FlattenHierarchy).GetValue(optionsScreen);
            tabControl.Tabs.Add(_texturesTab);
        }

        public static void LoadDoc()
        {
            Directory.CreateDirectory("./Content/Textures/TexturePacks");

            var folders = Directory.GetDirectories("./Content/Textures/TexturePacks");
            var dirInfos = folders.Select(d => new DirectoryInfo(d));

            var packList = new List<Pack>();

            int loaded = 0;

            toLoad = dirInfos.Count();

            packList.Add(new Pack("Vanilla", "DigitalDNAGames", "11/9/2011", "The vanilla textures of CMZ 1.9.8.0", false, new Sprite(CastleMinerZGame.Instance._terrain._diffuseAlpha, new Rectangle(1024, 0, 256, 256)), new Sprite(CastleMinerZGame.Instance._terrain._diffuseAlpha, new Rectangle(0, 0, 2048, 2048)), ""));
            foreach (DirectoryInfo dir in dirInfos)
            {
                //if (directory.FullName == BlockTerrain.fullPath)
                //PackInfo.currentPack = num2 + 1;
                string str = "";
                var file = Path.Combine(dir.FullName, "PackInfo.xml");
                XmlTextReader xmlTextReader = new XmlTextReader(file);
                string name = "Unknown";
                string author = "Unknown";
                string date = "Unknown";
                string desc = "PackInfo.xml is invalid.";
                str = "null";
                bool shaders = true;
                bool flag = false;
                if (File.Exists(file))
                {
                    while (xmlTextReader.Read())
                    {
                        if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.Name == "PackName")
                            name = xmlTextReader.ReadElementContentAsString().Trim();
                        else if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.Name == "PackAuthor")
                            author = xmlTextReader.ReadElementContentAsString().Trim();
                        else if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.Name == "PackDate")
                            date = xmlTextReader.ReadElementContentAsString().Trim();
                        else if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.Name == "PackDesc")
                        {
                            desc = xmlTextReader.ReadElementContentAsString().Trim();
                            flag = true;
                        }
                        else if (xmlTextReader.NodeType == XmlNodeType.Element && xmlTextReader.Name == "UseSimpleShaders")
                            shaders = xmlTextReader.ReadElementContentAsBoolean();
                    }
                    xmlTextReader.Close();
                }
                else
                    desc = "PackInfo.xml was not found. Simple shaders is enabled.";
                if (flag == false)
                    desc = "No description found.";
                if (name.Length > 40)
                    name = name.Substring(0, 40);
                Sprite textures = checkTexture(dir, "Textures.png", 1, 2048, 2048);
                if (textures != null)
                {
                    Sprite logo = checkTexture(dir, "Icon.png", 1, 256, 256) ?? new Sprite(CastleMinerZGame.Instance._terrain._diffuseAlpha, new Rectangle(512, 1280, 256, 256));
                    string fullName = dir.FullName;
                    ++loaded;

                    if (CastleMinerZGame.Instance.FrontEnd != null && _texturesTab.Loading != null)
                        _texturesTab.Loading.Progress = (int)(loaded / (double)toLoad * 100);

                    packList.Add(new Pack(name, author, date, desc, shaders, logo, textures, fullName));
                }
            }

            validPacks = packList.ToArray();

            Console.WriteLine(packList.Count + " packs found");
        }

        public static Sprite checkTexture(
          DirectoryInfo directory,
          string search,
          int time,
          int sizeX,
          int sizeY)
        {
            if (time > 5)
                return null;

            try
            {
                var key = Path.Combine(directory.FullName, search);
                if (!File.Exists(key))
                    return null;
                
                if (_cache.TryGetValue(key, out var sprite))
                    return sprite;

                using (var ms = new MemoryStream(File.ReadAllBytes(key)))
                {
                    var tex = Texture2D.FromStream(CastleMinerZGame.Instance.GraphicsDevice, ms);
                    var s = new Sprite(tex, new Rectangle(0, 0, sizeX, sizeY));

                    _cache.Add(key, s);

                    return s;
                }
                /*
                using (var bmp = System.Drawing.Image.FromFile(key))
                {
                    using (var scaled = new System.Drawing.Bitmap(bmp, sizeX, sizeY))
                    {
                        var tex = new Texture2D(, sizeX, sizeY, false, SurfaceFormat.Color);
                        tex.SetData(scaled.ToColors());

                        var s = new Sprite(tex, new Rectangle(0, 0, sizeX, sizeY));

                        _cache.Add(key, s);

                        return s;
                    }
                }*/
            }
            catch (OutOfMemoryException ex)
            {
                return PackInfo.checkTexture(directory, search, time + 1, sizeX, sizeY);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ResourcePacks] Failed to load texture:\n" + ex);

                return null;
            }
        }
    }
}
