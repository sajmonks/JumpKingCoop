using EntityComponent;
using JumpKing;
using JumpKing.Player;
using JumpKing.Util;
using JumpKingCoop.Networking.Entities;
using JumpKingCoop.Utils;
using JumpKingCoop.Wrappers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Input
{
    public static class InputManager
    {
        private delegate void OnClickHandler();

        private class KeyEntry
        {
            public bool IsPressed { get; set; }

            public Action OnClick;

            public KeyEntry()
            {
                IsPressed = false;
            }

            public KeyEntry(Action action) : base()
            {
                SetCallback(action);
            }

            public void SetCallback(Action action)
            {
                OnClick = action;
            }
        }

        private static Dictionary<Keys, KeyEntry> keyEntries = new Dictionary<Keys, KeyEntry>();

        public static void RegisterButton(Keys key, Action callback)
        {
            if(keyEntries.ContainsKey(key))
            {
                var existingValue = keyEntries[key];
                existingValue.SetCallback(callback);
            }
            else
                keyEntries.Add(key, new KeyEntry(callback));
        }

        public static void Update(float deltaTime)
        {
            var keyboardState = Keyboard.GetState();

            foreach (var keys in keyEntries)
            {
                if(keyboardState.IsKeyDown(keys.Key))
                {
                    keys.Value.IsPressed = true;
                }
                else if(keyboardState.IsKeyUp(keys.Key))
                {
                    if(keys.Value.IsPressed)
                    {
                        keys.Value.IsPressed = false;
                        keys.Value.OnClick?.Invoke();
                    }
                }
            }
        }
    }
}
