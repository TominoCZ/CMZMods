using DNA.CastleMinerZ;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.UI;
using DNA.Drawing;
using DNA.Drawing.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NerfMod
{
    /*
    [HarmonyPatch]
    static class Patch
    {
        [HarmonyFinalizer, HarmonyPatch(typeof(ZombieDig), "Enter")]
        static void Enter(ZombieDig __instance, BaseZombie ___entity)
        {
            if (___entity.EType.AttackAnimationSpeed == ___entity.CurrentPlayer.Speed)
                ___entity.CurrentPlayer.Speed = 1;
        }
    }*/

    public class NerfMod : ModBase
    {
        public static NerfMod Instance;

        public NerfMod(CastleMinerZGame game) : base(game, "ZombieNerf", "com.Morphox.ZombieNerf")
        {
            Instance = this;
        }

        protected override void LoadMain()
        {
            this.Log("This mod doesn't work yet.", LogType.Warn);
        }
    }
}