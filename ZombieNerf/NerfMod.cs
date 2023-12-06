using DNA;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing;
using DNA.Drawing.UI;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace NerfMod
{
    [HarmonyPatch]
    static class Patch
    {
        [HarmonyPrefix, HarmonyPatch(typeof(EnemyManager), "AttentuateVelocity")]
        static bool BypassSlowdown(out float __result)
        {
            __result = 1;

            return false;
        }

        /* USE THIS TO USE DEFAULT LOGIC IF YOU WANT TO MODIFY IT
        [HarmonyPrefix, HarmonyPatch(typeof(EnemyManager), "AttentuateVelocity")]
        static bool AttentuateVelocity(EnemyManager __instance, List<BaseZombie> ____enemies, Player plr, Vector3 fwd, Vector3 worldPos, out float __result)
        {
            float num = 1f;
            for (int i = 0; i < ____enemies.Count; i++)
            {
                if (____enemies[i].Target != plr || !____enemies[i].IsBlocking)
                {
                    continue;
                }

                Vector3 value = ____enemies[i].WorldPosition - worldPos;
                float num2 = value.LengthSquared();
                float num3 = 1f;
                if ((double)num2 < 4.0 && (double)Math.Abs(value.Y) < 1.5)
                {
                    num3 = 0.5f;
                    if ((double)num2 > 9.99999974737875E-05)
                    {
                        float num4 = Vector3.Dot(Vector3.Normalize(value), fwd);
                        if ((double)num4 > 0.0)
                        {
                            num3 *= Math.Min(1f, (float)(2.0 * (1.0 - (double)num4)));
                        }
                    }
                }

                num *= num3;
            }

            __result = num;

            return false;
        }*/
    }

    [MMLMod("No Zombie Slowdown", "com.Morphox.ZombieNerf")] 
    public class NerfMod : ModBase<NerfMod, CastleMinerZGame>
    {
        public NerfMod(CastleMinerZGame game) : base(game)
        {

        }
    }
}