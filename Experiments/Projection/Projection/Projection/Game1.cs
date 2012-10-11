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
        BasicEffect effect;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private readonly Vector2 fontPos  = new Vector2(1.0f, 1.0f);

        public static readonly Vector3 SpaceSize = new Vector3(20,20,20);

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            ResetCamera();
            effect = new BasicEffect(GraphicsDevice);
            object3D = new Object3D(GraphicsDevice, SpaceSize);

            base.Initialize();
        }

        private readonly Vector3 cameraStartingPos = new Vector3(0f, 0f, 0f);
        private void ResetCamera()
        {
            if (camera == null || camera.Position != cameraStartingPos)
                camera = new Camera(cameraStartingPos, Vector3.Zero, GraphicsDevice.Viewport.AspectRatio, float.Epsilon, float.MaxValue);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
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

            HandleUserInput(gameTime);

            base.Update(gameTime);
        }

        private void HandleUserInput(GameTime gameTime)
        {
            const float moveScale = 1.5f;
            const float rotateScale = MathHelper.PiOver2;

            var elapsed = (float) gameTime.ElapsedGameTime.TotalSeconds;
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.R))
                ResetCamera();

            if (keyState.IsKeyDown(Keys.LeftControl) || keyState.IsKeyDown(Keys.RightControl))
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.MoveTo(null, new Vector3(0, MathHelper.WrapAngle(rotateScale*elapsed), 0));
                if (keyState.IsKeyDown(Keys.Left))
                    camera.MoveTo(null, new Vector3(0, MathHelper.WrapAngle(-rotateScale * elapsed), 0));
                if (keyState.IsKeyDown(Keys.Up))
                    camera.MoveTo(null, new Vector3(MathHelper.WrapAngle(rotateScale * elapsed), 0, 0));
                if (keyState.IsKeyDown(Keys.Down))
                    camera.MoveTo(null, new Vector3(MathHelper.WrapAngle(-rotateScale * elapsed), 0, 0));
            }
            else if (keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift))
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.MoveTo(null, new Vector3(0, MathHelper.WrapAngle(rotateScale * elapsed), 0));
                if (keyState.IsKeyDown(Keys.Left))
                    camera.MoveTo(null, new Vector3(0, MathHelper.WrapAngle(-rotateScale * elapsed), 0));
                if (keyState.IsKeyDown(Keys.Up))
                    camera.MoveTo(null, new Vector3(MathHelper.WrapAngle(rotateScale * elapsed), 0, 0));
                if (keyState.IsKeyDown(Keys.Down))
                    camera.MoveTo(null, new Vector3(MathHelper.WrapAngle(-rotateScale * elapsed), 0, 0));
            }
            else
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.MoveTo(new Vector3(moveScale * elapsed, 0, 0), null);
                if (keyState.IsKeyDown(Keys.Left))
                    camera.MoveTo(new Vector3(-moveScale * elapsed, 0, 0), null);
                if (keyState.IsKeyDown(Keys.Up))
                    camera.MoveTo(new Vector3(0, 0, moveScale*elapsed), null);
                if (keyState.IsKeyDown(Keys.Down))
                    camera.MoveTo(new Vector3(0, 0, -moveScale*elapsed), null);
            }
        }

        private void DrawInfoText()
        {

            var infoText = new StringBuilder();
            infoText.AppendLine(string.Format("Camera Pos: {0}, {1}, {2}", camera.Position.X, camera.Position.Y, camera.Position.Z));
            infoText.AppendLine(string.Format("Camera Rot: {0}, {1}, {2}", camera.Rotation.X, camera.Rotation.Y, camera.Rotation.Z));
            infoText.AppendLine(string.Format("Look At: {0}, {1}, {2}", camera.LookAt.X, camera.LookAt.Y, camera.LookAt.Z));

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            spriteBatch.DrawString(spriteFont, infoText.ToString(), fontPos, Color.Yellow);
            spriteBatch.End();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;  // vertex order doesn't matter
            GraphicsDevice.BlendState = BlendState.Opaque;    // use alpha blending
            //GraphicsDevice.DepthStencilState = DepthStencilState.None;  // don't bother with the depth/stencil buffer

            object3D.Draw(camera, effect);
            
            DrawInfoText();

            base.Draw(gameTime);
        }
    }
}
