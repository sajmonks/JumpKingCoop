using BehaviorTree;
using HarmonyLib;
using JumpKing.PauseMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Patch
{
    public class MenuSelector_Internal
    {
        public static Type MenuSelectorType { get; protected set; }

        static MenuSelector_Internal()
        {
            var executingAssembly = Assembly.GetEntryAssembly();
            MenuSelectorType = executingAssembly.GetType("JumpKing.PauseMenu.BT.MenuSelector");
        }

        private readonly object instance = null;
        private MethodInfo addChildMethod = null;

        public MenuSelector_Internal(object instance)
        {
            this.instance = instance;
            if(MenuSelectorType != null)
            {
                addChildMethod = MenuSelectorType.GetMethods().Where(x=>x.Name.Equals("AddChild") && x.IsGenericMethod == true).FirstOrDefault();
            }
        }

        public void AddChild(Type type, object p_child)
        {
            var AddChild_Generic = addChildMethod.MakeGenericMethod(type);
            AddChild_Generic.Invoke(instance, new object[1] { p_child });
        }
    }

    public class TextButton_Internal
    {
        public static Type TextButtonType { get; protected set; }

        static TextButton_Internal()
        {
            var executingAssembly = Assembly.GetEntryAssembly();
            TextButtonType = executingAssembly.GetType("JumpKing.PauseMenu.BT.TextButton");
        }

        private readonly object instance = null;

        public TextButton_Internal(object instance)
        {
            this.instance = instance;
        }

        public static TextButton_Internal Create(string p_text, IBTnode p_child)
        {
            if(TextButtonType != null)
            {
                var instance = Activator.CreateInstance(TextButtonType, new object[] { p_text, p_child });
                var newButton = new TextButton_Internal(instance);
                return newButton;
            }

            return null;
        }

        public object GetTextButton()
        {
            return instance;
        }
    }


    [HarmonyPatch]
    public static class Patch_GameTitleScreen
    {
        //Index of CIL to place custom code
        private const int ButtonHookIndex = 203;

        //Main MenuSelector variable index
        private const int MenuSelectorVariableIndex = 7;

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            codes.Insert(ButtonHookIndex, CodeInstruction.Call<IBTnode>(menuSelector => CreateMenu_AfterContinue(menuSelector)));
            codes.Insert(ButtonHookIndex, new CodeInstruction(OpCodes.Ldloc_S, MenuSelectorVariableIndex));

            return codes.AsEnumerable();
        }

        static void CreateMenu_AfterContinue(object menuSelector)
        {
            var textButton = TextButton_Internal.Create("Join a friend", null);

            var menuTemporary = new MenuSelector_Internal(menuSelector);
            menuTemporary.AddChild(TextButton_Internal.TextButtonType, textButton.GetTextButton());
        }
    }
}
