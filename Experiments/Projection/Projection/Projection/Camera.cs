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
        private float rotationX, rotationY;

        private readonly Vector3 baseCameraReference = new Vector3(0, 0, 1);

        private Matrix? rotationMatrixCached;
        private Matrix? viewMatrix;
        public Matrix Projection { get; private set; }

        public Camera(Vector3 position, float rotationX, float rotationY, float aspectRatio, float nearClip, float farClip)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearClip, farClip);
            this.MoveTo(position, rotationX, rotationY);
        }

        public void MoveTo(Vector3? deltaCameraPosition, float? deltaXRotation, float? deltaYRotation)
        {
            if (deltaXRotation.HasValue)
            {
                rotationX += deltaXRotation.Value;
                viewMatrix = null;
                rotationMatrixCached = null;
            }
            if (deltaYRotation.HasValue)
            {
                rotationY += deltaYRotation.Value;
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
                rotationMatrixCached = Matrix.CreateRotationX(rotationX) * Matrix.CreateRotationY(rotationY);
                return rotationMatrixCached.Value;
            }
        }

        private Matrix CreateViewMatrix()
        {
            var rotationMatrix = GetRotationMatrix();
            var lookAtOffset = Vector3.Transform(baseCameraReference, rotationMatrix);
            var lookAt = position + lookAtOffset;
            return Matrix.CreateLookAt(Position, lookAt, Vector3.Up);
        }

        public Vector3 Position
        {
            get
            {
                return position;
            }
        }

        public float RotationX
        {
            get
            {
                return rotationX;
            }
        }

        public float RotationY
        {
            get
            {
                return rotationY;
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
