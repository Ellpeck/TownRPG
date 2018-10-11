using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TownRPG.Main;
using TownRPG.Maps.Objects;

namespace TownRPG.Interfaces {
    public class Inventory : Interface {

        private readonly Player player;

        public Inventory(Player player) {
            this.player = player;
        }

        public override void InitPositions(Viewport viewport) {
            this.Components.Clear();
            for (var i = 0; i < this.player.Inventory.Length; i++) {
                this.Components.Add(new Slot(
                    new Vector2(i % 8 * 22, i / 8 * 22),
                    new Size2(20, 20),
                    this.player.Inventory, i));
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

    }
}