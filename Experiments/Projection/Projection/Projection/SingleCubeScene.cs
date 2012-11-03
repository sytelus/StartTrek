using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projection
{
    public class SingleCubeScene : Scene
    {
        private Cube cube;
        public SingleCubeScene(GraphicsDevice graphicsDevice, Vector3 center, float cubeWidth)
            :base(graphicsDevice, center)
        {
            //Create cube
            cube = new Cube(graphicsDevice
                , position: center    //We'll rotate around origin so place object there
                , bounds: new Vector3(cubeWidth, cubeWidth, cubeWidth));

            this.AddObject(cube);
        }

        public override Vector3 SuggestedInitialCameraPosition
        {
            get { return this.Center + this.cube.Bounds.GetZVector() * 3; }   //Stay away 10X the width of cube
        }
    }
}
