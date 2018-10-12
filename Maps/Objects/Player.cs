using Microsoft.Xna.Framework;
using TownRPG.Items;
using TownRPG.Main;

namespace TownRPG.Maps.Objects {
    public class Player : Character {

        public readonly Item[] MainInventory = new Item[24];
        public readonly Item[] Tools = new Item[4];

        public Player(Map map, Vector2 position) : base("Player", map, position) {
        }

        public override void Update(GameTime time) {
            var game = GameImpl.Instance;
            if (game.CurrentInterface == null && game.CurrentCutscene == null) {
                var speed = game.GetKeyType("Slow") > 0 ? 0.3F : 0.5F;
                var vel = new Vector2();

                if (game.GetKeyType("Up") > 0) {
                    vel.Y -= speed;
                }
                if (game.GetKeyType("Down") > 0) {
                    vel.Y += speed;
                }

                if (game.GetKeyType("Left") > 0) {
                    vel.X -= speed;
                }
                if (game.GetKeyType("Right") > 0) {
                    vel.X += speed;
                }

                if (vel.X != 0 && vel.Y != 0) {
                    vel /= 1.42F;
                }
                this.Velocity += vel;
            }

            foreach (var obj in this.Map.StaticObjects.Values) {
                var tele = obj as Teleporter;
                if (tele != null && this.Position.Intersects(this.Size, tele.MapPosition, tele.Size)) {
                    tele.OnPlayerIntersection(this);
                }
            }

            base.Update(time);
        }

        public override void Teleport(Map newMap, Point pos) {
            base.Teleport(newMap, pos);

            var game = GameImpl.Instance;
            game.Camera.FixPosition();
            game.UpdateLighting();
        }

    }
}