using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Projection
{
    class Camera
    {
        private Matrix viewMatrix, projectionMatrix;
        public Matrix ViewMatrix { get { return viewMatrix; } }
        public Matrix ProjectionMatrix { get { return projectionMatrix; } }

        private Vector3 position, up, forward;
        public Vector3 Position { get { return position; } }
        public Vector3 Up { get { return up; } }
        public Vector3 Forward { get { return forward; } }

        public Dictionary<string, string> DebugMessages { get; private set; }
        public Dictionary<string, Tuple<VertexPositionColor>> DebugLines { get; private set; }

        public Camera(Vector3 position, Vector3 lookAt, Vector3 cameraUp, float aspectRatio, float nearClip, float farClip)
        {
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, nearClip, farClip);
            viewMatrix = Matrix.CreateLookAt(position, lookAt, cameraUp);
            
            this.position = position;
            this.up = cameraUp;
            this.forward = Vector3.Normalize(lookAt - position);

            this.DebugMessages = new Dictionary<string, string>();
            this.DebugLines = new Dictionary<string, Tuple<VertexPositionColor>>();
        }

        public void Rotate(Vector3 deltaRotation)
        {
            var rotation = Matrix.CreateFromYawPitchRoll(deltaRotation.Y, deltaRotation.X, deltaRotation.Z);

            up = Vector3.Transform(up, rotation);
            forward = Vector3.Transform(forward, rotation);

            viewMatrix = Matrix.CreateLookAt(position, position + forward, up);
        }

        public void MoveTo(Vector3 deltaPosition)
        {
            position += deltaPosition;
            viewMatrix = Matrix.CreateLookAt(position, position + forward, up);
        }

        public Vector3 GetMouseProjectionOnArcBall(Vector3 normalizedMouseVector, Vector3 aroundTarget)
        {
            var normalizedRelativePosition = Vector3.Normalize(position - aroundTarget);
            var mouseVectorTranslated =
                 normalizedRelativePosition * normalizedMouseVector.Z
                + up * normalizedMouseVector.Y
                + Vector3.Cross(up, normalizedRelativePosition) * normalizedMouseVector.X;

            return mouseVectorTranslated;
        }

        public void Rotate(ref Vector3 startVector, ref Vector3 endVector, Vector3 aroundTarget)
        {
            var angle = startVector.AngleWith(endVector) * 1f;
            this.DebugMessages["Angle"] = angle.ToString();
            this.DebugMessages["startVector"] = startVector.ToString();
            this.DebugMessages["endVector"] = endVector.ToString();

            if (angle > 0)
            {
                var axis = Vector3.Cross(startVector, endVector);
                axis.Normalize();

                var q = Quaternion.CreateFromAxisAngle(axis, -angle);
                var relativePosition = position - aroundTarget;
                var rotatedRelativePosition = Vector3.Transform(relativePosition, q);

                position = rotatedRelativePosition + aroundTarget;
                up = Vector3.Transform(up, q);
                //forward = Vector3.Normalize(aroundTarget - position); 
                forward = Vector3.Transform(forward, q);

                endVector = Vector3.Transform(endVector, q); //new Vector3(endVector.X, endVector.Y, endVector.Z);
                startVector = new Vector3(endVector.X, endVector.Y, endVector.Z);


                //if (Vector3.Distance(position, aroundTarget) > 11)
                //    Debugger.Break();

                this.viewMatrix = Matrix.CreateLookAt(position, forward + position, up);
                this.DebugMessages["rotatedUp"] = up.ToString();
            }
        }

        public void Rotate(float angle, Vector3 aroundTarget)
        {
            var relativePosition = position - aroundTarget;

            var rotationAxis = Vector3.Cross(relativePosition, up);
            if (rotationAxis.Length() < 1E-2)
                rotationAxis = Vector3.Cross(relativePosition, forward);
            
            rotationAxis.Normalize();
            var rotation = Matrix.CreateFromAxisAngle(rotationAxis, -angle);
            
            var rotatedRelativePosition = Vector3.Transform(relativePosition, rotation);
            position = rotatedRelativePosition + aroundTarget;

            up = Vector3.Transform(up, rotation);
            forward = Vector3.Transform(forward, rotation);

            viewMatrix = Matrix.CreateLookAt(position, forward + position, up);
        }
    }
}
