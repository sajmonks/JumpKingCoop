using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Wrappers
{
    public sealed class BodyComp_Wrapper
    {
        private static Type BodyCompType;

        private static FieldInfo PositionInfo;
        private static FieldInfo VelocityInfo;
        private static FieldInfo OverlapInfo;
        private static FieldInfo IsOnGroundInfo;

        static BodyComp_Wrapper()
        {
            BodyCompType = Assembly.GetEntryAssembly().GetType("JumpKing.Player.BodyComp");
            PositionInfo = AccessTools.Field(BodyCompType, "position");
            VelocityInfo = AccessTools.Field(BodyCompType, "velocity");
            OverlapInfo = AccessTools.Field(BodyCompType, "overlap");
            IsOnGroundInfo = AccessTools.Field(BodyCompType, "_is_on_ground");
        }

        private object instance = null;

        public Vector2 PlayerCenter { get => GetPosition() + (new Vector2(9f, 26f) / 2.0f); }

        public BodyComp_Wrapper(object instance)
        {
            this.instance = instance;
        }

        public bool IsOnGround()
        {
            var value = IsOnGroundInfo.GetValue(instance);
            if (value != null)
            {
                return (bool)value;
            }
            return false;
        }

        public Rectangle GetOverlap()
        {
            var value = OverlapInfo.GetValue(instance);
            if (value != null)
            {
                return (Rectangle)value;
            }
            return Rectangle.Empty;
        }

        public Vector2 GetPosition()
        {
            var value = PositionInfo.GetValue(instance);
            if(value != null)
            {
                Vector2 result = (Vector2)value;
                return result;
            }
            return Vector2.Zero;
        }

        public void SetPosition(Vector2 value)
        {
            PositionInfo.SetValue(instance, value);
        }

        public Vector2 GetVelocity()
        {
            var value = VelocityInfo.GetValue(instance);
            if (value != null)
            {
                Vector2 result = (Vector2)value;
                return result;
            }
            return Vector2.Zero;
        }

        public void SetVelocity(Vector2 value)
        {
            VelocityInfo.SetValue(instance, value);
        }
    }
}
