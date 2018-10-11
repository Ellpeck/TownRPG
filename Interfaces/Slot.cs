using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TownRPG.Items;

namespace TownRPG.Interfaces {
    public class Slot : InterfaceComponent {

        public Item[] Inventory;
        public int Id;

        public Slot(Vector2 position, Size2 size, Item[] inventory, int id) : base(position, size) {
            this.Inventory = inventory;
            this.Id = id;
        }

        public override void Draw(SpriteBatch batch) {
            batch.DrawRectangle(this.Position, this.Size, Color.White);
            var item = this.Inventory[this.Id];
            if (item != null) {
                item.Draw(batch, this.Position);
            }
        }

    }
}