using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TownRPG.Main;

namespace TownRPG.Items {
    public class Item {

        public static readonly Texture2D Texture = GameImpl.LoadContent<Texture2D>("Interfaces/Items");

        public readonly int RenderIndex;
        public int Amount;

        public Item(int renderIndex) {
            this.RenderIndex = renderIndex;
        }

        public virtual void Draw(SpriteBatch batch, Vector2 position) {
            batch.Draw(Texture, position,
                new Rectangle(this.RenderIndex % 8 * 16, this.RenderIndex / 8 * 16, 16, 16),
                Color.White);
        }

    }
}