using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.Animations.SpriteSheets;
using MonoGame.Extended.TextureAtlases;
using TownRPG.Interfaces;
using TownRPG.Main;

namespace TownRPG.Maps.Objects {
    public class Character : MapObject {

        protected readonly SpriteSheetAnimationFactory AnimationFactory;
        protected SpriteSheetAnimation CurrentAnimation;

        public Queue<DialogMessage> DialogOptions = new Queue<DialogMessage>();

        public bool AllowWalking = true;
        public bool IsCutsceneRoute;
        private Stack<Point> path;
        private OnPathEnded onPathEnded;
        public RectangleF InteractionArea;
        public float WalkSpeed;

        public Character(string name, Map map, Vector2 position) : base(map, position) {
            this.Size = new Size2(0.75F, 0.95F) * this.Map.Scale;
            this.InteractionArea = new RectangleF(new Vector2(0, -0.5F) * this.Map.Scale, new Size2(0.75F, 1.95F) * this.Map.Scale);

            this.AnimationFactory = new SpriteSheetAnimationFactory(new TextureAtlas(
                name,
                GameImpl.LoadContent<Texture2D>("Objects/" + name),
                GameImpl.LoadContent<Dictionary<string, Rectangle>>("Objects/CharacterAnimation")));

            this.AnimationFactory.Add("StandingDown", new SpriteSheetAnimationData(new[] {0}));
            this.AnimationFactory.Add("Down", new SpriteSheetAnimationData(new[] {1, 2, 3}, isPingPong: true));
            this.AnimationFactory.Add("StandingUp", new SpriteSheetAnimationData(new[] {4}));
            this.AnimationFactory.Add("Up", new SpriteSheetAnimationData(new[] {5, 6, 7}, isPingPong: true));
            this.AnimationFactory.Add("StandingLeft", new SpriteSheetAnimationData(new[] {8}));
            this.AnimationFactory.Add("Left", new SpriteSheetAnimationData(new[] {9, 10, 11}, isPingPong: true));
            this.AnimationFactory.Add("StandingRight", new SpriteSheetAnimationData(new[] {12}));
            this.AnimationFactory.Add("Right", new SpriteSheetAnimationData(new[] {13, 14, 15}, isPingPong: true));

            this.CurrentAnimation = this.AnimationFactory.Create("StandingDown");
        }

        public override bool IsStatic() {
            return false;
        }

        public bool PathfindTo(Point pos, OnPathEnded callback = null, float speed = 0.3F) {
            this.path = Util.FindPath(this.Map, this, (this.Position / this.Map.Scale).ToPoint(), pos, 10000);
            this.onPathEnded = callback;
            this.WalkSpeed = speed;
            return this.path != null;
        }

        public override void Update(GameTime time) {
            if (this.path != null && this.AllowWalking && (GameImpl.Instance.CurrentCutscene == null || this.IsCutsceneRoute)) {
                while (true) {
                    var dest = (this.path.Peek().ToVector2() + Vector2.One / 2F) * this.Map.Scale;
                    if (Vector2.Distance(this.Position, dest) <= this.WalkSpeed) {
                        this.path.Pop();
                        if (this.path.Count <= 0) {
                            this.path = null;
                            if (this.onPathEnded != null) {
                                this.onPathEnded();
                            }
                        } else {
                            continue;
                        }
                    } else {
                        var move = dest - this.Position;
                        move.Normalize();
                        this.Velocity += move * this.WalkSpeed;
                    }
                    break;
                }
            }

            base.Update(time);

            this.UpdateAnimation();
            this.CurrentAnimation.Update(time);
        }

        protected void UpdateAnimation() {
            string toPlay;
            switch (this.Direction) {
                case 0:
                    toPlay = "Up";
                    break;
                case 1:
                    toPlay = "Right";
                    break;
                case 2:
                    toPlay = "Down";
                    break;
                default:
                    toPlay = "Left";
                    break;
            }
            if (!(this is Player) || Math.Abs(this.Velocity.X) < 0.1 && Math.Abs(this.Velocity.Y) < 0.1) {
                var diff = this.Position - this.LastPosition;
                if (Math.Abs(diff.X) < 0.1 && Math.Abs(diff.Y) < 0.1) {
                    toPlay = "Standing" + toPlay;
                }
            }
            if (this.CurrentAnimation.Name != toPlay) {
                this.CurrentAnimation = this.AnimationFactory.Create(toPlay);
            }
        }

        public override void Draw(SpriteBatch batch) {
            batch.Draw(
                this.CurrentAnimation.CurrentFrame,
                this.Position - new Vector2(0.5F, 1.5F) * this.Map.Scale,
                Color.White,
                0F, Vector2.Zero, Vector2.One, SpriteEffects.None,
                this.GetRenderDepth());
        }

        public override bool OnMouse(Vector2 posWorld, int clickType) {
            if (this.DialogOptions.Count <= 0) {
                return false;
            }
            if (!this.Intersects(posWorld, new Size2(1F, 1F), this.Position + this.InteractionArea.Position, this.InteractionArea.Size)) {
                return false;
            }
            GameImpl.Instance.CurrentCursor = 2;
            if (Vector2.Distance(this.Position, GameImpl.Instance.Player.Position) <= 2F * this.Map.Scale) {
                if (clickType >= 3) {
                    var message = this.DialogOptions.Dequeue();
                    GameImpl.Instance.SetInterface(new MessageBox(message.Dialog, this));
                    if (message.Repeats) {
                        this.DialogOptions.Enqueue(message);
                    }
                }
            } else {
                GameImpl.Instance.CursorAlpha = 0.5F;
            }
            return true;
        }

        public void StopMoving() {
            this.Velocity = Vector2.Zero;
        }

        public void StopAndFace(MapObject obj) {
            this.StopMoving();
            var diffX = obj.Position.X - this.Position.X;
            var diffY = obj.Position.Y - this.Position.Y;
            if (Math.Abs(diffX) > Math.Abs(diffY)) {
                if (diffX > 0) {
                    this.Direction = 1;
                } else if (diffX < 0) {
                    this.Direction = 3;
                }
            } else {
                if (diffY > 0) {
                    this.Direction = 2;
                } else if (diffY < 0) {
                    this.Direction = 0;
                }
            }
            this.UpdateAnimation();
        }

        public delegate void OnPathEnded();

    }

    public class DialogMessage {

        public readonly string Dialog;
        public readonly bool Repeats;

        public DialogMessage(string dialog, bool repeats = false) {
            this.Dialog = dialog;
            this.Repeats = repeats;
        }

    }
}