using BehaviorTree;
using HarmonyLib;
using JumpKing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Wrappers
{
    public sealed class JumpGame_Wrapper
    {
        private static FieldInfo m_game_loop_info = null;

        static JumpGame_Wrapper()
        {
            m_game_loop_info = AccessTools.Field(typeof(JumpGame), "m_game_loop");
        }

        private readonly object instance = null;

        private IBTnode selfTree = null;

        public JumpGame_Wrapper(object instance)
        {
            this.instance = instance;
            selfTree = m_game_loop_info.GetValue(instance) as IBTnode;
        }

        public bool IsRunning()
        {
            return selfTree?.IsRunning() ?? false;
        }
    }
}
