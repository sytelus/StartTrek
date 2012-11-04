using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Projection
{
    public abstract class Scene
    {
        List<Object3D> objects = new List<Object3D>();
        List<Object3D> updatableObjects = new List<Object3D>();

        public GraphicsDevice GraphicsDevice { get; private set; }
        public Vector3 Center { get; protected set; }

        public RecommandedSceneSettings RecommandedSettings { get; private set; }

        protected Scene(GraphicsDevice graphicsDevice, Vector3 center)
        {
            this.GraphicsDevice = graphicsDevice;
            this.Center = center;
            RecommandedSettings = new RecommandedSceneSettings();
        }

        public void LoadContent(ContentManager content)
        {
            foreach (var object3D in this.objects)
            {
                object3D.LoadContent(content);
            }
        }

        protected void Clear()
        {
            this.objects.Clear();
            this.updatableObjects.Clear();
        }

        public void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState)
        {
            foreach (var updatableObject in updatableObjects)
                updatableObject.Update(gameTime, mouseState, keyboardState, this.objects);
        }

        public void Draw()
        {
            foreach (var object3D in this.objects)
                object3D.Draw();
        }


        public void AddObject(Object3D object3D)
        {
            this.objects.Add(object3D);
            if (object3D.RequiresUpdate)
                this.updatableObjects.Add(object3D);
        }

        public IEnumerable<Object3D> Objects
        {
            get { return this.objects; }
        }

        public IEnumerable<Object3D> UpdatableObjects
        {
            get { return this.updatableObjects; }
        }

        public class RecommandedSceneSettings
        {
            public Vector3 CameraPosition { get; set; }
            public bool ArcBallOriginLocked { get; set; }
        }
    }
}
