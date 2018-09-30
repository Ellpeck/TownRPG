using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using TownRPG.Main;
using TownRPG.Maps.Objects;

namespace TownRPG.Maps {
    public class Map {

        public TiledMap Tiles { get; private set; }
        public readonly int Scale;
        public readonly string Name;

        public readonly List<MapObject> DynamicObjects = new List<MapObject>();
        public readonly Dictionary<Vector2, MapObject> StaticObjects = new Dictionary<Vector2, MapObject>();
        public readonly List<MapObject> AllObjects = new List<MapObject>();
        public readonly List<LightSource> LightSources = new List<LightSource>();

        public TiledMapTileLayer this[string name] {
            get { return this.Tiles.GetLayer<TiledMapTileLayer>(name); }
        }

        public TiledMapTile? this[string name, int x, int y] {
            get {
                TiledMapTile? tile;
                this[name].TryGetTile(x, y, out tile);
                return tile;
            }
        }

        public TiledMapTile? this[string name, Vector2 pos] {
            get { return this[name, (int) pos.X, (int) pos.Y]; }
        }

        public Map(TiledMap tiles) {
            this.Tiles = tiles;
            this.Name = tiles.Name.Substring(tiles.Name.LastIndexOf('/') + 1);
            this.Scale = (tiles.TileWidth + tiles.TileHeight) / 2;

            var objects = tiles.GetLayer<TiledMapObjectLayer>("StaticObjects");
            if (objects != null) {
                foreach (var obj in objects.Objects) {
                    switch (obj.Type) {
                        case "Bush":
                            this.AddObject(new StaticObject(this, obj.Position, 0, obj.Name));
                            break;
                        case "Teleporter":
                            this.AddObject(new Teleporter(this, obj.Position, obj.Size,
                                obj.Properties["Destination"],
                                new Point(int.Parse(obj.Properties["DestX"]), int.Parse(obj.Properties["DestY"])),
                                obj.Properties.ContainsKey("Interaction") && bool.Parse(obj.Properties["Interaction"])));
                            break;
                    }
                }
            }

            foreach (var layer in this.Tiles.TileLayers) {
                for (var x = 0; x < layer.Width; x++) {
                    for (var y = 0; y < layer.Height; y++) {
                        var tile = this[layer.Name, x, y];
                        if (tile.HasValue && !tile.Value.IsBlank) {
                            var props = this.GetTileProperties(tile.Value, "LightColor", "LightRadius", "LightType");
                            if (props[0] != null && props[1] != null) {
                                var size = float.Parse(props[1], NumberFormatInfo.InvariantInfo) * 2F;
                                this.LightSources.Add(new LightSource(
                                    new Vector2(x + 0.5F, y + 0.5F) * this.Scale,
                                    new Size2(size, size) * this.Scale,
                                    new Color(uint.Parse(props[0].Substring(1), NumberStyles.HexNumber)),
                                    props[2]));
                            }
                        }
                    }
                }
            }
        }

        public void AddObject(MapObject obj) {
            if (obj.IsStatic()) {
                this.StaticObjects.Add(obj.Position, obj);
            } else {
                this.DynamicObjects.Add(obj);
            }
            this.AllObjects.Add(obj);
        }

        public void RemoveObject(MapObject obj) {
            if (obj.IsStatic()) {
                this.StaticObjects.Remove(obj.Position);
            } else {
                this.DynamicObjects.Remove(obj);
            }
            this.AllObjects.Remove(obj);
        }

        public string[] GetTileProperties(TiledMapTile tile, params string[] names) {
            var props = new string[names.Length];
            var gid = tile.GlobalIdentifier;
            foreach (var tileset in this.Tiles.Tilesets) {
                if (tileset.ContainsGlobalIdentifier(gid)) {
                    foreach (var tilesetTile in tileset.Tiles) {
                        if (tilesetTile.LocalTileIdentifier == gid - tileset.FirstGlobalIdentifier) {
                            for (var i = 0; i < props.Length; i++) {
                                tilesetTile.Properties.TryGetValue(names[i], out props[i]);
                            }
                            break;
                        }
                    }
                    break;
                }
            }
            return props;
        }

        public void Update(GameTime time) {
            for (var i = this.DynamicObjects.Count - 1; i >= 0; i--) {
                var obj = this.DynamicObjects[i];
                obj.Update(time);
            }
        }

        public void Draw(SpriteBatch batch, Matrix viewMatrix) {
            batch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, viewMatrix);
            this.DrawLayerGroup("Ground", viewMatrix);
            this.DrawLayerGroup("Objects", viewMatrix);
            batch.End();

            batch.Begin(SpriteSortMode.FrontToBack, null, SamplerState.PointClamp, null, null, null, viewMatrix);
            foreach (var obj in this.AllObjects) {
                obj.Draw(batch);
            }
            batch.End();

            batch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null, viewMatrix);
            this.DrawLayerGroup("Above", viewMatrix);
            batch.End();
        }

        private void DrawLayerGroup(string name, Matrix viewMatrix) {
            var sub = 0;
            while (true) {
                var layer = this[sub == 0 ? name : name + sub];
                if (layer != null) {
                    GameImpl.Instance.MapRenderer.Draw(layer, viewMatrix);
                    sub++;
                } else {
                    break;
                }
            }
        }

    }
}