using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Projection
{
    class Camera
    {
        private Vector3 position = Vector3.Zero;
        private Vector3 rotation = Vector3.Zero;

        private readonly Vector3 baseCameraReference = new Vector3(0, 0, 1);

        private Matrix? rotationMatrixCached;
        private Matrix? viewMatrix;
        public Matrix Projection { get; private set; }

        public Camera(Vector3 position, Vector3 rotation, float aspectRatio, float nearClip, float farClip)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearClip, farClip);
            this.MoveTo(position, rotation);
        }

        public void MoveTo(Vector3? deltaCameraPosition, Vector3? deltaRotation)
        {
            if (deltaRotation.HasValue)
            {
                rotation += deltaRotation.Value;
                viewMatrix = null;
                rotationMatrixCached = null;
            }

            if (deltaCameraPosition != null)
            {
                var rotationMatrix = GetRotationMatrix();
                var delaWorldPosition = Vector3.Transform(deltaCameraPosition.Value, rotationMatrix);
                var newPosition = this.Position + delaWorldPosition;

                //if (newPosition.X >= 0 && newPosition.X <= Game1.SpaceSize.X && newPosition.Z >= 0 && newPosition.Z < Game1.SpaceSize.Z && newPosition.Y >= 0 && newPosition.Y < Game1.SpaceSize.Y)
                {
                    this.position = newPosition;
                    viewMatrix = null;
                }
            }
        }

        private Matrix GetRotationMatrix()
        {
            if (rotationMatrixCached.HasValue)
                return rotationMatrixCached.Value;
            else
            {
                rotationMatrixCached = Matrix.CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);
                return rotationMatrixCached.Value;
            }
        }

        private Vector3 lookAt;
        private Matrix CreateViewMatrix()
        {
            var rotationMatrix = GetRotationMatrix();
            var lookAtOffset = Vector3.Transform(baseCameraReference, rotationMatrix);
            lookAt = position + lookAtOffset;
            return Matrix.CreateLookAt(Position, lookAt, Vector3.Up);
        }

        public Vector3 LookAt
        {
            get { return this.lookAt; }
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return rotation;
            }
        }
        
        public Matrix View
        {
            get
            {
                if (viewMatrix == null)
                    viewMatrix = CreateViewMatrix();

                return viewMatrix.Value;
            }
        }
    }
}
