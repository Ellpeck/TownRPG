using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TownRPG.Maps;
using TownRPG.Maps.Objects;

namespace TownRPG.Main {
    public static class Util {

        public const int DefaultPathfindCost = 100;

        private static readonly Point[] PathfindDirs = {
            new Point(1, 0),
            new Point(-1, 0),
            new Point(0, 1),
            new Point(0, -1)
        };

        public static Stack<Point> FindPath(Map map, MapObject obj, Point start, Point goal, int maxTries) {
            var open = new HashSet<PathPoint>();
            var closed = new HashSet<PathPoint>();
            open.Add(new PathPoint(start, goal, null, 0));

            var count = 0;
            while (open.Count > 0) {
                PathPoint current = null;
                var lowestF = int.MaxValue;
                foreach (var point in open) {
                    if (point.F < lowestF) {
                        current = point;
                        lowestF = point.F;
                    }
                }

                if (current != null) {
                    open.Remove(current);
                    closed.Add(current);

                    if (current.Pos.Equals(goal)) {
                        var path = new Stack<Point>();
                        while (current != null) {
                            path.Push(current.Pos);
                            current = current.Parent;
                        }
                        return path;
                    }

                    foreach (var dir in PathfindDirs) {
                        var neighborPos = current.Pos + dir;
                        if (!obj.IsCollidingPos((neighborPos.ToVector2() + Vector2.One / 2F) * map.Scale, false)) {
                            var neighbor = new PathPoint(neighborPos, goal, current, GetTerrainWalkability(map, neighborPos));
                            if (!closed.Contains(neighbor)) {
                                PathPoint alreadyNeighbor;
                                open.TryGetValue(neighbor, out alreadyNeighbor);
                                if (alreadyNeighbor == null) {
                                    open.Add(neighbor);
                                } else if (neighbor.G < alreadyNeighbor.G) {
                                    open.Remove(alreadyNeighbor);
                                    open.Add(neighbor);
                                }
                            }
                        }
                    }
                }

                count++;
                if (count >= maxTries) {
                    break;
                }
            }
            return null;
        }

        private static int GetTerrainWalkability(Map map, Point pos) {
            var tile = map["Ground", pos.X, pos.Y];
            if (tile.HasValue && !tile.Value.IsBlank) {
                var prop = map.GetTileProperties(tile.Value, "Walkability")[0];
                if (prop != null) {
                    return int.Parse(prop);
                }
            }
            return DefaultPathfindCost;
        }

        public static string SplitString(this SpriteFont font, string text, float maxWidth, float scale) {
            if (font.MeasureString(text).X * scale < maxWidth) {
                return text;
            }

            var words = text.Split(' ');
            var space = font.MeasureString(" ").X * scale;

            var accumulatedWidth = 0F;
            var splitText = new StringBuilder();
            foreach (var word in words) {
                var width = font.MeasureString(word).X * scale;
                if (accumulatedWidth + width >= maxWidth) {
                    splitText.Append("\n");
                    accumulatedWidth = 0;
                }
                accumulatedWidth += width + space;
                splitText.Append(word).Append(" ");
            }
            return splitText.ToString();
        }

