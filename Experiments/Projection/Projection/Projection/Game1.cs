using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private Scene scene;
        Camera camera;
        private IObject3DControls cameraControls;

        Effect effect;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            SetupGameWindow();

            this.IsMouseVisible = true;
        }

        private void SetupGameWindow()
        {
            Window.ClientSizeChanged += Window_ClientSizeChanged;
            Window.AllowUserResizing = true;
            var form = (Form) Control.FromHandle(Window.Handle);
            form.WindowState = FormWindowState.Maximized;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            ResetScene(false);
            base.Initialize();
        }

        Vector3 rotationOrigin = Vector3.Zero;
        private enum SceneType {ArrowField = 1, Cube = 0}
        private SceneType currentScene = (SceneType)0;
        private void ResetScene(bool loadContent)
        {
            switch (currentScene)
            {
                case SceneType.Cube:
                    this.scene = new SingleCubeScene(this.graphics.GraphicsDevice, rotationOrigin, 20);
                    break;
                case SceneType.ArrowField:
                    this.scene = new ArrowFieldScene(this.graphics.GraphicsDevice, rotationOrigin);
                    break;
                default:
                    throw new Exception(string.Format("Unsupported scene type {0}", currentScene));
            }

            //Create camera
            var cameraPosition = scene.SuggestedInitialCameraPosition;
            var cameraUp = Vector3.Normalize((cameraPosition - rotationOrigin).SafeCross(Vector3.Right, Vector3.Up));
            camera = new Camera(graphics.GraphicsDevice, cameraPosition, rotationOrigin, cameraUp, graphics.GraphicsDevice.Viewport.AspectRatio, 0.05f, 1E+5f);

            //Create help/debug text
            var screenText = new ScreenText(graphics.GraphicsDevice, new Vector3(1.0f, 1.0f, 0), camera);

            //Create controls
            cameraControls = new ArcBallControls(camera, rotationOrigin, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            this.scene.AddObject(camera);
            this.scene.AddObject(screenText);

            if (loadContent)
                this.scene.LoadContent(this.Content);
        }


        protected override void LoadContent()
        {
            effect = Content.Load<Effect>(@"ReallyBasicEffect");

            this.scene.LoadContent(this.Content);
            
            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var keyState = Keyboard.GetState();

            //Keyboard shortcuts
            HandleKeyboardShortcuts(keyState);

            //Update objects
            scene.Update(gameTime, mouseState, keyState);

            //Update controls
            cameraControls.Update(gameTime, mouseState, keyState, scene.Objects);

            base.Update(gameTime);
        }

        private Stopwatch keyDelay = Stopwatch.StartNew();
        private void HandleKeyboardShortcuts(KeyboardState keyState)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (keyState.IsKeyDown(Keys.R) && keyDelay.ElapsedMilliseconds > 400)
            {
                ResetScene(true);
                keyDelay.Restart();
            }
            if (keyState.IsKeyDown(Keys.S) && keyDelay.ElapsedMilliseconds > 400)
            {
                currentScene = (SceneType) (((int) currentScene + 1)%2);
                ResetScene(true);
                keyDelay.Restart();
            }
        }

        private void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            if (cameraControls != null)
                cameraControls.UpdateScreenDimentions(graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            //These gets reset by SpriteBuffer so we need to set these to default
            graphics.GraphicsDevice.RasterizerState = RasterizerState.CullNone;
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

                scene.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
