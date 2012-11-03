using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Projection
{
    public abstract class Object3D
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Vector3 Position { get; private set; }
        public Vector3 Up { get; private set; }
        public Vector3 Forward { get; private set; }

        public Vector3 Bounds { get; protected set; }

        protected Object3D(GraphicsDevice graphicsDevice, Vector3 position, Vector3 lookAt, Vector3 up)
        {
            this.GraphicsDevice = graphicsDevice;

            this.SetPosition(position);
            this.SetOrientation(up, Vector3.Normalize(lookAt - position));
            ResetViewMatrix();
        }

        protected void SetPosition(Vector3 position)
        {
            position.Validate();
            this.Position = position;
            ResetViewMatrix();
        }

        protected void SetOrientation(Vector3 up, Vector3 forward)
        {
            this.Up = up;
            this.Forward = forward;
            ResetViewMatrix();
        }

        #region Derived class interface

        public abstract bool RequiresUpdate { get; }
        public virtual void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyState, List<Object3D> objects)
        {
            if (this.RequiresUpdate)
                throw new Exception("Derived class requires update but has not implemented it");
            else
                throw new Exception("Update should not be called on this object because it does not require it");
        }
        public virtual void Draw()
        {
            //for derived class
        }
        public virtual void LoadContent(ContentManager content)
        {
            //for derived class
        }

        #endregion 

        protected void ResetViewMatrix()
        {
            this.viewMatrix = null;
        }

        private Matrix? viewMatrix;
        public Matrix ViewMatrix
        {
            get
            {
                if (viewMatrix == null)
                    viewMatrix = Matrix.CreateLookAt(this.Position, this.Position + this.Forward, this.Up);

                return viewMatrix.Value;
            }
        }

        public void MoveTo(Vector3 deltaPosition)
        {
            this.SetPosition(this.Position + deltaPosition);
        }

        public Vector3 GetMouseProjectionOnArcBall(Vector3 normalizedMouseVector, Vector3 aroundTarget)
        {
            var normalizedRelativePosition = Vector3.Normalize(Position - aroundTarget);
            var mouseVectorTranslated =
                 normalizedRelativePosition * normalizedMouseVector.Z
                + this.Up * normalizedMouseVector.Y
                + Vector3.Cross(this.Up, normalizedRelativePosition) * normalizedMouseVector.X;

            return mouseVectorTranslated;
        }

        public Quaternion Rotate(Vector3 axis, float angle, Vector3 aroundTarget)
        {
            var rotation = Quaternion.CreateFromAxisAngle(axis, -angle);
            this.Rotate(rotation, aroundTarget);

            return rotation;
        }

        public void Rotate(Quaternion rotation)
        {
            this.SetOrientation(Vector3.Transform(Up, rotation), Vector3.Transform(Forward, rotation));
        }

        public void Rotate(Quaternion rotation, Vector3 aroundTarget)
        {
            var relativePosition = Position - aroundTarget;
            var rotatedRelativePosition = Vector3.Transform(relativePosition, rotation);
            this.SetPosition(rotatedRelativePosition + aroundTarget);
            this.Rotate(rotation);
        }

        public void Rotate(Vector3 deltaRotation)
        {
            var rotation = Quaternion.CreateFromYawPitchRoll(deltaRotation.Y, deltaRotation.X, deltaRotation.Z);
            this.Rotate(rotation);
        }

        public void Rotate(Vector3 deltaRotation, Vector3 aroundTarget)
        {
            var rotation = Quaternion.CreateFromYawPitchRoll(deltaRotation.Y, deltaRotation.X, deltaRotation.Z);
            this.Rotate(rotation, aroundTarget);
        }

        public bool Rotate(ref Vector3 startArcBallVector, ref Vector3 endArcBallVector, Vector3 aroundTarget)
        {
            var angle = startArcBallVector.AngleWith(endArcBallVector) * 1f;
            if (angle > 0)
            {
                var axis = Vector3.Cross(startArcBallVector, endArcBallVector);
                if (!axis.IsValid())    //If startArcBallVector and endArcBallVector does not have angle then Cross returns NaN
                    return false;

                axis.Normalize();

                var rotation = this.Rotate(axis, angle, aroundTarget);

                endArcBallVector = Vector3.Transform(endArcBallVector, rotation);
                startArcBallVector = new Vector3(endArcBallVector.X, endArcBallVector.Y, endArcBallVector.Z);

                return true;
            }
            else return false;
        }

        
        #region Lazy debug properties
        private Dictionary<string, string> debugMessages;
        public Dictionary<string, string> DebugMessages
        {
            get
            {
                if (debugMessages == null)
                    debugMessages = new Dictionary<string, string>();
                return debugMessages;
            }
        }

        private Dictionary<string, Tuple<VertexPositionColor>> debugLines;

        public Dictionary<string, Tuple<VertexPositionColor>> DebugLines
        {
            get
            {
                if (debugLines == null)
                    debugLines = new Dictionary<string, Tuple<VertexPositionColor>>();
                return debugLines;
            }
        }
#endregion
    }
}