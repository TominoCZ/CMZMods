// Decompiled with JetBrains decompiler
// Type: DNA.CastleMinerZ.UI.TexturesTab
// Assembly: CastleMinerZ, Version=1.9.8.0, Culture=neutral, PublicKeyToken=null
// MVID: BC9414ED-22F4-4D68-BF63-CB3255ED4BF4
// Assembly location: C:\Users\tom\Downloads\CMZTL_v1.0.0-beta\CastleMinerZ.exe

using DNA;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.UI;
using DNA.Drawing;
using DNA.Drawing.UI;
using DNA.Drawing.UI.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Modding;
using ResourcePacks.Packs;
using ResourcePacks.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using TabControl = DNA.Drawing.UI.Controls.TabControl;

namespace ResourcePacks.Gui
{
    public class MenuTab : TabControl.TabPage
    {
        private CastleMinerZGame _game;
        private TextControl _title;
        private SpriteFont _controlsFont;
        private Rectangle _screenLast;
        //private ImageControl[] texturepackLogos;
        private FrameButtonControl[] _packButtons = new FrameButtonControl[0];
        private CheckBoxControl[] _checkboxes = new CheckBoxControl[0];
        private TextControl[] _packNames = new TextControl[0];

        private FrameButtonControl _btnLast = new FrameButtonControl();
        private FrameButtonControl _btnNext = new FrameButtonControl();
        private FrameButtonControl _showDir = new FrameButtonControl();
        private int _pageIndex = 1;
        //private static int totalPacks;
        //private static PCDialogScreen confirmTexturePack;
        //private static PCDialogScreen alreadySelected;
        //public WaitScreen Loading = null;
        private TextControl _packName;
        private TextControl _packAuthor;
        private TextControl _packDesc;
        //private TextControl packDate;
        private TextControl _page;

        private ResourcePack[] _packs = new ResourcePack[0];
        private ResourcePack[] _prevValidPacks = new ResourcePack[0];

        public MenuTab()
          : base("Resource Packs")
        {
            this._game = CastleMinerZGame.Instance;
            this._controlsFont = this._game._medFont;
            this._title = new TextControl("Resource Packs", this._controlsFont);
            this._packName = new TextControl("", this._controlsFont);
            this._packAuthor = new TextControl("", this._controlsFont);
            this._packDesc = new TextControl("", this._controlsFont);
            this._page = new TextControl("Page: 1/1", this._controlsFont);

            this.Children.Add((UIControl)this._title);
            this.Children.Add((UIControl)this._packName);
            //this.packDate = new TextControl("", this._controlsFont);
            //this.Children.Add((UIControl)this.packDate);
            this.Children.Add((UIControl)this._packAuthor);
            this.Children.Add((UIControl)this._packDesc);
            this.Children.Add((UIControl)this._page);
            this.Children.Add((UIControl)this._btnLast);
            this.Children.Add((UIControl)this._btnNext);
            this.Children.Add((UIControl)this._showDir);

            this._btnLast.Text = "Previous";
            this._btnLast.Font = this._game._medFont;
            this._btnLast.Frame = this._game.ButtonFrame;
            this._btnLast.Pressed += new EventHandler(this._prevButton_Pressed);
            this._btnLast.ButtonColor = Color.Green;
            this._btnNext.Text = "Next";
            this._btnNext.Font = this._game._medFont;
            this._btnNext.Frame = this._game.ButtonFrame;
            this._btnNext.Pressed += new EventHandler(this._nextButton_Pressed);
            this._btnNext.ButtonColor = Color.Green;
            this._showDir.Text = "Browse";
            this._showDir.Font = this._game._medFont;
            this._showDir.Frame = this._game.ButtonFrame;
            this._showDir.Pressed += new EventHandler(this._openFile_pressed);
            this._showDir.ButtonColor = Color.Green;

            this.initData(true);
        }

        public override void OnLostFocus()
        {
            try
            {
                this._game.SavePlayerStats(this._game.PlayerStats);
                CastleMinerZGame.GlobalSettings.Save();
            }
            catch
            {
            }
            base.OnLostFocus();
        }

        public void UpdatePageText()
        {
            int num = (int)Math.Ceiling(_packs.Length / 5.0);
            this._page.Text = "Page: " + this._pageIndex.ToString() + "/" + num.ToString();
        }

        public override void OnSelected()
        {
            this._pageIndex = 1;
            //ScreenGroup group = this._game.CurrentNetworkSession == null ? this._game.FrontEnd._uiGroup : this._game.GameScreen._uiGroup;
            var packs = new List<ResourcePack>();
            foreach (var pack in PackMod.Instance.Manager.Packs.Values)
                packs.Add(pack);
            packs.OrderBy(p => p.Name);

            _packs = packs.ToArray();
            /*
            this.Loading = new WaitScreen("Loading Texture Packs...", false, new ThreadStart(() =>
            {

                //TODO: Load pack.json data
            }), new ThreadStart(this.update));
            this.Loading._drawProgress = true;
            this.Loading.Start(group);*/
            this.update();
            base.OnSelected();
        }

