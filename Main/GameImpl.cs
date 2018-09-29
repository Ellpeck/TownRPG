using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.TextureAtlases;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using TownRPG.Interfaces;
using TownRPG.Maps;
using TownRPG.Maps.Objects;

namespace TownRPG.Main {
    public class GameImpl : Game {

        public static GameImpl Instance { get; private set; }
        public SpriteFont NormalFont;

        private GraphicsDeviceManager GraphicsDeviceManager;
        public SpriteBatch SpriteBatch { get; private set; }
        public TiledMapRenderer MapRenderer;
        public RenderTarget2D Lightmap;

        public int CurrentCursor;
        public float CursorAlpha;
        private Texture2D cursorsTexture;
        private Point lastMousePos;
        private int lastClickType;

        public Dictionary<string, Map> Maps = new Dictionary<string, Map>();
        public Player Player;
        public Camera Camera;

        public Color DaylightModifier = new Color(0, 0, 0);

        public Map CurrentMap {
            get { return this.Player.Map; }
        }

        public bool IsPaused {
            get {
                return this.fadeSpeed != 0 && this.doesFadePauseGame
                       || this.CurrentInterface != null && this.CurrentInterface.Pauses();
            }
        }

        private bool doesFadePauseGame;
        private float fadeSpeed;
        private float fadePercentage;
        private OnFaded fadeCallback;

        public Interface CurrentInterface { get; private set; }
        public Cutscene CurrentCutscene;

        public GameImpl() {
            Instance = this;

            this.GraphicsDeviceManager = new GraphicsDeviceManager(this) {
                PreferredBackBufferWidth = 1280,
                PreferredBackBufferHeight = 720
            };
            this.Content.RootDirectory = "Content";
            this.Window.AllowUserResizing = true;
            this.Window.ClientSizeChanged += this.OnWindowSizeChange;
        }

        private void OnWindowSizeChange(object window, EventArgs args) {
            if (this.CurrentInterface != null) {
                this.CurrentInterface.InitPositions(this.GraphicsDevice.Viewport);
            }
            this.Camera.FixPosition();
            var view = this.GraphicsDevice.Viewport;
            this.Lightmap = new RenderTarget2D(this.GraphicsDevice, view.Width, view.Height);
        }

        protected override void LoadContent() {
            this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
            var view = this.GraphicsDevice.Viewport;
            this.Lightmap = new RenderTarget2D(this.GraphicsDevice, view.Width, view.Height);
            this.MapRenderer = new TiledMapRenderer(this.GraphicsDevice);
            this.cursorsTexture = this.Content.Load<Texture2D>("Interfaces/Cursors");
            this.NormalFont = this.Content.Load<SpriteFont>("Interfaces/NormalFont");

            this.AddMap(new Map(this.Content.Load<TiledMap>("Maps/Town/Town1")));

            this.Player = new Player(this.Maps["Town1"], new Vector2(200, 400));
            this.CurrentMap.AddObject(this.Player);

            this.Camera = new Camera(this.Player) {Scale = 4F};
            this.Camera.FixPosition();
        }

        protected override void Update(GameTime gameTime) {
            if (!this.IsPaused) {
                foreach (var map in this.Maps.Values) {
                    map.Update(gameTime);
                }

                this.MapRenderer.Update(this.CurrentMap.Tiles, gameTime);
            }

            this.Camera.Update();
            if (this.CurrentInterface != null) {
                this.CurrentInterface.Update(gameTime);
            }
            if (this.CurrentCutscene != null) {
                this.CurrentCutscene.Update(gameTime);
            }
            this.DoMouseStuff();

            if (this.fadeSpeed != 0) {
                this.fadePercentage += this.fadeSpeed;
                if (this.fadeSpeed > 0 ? this.fadePercentage >= 1 : this.fadePercentage <= 0) {
                    this.fadeSpeed = 0;
                    if (this.fadeCallback != null) {
                        this.fadeCallback();
                    }
                }
            }

            base.Update(gameTime);
        }