        public static void DrawDynamicArea(this SpriteBatch batch, Rectangle destination, Texture2D texture, Rectangle textureArea, Color color) {
            //Draw four corner parts first
            batch.Draw(texture,
                new Rectangle(destination.Location, textureArea.Size),
                textureArea,
                color);
            var tr = new Point(destination.Width - textureArea.Width, 0);
            var trT = new Point(textureArea.Width * 2, 0);
            batch.Draw(texture,
                new Rectangle(destination.Location + tr, textureArea.Size),
                new Rectangle(textureArea.Location + trT, textureArea.Size),
                color);
            var bl = new Point(0, destination.Height - textureArea.Height);
            var blT = new Point(0, textureArea.Height * 2);
            batch.Draw(texture,
                new Rectangle(destination.Location + bl, textureArea.Size),
                new Rectangle(textureArea.Location + blT, textureArea.Size),
                color);
            batch.Draw(texture,
                new Rectangle(destination.Location + tr + bl, textureArea.Size),
                new Rectangle(textureArea.Location + trT + blT, textureArea.Size),
                color);

            var centerT = new Rectangle(textureArea.Location + new Point(textureArea.Width), textureArea.Size);
            var horRemainder = new Point((destination.Width - textureArea.Width * 3) % textureArea.Width, textureArea.Height);
            var vertRemainder = new Point(textureArea.Width, (destination.Height - textureArea.Height * 3) % textureArea.Height);

            //Now draw along the width and height the filling and sides
            var topT = new Rectangle(textureArea.Location + new Point(textureArea.Width, 0), textureArea.Size);
            var bottomT = new Rectangle(textureArea.Location + new Point(textureArea.Width, textureArea.Height * 2), textureArea.Size);
            for (var x = destination.Width - textureArea.Width * 3; x >= 0; x -= textureArea.Width) {
                batch.Draw(texture,
                    new Rectangle(destination.Location + new Point(textureArea.Width + x, 0), textureArea.Size),
                    topT, color);
                batch.Draw(texture,
                    new Rectangle(destination.Location + new Point(textureArea.Width + x, bl.Y), textureArea.Size),
                    bottomT, color);
                //Draw the horizontal remainder of the filling while we're at it
                if (vertRemainder.Y > 0) {
                    batch.Draw(texture,
                        new Rectangle(destination.Location + new Point(textureArea.Width + x, textureArea.Height), vertRemainder),
                        new Rectangle(centerT.Location, vertRemainder),
                        color);
                }
            }
            var leftT = new Rectangle(textureArea.Location + new Point(0, textureArea.Height), textureArea.Size);
            var rightT = new Rectangle(textureArea.Location + new Point(textureArea.Width * 2, textureArea.Height), textureArea.Size);
            for (var y = destination.Height - textureArea.Height * 3; y >= 0; y -= textureArea.Height) {
                batch.Draw(texture,
                    new Rectangle(destination.Location + new Point(0, textureArea.Height + y), textureArea.Size),
                    leftT, color);
                batch.Draw(texture,
                    new Rectangle(destination.Location + new Point(tr.X, textureArea.Height + y), textureArea.Size),
                    rightT, color);
                //Draw the vertical center remainder of the filling while we're at it
                if (horRemainder.X > 0) {
                    batch.Draw(texture,
                        new Rectangle(destination.Location + new Point(textureArea.Width, textureArea.Height + y), horRemainder),
                        new Rectangle(centerT.Location, horRemainder),
                        color);
                }
            }

            //Now draw the remaining fragments that don't lign up with the ratio
            if (horRemainder.X > 0) {
                batch.Draw(texture,
                    new Rectangle(destination.Location + new Point(textureArea.Width, 0), horRemainder),
                    new Rectangle(topT.Location, horRemainder),
                    color);
                batch.Draw(texture,
                    new Rectangle(destination.Location + new Point(textureArea.Width, bl.Y), horRemainder),
                    new Rectangle(bottomT.Location, horRemainder),
                    color);
            }
            if (vertRemainder.Y > 0) {
                batch.Draw(texture,
                    new Rectangle(destination.Location + new Point(0, textureArea.Height), vertRemainder),
                    new Rectangle(leftT.Location, vertRemainder),
                    color);
                batch.Draw(texture,
                    new Rectangle(destination.Location + new Point(tr.X, textureArea.Height), vertRemainder),
                    new Rectangle(rightT.Location, vertRemainder),
                    color);
            }

            //Now draw the filling
            for (var x = destination.Width - textureArea.Width * 3; x >= 0; x -= textureArea.Width) {
                for (var y = destination.Height - textureArea.Height * 3; y >= 0; y -= textureArea.Height) {
                    batch.Draw(texture,
                        new Rectangle(destination.Location + new Point(textureArea.Width + x, textureArea.Height + y), textureArea.Size),
                        centerT, color);
                }
            }

            //Finally, draw the last little remaining corner fragment of the filling
            var centerRemainder = new Point(horRemainder.X, vertRemainder.Y);
            if (centerRemainder.X > 0 && centerRemainder.Y > 0) {
                batch.Draw(texture,
                    new Rectangle(destination.Location + new Point(textureArea.Width, textureArea.Height), centerRemainder),
                    new Rectangle(centerT.Location, centerRemainder),
                    color);
            }
        }

    }

    public class PathPoint {

        public readonly PathPoint Parent;
        public readonly Point Pos;
        public readonly int F;
        public readonly int G;

        public PathPoint(Point pos, Point goal, PathPoint parent, int terrainCostForThisPos) {
            this.Pos = pos;
            this.Parent = parent;

            this.G = (parent == null ? 0 : parent.G) + terrainCostForThisPos;
            var manhattan = (Math.Abs(goal.X - pos.X) + Math.Abs(goal.Y - pos.Y)) * Util.DefaultPathfindCost;
            this.F = this.G + manhattan;
        }

        public override bool Equals(object obj) {
            if (obj == this) {
                return true;
            }
            var point = obj as PathPoint;
            return point != null && point.Pos.Equals(this.Pos);
        }

        public override int GetHashCode() {
            return this.Pos.GetHashCode();
        }

    }
}