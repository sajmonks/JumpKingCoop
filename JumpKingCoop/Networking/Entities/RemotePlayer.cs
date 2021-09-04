using EntityComponent;
using JumpKing;
using JumpKing.Util;
using JumpKingCoop.Patch;
using JumpKingCoop.Utils;
using JumpKingCoop.Wrappers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace JumpKingCoop.Networking.Entities
{
    public class PlayerState : EntityState
    {
        public string Name { get; set; }

        public Vector2 Position { get; set; }

        public SpriteEffects FlipEffect { get; set; }

        public string SpriteName { get; set; }

        public PlayerState()
        {
            Position = Vector2.Zero;
            Name = string.Empty;
        }

        public PlayerState(PlayerState other)
        {
            Name = other.Name;
            Position = other.Position;
            FlipEffect = other.FlipEffect;
            SpriteName = other.SpriteName;
        }

        public override bool Equals(Object other)
        {
            if ((other == null) || !this.GetType().Equals(other.GetType()))
            {
                return false;
            }
            else
            {
                PlayerState otherState = (PlayerState)other;

                var same1 = otherState.Name.Equals(Name);
                var same2 = otherState.Position.Equals(Position);
                var same3 = otherState.FlipEffect.Equals(FlipEffect);
                var same4 = otherState.SpriteName.Equals(SpriteName);
                return same1 && same2 && same3 && same4;
            }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + Name.GetHashCode();
            hash = hash * 31 + Position.GetHashCode();
            hash = hash * 31 + FlipEffect.GetHashCode();
            hash = hash * 31 + SpriteName.GetHashCode();
            return hash;
        }

        public static Type GetEntityStateType()
        {
            return typeof(PlayerState);
        }

        public override void UpdateState(EntityState destinationState)
        {
            if (destinationState is PlayerState)
            {
                //#TODO make sure none of the fields in the state are null
                var newState = destinationState as PlayerState;
                if (newState.Position != null)
                    Position = newState.Position;
                if (newState.Name != null)
                    Name = newState.Name;
                if (newState.SpriteName != null)
                    SpriteName = newState.SpriteName;

                FlipEffect = newState.FlipEffect;
            }
        }
    }


    public class RemotePlayer : Entity, INetworkEntity
    {
        private const float TransparencyOnOverlap = 0.15f;

        private const float OverlapDistance = 120.0f;

        private const float OverlapDistanceSquared = OverlapDistance * OverlapDistance;

        public bool IsDirty { get; private set; }

        public Vector2 PlayerCenter { get => CurrentPosition + (new Vector2(9f, 26f) / 2.0f); }

        public string Name { get => State.Name; }

        protected PlayerState State { get; set; } = new PlayerState();

        protected Sprite PlayerSprite { get; set; }

        protected Vector2 CurrentPosition { get; set; } = Vector2.Zero;

        protected readonly Color TransparentSpriteColor = new Color(TransparencyOnOverlap, TransparencyOnOverlap, TransparencyOnOverlap, TransparencyOnOverlap);

        public RemotePlayer()
        {
            State = new PlayerState();
            SetSprite(JKContentManager.PlayerSprites.idle);
        }

        public void UpdateEntity(EntityState state)
        {
            State.UpdateState(state);

            var playerSpriteType = typeof(JKContentManager.PlayerSprites);
            var spriteProperties = playerSpriteType.GetProperties();

            foreach (var spriteProperty in spriteProperties)
            {
                if (spriteProperty.Name == State.SpriteName)
                {
                    SetSprite(spriteProperty.GetValue(null) as Sprite);
                    break;
                }
            }
        }

        protected override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            //Lerp the positions to remove cutting effect
            var difference = State.Position - CurrentPosition;
            var distance = difference.Length();
            if (distance <= 0.00001f)
                distance = 0.0f;

            if (distance >= 0.1f)
            {
                var interpolate = (distance - deltaTime) / distance;
                CurrentPosition = CurrentPosition + (difference * interpolate);
            }
        }

        public PlayerState GetStateForUpdate()
        {
            IsDirty = false;
            return State;
        }

        protected void SetSprite(Sprite sprite)
        {
            PlayerSprite = sprite;
        }

        public override void Draw()
        {
            Vector2 vector = CurrentPosition + new Vector2(9f, 26f);
            var playerPositionInCameraSpace = Camera.TransformVector2(vector);

            var isTooCloseToLocalPlayer = (CurrentPosition - LocalPlayer.BodyComp.GetPosition()).LengthSquared() <= OverlapDistanceSquared;

            var layeredSpriteWrapper = new LayeredSprite_Wrapper(PlayerSprite);

            if(isTooCloseToLocalPlayer)
            {
                foreach (var childSprite in layeredSpriteWrapper.GetSprites())
                    childSprite.SetColor(TransparentSpriteColor);
            }

            PlayerSprite.Draw(Camera.TransformVector2(vector), State.FlipEffect);

            if(isTooCloseToLocalPlayer)
            {
                foreach (var childSprite in layeredSpriteWrapper.GetSprites())
                    childSprite.SetColor(Color.White);
            } 

            if (Patch_JumpGame.ToggleNameplates)
            {
                var isPlayerClamped = false;
                var clampedY = 0.0f;

                if (playerPositionInCameraSpace.Y < 0)
                {
                    isPlayerClamped = true;
                    clampedY = 0.0f;
                }
                else if (playerPositionInCameraSpace.Y > 360.0f)
                {
                    isPlayerClamped = true;
                    clampedY = 360.0f;
                }

                var textPositionInCameraSpace = playerPositionInCameraSpace + new Vector2(0.0f, -55.0f);
                textPositionInCameraSpace.Y = MathHelper.Clamp(textPositionInCameraSpace.Y, 0.0f, 340.0f);

                var distanceText = String.Format("({0})", Convert.ToInt32(Math.Abs(playerPositionInCameraSpace.Y - clampedY)));
                var playerText = String.Format("{0} {1}", State.Name, isPlayerClamped ? distanceText : "");

                var textColor = isTooCloseToLocalPlayer ? new Color(Color.White, TransparencyOnOverlap) : new Color(Color.White, 0.6f);

                TextHelper.DrawString(JKContentManager.Font.MenuFontSmall, playerText,
                    textPositionInCameraSpace, textColor, new Vector2(0.5f, -0.5f));
            }
        }

        ////#TODO Make extention out of this function
        //protected void CustomDrawRemotePlayerSprite(Vector2 position, SpriteEffects effects)
        //{
        //    if (PlayerSprite.texture == null)
        //        return;

        //    position -= PlayerSprite.source.Size.ToVector2() * PlayerSprite.center;
        //    position = position.ToPoint().ToVector2();

        //    Game1.spriteBatch.Draw(PlayerSprite.texture, position, new Rectangle?(PlayerSprite.source), new Color(Color.White, 0.1f), 0.0f, Vector2.Zero, 1.0f, effects, 0.0f);
        //}

        public static Type GetEntityStateType()
        {
            return typeof(PlayerState);
        }
    }
}
