using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using TownRPG.Main;
using TownRPG.Maps.Objects;

namespace TownRPG.Interfaces {
    public class MessageBox : Interface {

        private readonly Character character;
        private readonly string message;
        private Rectangle rect;
        private string splitMessage;

        private float characterCount;
        private float fade;
        private bool done;

        public MessageBox(string message, Character character = null) {
            this.message = message;
            this.character = character;
        }

        public override void Draw(SpriteBatch batch) {
            base.Draw(batch);

            var color = Color.White * this.fade;
            batch.DrawDynamicArea(this.rect, Texture, new Rectangle(0, 0, 4, 4), color);
            batch.DrawString(GameImpl.Instance.NormalFont,
                this.splitMessage.Substring(0, Math.Min(this.splitMessage.Length, (int) this.characterCount)),
                this.rect.Location.ToVector2() + new Vector2(5, 5),
                color, 0F, Vector2.Zero, 0.25F, SpriteEffects.None, 0F);
        }

        public override void Update(GameTime time) {
            base.Update(time);

            if (!this.done) {
                if (this.fade < 1F) {
                    this.fade += 0.065F;
                }
            } else if (this.fade > 0F) {
                this.fade -= 0.065F;
                if (this.fade <= 0F) {
                    GameImpl.Instance.SetInterface(null);
                }
            }

            if (this.fade >= 1F && this.characterCount < this.splitMessage.Length) {
                this.characterCount += 0.5F;
            }
        }

        public override bool OnMouse(Point pos, int clickType) {
            if (clickType >= 3) {
                if (this.characterCount < this.splitMessage.Length) {
                    this.characterCount = this.splitMessage.Length;
                } else if (!this.done) {
                    this.done = true;
                }
                return true;
            }
            return false;
        }

        public override void InitPositions(Viewport viewport) {
            this.rect = new Rectangle(
                new Point(75, viewport.Height / Scale - 70),
                new Point(viewport.Width / Scale - 150, 60));
            this.splitMessage = GameImpl.Instance.NormalFont.SplitString(this.message, this.rect.Width - 10, 0.25F);
        }

        public override void OnOpen() {
            if (this.character != null) {
                this.character.AllowWalking = false;

                var player = GameImpl.Instance.Player;
                this.character.StopAndFace(player);
                player.StopAndFace(this.character);
            }
        }

        public override void OnClose() {
            if (this.character != null) {
                this.character.AllowWalking = true;
            }
        }

    }
}