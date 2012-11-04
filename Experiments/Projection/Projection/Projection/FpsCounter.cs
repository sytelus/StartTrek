using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projection
{
    public class FpsCounter
    {
        public int Rate { get; private set; }
        private int counter = 0;
        private TimeSpan elapsedCounterTime = TimeSpan.Zero;
        private static readonly TimeSpan OneSecond = TimeSpan.FromSeconds(1);
        public void OnUpdate(GameTime gameTime)
        {
            elapsedCounterTime += gameTime.ElapsedGameTime;
            if (elapsedCounterTime > OneSecond)
            {
                elapsedCounterTime -= OneSecond;
                this.Rate = counter;
                counter = 0;
            }
        }
        public void OnDraw()
        {
            counter++;
        }
    }
}
