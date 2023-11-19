using DNA.CastleMinerZ;
using DNA.CastleMinerZ.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Modding;
using ResourcePacks.Gui;
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

        static bool keyDown = false;
        static Queue<string> packsQueue = new Queue<string>();

        public PackMod(Game game) : base(game, "Resource Packs", "com.Morphox.ResourcePacks")
        {
            Manager = new PackManager((CastleMinerZGame)Game);
            GuiManager.AddHandler(_handler);

            Settings.Default.Reload();
        }

        protected override void LoadMain()
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

        protected override void Draw(GameTime time)
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
