using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using TownRPG.Main;

namespace TownRPG.Interfaces {
    public class Interface {

        public static readonly Texture2D Texture = GameImpl.LoadContent<Texture2D>("Interfaces/Interface");
        public const int Scale = 4;

        public readonly List<InterfaceComponent> Components = new List<InterfaceComponent>();

        public virtual void Update(GameTime time) {
            foreach (var comp in this.Components) {
                comp.Update(time);
            }
        }

        public virtual void Draw(SpriteBatch batch) {
            foreach (var comp in this.Components) {
                comp.Draw(batch);
            }
        }

        public virtual void InitPositions(Viewport viewport) {
        }

        public virtual bool OnMouse(Point pos, int clickType) {
            return false;
        }

        public virtual bool Pauses() {
            return false;
        }

        public virtual void OnOpen() {
        }

        public virtual void OnClose() {
        }

        public bool HandleMouse(Point pos, int clickType) {
            if (this.OnMouse(pos, clickType)) {
                return true;
            }

            foreach (var component in this.Components) {
                if (component.OnMouse(pos, clickType)) {
                    return true;
                }
            }
            return false;
        }

    }
}