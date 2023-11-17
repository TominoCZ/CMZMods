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
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace ResourcePacks
{
    public class TexturesTab : DNA.Drawing.UI.Controls.TabControl.TabPage
    {
        private CastleMinerZGame _game;
        private SpriteFont _controlsFont;
        private Rectangle prevScreenSize;
        private TextControl texturepackLabel;
        private ImageControl[] texturepackLogos;
        private CheckBoxControl[] texturepackCheckboxes;
        private TextControl[] texturepackNames;
        private FrameButtonControl[] texturepackButtons;
        private FrameButtonControl prevButton = new FrameButtonControl();
        private FrameButtonControl nextButton = new FrameButtonControl();
        private FrameButtonControl openFile = new FrameButtonControl();
        private int pageNum = 1;
        private static int totalPacks;
        private static PCDialogScreen confirmTexturePack;
        private static PCDialogScreen alreadySelected;
        public WaitScreen Loading = (WaitScreen)null;
        private TextControl packName;
        private TextControl packDesc;
        private TextControl packAuth;
        private TextControl packDate;
        private TextControl currentPage;
        private Pack[] prevValidPacks;

        static TexturesTab() => PackInfo.LoadDoc();

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
            int num = (int)Math.Ceiling((double)PackInfo.validPacks.Length / 5.0);
            this.currentPage.Text = "Page: " + this.pageNum.ToString() + "/" + num.ToString();
        }

        public override void OnSelected()
        {
            this.pageNum = 1;
            ScreenGroup group = this._game.CurrentNetworkSession == null ? this._game.FrontEnd._uiGroup : this._game.GameScreen._uiGroup;
            this.Loading = new WaitScreen("Loading Texture Packs...", false, new ThreadStart(PackInfo.LoadDoc), new ThreadStart(this.update));
            this.Loading._drawProgress = true;
            this.Loading.Start(group);
            this.update();
            base.OnSelected();
        }

        protected override void OnUpdate(DNAGame game, GameTime gameTime)
        {
            if (this.SelectedTab)
            {
                int index = 0;
                foreach (FrameButtonControl texturepackButton in this.texturepackButtons)
                {
                    if (texturepackButton.Visible && (bool)typeof(ButtonControl).GetProperty("Hovering", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object)texturepackButton))
                    {
                        Pack validPack = PackInfo.validPacks[index];
                        this.packName.Text = validPack.Name;
                        this.packAuth.Text = "Author: " + validPack.Author;
                        this.packDate.Text = "Updated: " + validPack.Date;
                        string description = validPack.Description;
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
                        this.packDesc.Text = str1;
                        this.packAuth.Text = this.CropString(this.packAuth, (int)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.X * 500).Text;
                        this.packName.Text = this.CropString(this.packName, (int)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.X * 350).Text;
                        if (validPack.Date != "Unknown")
                        {
                            if (!DateTime.TryParse(validPack.Date, out DateTime _))
                                this.packDate.Text = "Updated: Invalid";
                        }
                        else
                            this.packDate.Text = "Updated: Unknown";
                    }
                    ++index;
                }
            }
            if (this.prevScreenSize != DNA.Drawing.UI.Screen.Adjuster.ScreenRect)
            {
                int num3 = (int)(50.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y);
                Point point = new Point(0, (int)(75.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y));
                float y = DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y;
                int num4 = (int)(215.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y);
                int num5 = (int)(10.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y);
                this.prevScreenSize = DNA.Drawing.UI.Screen.Adjuster.ScreenRect;
                this.texturepackLabel.Scale = y * 1.5f;
                this.texturepackLabel.LocalPosition = new Point(point.X, point.Y);
                this.currentPage.Scale = y / 2f;
                this.currentPage.LocalPosition = new Point(point.X, point.Y + num3);
                point.Y += (int)((double)num3 * 1.32);
                this.packName.Scale = y * 1.5f;
                this.packName.LocalPosition = new Point((int)((double)point.X + (double)num4 * 2.2), point.Y);
                this.packAuth.Scale = y;
                this.packAuth.LocalPosition = new Point((int)((double)point.X + (double)num4 * 2.2), (int)((double)point.Y + (double)num3 / 1.15));
                this.packDate.Scale = y / 1.5f;
                this.packDate.LocalPosition = new Point((int)((double)point.X + (double)num4 * 2.2), (int)((double)point.Y + (double)num3 * 1.5));
                this.packDesc.Scale = y;
                this.packDesc.LocalPosition = new Point((int)((double)point.X + (double)num4 * 2.2), (int)((double)point.Y + (double)num3 * 2.25));
                this.update();
                this.prevButton.Scale = DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y;
                this.prevButton.Size = new Size(135, this._game._medFont.LineSpacing);
                this.prevButton.LocalPosition = new Point((int)(280.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y), this.Size.Height - (int)(40.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y));
                this.nextButton.Scale = DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y;
                this.nextButton.Size = new Size(135, this._game._medFont.LineSpacing);
                this.nextButton.LocalPosition = new Point((int)(420.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y), this.Size.Height - (int)(40.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y));
                this.openFile.Scale = DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y;
                this.openFile.Size = new Size(135, this._game._medFont.LineSpacing);
                this.openFile.LocalPosition = new Point((int)(140.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y), this.Size.Height - (int)(40.0 * (double)DNA.Drawing.UI.Screen.Adjuster.ScaleFactor.Y));
            }
            base.OnUpdate(game, gameTime);
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

        public void initData(bool first)
        {
            this._game = CastleMinerZGame.Instance;
            this._controlsFont = this._game._medFont;
            int length = PackInfo.validPacks.Length;
            int index1 = 0;
            if (!first)
            {
                foreach (Pack prevValidPack in this.prevValidPacks)
                {
                    this.Children.Remove((UIControl)this.texturepackButtons[index1]);
                    this.Children.Remove((UIControl)this.texturepackLogos[index1]);
                    this.Children.Remove((UIControl)this.texturepackNames[index1]);
                    this.Children.Remove((UIControl)this.texturepackCheckboxes[index1]);
                    ++index1;
                }
            }
            this.prevValidPacks = PackInfo.validPacks;
            this.texturepackLogos = new ImageControl[length];
            this.texturepackButtons = new FrameButtonControl[length];
            this.texturepackNames = new TextControl[length];
            this.texturepackCheckboxes = new CheckBoxControl[length];
            int num = -1;
            try
            {
                num = PackInfo.currentPack;
            }
            catch
            {
            }
            int index2 = 0;
            foreach (Pack validPack in PackInfo.validPacks)
            {
                this.texturepackButtons[index2] = new FrameButtonControl();
                this.texturepackButtons[index2].Size = new Size(200, this._controlsFont.LineSpacing + 2);
                this.texturepackButtons[index2].Visible = false;
                this.texturepackButtons[index2].Text = "";
                this.texturepackButtons[index2].Font = this._controlsFont;
                this.texturepackButtons[index2].Frame = this._game.ButtonFrame;
                this.texturepackButtons[index2].ButtonColor = new Color(78, 177, 61);//CMZColors.MenuGreen;
                this.Children.Add((UIControl)this.texturepackButtons[index2]);
                this.texturepackLogos[index2] = new ImageControl(validPack.Logo);
                this.texturepackLogos[index2].Visible = false;
                this.Children.Add((UIControl)this.texturepackLogos[index2]);
                this.texturepackNames[index2] = new TextControl(validPack.Name, this._controlsFont);
                this.texturepackNames[index2] = this.CropString(this.texturepackNames[index2], 275);
                this.texturepackNames[index2].Visible = false;
                this.Children.Add((UIControl)this.texturepackNames[index2]);
                this.texturepackCheckboxes[index2] = new CheckBoxControl(this._game._uiSprites["Unchecked"], this._game._uiSprites["Checked"]);
                this.texturepackCheckboxes[index2].Text = "";
                this.texturepackCheckboxes[index2].Enabled = false;
                this.texturepackCheckboxes[index2].TextColor = Color.White;
                this.texturepackCheckboxes[index2].Font = this._controlsFont;
                this.texturepackCheckboxes[index2].Checked = num == index2;
                this.texturepackCheckboxes[index2].Visible = false;
                this.Children.Add((UIControl)this.texturepackCheckboxes[index2]);
                ++index2;
            }
        }

        public TexturesTab()
          : base("Texture Packs")
        {
            this._game = CastleMinerZGame.Instance;
            this._controlsFont = this._game._medFont;
            this.texturepackLabel = new TextControl("Texture Packs", this._controlsFont);
            this.Children.Add((UIControl)this.texturepackLabel);
            this.packName = new TextControl("", this._controlsFont);
            this.Children.Add((UIControl)this.packName);
            this.packDate = new TextControl("", this._controlsFont);
            this.Children.Add((UIControl)this.packDate);
            this.packAuth = new TextControl("", this._controlsFont);
            this.Children.Add((UIControl)this.packAuth);
            this.packDesc = new TextControl("", this._controlsFont);
            this.Children.Add((UIControl)this.packDesc);
            this.currentPage = new TextControl("Page: 1/1", this._controlsFont);
            this.Children.Add((UIControl)this.currentPage);
            this.initData(true);
            this.prevButton.Text = "Previous";
            this.prevButton.Font = this._game._medFont;
            this.prevButton.Frame = this._game.ButtonFrame;
            this.prevButton.Pressed += new EventHandler(this._prevButton_Pressed);
            this.prevButton.ButtonColor = new Color(78, 177, 61);//CMZColors.MenuGreen;
            this.Children.Add((UIControl)this.prevButton);
            this.nextButton.Text = "Next";
            this.nextButton.Font = this._game._medFont;
            this.nextButton.Frame = this._game.ButtonFrame;
            this.nextButton.Pressed += new EventHandler(this._nextButton_Pressed);
            this.nextButton.ButtonColor = new Color(78, 177, 61);//CMZColors.MenuGreen;
            this.Children.Add((UIControl)this.nextButton);
            this.openFile.Text = "Packs Folder";
            this.openFile.Font = this._game._medFont;
            this.openFile.Frame = this._game.ButtonFrame;
            this.openFile.Pressed += new EventHandler(this._openFile_pressed);
            this.openFile.ButtonColor = new Color(78, 177, 61);//CMZColors.MenuGreen;
            this.Children.Add((UIControl)this.openFile);
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
            totalPacks = PackInfo.validPacks.Length;
            if (this.pageNum == 1)
            {
                this.prevButton.Visible = false;
                this.prevButton.Enabled = false;
            }
            else
            {
                this.prevButton.Visible = true;
                this.prevButton.Enabled = true;
            }
            if (this.pageNum * 5 < TexturesTab.totalPacks)
            {
                this.nextButton.Visible = true;
                this.nextButton.Enabled = true;
            }
            else
            {
                this.nextButton.Visible = false;
                this.nextButton.Enabled = false;
            }
            int index = 0;
            foreach (Pack validPack in PackInfo.validPacks)
            {
                if (index >= this.pageNum * 5 - 5 && index < this.pageNum * 5)
                {
                    this.texturepackNames[index].Scale = y;
                    this.texturepackNames[index].LocalPosition = new Point((int)((double)point.X + (double)(num1 * 2) * 1.4), (int)((double)point.Y + (double)num2 * 1.1));
                    this.texturepackNames[index].Visible = true;
                    this.texturepackButtons[index].Scale = y * 2f;
                    this.texturepackButtons[index].LocalPosition = new Point(point.X + num1, point.Y);
                    this.texturepackButtons[index].Visible = true;
                    this.texturepackButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed0);
                    this.texturepackButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed1);
                    this.texturepackButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed2);
                    this.texturepackButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed3);
                    this.texturepackButtons[index].Pressed -= new EventHandler(this.texturepackButton_Pressed4);
                    int num3 = index;
                    while (num3 > 4)
                        num3 -= 5;
                    switch (num3)
                    {
                        case 0:
                            this.texturepackButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed0);
                            break;
                        case 1:
                            this.texturepackButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed1);
                            break;
                        case 2:
                            this.texturepackButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed2);
                            break;
                        case 3:
                            this.texturepackButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed3);
                            break;
                        case 4:
                            this.texturepackButtons[index].Pressed += new EventHandler(this.texturepackButton_Pressed4);
                            break;
                    }
                    this.texturepackLogos[index].Size = new Size(this.texturepackButtons[index].LocalBounds.Height - 2, this.texturepackButtons[index].LocalBounds.Height - 2);
                    this.texturepackLogos[index].LocalPosition = new Point(point.X + num1 + 1, point.Y + 1);
                    this.texturepackLogos[index].Visible = true;
                    this.texturepackCheckboxes[index].Scale = y * 2f;
                    this.texturepackCheckboxes[index].LocalPosition = new Point(point.X - num1 / 5, point.Y);
                    this.texturepackCheckboxes[index].Visible = true;
                    point.Y += (int)((double)num1 * 1.65);
                }
                else
                {
                    this.texturepackLogos[index].Visible = false;
                    this.texturepackNames[index].Visible = false;
                    this.texturepackButtons[index].Visible = false;
                    this.texturepackCheckboxes[index].Visible = false;
                }
                ++index;
            }
        }

        private void resetInfo()
        {
            this.packName.Text = "";
            this.packAuth.Text = "";
            this.packDesc.Text = "";
            this.packDate.Text = "";
        }

        private void _nextButton_Pressed(object sender, EventArgs e)
        {
            if (this.pageNum * 5 < TexturesTab.totalPacks)
                ++this.pageNum;
            this.resetInfo();
            this.update();
        }

        private void _prevButton_Pressed(object sender, EventArgs e)
        {
            if (this.pageNum != 1)
                --this.pageNum;
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
            int current = (this.pageNum - 1) * 5 + locPressed;
            int num = -1;
            try
            {
                num = PackInfo.currentPack;
            }
            catch
            {
            }
            TexturesTab.confirmTexturePack = new PCDialogScreen(this.texturepackNames[current].Text, "Are you sure you want to load this texture pack? Doing so will restart your game. Your progress will be saved.", (string[])null, true, this._game.DialogScreenImage, this._game._myriadMed, false, this._game.ButtonFrame);
            TexturesTab.confirmTexturePack.UseDefaultValues();
            TexturesTab.alreadySelected = new PCDialogScreen("Pack Already Selected", "The " + this.texturepackNames[current].Text + " texture pack is already loaded. Are you sure you want to load this pack?", (string[])null, true, this._game.DialogScreenImage, this._game._myriadMed, false, this._game.ButtonFrame);
            TexturesTab.alreadySelected.UseDefaultValues();
            (this._game.CurrentNetworkSession == null ? this._game.FrontEnd._uiGroup : this._game.GameScreen._uiGroup).ShowPCDialogScreen(num == current ? TexturesTab.alreadySelected : TexturesTab.confirmTexturePack, (ThreadStart)(() =>
            {
                if (TexturesTab.confirmTexturePack.OptionSelected == -1)
                    return;
                if (this._game.CurrentNetworkSession != null)
                    this._game.EndGame(true);
                string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string dir = Path.Combine(folderPath, ".cmz/CMZTL");
                Directory.CreateDirectory(dir);
                File.WriteAllText(Path.Combine(dir, "CurrentPack.txt"), PackInfo.validPacks[current].Path);
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                FileStream fileStream = new FileStream(Path.Combine(dir, "Data.bin"), FileMode.Create);
                bool useSimpleShaders = PackInfo.validPacks[current].UseSimpleShaders;
                binaryFormatter.Serialize(fileStream, useSimpleShaders);
                fileStream.Close();
                Application.Restart();
            }));
        }
    }
}
