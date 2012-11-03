using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Projection
{
    public class ArcBallControls : IObject3DControls
    {
        private readonly Object3D object3D;
        private readonly Vector3 rotationOrigin;

        public float RotateSpeed { get; set; }
        public float PanSpeed { get; set; }
        public float ZoomSpeed { get; set; }
        public float MaxZoom { get; set; }

        public ArcBallControls(Object3D object3D, Vector3 rotationOrigin, float screenWidth, float screenHeight)
        {
            this.object3D = object3D;
            this.rotationOrigin = rotationOrigin;
            
            this.RotateSpeed = 1.5f;
            this.ZoomSpeed = 1f;
            this.MaxZoom = 15f;
            this.PanSpeed = 300f;

            UpdateScreenDimentions(screenWidth, screenHeight);
        }

        private float arcBallRadius, screenWidth, screenHeight;
        public void UpdateScreenDimentions(float screenWidth, float screenHeight)
        {
            this.screenWidth = screenWidth;
            this.screenHeight = screenHeight;
            this.arcBallRadius = (this.screenWidth + this.screenHeight) / 4;
        }

        public void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState, IEnumerable<Object3D> objects)
        {
            HandleKeyboardInput(gameTime, keyboardState);

            HandleLeftMouseButtonInput(gameTime, mouseState);
            HandleMiddleMouseButtonInput(gameTime, mouseState);
            HandleRightMouseButtonInput(gameTime, mouseState);
        }

        #region Mouse events
        [Flags]
        enum MouseActivityState
        {
            None = 0, MouseRotate = 1, MouseZoom = 2, MousePan = 4
        }
        private MouseActivityState mouseActivityState = MouseActivityState.None;

        private Vector3 GetNormalizedMouseCoordinates(MouseState mouseState)
        {
            return new Vector3(-mouseState.X / screenWidth * 0.5f, -mouseState.Y / screenHeight * 0.5f, 0);
        }

        private Vector3 GetMouseArcBallVector(MouseState mouseState)
        {
            var mouseArcBallVector = new Vector3((mouseState.X - 0.5f * this.screenWidth) / arcBallRadius
                , (0.5f * screenHeight - mouseState.Y) / arcBallRadius, 0);

            var length = mouseArcBallVector.Length();

            if (length > 1)
                mouseArcBallVector.Normalize();
            else
                mouseArcBallVector.Z = (float)Math.Sqrt(1f - length * length);

            return mouseArcBallVector;
        }

        private Vector3 mousePanStart, mousePanEnd;
        private void HandleRightMouseButtonInput(GameTime gameTime, MouseState mouseState)
        {
            if (mouseState.RightButton == ButtonState.Pressed)
            {
                if ((mouseActivityState & MouseActivityState.MousePan) != MouseActivityState.MousePan)  //Mouse down event
                {
                    mouseActivityState |= MouseActivityState.MousePan;
                    mousePanStart = GetNormalizedMouseCoordinates(mouseState);
                }
                else //Mouse move while middle down
                {
                    mousePanEnd = GetNormalizedMouseCoordinates(mouseState);
                    var panAmount = (mousePanStart - mousePanEnd) * this.PanSpeed;
                    if (panAmount.LengthSquared() > 0)
                    {
                        object3D.MoveTo(panAmount);
                        mousePanStart = mousePanEnd;
                    }
                }
            }
            else
                mouseActivityState &= ~MouseActivityState.MousePan;
        }

        private float mouseZoomStart, mouseZoomEnd;
        private void HandleMiddleMouseButtonInput(GameTime gameTime, MouseState mouseState)
        {
            if (mouseState.MiddleButton == ButtonState.Pressed)
            {
                if ((mouseActivityState & MouseActivityState.MouseZoom) != MouseActivityState.MouseZoom)  //Mouse down event
                {
                    mouseActivityState |= MouseActivityState.MouseZoom;
                    mouseZoomStart = mouseState.Y;
                }
                else //Mouse move while middle down
                {
                    mouseZoomEnd = mouseState.Y;
                    var zoomFactor = (float) Math.Pow(MaxZoom, (mouseZoomEnd - mouseZoomStart) / screenHeight) * this.ZoomSpeed;
                    if (Math.Abs(zoomFactor - 0) > 1E-3)
                    {
                        object3D.Zoom(zoomFactor, rotationOrigin);
                        mouseZoomStart = mouseZoomEnd;
                    }
                }
            }
            else
                mouseActivityState &= ~MouseActivityState.MouseZoom;
        }

        private Vector3 mouseRotationStart, mouseRotationEnd;
        private void HandleLeftMouseButtonInput(GameTime gameTime, MouseState mouseState)
        {
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if ((mouseActivityState & MouseActivityState.MouseRotate) != MouseActivityState.MouseRotate)  //Mouse down event
                {
                    mouseActivityState |= MouseActivityState.MouseRotate;
                    var mouseVector = GetMouseArcBallVector(mouseState);
                    mouseRotationStart = object3D.GetMouseProjectionOnArcBall(mouseVector, rotationOrigin);
                }
                else //Mouse move while left down
                {
                    var mouseVector = GetMouseArcBallVector(mouseState);
                    mouseRotationEnd = object3D.GetMouseProjectionOnArcBall(mouseVector, rotationOrigin);
                    var rotatedMouseRotationEnd = object3D.Rotate(mouseRotationStart, mouseRotationEnd, this.rotationOrigin, this.RotateSpeed);
                    if (rotatedMouseRotationEnd != null)
                    {
                        mouseRotationEnd = rotatedMouseRotationEnd.Value;
                        mouseRotationStart = mouseRotationEnd;
                    }
                }
            }
            else
                mouseActivityState &= ~MouseActivityState.MouseRotate;
        }
        #endregion

        private void HandleKeyboardInput(GameTime gameTime, KeyboardState keyboardState)
        {
            const float moveSpeed = 0.7f;
            const float tiltSpeed = MathHelper.PiOver2 / 80;
            const float rotateSpeed = MathHelper.PiOver2 / 100;

            if (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl))  //Rotate
            {
                if (keyboardState.IsKeyDown(Keys.Right))
                    object3D.Rotate(new Vector3(0, -rotateSpeed, 0), rotationOrigin);
                if (keyboardState.IsKeyDown(Keys.Left))
                    object3D.Rotate(new Vector3(0, rotateSpeed, 0), rotationOrigin);
                if (keyboardState.IsKeyDown(Keys.Up))
                    object3D.Rotate(new Vector3(-rotateSpeed, 0, 0), rotationOrigin);
                if (keyboardState.IsKeyDown(Keys.Down))
                    object3D.Rotate(new Vector3(rotateSpeed, 0, 0), rotationOrigin);
                if (keyboardState.IsKeyDown(Keys.PageUp))
                    object3D.Rotate(new Vector3(0, 0, rotateSpeed), rotationOrigin);
                if (keyboardState.IsKeyDown(Keys.PageDown))
                    object3D.Rotate(new Vector3(0, 0, -rotateSpeed), rotationOrigin);
            }
            else if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift)) //Tilt
            {
                if (keyboardState.IsKeyDown(Keys.Right))
                    object3D.Rotate(new Vector3(0, tiltSpeed, 0));
                if (keyboardState.IsKeyDown(Keys.Left))
                    object3D.Rotate(new Vector3(0, -tiltSpeed, 0));
                if (keyboardState.IsKeyDown(Keys.Up))
                    object3D.Rotate(new Vector3(tiltSpeed, 0, 0));
                if (keyboardState.IsKeyDown(Keys.Down))
                    object3D.Rotate(new Vector3(-tiltSpeed, 0, 0));
                if (keyboardState.IsKeyDown(Keys.PageUp))
                    object3D.Rotate(new Vector3(0, 0, tiltSpeed));
                if (keyboardState.IsKeyDown(Keys.PageDown))
                    object3D.Rotate(new Vector3(0, 0, -tiltSpeed));
            }
            else                                                                                //Pan
            {
                if (keyboardState.IsKeyDown(Keys.Right))
                    object3D.MoveTo(new Vector3(moveSpeed, 0, 0));
                if (keyboardState.IsKeyDown(Keys.Left))
                    object3D.MoveTo(new Vector3(-moveSpeed, 0, 0));
                if (keyboardState.IsKeyDown(Keys.Up))
                    object3D.MoveTo(new Vector3(0, 0, moveSpeed));
                if (keyboardState.IsKeyDown(Keys.Down))
                    object3D.MoveTo(new Vector3(0, 0, -moveSpeed));
                if (keyboardState.IsKeyDown(Keys.PageUp))
                    object3D.MoveTo(new Vector3(0, -moveSpeed, 0));
                if (keyboardState.IsKeyDown(Keys.PageDown))
                    object3D.MoveTo(new Vector3(0, moveSpeed, 0));
            }
        }

    }
}
