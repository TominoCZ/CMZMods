using DNA;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
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

namespace BlockEvents
{
    // TODO: This is a W.I.P.

    [HarmonyPatch]
    static class Patch
    {
        /*
        [HarmonyPostfix, HarmonyPatch(typeof(BlockInventoryItem), "AlterBlock")]
        static void AlterBlock(BlockInventoryItem __instance, Player player, IntVector3 addSpot, BlockFace inFace)
        {
            var name = player.Name;

            BlockEventsMod.Instance.Log($"{name} -> {__instance.Name} @ ({addSpot.X}, {addSpot.Y}, {addSpot.Z})");
        }*/
    }

    [MMLMod("BlockEvents", "com.Morphox.BlockEvents")]
    public class BlockEventsMod : ModBase<BlockEventsMod, CastleMinerZGame>
    {
        public BlockEventsMod(CastleMinerZGame game) : base(game)
        {

        }
    }
}
