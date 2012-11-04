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
            Help = 0,
            Debug = 1,
            Verbouse = 2,
            None = -1
        }

        public Camera Camera { get; set; }
        private DebugLevelType? lastDebugLevel;
        private FpsCounter fpsCounter = new FpsCounter();

        public ScreenText(GraphicsDevice graphicsDevice, string name, Vector3 position, Camera camera)
            :base(graphicsDevice, name, position, Vector3.Zero, Vector3.Up)
        {
            this.Camera = camera;
            lastDebugLevel = null;
        }

        public override void LoadContent(ContentManager content)
        {
            spriteBatch = new SpriteBatch(this.GraphicsDevice);
            spriteFont = content.Load<SpriteFont>(@"InfoFont");
        }

        private string additionalHelpText;
        public string AdditionalHelpText
        {
            get { return this.additionalHelpText; }
            set 
            { 
                this.additionalHelpText = value;
                lastDebugLevel = null;
            }
        }

        public override bool RequiresUpdate
        {
            get { return true; }
        }

        StringBuilder screenText = new StringBuilder();
        public override void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyState, List<Object3D> objects)
        {
            fpsCounter.OnUpdate(gameTime);

            if (this.lastDebugLevel != this.DebugLevel)
                RebuildScreenText(mouseState, objects);
        }

        private void RebuildScreenText(MouseState mouseState, List<Object3D> objects)
        {
            switch (this.DebugLevel)
            {
                case DebugLevelType.None:
                    RebuildHelpText();
                    break;
                default:
                    RebuildInfoText(mouseState, objects);
                    break;
            }
            lastDebugLevel = this.DebugLevel;
        }

        private void RebuildHelpText()
        {
            screenText.Clear();
            screenText.Append(HelpText);
            screenText.AppendLine();
            screenText.Append(this.AdditionalHelpText);
        }

        private void RebuildInfoText(MouseState mouseState, List<Object3D> objects)
        {
            screenText.Clear();
            screenText.AppendLine(string.Format("FPS: {0}", this.fpsCounter.Rate));
            screenText.AppendLine(string.Format("Camera Pos: {0}, {1}, {2}", this.Camera.Position.X, this.Camera.Position.Y, this.Camera.Position.Z));
            screenText.AppendLine(string.Format("Camera Forward: {0}, {1}, {2}, {3}", this.Camera.ViewMatrix.Forward.X, this.Camera.ViewMatrix.Forward.Y, this.Camera.ViewMatrix.Forward.Z, this.Camera.ViewMatrix.Forward.Length()));
            screenText.AppendLine(string.Format("Camera Up: {0}, {1}, {2}, {3}", this.Camera.ViewMatrix.Up.X, this.Camera.ViewMatrix.Up.Y, this.Camera.ViewMatrix.Up.Z, this.Camera.ViewMatrix.Up.Length()));
            screenText.AppendLine(string.Format("Camera Right: {0}, {1}, {2}, {3}", this.Camera.ViewMatrix.Right.X, this.Camera.ViewMatrix.Right.Y, this.Camera.ViewMatrix.Right.Z, this.Camera.ViewMatrix.Right.Length()));
            screenText.AppendLine(string.Format("Mouse: {0}, {1}", mouseState.X, mouseState.Y));
            screenText.AppendLine(string.Format("Distance: {0}", (Vector3.Zero - this.Camera.Position).Length()));
            screenText.AppendLine(string.Format("Camera Basis Angles: {0}, {1}, {2}", this.Camera.ViewMatrix.Forward.AngleWith(this.Camera.Up, false), this.Camera.Up.AngleWith(this.Camera.ViewMatrix.Left, false), this.Camera.ViewMatrix.Left.AngleWith(this.Camera.ViewMatrix.Forward, false)));
            screenText.AppendLine(string.Format("DebugLevel: {0}", this.DebugLevel.ToString()));

            foreach (var object3D in objects)
            {
                var debugMessages = object3D.GetDebugMessages();
                if (debugMessages == null)
                    continue;

                foreach (var debugMessage in debugMessages)
                    screenText.AppendLine(string.Format("{0}-{1}:{2}", object3D.Name, debugMessage.Key, debugMessage.Value));
            }
        }

        public override void Draw()
        {
            fpsCounter.OnDraw();

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive);
            spriteBatch.DrawString(spriteFont, screenText.ToString(), new Vector2(this.Position.X, this.Position.Y), Color.Yellow);
            spriteBatch.End();

            base.Draw();
        }

        private const string HelpText = @"Use Arrows & PageUp/Down keys or Mouse dragging
Ctrl+Keys Or Left Mouse -> Rotate
Shift+Keys -> Tilt
Keys Or Right Mouse -> Pan";
    }
}
