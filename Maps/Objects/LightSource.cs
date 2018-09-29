using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using TownRPG.Main;

namespace TownRPG.Maps.Objects {
    public class LightSource {

        private static readonly Texture2D LampTexture = GameImpl.LoadContent<Texture2D>("Objects/Light/Lamp");
        private static readonly Texture2D WindowTexture = GameImpl.LoadContent<Texture2D>("Objects/Light/Window");

        public Vector2 Position;
        public Size2 Size;
        public Color ColorModifier;
        public Texture2D Texture;

        public LightSource(Vector2 position, Size2 size, Color colorModifier, string type) {
            this.Position = position;
            this.Size = size;
            this.ColorModifier = colorModifier;
            switch (type) {
                case "Window":
                    this.Texture = WindowTexture;
                    break;
                case "Lamp":
                    this.Texture = LampTexture;
                    break;
            }
        }

    }
}