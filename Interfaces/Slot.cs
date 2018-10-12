using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TownRPG.Items;
using TownRPG.Main;

namespace TownRPG.Interfaces {
    public class Slot : InterfaceComponent {

        private bool IsOver;
        public Item[] Inventory;
        public int Id;

        public Slot(Vector2 position, Size2 size, Item[] inventory, int id) : base(position, size) {
            this.Inventory = inventory;
            this.Id = id;
        }

        public override void Draw(SpriteBatch batch) {
            var color = this.IsOver ? Color.White : Color.LightGray;
            batch.Draw(Interface.Texture, this.Position, new Rectangle(24, 0, 16, 16), color);
            var item = this.Inventory[this.Id];
            if (item != null) {
                item.Draw(batch, this.Position);
            }
        }

        public override bool OnMouse(Point pos, int clickType) {
            if ((this.Position + (Vector2) this.Size / 2).Intersects(this.Size, pos.ToVector2() / Interface.Scale, new Size2(1F, 1F))) {
                this.IsOver = true;
                if (clickType >= 3) {
                    // Click stuff here
                    return true;
                }
            } else {
                this.IsOver = false;
            }
            return false;
        }

    }
}