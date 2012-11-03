using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Projection
{
    public class ScreenText : Object3D
    {
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        public enum ScreenTextModeType
        {
            Help,
            Info,
            None
        }

        public Camera Camera { get; set; }
        public ScreenTextModeType ScreenTextMode { get; set; }

        public ScreenText(GraphicsDevice graphicsDevice, Vector3 position, Camera camera)
            :base(graphicsDevice, position, Vector3.Zero, Vector3.Up)
        {
            this.Camera = camera;

            ScreenTextMode = ScreenTextModeType.Help;
            infoText.Append(HelpText);
        }

        public override void LoadContent(ContentManager content)
        {
            spriteBatch = new SpriteBatch(this.GraphicsDevice);
            spriteFont = content.Load<SpriteFont>(@"InfoFont");
        }

        public override bool RequiresUpdate
        {
            get { return true; }
        }

        StringBuilder infoText = new StringBuilder();
        Stopwatch lastChangeTime = Stopwatch.StartNew();
        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyState, List<Object3D> objects)
        {
            if (keyState.IsKeyDown(Keys.F1) && lastChangeTime.ElapsedMilliseconds > 400)
            {
                if (this.ScreenTextMode == ScreenTextModeType.Help)
                {
                    this.ScreenTextMode = ScreenTextModeType.None;
                    infoText.Clear();
                }
                else
                {
                    this.ScreenTextMode = ScreenTextModeType.Help;
                    infoText.Clear();
                    infoText.Append(HelpText);
                }
                
                lastChangeTime.Restart();
            }
            if (keyState.IsKeyDown(Keys.D) && lastChangeTime.ElapsedMilliseconds > 400)
            {
                if (this.ScreenTextMode == ScreenTextModeType.Info)
                {
                    this.ScreenTextMode = ScreenTextModeType.None;
                    infoText.Clear();
                }
                else
                    this.ScreenTextMode = ScreenTextModeType.Info;

                lastChangeTime.Restart();
            }

            switch (this.ScreenTextMode)
            {
                case ScreenTextModeType.None:
                    break;  //Static info text
                case ScreenTextModeType.Help:
                    break;  //Static info text
                case ScreenTextModeType.Info:
                    GetInfoText(mouseState);
                    break;
                default:
                    throw new ArgumentException(string.Format("ScreenTextMode value {0} is not recognized", ScreenTextMode.ToString()));
            }
        }

        private void GetInfoText(MouseState mouseState)
        {
            infoText.Clear();
            infoText.AppendLine(string.Format("Camera Pos: {0}, {1}, {2}", this.Camera.Position.X, this.Camera.Position.Y, this.Camera.Position.Z));
            infoText.AppendLine(string.Format("Camera Forward: {0}, {1}, {2}, {3}", this.Camera.ViewMatrix.Forward.X, this.Camera.ViewMatrix.Forward.Y, this.Camera.ViewMatrix.Forward.Z, this.Camera.ViewMatrix.Forward.Length()));
            infoText.AppendLine(string.Format("Camera Up: {0}, {1}, {2}, {3}", this.Camera.ViewMatrix.Up.X, this.Camera.ViewMatrix.Up.Y, this.Camera.ViewMatrix.Up.Z, this.Camera.ViewMatrix.Up.Length()));
            infoText.AppendLine(string.Format("Camera Right: {0}, {1}, {2}, {3}", this.Camera.ViewMatrix.Right.X, this.Camera.ViewMatrix.Right.Y, this.Camera.ViewMatrix.Right.Z, this.Camera.ViewMatrix.Right.Length()));
            infoText.AppendLine(string.Format("Mouse: {0}, {1}", mouseState.X, mouseState.Y));
            infoText.AppendLine(string.Format("Distance: {0}", (Vector3.Zero - this.Camera.Position).Length()));
            infoText.AppendLine(string.Format("Camera Basis Angles: {0}, {1}, {2}", this.Camera.ViewMatrix.Forward.AngleWith(this.Camera.Up, false), this.Camera.Up.AngleWith(this.Camera.ViewMatrix.Left, false), this.Camera.ViewMatrix.Left.AngleWith(this.Camera.ViewMatrix.Forward, false)));

            foreach (var debugMessage in this.Camera.DebugMessages)
            {
                infoText.AppendLine(string.Format("{0}: {1}", debugMessage.Key, debugMessage.Value));
            }
        }

        public override void Draw()
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            spriteBatch.DrawString(spriteFont, infoText.ToString(), new Vector2(this.Position.X, this.Position.Y), Color.Yellow);
            spriteBatch.End();
        }

        private const string HelpText = @"Use Arrows & PageUp/Down keys or Mouse dragging
Ctrl+Keys Or Left Mouse -> Rotate
Shift+Keys -> Tilt
Keys Or Right Mouse -> Pan
F1 -> Show Hide Help
D -> Show Hide Debug Info
";
    }
}
