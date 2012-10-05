using System;
using System.Collections.Generic;
using System.Linq;
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
            camera = new Camera(new Vector3(10f, 12f, 0.5f), 0, 0, GraphicsDevice.Viewport.AspectRatio, 0.05f, 100f);
            effect = new BasicEffect(GraphicsDevice);
            object3D = new Object3D(GraphicsDevice, SpaceSize);

            base.Initialize();
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

            const float moveScale = 1.5f;
            const float rotateScale = MathHelper.PiOver2;

            var elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var keyState = Keyboard.GetState();

            if (keyState.IsKeyDown(Keys.LeftControl) || keyState.IsKeyDown(Keys.RightControl))
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.MoveTo(null, null, MathHelper.WrapAngle(rotateScale * elapsed));
                if (keyState.IsKeyDown(Keys.Left))
                    camera.MoveTo(null, null, MathHelper.WrapAngle(-rotateScale * elapsed));
                if (keyState.IsKeyDown(Keys.Up))
                    camera.MoveTo(null, MathHelper.WrapAngle(rotateScale * elapsed), null);
                if (keyState.IsKeyDown(Keys.Down))
                    camera.MoveTo(null, MathHelper.WrapAngle(-rotateScale * elapsed), null);
            }
            else if (keyState.IsKeyDown(Keys.LeftShift) || keyState.IsKeyDown(Keys.RightShift))
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.MoveTo(new Vector3(moveScale * elapsed, 0, 0), null, null);
                if (keyState.IsKeyDown(Keys.Left))
                    camera.MoveTo(new Vector3(-moveScale * elapsed, 0, 0), null, null);
                if (keyState.IsKeyDown(Keys.Up))
                    camera.MoveTo(new Vector3(0, moveScale * elapsed, 0), null, null);
                if (keyState.IsKeyDown(Keys.Down))
                    camera.MoveTo(new Vector3(0, -moveScale * elapsed, 0), null, null);
            }
            else
            {
                if (keyState.IsKeyDown(Keys.Right))
                    camera.MoveTo(null, null, MathHelper.WrapAngle(rotateScale * elapsed));
                if (keyState.IsKeyDown(Keys.Left))
                    camera.MoveTo(null, null, MathHelper.WrapAngle(-rotateScale * elapsed));
                if (keyState.IsKeyDown(Keys.Up))
                    camera.MoveTo(new Vector3(0, 0, moveScale * elapsed), null, null);
                if (keyState.IsKeyDown(Keys.Down))
                    camera.MoveTo(new Vector3(0, 0, -moveScale * elapsed), null, null);
            }

            base.Update(gameTime);
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

            base.Draw(gameTime);
        }
    }
}
