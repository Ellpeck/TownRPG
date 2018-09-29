using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using TownRPG.Main;

namespace TownRPG.Maps.Objects {
    public class StaticObject : MapObject {

        private static readonly Texture2D Texture = GameImpl.LoadContent<Texture2D>("Objects/StaticObjects");
        public readonly int Type;
        public readonly string Name;

        public StaticObject(Map map, Vector2 position, int type, string name) : base(map, position) {
            this.Type = type;
            this.Name = name;

            switch (this.Type) {
                case 0:
                    this.Size = new Size2(2, 0.5F) * this.Map.Scale;
                    break;
            }
        }

        public override bool IsStatic() {
            return true;
        }

        public override void Draw(SpriteBatch batch) {
            Rectangle source;
            switch (this.Type) {
                case 0:
                    source = new Rectangle(0, 0, 2 * this.Map.Scale, 2 * this.Map.Scale);
                    break;
                default:
                    return;
            }
            batch.Draw(
                Texture,
                this.Position - Vector2.One * this.Map.Scale - new Vector2(0, this.Size.Height / 2F),
                source,
                Color.White, 0F, Vector2.Zero, Vector2.One, SpriteEffects.None,
                this.GetRenderDepth());
        }

    }
}