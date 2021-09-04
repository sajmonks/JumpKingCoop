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
    public class LayeredSprite_Wrapper
    {
        private static Type LayeredSpriteType;


        private static readonly FieldInfo SpritesInfo;

        static LayeredSprite_Wrapper()
        {
            LayeredSpriteType = Assembly.GetEntryAssembly().GetType("JumpKing.XnaWrappers.LayeredSprite");
            SpritesInfo = AccessTools.Field(LayeredSpriteType, "m_sprites");
        }

        private readonly object instance = null;

        public LayeredSprite_Wrapper(object instance)
        {
            this.instance = instance;
        }

        public List<Sprite> GetSprites() => SpritesInfo?.GetValue(instance) as List<Sprite> ?? null;
    }
}
