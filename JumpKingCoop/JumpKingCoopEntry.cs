using HarmonyLib;
using JumpKingCoop.Logs;
using JumpKingCoop.Patch;
using System.Reflection;

namespace JumpKingCoop
{
    public class JumpKingCoopEntry
    {
        public static void Init()
        {
            var harmony = new Harmony("com.sajmonks.jumpking.jumpkingcoop");

            //First patch GameTitleScreen to add 'Join a friend button'
            //if(!PatchGameTitleScreenCreateMenu(harmony))
            //{
            //    Logger.Log("Failed to patch GameTitleScreen_CreateMenu");
            //    return;
            //}

            //First patch GameTitleScreen to add 'Join a friend button'
            if (!PatchJumpGameDraw(harmony))
                Logger.Log("Failed to patch the JumpGame.");
            else
                Logger.Log("Successfully patched the JumpGame.");
        }

        private static bool PatchGameTitleScreenCreateMenu(Harmony harmony)
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            var originalType = entryAssembly.GetType("JumpKing.GameManager.TitleScreen.GameTitleScreen");
            if(originalType != null)
            {
                var originalMethod = originalType.GetMethod("CreateMenu", BindingFlags.NonPublic | BindingFlags.Instance);
                if(originalMethod != null)
                {
                    var transplier = typeof(Patch_GameTitleScreen).
                        GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static);

                    harmony.Patch(originalMethod, transpiler: new HarmonyMethod(transplier));
                    return true;
                }
            }

            return false;
        }

        private static bool PatchJumpGameDraw(Harmony harmony)
        {
            var originalDrawMethod = typeof(JumpKing.JumpGame).GetMethod("Draw", BindingFlags.Public | BindingFlags.Instance);
            var prefixDrawMethod = typeof(Patch.Patch_JumpGame).GetMethod("PrefixDraw", BindingFlags.NonPublic | BindingFlags.Static);
            var postfixDrawMethod = typeof(Patch.Patch_JumpGame).GetMethod("PostfixDraw", BindingFlags.NonPublic | BindingFlags.Static);

            var originalUpdateMethod = typeof(JumpKing.JumpGame).GetMethod("Update", BindingFlags.Public | BindingFlags.Instance);
            var prefixUpdateMethod = typeof(Patch.Patch_JumpGame).GetMethod("PrefixUpdate", BindingFlags.NonPublic | BindingFlags.Static);
            var postfixUpdateMethod = typeof(Patch.Patch_JumpGame).GetMethod("PostfixUpdate", BindingFlags.NonPublic | BindingFlags.Static);

            if (originalDrawMethod != null && prefixDrawMethod != null && postfixDrawMethod != null)
            {
                harmony.Patch(originalDrawMethod, prefix: new HarmonyMethod(prefixDrawMethod), postfix: new HarmonyMethod(postfixDrawMethod));
            }
            else
                return false;

            if (originalUpdateMethod != null && prefixUpdateMethod != null && postfixUpdateMethod != null)
            {
                harmony.Patch(originalUpdateMethod, prefix: new HarmonyMethod(prefixUpdateMethod), postfix: new HarmonyMethod(postfixUpdateMethod));
            }
            else
                return false;

            return true;
        }
    }
}
