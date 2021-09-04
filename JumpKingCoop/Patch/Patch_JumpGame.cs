using BehaviorTree;
using HarmonyLib;
using JumpKing;
using JumpKing.SaveThread;
using JumpKing.Util;
using JumpKingCoop.Input;
using JumpKingCoop.Logs;
using JumpKingCoop.Modes;
using JumpKingCoop.Networking;
using JumpKingCoop.Utils;
using JumpKingCoop.Wrappers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Patch
{
    [HarmonyPatch]
    public static class Patch_JumpGame
    {
        private static bool IsGameLoopRunning = false;
        private static SpectateMode SpectateMode = new SpectateMode();
        public static bool ToggleNameplates { get; private set; } = true;

        private static void PrefixDraw(JumpGame __instance)
        {
            InputManager.RegisterButton(Microsoft.Xna.Framework.Input.Keys.S, () => SpectateMode.Toggle());
            InputManager.RegisterButton(Microsoft.Xna.Framework.Input.Keys.N, () => ToggleNameplates = !ToggleNameplates);
        }

        private static void PostfixDraw(JumpGame __instance)
        {
            var jumpGameWrapper = new JumpGame_Wrapper(__instance);

            if(jumpGameWrapper.IsRunning() != IsGameLoopRunning)
            {
                IsGameLoopRunning = jumpGameWrapper.IsRunning();

                if (IsGameLoopRunning)
                {
                    Logger.Log("GameLoop started");
                    NetworkManager.Initialize();

                }
                else
                {
                    Logger.Log("GameLoop stopped");
                    NetworkManager.Stop();
                }
            }

            if(jumpGameWrapper.IsRunning())
            {
                TextScheduler.Draw();
                SpectateMode.Draw();
            }
        }

        private static void PrefixUpdate(JumpGame __instance, GameTime gameTime)
        {
            var jumpGameWrapper = new JumpGame_Wrapper(__instance);
            if (jumpGameWrapper.IsRunning())
            {
                NetworkManager.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }

        private static void PostfixUpdate(JumpGame __instance, GameTime gameTime)
        {
            var jumpGameWrapper = new JumpGame_Wrapper(__instance);
            if (jumpGameWrapper.IsRunning())
            {
                SpectateMode.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
                InputManager.Update((float)gameTime.ElapsedGameTime.TotalSeconds);
            }
        }
    }
}
