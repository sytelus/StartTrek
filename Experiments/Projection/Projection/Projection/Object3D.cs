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

        public string Name { get; protected set; }

        protected Object3D(GraphicsDevice graphicsDevice, string name, Vector3 position, Vector3 lookAt, Vector3 up)
        {
            this.GraphicsDevice = graphicsDevice;
            this.Name = name;
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
        public virtual void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState, List<Object3D> objects)
        {
            if (this.RequiresUpdate)
                throw new Exception("Derived class requires update but has not implemented it");
            else
                throw new Exception("Update should not be called on this object because it does not require it");
        }
        public virtual void Draw()
        {
            DrawDebugLines();
        }
        public virtual void LoadContent(ContentManager content)
        {
            //for derived class
        }
        public virtual IEnumerable<VertexPositionColor> GetDebugVertices()
        {
            return Enumerable.Empty<VertexPositionColor>();
            //for derived class
        }
        #endregion 

        protected void DrawDebugLines()
        {
            if (this.debugLines != null && this.DebugLevel > 0)
            {
                var vertices = this.debugLines.SelectMany(l => l.Value).ToArray();
                this.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vertices, 0, vertices.Length/2);
            }
        }

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

        public Vector3 MoveTo(Vector3 deltaPosition)
        {
            var displacement = this.Right()*deltaPosition.X + this.Up*deltaPosition.Y + this.Forward*deltaPosition.Z;
            this.SetPosition(this.Position + displacement);

            return displacement;
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

        public void Zoom(float zoomFactor, Vector3 atTarget)
        {
            var relativePosition = this.Position - atTarget;
            relativePosition = relativePosition.GetWithNewLength(relativePosition.Length()*zoomFactor);
            this.SetPosition(relativePosition + atTarget);
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

        public Quaternion Rotate(Vector3 deltaRotation)
        {
            var rotation = GetRotation(deltaRotation);

            this.Rotate(rotation);

            return rotation;
        }

        private Quaternion GetRotation(Vector3 deltaRotation)
        {
            var rotation = Quaternion.CreateFromAxisAngle(this.Right(), deltaRotation.X)
                           * Quaternion.CreateFromAxisAngle(this.Up, deltaRotation.Y)
                           * Quaternion.CreateFromAxisAngle(this.Forward, deltaRotation.Z);
            return rotation;
        }

        public Vector3 Right()
        {
            return Vector3.Cross(this.Up, this.Forward);
        }

        public Quaternion Rotate(Vector3 deltaRotation, Vector3 aroundTarget)
        {
            var rotation = GetRotation(deltaRotation);
            this.Rotate(rotation, aroundTarget);
            return rotation;
        }

        public Vector3? Rotate(Vector3 startArcBallVector, Vector3 endArcBallVector, Vector3 aroundTarget, float rotateSpeed)
        {
            var angle = startArcBallVector.AngleWith(endArcBallVector) * rotateSpeed;
            if (angle > 0)
            {
                var axis = Vector3.Cross(startArcBallVector, endArcBallVector);
                if (!axis.IsValid())    //If startArcBallVector and endArcBallVector does not have angle then Cross returns NaN
                    return null;

                if (this.DebugLevel > 0)
                {
                    this.SetDebugLine("startArcBallVector", aroundTarget, aroundTarget + startArcBallVector * 200, Color.Azure);
                    this.SetDebugLine("endArcBallVector", aroundTarget, aroundTarget + endArcBallVector * 200, Color.Blue);
                }

                axis.Normalize();

                var rotation = this.Rotate(axis, angle, aroundTarget);

                return Vector3.Transform(endArcBallVector, rotation);
            }
            else return null;
        }

        
        #region Lazy debug properties
        private Dictionary<string, string> debugMessages;
        public void SetDebugMessage(string key, string message)
        {
            if (debugMessages == null)
                debugMessages = new Dictionary<string, string>();

            debugMessages[key] = message;
        }
        public Dictionary<string, string> GetDebugMessages()
        {
            return debugMessages;
        }

        private Dictionary<string, VertexPositionColor[]> debugLines;
        public void SetDebugLine(string key, Vector3 startPoint, Vector3 endPoint, Color color)
        {
            if (debugLines == null)
                debugLines = new Dictionary<string, VertexPositionColor[]>();

            var startVertex = new VertexPositionColor(startPoint, color);
            var endVertex = new VertexPositionColor(endPoint, color);
            debugLines[key] = new [] {startVertex, endVertex};
        }
        public Dictionary<string, VertexPositionColor[]> GetDebugLines()
        {
            return debugLines;
        }
        public enum DebugLevelType
        {
            None = 0, Basic = 1, Verbose = 2
        }

        public DebugLevelType DebugLevel { get; set; } 
        #endregion
        
    }
}