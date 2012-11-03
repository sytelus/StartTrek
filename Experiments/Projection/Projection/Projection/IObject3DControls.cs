using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Projection
{
    public interface IObject3DControls
    {
        void UpdateScreenDimentions(float screenWidth, float screenHeight);
        void Update(GameTime gameTime, MouseState mouseState, KeyboardState keyboardState, List<Object3D> objects);
    }
}