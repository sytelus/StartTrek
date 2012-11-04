using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projection
{
    public class ArrowFieldScene : Scene
    {
        Random random = new Random(42);
        private static readonly float[] ArrowCubeVertexMultipliers = new float[] { 1, 1, 1, 1, 0.01f, 0.01f, 0.01f, 0.01f, 1 };
        public ArrowFieldScene(GraphicsDevice graphicsDevice, Vector3 center) : base(graphicsDevice, center)
        {
            for (int i = 0; i < 500; i++)
            {
                var cube = new Cube(graphicsDevice
                                    , new Vector3(
                                          (float) (random.NextDouble() - 0.5)*1000,
                                          (float) (random.NextDouble() - 0.5)*1000,
                                          (float) (random.NextDouble() - 0.5)*1000)
                                    , new Vector3(20, 60, 20)
                                    , ArrowCubeVertexMultipliers    //Enumerable.Range(0, 8).Select(r => (float)random.NextDouble()).ToArray()
                                    , null  //Enumerable.Range(0,8).Select(r => XnaExtentions.GetRandomColor()).ToArray()
                                    );

                this.AddObject(cube);

                this.RecommandedSettings.CameraPosition = new Vector3(0, 0, 500);
                this.RecommandedSettings.ArcBallOriginLocked = false;
            }
        }
    }
}
