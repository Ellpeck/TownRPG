using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Sprites;

namespace TownRPG.Interfaces {
    public class InterfaceComponent {

        public Vector2 Position;
        public Size2 Size;

        public InterfaceComponent(Vector2 position, Size2 size) {
            this.Position = position;
            this.Size = size;
        }

        public void Update(GameTime time) {

        }

        public void Draw(SpriteBatch batch) {

        }

        public virtual bool OnMouse(Point pos, int clickType) {
            return false;
        }
    }
}