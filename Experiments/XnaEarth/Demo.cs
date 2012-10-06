//-----------------------------------------------------------------------------
// Copyright (c) 2009-2011 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace XNAEarth
{
    /// <summary>
    /// This Windows XNA application demonstrates how various shader effects
    /// can be combined to render the planet Earth. Some of the effects used
    /// in this application include: per pixel lighting, tangent space normal
    /// mapping, using compressed normal map textures stored in DXT5nm format,
    /// controlling specular reflectivity using a specular map, and
    /// multi-texturing.
    /// <para>
    /// A single directional light shining down the world negative Z axis
    /// simulates sunlight. Use the left mouse button to orbit the camera
    /// around the Earth. As the camera orbits the Earth notice the smooth
    /// transitions between day and night. To enhance the night effect a
    /// separate night texture map is used that displays the city lights.
    /// </para>
    /// <para>
    /// The normal map for the Earth is stored in the DXT5nm DDS file format.
    /// The uncompressed normal map is about 8 MB in size. After compression it
    /// is about 2 MB in size. The DXT5nm format uses a DXT5 block to represent
    /// 3D normals. The X component of the normal is stored in the alpha
    /// channel and the Y component is stored in the green channel. The Z
    /// component of the normal is reconstructed in the pixel shader using an
    /// orthogonal projection: Z = sqrt(1 - X * X - Y * Y).
    /// </para>
    /// <para>
    /// The starfield is procedurally generated and rendered as camera-aligned
    /// billboards. The stars are randomly distributed on the surface of a 
    /// sphere.
    /// </para>
    /// </summary>
    public class Demo : Microsoft.Xna.Framework.Game
    {
        private static void Main()
        {
            using (Demo demo = new Demo())
            {
                demo.Run();
            }
        }
                
        private struct Sunlight
        {
            public Vector4 direction;
            public Vector4 color;
        }

        private struct Earth
        {
            public Model model;
            public Effect effect;
            public BoundingSphere bounds;

            public Texture2D dayTexture;
            public Texture2D nightTexture;
            public Texture2D cloudTexture;
            public Texture2D normalMapTexture;

            public Vector4 ambient;
            public Vector4 diffuse;
            public Vector4 specular;
            public float shininess;
            public float cloudStrength;

            public float rotation;
        }

        private struct Camera
        {
            public const float DOLLYING_SPEED = 0.10f;
            public const float ROTATION_SPEED = 0.50f;
            public const float MOUSEWHEEL_SPEED = 0.50f;
            public const float TRACKING_SPEED = 0.01f;

            public float offset;
            public Vector2 rotation;
            public Vector2 translate;
            public Vector3 position;
            public Vector3 target;
            public Vector3 viewDir;
            public Quaternion orientation;
            public Matrix viewMatrix;
            public Matrix projectionMatrix;
        }

        private StarfieldComponent starfieldComponent;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private Vector2 fontPos;
        private KeyboardState currentKeyboardState;
        private KeyboardState prevKeyboardState;
        private MouseState currentMouseState;
        private MouseState prevMouseState;
        private Earth earth;
        private Sunlight sunlight;
        private Camera camera;
        private Vector4 globalAmbient;
        private int windowWidth;
        private int windowHeight;
        private TimeSpan mouseIdleElapsedTime = TimeSpan.Zero;
        private bool displayHelp;
        private bool hideClouds;
        
        public Demo()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            Window.Title = "XNA 4.0 Earth Demo";
            IsMouseVisible = true;
            IsFixedTimeStep = false;

            starfieldComponent = new StarfieldComponent(this);
            Components.Add(starfieldComponent);
        }

        protected override void Draw(GameTime gameTime)
        {
            if (!IsActive)
                return;

            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;
            GraphicsDevice.SamplerStates[2] = SamplerState.LinearWrap;           

            DrawEarth();
            DrawText();
        }

        private void DrawEarth()
        {
            Matrix rotation = Matrix.CreateRotationY(earth.rotation) *
                              Matrix.CreateRotationZ(MathHelper.ToRadians(-23.4f));

            foreach (ModelMesh m in earth.model.Meshes)
            {
                foreach (Effect e in m.Effects)
                {
                    if (hideClouds)
                    {
                        e.CurrentTechnique = e.Techniques["EarthWithoutClouds"];
                    }
                    else
                    {
                        e.CurrentTechnique = e.Techniques["EarthWithClouds"];
                        e.Parameters["cloudStrength"].SetValue(earth.cloudStrength);
                    }

                    e.Parameters["world"].SetValue(rotation);
                    e.Parameters["view"].SetValue(camera.viewMatrix);
                    e.Parameters["projection"].SetValue(camera.projectionMatrix);
                    e.Parameters["cameraPos"].SetValue(new Vector4(camera.position, 1.0f));
                    e.Parameters["globalAmbient"].SetValue(globalAmbient);
                    e.Parameters["lightDir"].SetValue(sunlight.direction);
                    e.Parameters["lightColor"].SetValue(sunlight.color);
                    e.Parameters["materialAmbient"].SetValue(earth.ambient);
                    e.Parameters["materialDiffuse"].SetValue(earth.diffuse);
                    e.Parameters["materialSpecular"].SetValue(earth.specular);
                    e.Parameters["materialShininess"].SetValue(earth.shininess);
                    e.Parameters["landOceanColorGlossMap"].SetValue(earth.dayTexture);
                    e.Parameters["cloudColorMap"].SetValue(earth.cloudTexture);
                    e.Parameters["nightColorMap"].SetValue(earth.nightTexture);
                    e.Parameters["normalMap"].SetValue(earth.normalMapTexture);
                }

                m.Draw();
            }
        }

        private void DrawText()
        {
            StringBuilder buffer = new StringBuilder();

            if (displayHelp)
            {
                buffer.AppendLine("Right mouse button and drag to rotate around the Earth");
                buffer.AppendLine("Middle mouse button and drag to zoom in and out of the Earth");
                buffer.AppendLine("Mouse wheel to zoom in and out of the Earth");
                buffer.AppendLine();
                buffer.AppendLine("Press NUMPAD +/- to change brightness of clouds");
                buffer.AppendLine("Press SPACE to show/hide clouds");
                buffer.AppendLine("Press ALT and ENTER to toggle full screen");
                buffer.AppendLine("Press ESCAPE to exit");
                buffer.AppendLine();
                buffer.AppendLine("Press H to hide help");
            }
            else
            {
                buffer.AppendLine("Press H to display help");
            }

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.DrawString(spriteFont, buffer.ToString(), fontPos, Color.Yellow);
            spriteBatch.End();
        }

        protected override void Initialize()
        {
            // Setup the window to be a quarter the size of the desktop.
            windowWidth = GraphicsDevice.DisplayMode.Width / 2;
            windowHeight = GraphicsDevice.DisplayMode.Height / 2;

            // Setup frame buffer.
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.PreferMultiSampling = true;
            graphics.ApplyChanges();
                        
            // Position the text.
            fontPos = new Vector2(1.0f, 1.0f);

            // Setup the initial input states.
            currentKeyboardState = Keyboard.GetState();

            // Setup direction light source.
            sunlight.direction = new Vector4(Vector3.Forward, 0.0f);
            sunlight.color = new Vector4(1.0f, 0.941f, 0.898f, 1.0f);
            
            // Setup scene's global ambient.
            globalAmbient = new Vector4(0.2f, 0.2f, 0.2f, 1.0f);
                        
            base.Initialize();
        }

        private bool KeyJustPressed(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key) && prevKeyboardState.IsKeyUp(key);
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            spriteFont = Content.Load<SpriteFont>(@"Fonts\DemoFont");
            
            // Load the assets for the Earth.
            earth.model = Content.Load<Model>(@"Models\earth");
            earth.effect = Content.Load<Effect>(@"Effects\earth");
            earth.dayTexture = Content.Load<Texture2D>(@"Textures\earth_day_color_spec");
            earth.nightTexture = Content.Load<Texture2D>(@"Textures\earth_night_color");
            earth.cloudTexture = Content.Load<Texture2D>(@"Textures\earth_clouds_alpha");
            earth.normalMapTexture = Content.Load<Texture2D>(@"Textures\earth_nrm");

            // Setup material settings for the Earth.
            earth.ambient = new Vector4(0.3f, 0.3f, 0.3f, 1.0f);
            earth.diffuse = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
            earth.specular = new Vector4(0.7f, 0.7f, 0.7f, 1.0f);
            earth.shininess = 20.0f;
            earth.cloudStrength = 1.15f;

            // Calculate the bounding sphere of the Earth model and bind the
            // custom Earth effect file to the model.
            foreach (ModelMesh mesh in earth.model.Meshes)
            {
                earth.bounds = BoundingSphere.CreateMerged(earth.bounds, mesh.BoundingSphere);

                foreach (ModelMeshPart part in mesh.MeshParts)
                    part.Effect = earth.effect;
            }

            // Position the camera based on the Earth model's size.
            camera.target = earth.bounds.Center;
            camera.offset = earth.bounds.Radius * 3.0f;
            camera.orientation = Quaternion.Identity;
            
            // Setup starfield.
            starfieldComponent.Generate(5000, earth.bounds.Radius * 45.0f);
        }

        private void ProcessKeyboard()
        {
            prevKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            if (KeyJustPressed(Keys.Escape))
                this.Exit();

            if (KeyJustPressed(Keys.H))
                displayHelp = !displayHelp;

            if (currentKeyboardState.IsKeyDown(Keys.LeftAlt) ||
                currentKeyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (KeyJustPressed(Keys.Enter))
                    ToggleFullScreen();
            }

            if (KeyJustPressed(Keys.Space))
                hideClouds = !hideClouds;

            if (KeyJustPressed(Keys.Add))
            {
                if ((earth.cloudStrength += 0.05f) > 2.0f)
                    earth.cloudStrength = 2.0f;
            }

            if (KeyJustPressed(Keys.Subtract))
            {
                if ((earth.cloudStrength -= 0.05f) < 0.0f)
                    earth.cloudStrength = 0.0f;
            }
        }

        private void ProcessMouse(GameTime gameTime)
        {
            prevMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();

            camera.translate.X = camera.translate.Y = 0.0f;
            camera.rotation.X = camera.rotation.Y = 0.0f;

            float dx = 0.0f;
            float dy = 0.0f;
            float dz = 0.0f;
               
            if (currentMouseState.MiddleButton == ButtonState.Pressed)
            {
                dz = currentMouseState.Y - prevMouseState.Y;
                dz *= Camera.DOLLYING_SPEED;

                camera.offset += dz;
            }
            else if (currentMouseState.RightButton == ButtonState.Pressed)
            {
                dx = currentMouseState.X - prevMouseState.X;
                dx *= Camera.ROTATION_SPEED;

                dy = currentMouseState.Y - prevMouseState.Y;
                dy *= Camera.ROTATION_SPEED;

                camera.rotation.X = dy;
                camera.rotation.Y = dx;
            }

            // Process mouse wheel scrolling.

            if (currentMouseState.ScrollWheelValue > prevMouseState.ScrollWheelValue)
                camera.offset -= Camera.MOUSEWHEEL_SPEED;
            else if (currentMouseState.ScrollWheelValue < prevMouseState.ScrollWheelValue)
                camera.offset += Camera.MOUSEWHEEL_SPEED;

            if (camera.offset <= (earth.bounds.Radius + earth.bounds.Radius * 0.3f))
                camera.offset = earth.bounds.Radius + earth.bounds.Radius * 0.3f;
            else if (camera.offset > earth.bounds.Radius * 4.0f)
                camera.offset = earth.bounds.Radius * 4.0f;

            // Hide the mouse cursor when the mouse hasn't moved.

            int deltaX = currentMouseState.X - prevMouseState.X;
            int deltaY = currentMouseState.Y - prevMouseState.Y;
            bool idle = deltaX == 0 && deltaY == 0;

            if (idle)
            {
                mouseIdleElapsedTime += gameTime.ElapsedGameTime;

                if (mouseIdleElapsedTime > TimeSpan.FromSeconds(2.0))
                    IsMouseVisible = false;
            }
            else
            {
                IsMouseVisible = true;
                mouseIdleElapsedTime = TimeSpan.Zero;
            }
        }

        private void ToggleFullScreen()
        {
            int newWidth = 0;
            int newHeight = 0;

            graphics.IsFullScreen = !graphics.IsFullScreen;

            if (graphics.IsFullScreen)
            {
                newWidth = GraphicsDevice.DisplayMode.Width;
                newHeight = GraphicsDevice.DisplayMode.Height;
            }
            else
            {
                newWidth = windowWidth;
                newHeight = windowHeight;
            }

            graphics.PreferredBackBufferWidth = newWidth;
            graphics.PreferredBackBufferHeight = newHeight;
            graphics.ApplyChanges();
        }

        protected override void Update(GameTime gameTime)
        {
            if (!IsActive)
                return;

            ProcessKeyboard();
            ProcessMouse(gameTime);

            UpdateCamera();

            earth.rotation += (float)gameTime.ElapsedGameTime.TotalMilliseconds * MathHelper.ToRadians(0.01f);

            starfieldComponent.WorldMatrix = Matrix.Identity;
            starfieldComponent.ViewMatrix = camera.viewMatrix;
            starfieldComponent.ProjectionMatrix = camera.projectionMatrix;

            base.Update(gameTime);
        }

        private void UpdateCamera()
        {
            // Calculate the view matrix.

            Quaternion rotation = Quaternion.Identity;
            float heading = MathHelper.ToRadians(camera.rotation.Y);
            float pitch = MathHelper.ToRadians(camera.rotation.X);

            if (heading != 0.0f)
            {
                rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, heading);
                Quaternion.Concatenate(ref rotation, ref camera.orientation, out camera.orientation);
            }

            if (pitch != 0.0f)
            {
                rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, pitch);
                Quaternion.Concatenate(ref camera.orientation, ref rotation, out camera.orientation);
            }

            Matrix.CreateFromQuaternion(ref camera.orientation, out camera.viewMatrix);

            Vector3 xAxis = new Vector3(camera.viewMatrix.M11, camera.viewMatrix.M21, camera.viewMatrix.M31);
            Vector3 yAxis = new Vector3(camera.viewMatrix.M12, camera.viewMatrix.M22, camera.viewMatrix.M32);
            Vector3 zAxis = new Vector3(camera.viewMatrix.M13, camera.viewMatrix.M23, camera.viewMatrix.M33);

            camera.target -= xAxis * camera.translate.X;
            camera.target -= yAxis * camera.translate.Y;

            camera.position = camera.target + zAxis * camera.offset;

            camera.viewMatrix.M41 = -Vector3.Dot(xAxis, camera.position);
            camera.viewMatrix.M42 = -Vector3.Dot(yAxis, camera.position);
            camera.viewMatrix.M43 = -Vector3.Dot(zAxis, camera.position);

            Vector3.Negate(ref zAxis, out camera.viewDir);

            // Calculate the projection matrix.

            camera.projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                (float)windowWidth / (float)windowHeight, 0.1f, earth.bounds.Radius * 100.0f);
        }
    }
}