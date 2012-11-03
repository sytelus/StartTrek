using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using ButtonState = Microsoft.Xna.Framework.Input.ButtonState;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Projection
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        
        Camera camera;
        List<Object3D> objects = new List<Object3D>();

        private Vector3 rotationOrigin = Vector3.Zero;

        Effect effect;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private readonly Vector2 fontPos  = new Vector2(1.0f, 1.0f);
        private float arcBallRadius, screenWidth, screenHeight;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.AllowUserResizing = true;
            var form = (Form)Control.FromHandle(Window.Handle);
            form.WindowState = FormWindowState.Maximized;

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
            ResetScene();
            Window_ClientSizeChanged(null, EventArgs.Empty); 
            base.Initialize();
        }

        private void ResetScene()
        {
            objects.Clear();

            var cube = new Cube(graphics.GraphicsDevice
                , position: rotationOrigin    //We'll rotate around origin so place object there
                , bounds: new Vector3(20, 20, 20));

            var cameraPosition = cube.Position - cube.Bounds.GetZVector()*5; //Stay away 10X the width of cube
            var cameraUp = Vector3.Cross(Vector3.Normalize(cameraPosition - cube.Position), Vector3.Right);
            if (cameraUp.LengthSquared() < 1E-02)
                cameraUp = Vector3.Cross(Vector3.Normalize(cameraPosition - cube.Position), Vector3.Up);

            if (camera == null || camera.Position != cameraPosition)
                camera = new Camera(graphics.GraphicsDevice, cameraPosition, cube.Position, cameraUp, graphics.GraphicsDevice.Viewport.AspectRatio, 0.05f, 1E+5f);

            objects.Add(cube);
            objects.Add(camera);
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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            HandleKeyboardInput(gameTime);
            HandleMouseInput(gameTime);

            if ((userActivityState & UserActivityState.Rotate) == UserActivityState.Rotate)
                camera.Rotate(ref mouseRotationStart, ref mouseRotationEnd, rotationOrigin);

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
                    mouseRotationStart = camera.GetMouseProjectionOnArcBall(mouseStartVector, rotationOrigin);
                    mouseRotationEnd = mouseRotationStart;
                    mouseDownCount++;
                }
                else //Mouse move while left down
                {
                    var mouseEndVector = GetMouseArcBallVector(mouseState);
                    mouseRotationEnd = camera.GetMouseProjectionOnArcBall(mouseEndVector, rotationOrigin);
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
            const float moveScale = 0.07f;
            const float rotateScale = MathHelper.PiOver2 / 100;
            const float rotateScale2 = MathHelper.PiOver2 / 1000;

            var elapsed = (float) gameTime.ElapsedGameTime.TotalSeconds;
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.R))
                ResetScene();

            if (keyState.IsKeyDown(Keys.LeftControl) || keyState.IsKeyDown(Keys.RightControl))
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.Rotate(new Vector3(0, rotateScale2, 0), rotationOrigin); 
                if (keyState.IsKeyDown(Keys.Left))
                    camera.Rotate(new Vector3(0, -rotateScale2, 0), rotationOrigin);
                if (keyState.IsKeyDown(Keys.Up))
                    camera.Rotate(new Vector3(rotateScale2, 0, 0), rotationOrigin);
                if (keyState.IsKeyDown(Keys.Down))
                    camera.Rotate(new Vector3(-rotateScale2, 0, 0), rotationOrigin); 
            }
            else if (keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift))
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.Rotate(new Vector3(0, rotateScale, 0));
                if (keyState.IsKeyDown(Keys.Left))
                    camera.Rotate(new Vector3(0, -rotateScale, 0));
                if (keyState.IsKeyDown(Keys.Up))
                    camera.Rotate(new Vector3(rotateScale, 0, 0));
                if (keyState.IsKeyDown(Keys.Down))
                    camera.Rotate(new Vector3(-rotateScale, 0, 0));
            }
            else
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.MoveTo(new Vector3(-moveScale, 0, 0));
                if (keyState.IsKeyDown(Keys.Left))
                    camera.MoveTo(new Vector3(moveScale, 0, 0));
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
            infoText.AppendLine(string.Format("Camera Up: {0}, {1}, {2}, {3}", camera.ViewMatrix.Up.X, camera.ViewMatrix.Up.Y, camera.ViewMatrix.Up.Z, camera.ViewMatrix.Up.Length()));
            infoText.AppendLine(string.Format("Camera Right: {0}, {1}, {2}, {3}", camera.ViewMatrix.Right.X, camera.ViewMatrix.Right.Y, camera.ViewMatrix.Right.Z, camera.ViewMatrix.Right.Length()));
            infoText.AppendLine(string.Format("Mouse: {0}, {1}", mouseState.X, mouseState.Y));
            infoText.AppendLine(string.Format("Distance: {0}", (rotationOrigin - camera.Position).Length()));
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

                foreach(var object3D in this.objects)
                    object3D.Draw();
            }
           
            DrawInfoText();

            base.Draw(gameTime);
        }
    }
}
