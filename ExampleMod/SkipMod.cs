using DNA.CastleMinerZ;
using DNA.CastleMinerZ.UI;
using DNA.Drawing.UI;
using DNA.Timers;
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
        protected override void OnPush(Screen screen)
        {
            if (screen is LoadScreen loading)
            {
                loading.Finished = true; // This will make the screen disappear right after the game is all loaded

                // This will make the splash fade in and show until the game is fully loaded
                loading.GetValue<OneShotTimer>("preBlackness").MaxTime = TimeSpan.FromSeconds(0.5);
                loading.GetValue<OneShotTimer>("fadeIn").MaxTime = TimeSpan.FromSeconds(1);
                loading.GetValue<OneShotTimer>("display").MaxTime = TimeSpan.FromMinutes(60);
                //loading.GetValue<OneShotTimer>("fadeOut").MaxTime = TimeSpan.FromSeconds(0.5);
                //loading.GetValue<OneShotTimer>("postBlackness").MaxTime = TimeSpan.FromSeconds(0.5);

                SkipMod.Instance.Log("Intro skipped!", LogType.Success);

                Dispose(); // We don't need the handler anymore, it will be discarded by the GuiManager
            }
        }
    }

    public class SkipMod : ModBase
    {
        public static SkipMod Instance;

        public SkipMod(CastleMinerZGame game) : base(game, "IntroSkip", "com.Morphox.IntroSkip")
        {
            Instance = this;
        }

        protected override void LoadPre()
        {
            GuiManager.AddHandler(new MyHandler());
        }
    }
}