        private void DoMouseStuff() {
            this.CurrentCursor = this.CurrentCutscene != null && this.CurrentInterface == null ? -1 : 0;
            this.CursorAlpha = 1;

            var state = Mouse.GetState();
            var type = state.LeftButton == ButtonState.Pressed ? 1 : state.RightButton == ButtonState.Pressed ? 2 : 0;
            if (this.lastClickType != type) {
                this.lastClickType = type;
                type += 2;
            }

            if (this.CurrentInterface != null) {
                if (!this.CurrentInterface.OnMouse(state.Position, type)) {
                    foreach (var component in this.CurrentInterface.Components) {
                        if (component.OnMouse(state.Position, type)) {
                            break;
                        }
                    }
                }
            } else {
                var world = this.Camera.ToWorldPos(state.Position.ToVector2());
                foreach (var obj in this.CurrentMap.AllObjects) {
                    if (obj.OnMouse(world, type)) {
                        break;
                    }
                }
            }
        }

        protected override void Draw(GameTime gameTime) {
            if (this.fadePercentage < 1F) {
                if (this.CurrentMap != null) {
                    if (this.DaylightModifier != Color.Black) {
                        this.GraphicsDevice.SetRenderTarget(this.Lightmap);
                        this.GraphicsDevice.Clear(this.DaylightModifier);
                        this.SpriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp);

                        foreach (var light in this.CurrentMap.LightSources) {
                            Vector2 size = light.Size * this.Camera.Scale;
                            this.SpriteBatch.Draw(
                                light.Texture,
                                new Rectangle(
                                    (this.Camera.ToCameraPos(light.Position) - size / 2).ToPoint(),
                                    size.ToPoint()),
                                light.ColorModifier);
                        }

                        this.SpriteBatch.End();
                        this.GraphicsDevice.SetRenderTarget(null);
                    }

                    this.GraphicsDevice.Clear(this.CurrentMap.Tiles.BackgroundColor.GetValueOrDefault());
                    this.CurrentMap.Draw(this.SpriteBatch, this.Camera.ViewMatrix);

                    if (this.DaylightModifier != Color.Black) {
                        this.SpriteBatch.Begin(SpriteSortMode.Deferred, new BlendState {
                            ColorBlendFunction = BlendFunction.ReverseSubtract,
                            ColorSourceBlend = Blend.SourceColor,
                            ColorDestinationBlend = Blend.One
                        }, SamplerState.PointClamp);
                        this.SpriteBatch.Draw(this.Lightmap, Vector2.Zero, this.Lightmap.Bounds, Color.White);
                        this.SpriteBatch.End();
                    }
                } else {
                    this.GraphicsDevice.Clear(Color.Black);
                }

                if (this.CurrentInterface != null) {
                    this.SpriteBatch.Begin(
                        SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null,
                        Matrix.CreateScale(Interface.Scale));
                    this.CurrentInterface.Draw(this.SpriteBatch);
                    this.SpriteBatch.End();
                }

                base.Draw(gameTime);
            }

            if (this.fadePercentage > 0F) {
                var view = this.GraphicsDevice.Viewport;
                this.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);
                this.SpriteBatch.FillRectangle(Vector2.Zero, new Size2(view.Width, view.Height), Color.Black * this.fadePercentage);
                this.SpriteBatch.End();
            }

            if (this.CurrentCursor >= 0) {
                this.SpriteBatch.Begin(SpriteSortMode.Immediate, null, SamplerState.PointClamp);
                this.SpriteBatch.Draw(
                    this.cursorsTexture,
                    new Rectangle(this.lastMousePos, new Point(32, 32)),
                    new Rectangle(this.CurrentCursor % 4 * 16, this.CurrentCursor / 4 * 16, 16, 16),
                    Color.Multiply(Color.White, this.CursorAlpha));
                this.SpriteBatch.End();
            }
            this.lastMousePos = Mouse.GetState().Position;
        }

        public void Fade(bool pauses, float speed, OnFaded callback = null) {
            this.fadeSpeed = speed;
            this.doesFadePauseGame = pauses;
            this.fadeCallback = callback;
        }

        public delegate void OnFaded();

        public void SetInterface(Interface inter) {
            if (this.CurrentInterface != null) {
                this.CurrentInterface.OnClose();
            }
            this.CurrentInterface = inter;
            if (inter != null) {
                inter.InitPositions(this.GraphicsDevice.Viewport);
                inter.OnOpen();
            }
        }

        public void AddMap(Map map) {
            this.Maps.Add(map.Name, map);
        }

        public static T LoadContent<T>(string name) {
            return Instance.Content.Load<T>(name);
        }

    }
}