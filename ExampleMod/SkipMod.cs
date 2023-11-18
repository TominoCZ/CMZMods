using DNA.CastleMinerZ;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.UI;
using Microsoft.Xna.Framework;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntroSkip
{
    class MyHandler : GuiHandler
    {
        protected override void OnShown(Screen screen)
        {
            if (screen is LoadScreen loading)
            {
                loading.Finished = true;

                ModBase.Instance.Log("Intro skipped!", LogType.Success);

                Dispose(); // We don't need the handler anymore, it will be discarded by the GuiManager
            }
        }
    }

    public class SkipMod : ModBase
    {
        public SkipMod(Game game) : base(game, "IntroSkip", "com.Morphox.IntroSkip")
        {

        }

        protected override void LoadPre()
        {
            GuiManager.AddHandler(new MyHandler());
        }
    }
}