        protected override void OnUpdate(DNAGame game, GameTime gameTime)
        {
            if (this.SelectedTab)
            {
                try
                {
                    UpdateInfo();

                    int index = 0;
                    foreach (var btn in this._packButtons)
                    {
                        if (!btn.Visible || !btn.GetValue<bool>("Hovering"))
                            continue;
                        if (index >= _packs.Length)
                            break;
                        var pack = _packs[index];
                        this._packName.Text = pack.Name;
                        this._packAuthor.Text = "Author: " + pack.Author;
                        //this.packDate.Text = "Updated: " + pack.Date;
                        string description = pack.Description;
                        int num1 = (int)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.X * 500;
                        string text = "";
                        List<string> stringList = new List<string>();
                        foreach (char ch in description)
                        {
                            text += ch.ToString();
                            if ((double)this._controlsFont.MeasureString(text).X > (double)num1 && ch == ' ')
                            {
                                stringList.Add(text + "\n");
                                text = "";
                            }
                        }
                        stringList.Add(text);
                        string str1 = "";
                        int num2 = 1;
                        foreach (string str2 in stringList)
                        {
                            if (num2 == 5)
                            {
                                str1 = str1.Substring(0, str1.Length - 2);
                                str1 += "...";
                                break;
                            }
                            str1 += str2;
                            ++num2;
                        }
                        this._packDesc.Text = str1;
                        this._packAuthor.Text = this.CropString(this._packAuthor, (int)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.X * 500).Text;
                        this._packName.Text = this.CropString(this._packName, (int)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.X * 350).Text;

                        /*
                        if (pack.Date != "Unknown")
                        {
                            if (!DateTime.TryParse(pack.Date, out DateTime _))
                                this.packDate.Text = "Updated: Invalid";
                        }
                        else
                            this.packDate.Text = "Updated: Unknown";*/
                        ++index;
                    }
                }
                catch (Exception e)
                {
                    PackMod.Instance.Log("ERROR:\n" + e.ToString(), LogType.Error);
                }
            }
            if (this._screenLast != DNA.Drawing.UI.Screen.Adjuster.ScreenRect)
            {
                int num3 = (int)(50.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y);
                Point point = new Point(0, (int)(75.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y));
                float y = DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y;
                int num4 = (int)(215.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y);
                int num5 = (int)(10.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y);
                this._screenLast = DNA.Drawing.UI.Screen.Adjuster.ScreenRect;
                this._title.Scale = y * 1.5f;
                this._title.LocalPosition = new Point(point.X, point.Y);
                this._page.Scale = y / 2f;
                this._page.LocalPosition = new Point(point.X, point.Y + num3);
                point.Y += (int)((double)num3 * 1.32);
                this._packName.Scale = y * 1.5f;
                this._packName.LocalPosition = new Point((int)((double)point.X + (double)num4 * 2.2), point.Y);
                this._packAuthor.Scale = y;
                this._packAuthor.LocalPosition = new Point((int)((double)point.X + (double)num4 * 2.2), (int)((double)point.Y + (double)num3 / 1.15));
                //this.packDate.Scale = y / 1.5f;
                //this.packDate.LocalPosition = new Point((int)((double)point.X + (double)num4 * 2.2), (int)((double)point.Y + (double)num3 * 1.5));
                this._packDesc.Scale = y;
                this._packDesc.LocalPosition = new Point((int)((double)point.X + (double)num4 * 2.2), (int)((double)point.Y + (double)num3 * 1.5));// * 2.25));
                this.update();
                this._btnLast.Scale = DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y;
                this._btnLast.Size = new Size(135, this._game._medFont.LineSpacing);
                this._btnLast.LocalPosition = new Point((int)(280.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y), this.Size.Height - (int)(40.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y));
                this._btnNext.Scale = DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y;
                this._btnNext.Size = new Size(135, this._game._medFont.LineSpacing);
                this._btnNext.LocalPosition = new Point((int)(420.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y), this.Size.Height - (int)(40.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y));
                this._showDir.Scale = Screen.Adjuster.ScaleFactor.Y;
                //this.openFile.Size = new Size(135, _game._medFont.LineSpacing
                this._showDir.Size = new Size(225, this._game._medFont.LineSpacing);
                this._showDir.LocalPosition = new Point((int)(150 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y), Screen.Adjuster.ScreenRect.Bottom - (int)(40.0 * (double)Screen.Adjuster.ScaleFactor.Y));
            }
            base.OnUpdate(game, gameTime);
        }

