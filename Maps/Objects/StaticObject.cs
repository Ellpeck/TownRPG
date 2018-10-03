using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace TownRPG.Maps.Objects {
    public class StaticObject {

        public Map Map;
        public readonly Point Position;
        public readonly Vector2 MapPosition;
        public readonly Size2 Size;

        public StaticObject(Map map, Point position, Size2 size) {
            this.Map = map;
            this.Position = position;
            this.Size = size;
            this.MapPosition = position.ToVector2() * map.Scale + (Vector2) size / 2;
        }

        public virtual bool OnMouse(Vector2 posWorld, int clickType) {
            return false;
        }

        public float GetRenderDepth() {
            return Math.Max(0, this.MapPosition.Y / this.Map.Tiles.HeightInPixels / 1000F);
        }

    }
}