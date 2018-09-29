using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace TownRPG.Maps.Objects {
    public abstract class MapObject {

        public Map Map;
        public Vector2 Position;
        public Vector2 LastPosition;
        public Vector2 Velocity;
        public Size2 Size;
        public int Direction = 2;
        public bool NoClip;

        public MapObject(Map map, Vector2 position) {
            this.Map = map;
            this.Position = position;
        }

        public abstract bool IsStatic();

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

        public bool IsCollidingPos(Vector2 pos, bool checkCurrent) {
            var width = this.Size.Width / 2F;
            var height = this.Size.Height / 2F;
            var corners = new[] {
                pos + new Vector2(width, height),
                pos + new Vector2(-width, height),
                pos + new Vector2(width, -height),
                pos + new Vector2(-width, -height)
            };
            foreach (var corner in corners) {
                var tile = this.Map["Objects", corner / this.Map.Scale];
                if (tile.HasValue && !tile.Value.IsBlank) {
                    return true;
                }
            }

            foreach (var obj in this.Map.AllObjects) {
                if (obj != this && this.Intersects(obj.Position, obj.Size, pos) && obj.ShouldCollideWith(this)) {
                    if (!checkCurrent || !this.Intersects(obj.Position, obj.Size, this.Position)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool Intersects(Vector2 otherPos, Size2 otherSize, Vector2? myPos = null, Size2? mySize = null) {
            var size = mySize.GetValueOrDefault(this.Size);
            var width = size.Width / 2F;
            var height = size.Height / 2F;
            var otherWidth = otherSize.Width / 2F;
            var otherHeight = otherSize.Height / 2F;

            var pos = myPos.GetValueOrDefault(this.Position);
            return pos.X - width < otherPos.X + otherWidth
                   && pos.X + width > otherPos.X - otherWidth
                   && pos.Y - height < otherPos.Y + otherHeight
                   && pos.Y + height > otherPos.Y - otherHeight;
        }

        public float GetRenderDepth() {
            return Math.Max(0, this.Position.Y / this.Map.Tiles.HeightInPixels / 1000F);
        }

        public virtual bool ShouldCollideWith(MapObject other) {
            return true;
        }

        public virtual bool OnMouse(Vector2 posWorld, int clickType) {
            return false;
        }

        public virtual void Teleport(Map newMap, Point pos) {
            this.Map.RemoveObject(this);
            this.Map = newMap;
            this.Map.AddObject(this);
            this.Position = (pos.ToVector2() + Vector2.One / 2F) * this.Map.Scale;
        }

    }
}