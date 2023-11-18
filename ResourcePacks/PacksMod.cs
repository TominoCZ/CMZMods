using DNA.CastleMinerZ;
using DNA.CastleMinerZ.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Modding;
using ResourcePacks.Properties;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ResourcePacks.Packs
{
    public class PackMod : ModBase
    {
        public PackManager Manager { get; private set; }

        private MyGuiHandler _handler = new MyGuiHandler();

        public PackMod(Game game) : base(game, "Resource Packs", "com.Morphox.ResourcePacks")
        {
            Manager = new PackManager((CastleMinerZGame)Game);
            GuiManager.AddHandler(_handler);

            Settings.Default.Reload();
        }

        protected override void LoadMain()
        {
            Manager.Init();

            if (!Manager.Set(Settings.Default.ResourcePack))
            {
                Settings.Default.ResourcePack = "Default";
                Settings.Default.Save();
            }
        }
    }
}
