using DNA.CastleMinerZ;
using DNA.CastleMinerZ.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Modding;
using ResourcePacks.Gui;
using ResourcePacks.Properties;
using System.Collections.Generic;

namespace ResourcePacks.Packs
{
    public class PackMod : ModBase<PackMod, CastleMinerZGame>
    {
        public PackManager Manager { get; private set; }

        private MyGuiHandler _handler = new MyGuiHandler();

        static bool keyDown = false;
        static Queue<string> packsQueue = new Queue<string>();

        public PackMod(CastleMinerZGame game) : base(game, "Resource Packs", "com.Morphox.ResourcePacks")
        {
            Manager = new PackManager();

            GuiManager.AddHandler(_handler);

            Settings.Default.Reload();
        }

        public override void LoadMain()
        {
            Manager.Init();

            foreach (var pack in Manager.Packs.Keys)
            {
                packsQueue.Enqueue(pack);
            }
            packsQueue.Enqueue(packsQueue.Dequeue());

            if (!Manager.Set(Settings.Default.ResourcePack))
            {
                Settings.Default.ResourcePack = "Default";
                Settings.Default.Save();
            }
        }

        public override void Draw(GameTime time)
        {
            if (Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Delete))
            {
                if (!keyDown)
                {
                    var pack = packsQueue.Dequeue();
                    packsQueue.Enqueue(pack);

                    Manager.Set(pack);
                }
                keyDown = true;
            }
            else
            {
                keyDown = false;
            }
        }
    }
}
