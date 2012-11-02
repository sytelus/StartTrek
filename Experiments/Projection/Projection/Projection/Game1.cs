using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Projection
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        Camera camera;
        Object3D object3D;
        Effect effect;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private readonly Vector2 fontPos  = new Vector2(1.0f, 1.0f);
        private float arcBallRadius, screenWidth, screenHeight;

        public static readonly Vector3 SpaceSize = new Vector3(20,20,20);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += Window_ClientSizeChanged;

            this.IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            object3D = new Object3D(graphics.GraphicsDevice, SpaceSize, new Vector3(0, 0, SpaceSize.Z / 2));
            ResetCamera();

            Window_ClientSizeChanged(null, EventArgs.Empty); 
            
            base.Initialize();
        }

        private readonly Vector3 cameraStartingPos = Vector3.Zero;
        private void ResetCamera()
        {
            if (camera == null || camera.Position != cameraStartingPos)
                camera = new Camera(cameraStartingPos, object3D.Center, Vector3.Up, graphics.GraphicsDevice.Viewport.AspectRatio, 0.05f, 1000f);
        }

        protected override void LoadContent()
        {
            effect = Content.Load<Effect>(@"ReallyBasicEffect");

            spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"InfoFont");

            base.LoadContent();
        }


        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            HandleKeyboardInput(gameTime);
            HandleMouseInput(gameTime);

            if ((userActivityState & UserActivityState.Rotate) == UserActivityState.Rotate)
                camera.Rotate(ref mouseRotationStart, ref mouseRotationEnd, object3D.Center);

            base.Update(gameTime);
        }

        [Flags]
        enum UserActivityState
        {
            None = 0, Rotate = 1, Zoom = 2, Pan = 4
        }

        private Vector3 mouseRotationStart, mouseRotationEnd;
        private UserActivityState userActivityState = UserActivityState.None;
        private int mouseDownCount = 0;
        private void HandleMouseInput(GameTime gameTime1)
        {
            var mouseState = Mouse.GetState();

            camera.DebugMessages["userActivityState"] = userActivityState.ToString();
            camera.DebugMessages["mouseDownCount"] = mouseDownCount.ToString();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if ((userActivityState & UserActivityState.Rotate) != UserActivityState.Rotate)  //Mouse down event
                {
                    userActivityState |= UserActivityState.Rotate;
                    var mouseStartVector = GetMouseArcBallVector(mouseState);
                    camera.DebugMessages["mouseStartVector"] = mouseStartVector.ToString();
                    mouseRotationStart = camera.GetMouseProjectionOnArcBall(mouseStartVector, object3D.Center);
                    mouseRotationEnd = mouseRotationStart;
                    mouseDownCount++;
                }
                else //Mouse move while left down
                {
                    var mouseEndVector = GetMouseArcBallVector(mouseState);
                    camera.DebugMessages["mouseEndVector"] = mouseEndVector.ToString();
                    mouseRotationEnd = camera.GetMouseProjectionOnArcBall(mouseEndVector, object3D.Center);
                }
            }
            else
            {
                userActivityState &= ~UserActivityState.Rotate;
            }
        }

        private Vector3 GetMouseArcBallVector(MouseState mouseState)
        {
            var mouseArcBallVector = new Vector3((mouseState.X - 0.5f * this.screenWidth) / arcBallRadius
                , (0.5f * screenHeight - mouseState.Y) / arcBallRadius, 0);

            var length = mouseArcBallVector.Length();

            if (length > 1)
                mouseArcBallVector.Normalize();
            else
                mouseArcBallVector.Z = (float) Math.Sqrt(1f - length*length);

            return mouseArcBallVector;
        }

        private void HandleKeyboardInput(GameTime gameTime)
        {
            const float moveScale = 1.5f / 100;
            const float rotateScale = MathHelper.PiOver2 / 100;
            const float rotateScale2 = MathHelper.PiOver2 / 1000;

            var elapsed = (float) gameTime.ElapsedGameTime.TotalSeconds;
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.R))
                ResetCamera();

            if (keyState.IsKeyDown(Keys.LeftControl) || keyState.IsKeyDown(Keys.RightControl))
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.Rotate(rotateScale2, object3D.Center);
                if (keyState.IsKeyDown(Keys.Left))
                    camera.Rotate(-rotateScale2, object3D.Center);
                if (keyState.IsKeyDown(Keys.Up))
                    camera.Rotate(rotateScale2, object3D.Center);
                if (keyState.IsKeyDown(Keys.Down))
                    camera.Rotate(-rotateScale2, object3D.Center);
            }
            else if (keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift))
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.Rotate(new Vector3(0, MathHelper.WrapAngle(rotateScale), 0));
                if (keyState.IsKeyDown(Keys.Left))
                    camera.Rotate(new Vector3(0, MathHelper.WrapAngle(-rotateScale), 0));
                if (keyState.IsKeyDown(Keys.Up))
                    camera.Rotate(new Vector3(MathHelper.WrapAngle(rotateScale), 0, 0));
                if (keyState.IsKeyDown(Keys.Down))
                    camera.Rotate(new Vector3(MathHelper.WrapAngle(-rotateScale), 0, 0));
            }
            else
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.MoveTo(new Vector3(moveScale, 0, 0));
                if (keyState.IsKeyDown(Keys.Left))
                    camera.MoveTo(new Vector3(-moveScale, 0, 0));
                if (keyState.IsKeyDown(Keys.Up))
                    camera.MoveTo(new Vector3(0, 0, moveScale));
                if (keyState.IsKeyDown(Keys.Down))
                    camera.MoveTo(new Vector3(0, 0, -moveScale));
            }
        }

        private void DrawInfoText()
        {
            var mouseState = Mouse.GetState();

            var infoText = new StringBuilder();
            infoText.AppendLine(string.Format("Camera Pos: {0}, {1}, {2}", camera.Position.X, camera.Position.Y, camera.Position.Z));
            infoText.AppendLine(string.Format("Camera Forward: {0}, {1}, {2}, {3}", camera.ViewMatrix.Forward.X, camera.ViewMatrix.Forward.Y, camera.ViewMatrix.Forward.Z, camera.ViewMatrix.Forward.Length()));
            infoText.AppendLine(string.Format("Camera Up: {0}, {1}, {2}, {3}", camera.Up.X, camera.Up.Y, camera.Up.Z, camera.Up.Length()));
            infoText.AppendLine(string.Format("Camera Right: {0}, {1}, {2}, {3}", camera.ViewMatrix.Right.X, camera.ViewMatrix.Right.Y, camera.ViewMatrix.Right.Z, camera.ViewMatrix.Right.Length()));
            infoText.AppendLine(string.Format("Mouse: {0}, {1}", mouseState.X, mouseState.Y));
            infoText.AppendLine(string.Format("Distance: {0}", (object3D.Center - camera.Position).Length()));
            infoText.AppendLine(string.Format("Camera Basis Angles: {0}, {1}, {2}", camera.ViewMatrix.Forward.AngleWith(camera.Up, false), camera.Up.AngleWith(camera.ViewMatrix.Left, false), camera.ViewMatrix.Left.AngleWith(camera.ViewMatrix.Forward, false)));

            foreach (var debugMessage in camera.DebugMessages)
            {
                infoText.AppendLine(string.Format("{0}: {1}", debugMessage.Key, debugMessage.Value));
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            spriteBatch.DrawString(spriteFont, infoText.ToString(), fontPos, Color.Yellow);
            spriteBatch.End();
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            screenWidth = graphics.GraphicsDevice.Viewport.Width;
            screenHeight = graphics.GraphicsDevice.Viewport.Height;
            arcBallRadius = (screenWidth + screenHeight)/4;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            //These gets reset by SpriteBuffer so we need to set these to default
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;
            graphics.GraphicsDevice.BlendState = BlendState.Opaque;
            graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            graphics.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            effect.CurrentTechnique = effect.Techniques["MainTechnique"];
            effect.Parameters["World"].SetValue(Matrix.Identity);
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                object3D.Draw(camera);
            }
           
            DrawInfoText();

            base.Draw(gameTime);
        }
    }
}
