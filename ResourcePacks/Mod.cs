using DNA.CastleMinerZ;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Modding;
using System.Collections.Generic;

namespace ResourcePacks
{
    public class ResourcePacksMod : ModBase
    {
        public static PackManager Manager { get; private set; }

        static bool keyDown = false;
        static Queue<string> packsQueue = new Queue<string>();

        public ResourcePacksMod(Game game) : base(game, "Resource Packs", "com.Morphox.ResourcePacks")
        {

        }

        public override void Run()
        {

        }

        public override void LoadPre()
        {

        }

        public override void LoadMain()
        {
            Manager = new PackManager((CastleMinerZGame)Game);

            foreach (var pack in Manager.Packs.Keys)
            {
                packsQueue.Enqueue(pack);
            }
            packsQueue.Enqueue(packsQueue.Dequeue());
        }

        public override void LoadPost()
        {

        }

        public override void Draw(GameTime time)
        {

        }

        public override void Update(GameTime time)
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
