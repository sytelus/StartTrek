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
        List<Object3D> updatableObjects = new List<Object3D>();
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
        private void ResetScene(bool loadContent)
        {
            this.objects.Clear();

            //Create cube
            var cube = new Cube(graphics.GraphicsDevice
                , position: rotationOrigin    //We'll rotate around origin so place object there
                , bounds: new Vector3(20, 20, 20));

            //Create camera
            var cameraPosition = cube.Position - cube.Bounds.GetZVector()*5; //Stay away 10X the width of cube
            var cameraUp = Vector3.Normalize((cameraPosition - cube.Position).SafeCross(Vector3.Right, Vector3.Up));
            camera = new Camera(graphics.GraphicsDevice, cameraPosition, cube.Position, cameraUp, graphics.GraphicsDevice.Viewport.AspectRatio, 0.05f, 1E+5f);

            //Create help/debug text
            var screenText = new ScreenText(graphics.GraphicsDevice, new Vector3(1.0f, 1.0f, 0), camera);

            //Create controls
            cameraControls = new ArcBallControls(camera, rotationOrigin, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);
 
            this.objects.Add(cube);
            this.objects.Add(camera);
            this.objects.Add(screenText);

            this.SetUpdatableObjects();

            if (loadContent)
            {
                foreach (var object3D in objects)
                {
                    object3D.LoadContent(this.Content);
                }
            }
        }

        private void SetUpdatableObjects()
        {
            this.updatableObjects = objects.Where(o => o.RequiresUpdate).ToList();
        }

        protected override void LoadContent()
        {
            effect = Content.Load<Effect>(@"ReallyBasicEffect");

            foreach (var object3D in this.objects)
                object3D.LoadContent(Content);

            base.LoadContent();
        }

        protected override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var keyState = Keyboard.GetState();

            //Keyboard shortcuts
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            if (keyState.IsKeyDown(Keys.R))
                ResetScene(true);

            //Update objects
            foreach (var updatableObject in updatableObjects)
                updatableObject.Update(gameTime, mouseState, keyState, this.objects);

            //Update controls
            cameraControls.Update(gameTime, mouseState, keyState, this.objects);

            base.Update(gameTime);
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

                foreach(var object3D in this.objects)
                    object3D.Draw();
            }

            base.Draw(gameTime);
        }
    }
}
