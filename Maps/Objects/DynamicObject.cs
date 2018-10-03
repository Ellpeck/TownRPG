using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TownRPG.Main;

namespace TownRPG.Maps.Objects {
    public class DynamicObject {

        public Map Map;
        public Vector2 Position;
        public Vector2 LastPosition;
        public Vector2 Velocity;
        public Size2 Size;
        public int Direction = 2;
        public bool NoClip;

        public DynamicObject(Map map, Vector2 position) {
            this.Map = map;
            this.Position = position;
        }

        public virtual void Update(GameTime time) {
            this.LastPosition = this.Position;
            if (this.Velocity.X != 0) {
                var newX = new Vector2(this.Position.X + this.Velocity.X, this.Position.Y);
                if (this.NoClip || !this.IsCollidingPos(newX, true)) {
                    this.Position = newX;
                }
                if (Math.Abs(this.Velocity.X) >= 0.2) {
                    this.Direction = this.Velocity.X > 0 ? 1 : 3;
                }
            }
            if (this.Velocity.Y != 0) {
                var newY = new Vector2(this.Position.X, this.Position.Y + this.Velocity.Y);
                if (this.NoClip || !this.IsCollidingPos(newY, true)) {
                    this.Position = newY;
                }
                if (Math.Abs(this.Velocity.Y) >= 0.2) {
                    this.Direction = this.Velocity.Y > 0 ? 2 : 0;
                }
            }
            this.Velocity *= 0.5F;
        }

        public virtual void Draw(SpriteBatch batch) {
        }

        public virtual bool OnMouse(Vector2 posWorld, int clickType) {
            return false;
        }

        public bool IsCollidingPos(Vector2 pos, bool checkCurrent) {
            var width = this.Size.Width / 2F;
            var height = this.Size.Height / 2F;
            var corners = new[] {
                pos + new Vector2(width, height),
                pos + new Vector2(-width, height),
                pos + new Vector2(width, -height),
                pos + new Vector2(-width, -height)
            };
            foreach (var cornerMap in corners) {
                var corner = cornerMap / this.Map.Scale;
                var tile = this.Map["Objects", corner];
                if (tile.HasValue && !tile.Value.IsBlank) {
                    return true;
                }
            }

            foreach (var obj in this.Map.DynamicObjects) {
                if (pos.Intersects(this.Size, obj.Position, obj.Size)) {
                    if (!checkCurrent || !this.Position.Intersects(this.Size, obj.Position, obj.Size)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual void Teleport(Map newMap, Point pos) {
            this.Map.DynamicObjects.Remove(this);
            this.Map = newMap;
            this.Map.DynamicObjects.Add(this);
            this.Position = (pos.ToVector2() + Vector2.One / 2F) * this.Map.Scale;
        }

        public float GetRenderDepth() {
            return Math.Max(0, this.Position.Y / this.Map.Tiles.HeightInPixels / 1000F);
        }

    }
}