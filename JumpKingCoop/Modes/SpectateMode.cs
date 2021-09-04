using EntityComponent;
using JumpKing;
using JumpKing.Player;
using JumpKing.Util;
using JumpKingCoop.Networking.Entities;
using JumpKingCoop.Utils;
using JumpKingCoop.Wrappers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Modes
{
    public class SpectateMode
    {
        private bool IsSpectateMode { get; set; } = false;

        private RemotePlayer currentSpectatedPlayer = null;

        private int currentSpectatedPlayerIndex = -1;

        private bool isPositionLocked = false;

        private Vector2 localPlayerPosition = Vector2.Zero;

        public void Update(float deltaTime)
        {
            if (IsSpectateMode)
                HandleSpectateMode();
            else if (!IsSpectateMode && isPositionLocked)
            {
                //Try to update camera to the position of PlayerEntity
                //This mainly creates a problem when the PlayerEntity is so far away, that game doesn't
                //update it (probably for performance reasons). One of the player components is camera
                //follower, so we will never get an update unless we trigger it manually.
                int x = Convert.ToInt32(LocalPlayer.BodyComp.PlayerCenter.X);
                int y = Convert.ToInt32(LocalPlayer.BodyComp.PlayerCenter.Y);
                Camera.UpdateCamera(new Point(x, y));

                //Wait until the camera has moved to the screen where player resides.
                //Otherwise he will fall through the levels because the sprites are not loaded yet
                if (LocalPlayer.BodyComp.IsOnGround())
                {
                    //When the sprites are unloaded, the game still updates the bodycomp for some time
                    //and it adds up the velocity given by the gravity. If we release the spectate mode
                    //then player would faceplant on the ground, unless we reset it here (#TODO it doesnt work).
                    LocalPlayer.BodyComp.SetVelocity(Vector2.Zero);
                    isPositionLocked = false;
                }
            }

            if (isPositionLocked)
            {
                //Also freeze the player so that he won't fall under the ground when sprites are unloaded
                LocalPlayer.BodyComp.SetPosition(localPlayerPosition);
            }
        }

        public void Draw()
        {
            if (IsSpectateMode)
            {
                TextHelper.DrawString(JKContentManager.Font.MenuFontSmall, "Spectating " + currentSpectatedPlayer.Name,
                    new Vector2(360.0f, 20.0f), new Color(Color.Red, 0.7f), new Vector2(0.5f, -0.5f));
            }
        }

        public void Toggle()
        {
            if (LocalPlayer.BodyComp.IsOnGround() || IsSpectateMode)
                SwitchSpectateMode();
        }

        private void HandleSpectateMode()
        {
            if (IsSpectateMode)
            {
                //Verify if entity exist in entity manager
                if (!EntityManager.instance.Entities.Any(entity => entity.Equals(currentSpectatedPlayer)))
                {
                    SwitchSpectateMode();
                    return;
                }

                int x = Convert.ToInt32(currentSpectatedPlayer.PlayerCenter.X);
                int y = Convert.ToInt32(currentSpectatedPlayer.PlayerCenter.Y);
                Camera.UpdateCamera(new Point(x, y));
            }
        }

        private void SwitchSpectateMode()
        {
            var allRemotePlayers = EntityManager.instance.FindAll<RemotePlayer>();
            if (allRemotePlayers.Length > currentSpectatedPlayerIndex + 1)
            {
                //Do not update player position if the spectate mode is turned on
                //Because there is a chance that body comp will calculate gravity
                //physics and player will sink to the ground.
                if (!IsSpectateMode)
                    localPlayerPosition = LocalPlayer.BodyComp.GetPosition();

                currentSpectatedPlayerIndex = currentSpectatedPlayerIndex + 1;
                currentSpectatedPlayer = allRemotePlayers[currentSpectatedPlayerIndex];
                IsSpectateMode = true;
                isPositionLocked = true;
            }
            else
            {
                currentSpectatedPlayer = null;
                currentSpectatedPlayerIndex = -1;
                IsSpectateMode = false;
            }
        }
    }
}
