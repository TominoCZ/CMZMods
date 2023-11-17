// Decompiled with JetBrains decompiler
// Type: DNA.CastleMinerZ.UI.Pack
// Assembly: CastleMinerZ, Version=1.9.8.0, Culture=neutral, PublicKeyToken=null
// MVID: BC9414ED-22F4-4D68-BF63-CB3255ED4BF4
// Assembly location: C:\Users\tom\Downloads\CMZTL_v1.0.0-beta\CastleMinerZ.exe

using DNA.Drawing;

namespace ResourcePacks
{
  public class Pack
  {
    public string Name;
    public string Author;
    public string Date;
    public string Description;
    public bool UseSimpleShaders;
    public Sprite Logo;
    public Sprite Textures;
    public string Path;

    public Pack(
      string name,
      string author,
      string date,
      string desc,
      bool shaders,
      Sprite logo,
      Sprite textures,
      string path)
    {
      this.Name = name;
      this.Author = author;
      this.Date = date;
      this.Description = desc;
      this.UseSimpleShaders = shaders;
      this.Logo = logo;
      this.Textures = textures;
      this.Path = path;
    }
  }
}
