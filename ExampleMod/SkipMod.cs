using DNA;
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
                loading.GetValue<OneShotTimer>("fadeOut").MaxTime = TimeSpan.Zero;
                loading.GetValue<OneShotTimer>("postBlackness").MaxTime = TimeSpan.Zero;

                SkipMod.Instance.Log("Intro skipped!", LogType.Success);

                Dispose(); // We don't need the handler anymore, it will be discarded by the GuiManager
            }
        }
    }

    [MMLMod("Intro Skip", "com.Morphox.IntroSkip")]
    public class SkipMod : ModBase<SkipMod, CastleMinerZGame>
    {
        public SkipMod(CastleMinerZGame game) : base(game)
        {

        }

        public override void LoadPre()
        {
            GuiManager.AddHandler(new MyHandler());
        }
    }
}