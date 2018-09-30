using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using TownRPG.Interfaces;
using TownRPG.Main;

namespace TownRPG.Maps.Objects {
    public class Player : Character {

        public Player(Map map, Vector2 position) : base("Player", map, position) {
        }

        public override void Update(GameTime time) {
            if (GameImpl.Instance.CurrentInterface == null && GameImpl.Instance.CurrentCutscene == null) {
                var state = Keyboard.GetState();
                var speed = state.IsKeyDown(Keys.LeftShift) ? 0.3F : 0.5F;
                var vel = new Vector2();

                if (state.IsKeyDown(Keys.W)) {
                    vel.Y -= speed;
                }
                if (state.IsKeyDown(Keys.S)) {
                    vel.Y += speed;
                }

                if (state.IsKeyDown(Keys.A)) {
                    vel.X -= speed;
                }
                if (state.IsKeyDown(Keys.D)) {
                    vel.X += speed;
                }

                if (vel.X != 0 && vel.Y != 0) {
                    vel /= 1.42F;
                }
                this.Velocity += vel;
            }

            foreach (var obj in this.Map.StaticObjects.Values) {
                var tele = obj as Teleporter;
                if (tele != null && this.Intersects(tele.Position, tele.Size)) {
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