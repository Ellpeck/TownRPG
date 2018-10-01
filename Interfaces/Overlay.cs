using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TownRPG.Main;

namespace TownRPG.Interfaces {
    public class Overlay : Interface {

        private Rectangle timeArea;

        public Overlay() {
            this.InitPositions(GameImpl.Instance.GraphicsDevice.Viewport);
        }

        public override void Draw(SpriteBatch batch) {
            base.Draw(batch);

            var game = GameImpl.Instance;
            batch.DrawDynamicArea(this.timeArea, Texture, new Rectangle(12, 0, 4, 4), Color.White);
            batch.DrawCenteredString(game.NormalFont,
                string.Format(Locale.GetInterface("Day"), game.CurrentTime.Day + 1),
                this.timeArea.Location.ToVector2() + new Vector2(this.timeArea.Width / 2F, 5),
                true, false, Color.White, 0.25F);
            batch.DrawCenteredString(game.NormalFont,
                game.CurrentTime.TimeToString(),
                this.timeArea.Location.ToVector2() + new Vector2(this.timeArea.Width / 2F, 15),
                true, false, Color.White, 0.3F);
        }

        public override void InitPositions(Viewport viewport) {
            this.timeArea = new Rectangle(2, 2, 50, 30);
        }

    }
}