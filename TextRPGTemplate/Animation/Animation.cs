using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextRPGTemplate.Animation
{
    public class Animation
    {
        public string[][] frames { get; set; } = Array.Empty<string[]>();
        public int[] x { get; set; } = Array.Empty<int>();
        public int[] y { get; set; } = Array.Empty<int>();
        public int frameDurationMs { get; set; } = 100;
        public Action? OnComplete { get; set; } = null;

        public Animation() { }

        public Animation(string[][] frames, int[] x, int[] y, int frameDurationMs, Action? onComplete)
        {
            this.frames = frames;
            this.x = x;
            this.y = y;
            this.frameDurationMs = frameDurationMs;
            OnComplete = onComplete;
        }

        public Animation DeepCopy()
        {
            return new Animation
            {
                frames = (string[][])this.frames.Clone(),
                x = (int[])this.x.Clone(),
                y = (int[])this.y.Clone(),
                frameDurationMs = this.frameDurationMs,
                OnComplete = null
            };


        }
    }
}