        public void initData(bool first)
        {
            this._game = CastleMinerZGame.Instance;
            this._controlsFont = this._game._medFont;
            int length = _packs.Length;
            if (length == 0)
                return;

            int index1 = 0;
            if (!first)
            {
                foreach (var pack in this._prevValidPacks)
                {
                    this.Children.Remove((UIControl)this._packButtons[index1]);
                    //this.Children.Remove((UIControl)this.texturepackLogos[index1]);
                    this.Children.Remove((UIControl)this._packNames[index1]);
                    this.Children.Remove((UIControl)this._checkboxes[index1]);
                    ++index1;
                }
            }
            this._prevValidPacks = _packs;
            //this.texturepackLogos = new ImageControl[length];
            this._packButtons = new FrameButtonControl[length];
            this._packNames = new TextControl[length];
            this._checkboxes = new CheckBoxControl[length];
            int num = Array.IndexOf(_packs, PackMod.Instance.Manager.Active);
            for (int index2 = 0; index2 < _packs.Length; index2++)
            {
                ResourcePack pack = _packs[index2];
                this._packButtons[index2] = new FrameButtonControl();
                this._packButtons[index2].Size = new Size(200, this._controlsFont.LineSpacing + 2);
                this._packButtons[index2].Visible = false;
                this._packButtons[index2].Text = "";
                this._packButtons[index2].Font = this._controlsFont;
                this._packButtons[index2].Frame = this._game.ButtonFrame;
                this._packButtons[index2].ButtonColor = Color.Green;
                this.Children.Add((UIControl)this._packButtons[index2]);
                //this.texturepackLogos[index2] = new ImageControl(pack.Logo);
                //this.texturepackLogos[index2].Visible = false;
                //this.Children.Add((UIControl)this.texturepackLogos[index2]);
                this._packNames[index2] = new TextControl(pack.Name, this._controlsFont);
                this._packNames[index2] = this.CropString(this._packNames[index2], 275);
                this._packNames[index2].Visible = false;
                this.Children.Add((UIControl)this._packNames[index2]);
                this._checkboxes[index2] = new CheckBoxControl(this._game._uiSprites["Unchecked"], this._game._uiSprites["Checked"]);
                this._checkboxes[index2].Text = "";
                this._checkboxes[index2].Enabled = false;
                this._checkboxes[index2].TextColor = Color.White;
                this._checkboxes[index2].Font = this._controlsFont;
                this._checkboxes[index2].Checked = num == index2;
                this._checkboxes[index2].Visible = false;
                this.Children.Add((UIControl)this._checkboxes[index2]);
            }
        }

