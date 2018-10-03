using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using TownRPG.Main;

namespace TownRPG.Maps.Objects {
    public class Teleporter : StaticObject {

        private readonly string destination;
        private readonly Point destCoords;
        private readonly bool requiresInteraction;

        public Teleporter(Map map, Point position, Size2 size, string destination, Point destCoords, bool requiresInteraction) : base(map, position, size) {
            this.destination = destination;
            this.destCoords = destCoords;
            this.requiresInteraction = requiresInteraction;
        }

        public void OnPlayerIntersection(Player player) {
            if (!this.requiresInteraction) {
                this.Teleport(player);
            }
        }

        public override bool OnMouse(Vector2 posWorld, int clickType) {
            if (!this.requiresInteraction) {
                return false;
            }
            if (this.MapPosition.Intersects(this.Size, posWorld, new Size2(1F, 1F))) {
                GameImpl.Instance.CurrentCursor = 1;
                if (Vector2.Distance(this.MapPosition, GameImpl.Instance.Player.Position) <= 1.75F * this.Map.Scale) {
                    if (clickType >= 3) {
                        this.Teleport(GameImpl.Instance.Player);
                    }
                } else {
                    GameImpl.Instance.CursorAlpha = 0.5F;
                }
                return true;
            }
            return false;
        }

        private void Teleport(DynamicObject player) {
            var game = GameImpl.Instance;
            game.Fade(true, 0.03F, () => {
                player.Teleport(game.Maps[this.destination], this.destCoords);
                game.Fade(true, -0.03F);
            });
        }

    }
}