using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextRPG.View;

namespace TextRPGTemplate.Animation
{
    public class AnimationPlayer
    {
        public AnimationPlayer()
        {

        }

        public void play(Animation?[] animations, SpriteView spriteView)
        {
            ViewPort viewPort = spriteView.view!;
            int x = viewPort.width;
            int y = viewPort.height;
            if (animations[0] == null)
            {
                return;
            }

            Thread thread = new(() =>
            {
                int elapsedMs = 0;
                string[]? composedFrame = Array.Empty<string>();
                while((composedFrame = composeFrame(animations!, elapsedMs, x - 2, y - 2)) != null)
                {
                    spriteView.SetText(composedFrame);
                    spriteView.Update();
                    spriteView.Render();
                    Thread.Sleep(20);
                    elapsedMs += 20;
                }
            });
            thread.IsBackground = true;
            thread.Start();
            thread.Join();
        }

        string[]? composeFrame(Animation[] animations, int elapsedMs, int width, int height)
        {
            char[, ] screen = new char[height, width];
            int animationsCount = 0;
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    screen[i, j] = ' ';
                }
            };
            foreach (var animation in animations)
            {
                int frameCount = animation.frames.Length;
                int curFrame = elapsedMs / animation.frameDurationMs;
                if (curFrame >= frameCount)
                {
                    continue;
                }
                animationsCount++;
                string[] frame = animation.frames[curFrame];
                int offsetX = animation.x[curFrame];
                int offsetY = animation.y[curFrame];
                for (int j = 0; j < frame.Length; j++)
                {
                    for (int k = 0; k < frame[j].Length; k++)
                    {
                        if (offsetY + j < height && offsetX + k < width)
                        {
                            screen[offsetY + j, offsetX + k] = frame[j][k];
                        }
                    }
                }
            }
            if (animationsCount == 0)
            {
                return null;
            }
            string[] result = new string[height];
            for(int i = 0; i < height; i++)
            {
                char[] row = new char[width];
                for(int j = 0; j < width; j++)
                {
                    row[j] = screen[i, j];
                }
                result[i] = new string(row);
            }
            return result;
        }
    }
}