        public void update()
        {
            this.UpdatePageText();
            this.initData(false);
            int num1 = (int)(50.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y);
            Point point = new Point(0, (int)(75.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y));
            float y = DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y;
            int num2 = (int)(10.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y);
            point.Y += (int)((double)num1 * 1.5);
            //totalPacks = _packs.Length
            if (this._pageIndex == 1)
            {
                this._btnLast.Visible = false;
                this._btnLast.Enabled = false;
            }
            else
            {
                this._btnLast.Visible = true;
                this._btnLast.Enabled = true;
            }
            if (this._pageIndex * 5 < _packs.Length)
            {
                this._btnNext.Visible = true;
                this._btnNext.Enabled = true;
            }
            else
            {
                this._btnNext.Visible = false;
                this._btnNext.Enabled = false;
            }
            int index = 0;
            foreach (var pack in _packs)
            {
                if (index >= this._pageIndex * 5 - 5 && index < this._pageIndex * 5)
                {
                    this._packNames[index].Scale = y;
                    this._packNames[index].LocalPosition = new Point((int)((double)point.X + (double)(num1 * 2) * 1.4), (int)((double)point.Y + (double)num2 * 1.1));
                    this._packNames[index].Visible = true;
                    this._packButtons[index].Scale = y * 2f;
                    this._packButtons[index].LocalPosition = new Point(point.X + num1, point.Y);
                    this._packButtons[index].Visible = true;
                    this._packButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed0);
                    this._packButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed1);
                    this._packButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed2);
                    this._packButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed3);
                    this._packButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed4);

                    switch (index % 5)
                    {
                        case 0:
                            this._packButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed0);
                            break;
                        case 1:
                            this._packButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed1);
                            break;
                        case 2:
                            this._packButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed2);
                            break;
                        case 3:
                            this._packButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed3);
                            break;
                        case 4:
                            this._packButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed4);
                            break;
                    }
                    //this.texturepackLogos[index].Size = new Size(this.texturepackButtons[index].LocalBounds.Height - 2, this.texturepackButtons[index].LocalBounds.Height - 2);
                    //this.texturepackLogos[index].LocalPosition = new Point(point.X + num1 + 1, point.Y + 1);
                    //this.texturepackLogos[index].Visible = true;
                    this._checkboxes[index].Scale = y * 2f;
                    this._checkboxes[index].LocalPosition = new Point(point.X - num1 / 5, point.Y);
                    this._checkboxes[index].Visible = true;
                    point.Y += (int)((double)num1 * 1.65);
                }
                else
                {
                    //this.texturepackLogos[index].Visible = false;
                    this._packNames[index].Visible = false;
                    this._packButtons[index].Visible = false;
                    this._checkboxes[index].Visible = false;
                }
                ++index;
            }
        }

        public TextControl CropString(TextControl control, int cropSize)
        {
            if ((double)this._controlsFont.MeasureString(control.Text).X > (double)cropSize)
            {
                while ((double)this._controlsFont.MeasureString(control.Text).X > (double)cropSize)
                    control = new TextControl(control.Text.Substring(0, control.Text.Length - 1), this._controlsFont);
                control.Text += " ...";
            }
            return control;
        }

        private void resetInfo()
        {
            this._packName.Text = "";
            this._packAuthor.Text = "";
            this._packDesc.Text = "";
            //this.packDate.Text = "";
        }

        private void _nextButton_Pressed(object sender, EventArgs e)
        {
            if (this._pageIndex * 5 < _packs.Length)
                ++this._pageIndex;
            this.resetInfo();
            this.update();
        }

        private void _prevButton_Pressed(object sender, EventArgs e)
        {
            if (this._pageIndex != 1)
                --this._pageIndex;
            this.resetInfo();
            this.update();
        }

        private void _openFile_pressed(object sender, EventArgs e) => Process.Start(Assembly.GetEntryAssembly().Location.Substring(0, Assembly.GetEntryAssembly().Location.Length - 16) + "Content\\Textures\\TexturePacks\\");

        public void texturepackButton_Pressed0(object sender, EventArgs e) => this.texturePressed(0);

        public void texturepackButton_Pressed1(object sender, EventArgs e) => this.texturePressed(1);

        public void texturepackButton_Pressed2(object sender, EventArgs e) => this.texturePressed(2);

        public void texturepackButton_Pressed3(object sender, EventArgs e) => this.texturePressed(3);

        public void texturepackButton_Pressed4(object sender, EventArgs e) => this.texturePressed(4);

        private void texturePressed(int locPressed)
        {
            int current = (this._pageIndex - 1) * 5 + locPressed;
            var pack = _packs[current];
            //int num = Array.IndexOf(_packs, ((MyMod)ModBase.Instance).Manager.Active);
            //confirmTexturePack = new PCDialogScreen(this.texturepackNames[current].Text, "Are you sure you want to load this texture pack? Doing so will restart your game. Your progress will be saved.", (string[])null, true, this._game.DialogScreenImage, this._game._myriadMed, false, this._game.ButtonFrame);
            //confirmTexturePack.UseDefaultValues();
            //alreadySelected = new PCDialogScreen("Pack Already Selected", "The " + this.texturepackNames[current].Text + " texture pack is already loaded. Are you sure you want to load this pack?", (string[])null, true, this._game.DialogScreenImage, this._game._myriadMed, false, this._game.ButtonFrame);
            //alreadySelected.UseDefaultValues();

            //var uiGroup = this._game.CurrentNetworkSession == null ? this._game.FrontEnd._uiGroup : this._game.GameScreen._uiGroup;

            //uiGroup.ShowPCDialogScreen(num == current ? alreadySelected : confirmTexturePack, (ThreadStart)(() =>
            //{
            if (PackMod.Instance.Manager.Set(pack))
            {
                Settings.Default.ResourcePack = pack.Name;
                Settings.Default.Save();
            }

            UpdateInfo();

            //if (confirmTexturePack.OptionSelected == -1)
            // return;
            //if (this._game.CurrentNetworkSession != null)
            //this._game.EndGame(true);
            //string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //Directory.CreateDirectory(folderPath + "\\.cmz\\CMZTL");
            //File.WriteAllText(folderPath + "\\.cmz\\CMZTL\\CurrentPack.txt", PackInfo.validPacks[current].Path);
            //BinaryFormatter binaryFormatter = new BinaryFormatter();
            //FileStream fileStream = new FileStream(folderPath + "\\.cmz\\CMZTL\\Data.bin", FileMode.Create);
            //bool useSimpleShaders = _packs[current].UseSimpleShaders;
            //binaryFormatter.Serialize((Stream)fileStream, (object)useSimpleShaders);
            //fileStream.Close();
            //Application.Restart();
            //}));
        }

        private void UpdateInfo()
        {
            int current = Array.IndexOf(_packs, PackMod.Instance.Manager.Active);

            for (int i = 0; i < _checkboxes.Length && i < _packs.Length; i++)
            {
                _checkboxes[i].Checked = i == current;
            }
        }
    }
}
