using JumpKing;
using JumpKing.Util;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Utils
{
    public static class TextScheduler
    {
        public class TextEntry
        {
            public string Text { get; set; }
            public DateTime StartTime { get; set; }
            public float TimeSpan { get; set; }

            public TextEntry(string text, float time)
            {
                Text = text;
                TimeSpan = time;
                StartTime = DateTime.Now;
            }
        }

        private static List<TextEntry> textLines = new List<TextEntry>();

        public static void Draw()
        {
            textLines.RemoveAll(x => (DateTime.Now - x.StartTime).TotalSeconds >= x.TimeSpan);

            string stringToBuild = string.Empty;

            for(int i = 0; i < textLines.Count; i++)
                stringToBuild += textLines[i].Text + "\n";

            if(stringToBuild != string.Empty)
            {
                TextHelper.DrawString(JKContentManager.Font.MenuFontSmall, stringToBuild, new Vector2(100.0f, 30f), Color.Red, new Vector2(0.5f, -0.5f));
            }
        }

        public static void ScheduleText(string text, float time = 5.0f)
        {
            textLines.Add(new TextEntry ( text, time ));
        }
    }
}
