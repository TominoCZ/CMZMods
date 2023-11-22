using DNA.Audio;
using DNA;
using DNA.CastleMinerZ;
using DNA.CastleMinerZ.AI;
using DNA.CastleMinerZ.Inventory;
using DNA.CastleMinerZ.Terrain;
using DNA.CastleMinerZ.UI;
using DNA.CastleMinerZ.Utils.Trace;
using DNA.Drawing.Particles;
using DNA.Drawing;
using DNA.Net.GamerServices;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Modding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using DNA.Drawing.Imaging.Photoshop;
using DNA.Data.Units;

namespace LaserGunFix
{
    [HarmonyPatch]
    static class Patch
    {
        static FieldInfo Garbage = typeof(BlasterShot).GetField("_garbage", BindingFlags.NonPublic | BindingFlags.Static);
        static ConstructorInfo Constructor = AccessTools.Constructor(typeof(BlasterShot), new Type[] { typeof(byte) });//typeof(BlasterShot).GetConstructor(BindingFlags.NonPublic, null, new[] { typeof(byte) }, null);
        static object[] ContstructorArgs = new object[] { (byte)0 };

        static BlasterShot Create(Vector3 position, Vector3 velocity, InventoryItemIDs item, LaserGunInventoryItemClass itemClass, out bool isNew)
        {
            isNew = false;
            var player = LaserGunFix.Instance.Game.LocalPlayer;
            var pool = (List<BlasterShot>)Garbage.GetValue(null);
            var shot = pool.FirstOrDefault(s => s.Parent == null);
            if (shot == null)
            {
                if (pool.Count >= 100)
                {
                    shot = pool[0];
                }
                else
                {
                    shot = (BlasterShot)Constructor.Invoke(ContstructorArgs);
                    isNew = true;
                }
            }

            var color = new Color(itemClass.TracerColor);
            shot.SetValue("_lifeTime", (TimeSpan)typeof(BlasterShot).GetField("TotalLifeTime", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null));
            shot.SetValue("_color", color);
            shot.GetValue<ModelEntity>("_tracer").EntityColor = color;
            shot.SetValue("ReflectedShot", false);
            shot.SetValue("_velocity", velocity * 200f);
            shot.SetValue("LocalToParent", MathTools.CreateWorld(position, velocity));
            shot.SetValue("_firstUpdate", true);
            shot.SetValue("_noCollideFrame", false);
            shot.SetValue("_weaponClassUsed", itemClass);
            shot.SetValue("_weaponUsed", item);
            shot.SetValue("_lastPosition", position);
            shot.SetValue("_explosiveType", itemClass.Call<bool>("IsHarvestWeapon") ? ExplosiveTypes.Harvest : ExplosiveTypes.Laser);

            if (isNew)
                return shot;

            player.Scene.Children.Remove(shot);
            shot.Children.Remove(shot);

            return shot;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BlasterShot), "Create", new[] { typeof(Vector3), typeof(Vector3), typeof(int), typeof(InventoryItemIDs) })]
        static bool Create(Vector3 position, Vector3 velocity, int enemyId, InventoryItemIDs item, ref BlasterShot __result)
        {
            __result = null;

            if (!(InventoryItem.GetClass(item) is LaserGunInventoryItemClass itemClass))
                return false;

            var pool = (List<BlasterShot>)Garbage.GetValue(null);
            var shot = Create(position, velocity, item, itemClass, out var isNew);

            #region init bullet
            shot.SetValue("CollisionsRemaining", 3);
            shot.SetValue("_enemyID", enemyId);
            shot.SetValue("_shooter", 0);
            #endregion

            if (isNew)
                pool.Add(shot);

            __result = shot;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BlasterShot), "Create", new[] { typeof(Vector3), typeof(Vector3), typeof(InventoryItemIDs), typeof(byte) })]
        static bool Create(Vector3 position, Vector3 velocity, InventoryItemIDs item, byte shooterID, ref BlasterShot __result)
        {
            __result = null;

            if (!(InventoryItem.GetClass(item) is LaserGunInventoryItemClass itemClass))
                return false;

            var pool = (List<BlasterShot>)Garbage.GetValue(null);
            var shot = Create(position, velocity, item, itemClass, out var isNew);

            #region init bullet
            shot.SetValue("CollisionsRemaining", 30);
            shot.SetValue("_enemyID", -1);
            shot.SetValue("_shooter", shooterID);
            #endregion

            if (isNew)
                pool.Add(shot);

            __result = shot;

            return false;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(BlasterShot), "HandleCollision")]
        static bool HandleCollision(
            BlasterShot __instance,
            ref ExplosiveTypes ____explosiveType,
            ref byte ____shooter,
            ref bool ___ReflectedShot,
            ref Vector3 ____lastPosition,
            ref Vector3 ____velocity,
            ref ParticleEffect ____spashEffect,
            ref ParticleEffect ____sparkEffect,
            ref ParticleEffect ____smokeEffect,
            Vector3 collisionNormal,
            Vector3 collisionLocation,
            bool bounce,
            bool destroyBlock,
            IntVector3 blockToDestroy)
        {
            Matrix localToParent = MathTools.CreateWorld(collisionLocation, -collisionNormal);
            if (__instance.CollisionsRemaining-- > 0)
                ___ReflectedShot = bounce;
            if (bounce)
            {
                Vector3 vector = __instance.WorldPosition - collisionLocation;
                Vector3 position = Vector3.Reflect(vector, collisionNormal) + collisionLocation;
                ____lastPosition = collisionLocation;
                ____velocity = Vector3.Reflect(____velocity, collisionNormal);
                __instance.LocalToParent = MathTools.CreateWorld(position, ____velocity);
                return false;
            }

            if (CastleMinerZGame.Instance.IsActive)
            {
                ParticleEmitter particleEmitter = ____spashEffect.CreateEmitter(CastleMinerZGame.Instance);
                particleEmitter.LocalScale = new Vector3(0.01f);
                particleEmitter.Reset();
                particleEmitter.Emitting = true;
                particleEmitter.LocalToParent = localToParent;
                __instance.Scene.Children.Add(particleEmitter);
                particleEmitter.DrawPriority = 900;
                ParticleEmitter particleEmitter2 = ____sparkEffect.CreateEmitter(CastleMinerZGame.Instance);
                particleEmitter2.Reset();
                particleEmitter2.Emitting = true;
                particleEmitter2.LocalToParent = localToParent;
                __instance.Scene.Children.Add(particleEmitter2);
                particleEmitter2.DrawPriority = 900;
                ParticleEmitter particleEmitter3 = ____smokeEffect.CreateEmitter(CastleMinerZGame.Instance);
                particleEmitter3.Reset();
                particleEmitter3.Emitting = true;
                particleEmitter3.LocalToParent = localToParent;
                __instance.Scene.Children.Add(particleEmitter3);
                particleEmitter3.DrawPriority = 900;
                __instance.Emitter.Velocity = new Vector3(0f, 0f, 0f);
                __instance.Emitter.Position = localToParent.Translation;
                __instance.Emitter.Up = new Vector3(0f, 1f, 0f);
                __instance.Emitter.Forward = new Vector3(0f, 0f, 1f);
            }
            if (CastleMinerZGame.Instance.IsLocalPlayerId(____shooter) && destroyBlock)
                Explosive.FindBlocksToRemove(blockToDestroy, ____explosiveType, showExplosionFlash: true);

            __instance.SetValue("_velocity", new Vector3(0f, 0f, 0f));
            __instance.RemoveFromParent();

            return false;
        }
    }

    public class LaserGunFix : ModBase<LaserGunFix, CastleMinerZGame>
    {
        public LaserGunFix(CastleMinerZGame game) : base(game, "LaserGunFix", "com.Morphox.LaserGunFix")
        {

        }
    }
}
