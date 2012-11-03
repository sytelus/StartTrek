using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projection
{
    public class Camera : Object3D
    {
        public Matrix ProjectionMatrix { get; protected set; }

        public Camera(GraphicsDevice graphicsDevice, Vector3 position, Vector3 lookAt, Vector3 up, float aspectRatio, float nearClip, float farClip)
            : base(graphicsDevice, position, lookAt, up)
        {
            Bounds = new Vector3(nearClip, nearClip, nearClip);
            ProjectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearClip, farClip);
        }

        public override bool RequiresUpdate
        {
            get { return false; }
        }
    }
}
