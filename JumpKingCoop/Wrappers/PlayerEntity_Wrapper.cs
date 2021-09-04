using HarmonyLib;
using JumpKing;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Wrappers
{
    public sealed class PlayerEntity_Wrapper
    {
        private static Type PlayerEntityType;

        private static FieldInfo BodyCompInfo;
        private static FieldInfo SpriteFlipInfo;
        private static FieldInfo SpriteInfo;

        static PlayerEntity_Wrapper()
        {
            PlayerEntityType = Assembly.GetEntryAssembly().GetType("JumpKing.Player.PlayerEntity");
            BodyCompInfo = AccessTools.Field(PlayerEntityType, "m_body");
            SpriteFlipInfo = AccessTools.Field(PlayerEntityType, "m_flip");
            SpriteInfo = AccessTools.Field(PlayerEntityType, "m_sprite");
        }

        private object instance = null;

        public PlayerEntity_Wrapper(object instance)
        {
            this.instance = instance;
        }

        public BodyComp_Wrapper GetBodyComp()
        {
            var value = BodyCompInfo.GetValue(instance);
            return new BodyComp_Wrapper(value);
        }

        public SpriteEffects GetFlipEffect()
        {
            var value = SpriteFlipInfo.GetValue(instance);
            if (value != null)
            {
                SpriteEffects result = (SpriteEffects)value;
                return result;
            }
            return SpriteEffects.None;
        }

        public Sprite GetSprite()
        {
            var value = SpriteInfo.GetValue(instance);
            if (value != null)
            {
                Sprite result = (Sprite)value;
                return result;
            }
            return null;
        }
    }
}
