using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TownRPG.Main;
using TownRPG.Maps.Objects;

namespace TownRPG.Interfaces {
    public class Inventory : Interface {

        private readonly Player player;
        private Vector2 position;
        private Size2 size;

        public Inventory(Player player) {
            this.player = player;
        }

        public override void InitPositions(Viewport viewport) {
            const int toolOffset = 8;
            var slotDistance = new Size2(17, 17);
            var slotOffset = new Vector2(8, 8);

            this.size = new Size2(slotDistance.Width * 7 + toolOffset, slotDistance.Height * 4) + (Size2) slotOffset * 2;
            this.position = new Vector2(viewport.Width / Scale - this.size.Width, viewport.Height / Scale - this.size.Height) / 2F;

            this.Components.Clear();
            for (var i = 0; i < this.player.MainInventory.Length; i++) {
                this.Components.Add(new Slot(
                    this.position + slotOffset + new Vector2(i % 6, i / 6) * slotDistance,
                    new Size2(16, 16),
                    this.player.MainInventory, i));
            }
            for (var i = 0; i < this.player.Tools.Length; i++) {
                this.Components.Add(new Slot(
                    this.position + slotOffset + new Vector2(6 * slotDistance.Width + toolOffset, i * slotDistance.Height),
                    new Size2(16, 16),
                    this.player.Tools, i));
            }
        }

        public override bool OnKeyboard(string bind, int type) {
            if (type == 2 && (bind == "Escape" || bind == "Inventory")) {
                GameImpl.Instance.SetInterface(null);
                return true;
            } else {
                return false;
            }
        }

        public override bool Pauses() {
            return true;
        }

        public override void Draw(SpriteBatch batch) {
            batch.DrawDynamicArea(
                new Rectangle(this.position.ToPoint(), (Point) this.size),
                Texture, new Rectangle(0, 12, 8, 8), Color.White);
            base.Draw(batch);
        }

    }
}