using System;
using System.Collections.Generic;
using System.Globalization;
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

        public readonly Dictionary<string, Map> Maps = new Dictionary<string, Map>();
        public Player Player;
        public Camera Camera;

        public Color DaylightModifier = Color.Black;
        public float LightsModifier;
        public VirtualTime CurrentTime = new VirtualTime();

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

        public Interface Overlay { get; private set; }
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
            this.Overlay.InitPositions(this.GraphicsDevice.Viewport);
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
            this.AddMap(new Map(this.Content.Load<TiledMap>("Maps/Town/Inside/BlueHouse")));

            this.Player = new Player(this.Maps["Town1"], new Vector2(200, 400));
            this.CurrentMap.DynamicObjects.Add(this.Player);

            this.Camera = new Camera(this.Player) {Scale = 4.5F};
            this.Camera.FixPosition();
            this.Overlay = new Overlay();

            var tess = new Character("Tess", this.Maps["Town1"], new Vector2(22.5F, 30.5F) * this.Maps["Town1"].Scale);
            tess.DialogOptions.Enqueue(new DialogMessage("Hey! I'm Tess. How are you?"));
            tess.DialogOptions.Enqueue(new DialogMessage("Did you know you can go into the blue house south-west of town?"));
            tess.PathfindTo(new Point(7, 42), () => {
                tess.StopAndFace(2);
                tess.DialogOptions.Enqueue(new DialogMessage("I really enjoy the river. It's nice and calm."));
            });
            tess.Map.DynamicObjects.Add(tess);
        }

        protected override void Update(GameTime gameTime) {
            if (!this.IsPaused) {
                foreach (var map in this.Maps.Values) {
                    map.Update(gameTime);
                }

                if (this.CurrentMap != null) {
                    this.CurrentTime.Update(gameTime);
                    this.UpdateLighting();

                    this.MapRenderer.Update(this.CurrentMap.Tiles, gameTime);
                }
            }

            this.Camera.Update();
            if (this.CurrentCutscene == null) {
                this.Overlay.Update(gameTime);
            }
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
                this.CurrentInterface.OnMouse(state.Position, type);
            } else {
                if (this.CurrentCutscene == null && this.Overlay.OnMouse(state.Position, type)) {
                    return;
                }

                var world = this.Camera.ToWorldPos(state.Position.ToVector2());
                foreach (var obj in this.CurrentMap.StaticObjects.Values) {
                    if (obj.OnMouse(world, type)) {
                        return;
                    }
                }
                foreach (var obj in this.CurrentMap.DynamicObjects) {
                    if (obj.OnMouse(world, type)) {
                        return;
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

                        if (this.LightsModifier > 0) {
                            foreach (var light in this.CurrentMap.LightSources) {
                                Vector2 size = light.Size * this.Camera.Scale;
                                this.SpriteBatch.Draw(
                                    light.Texture,
                                    new Rectangle(
                                        (this.Camera.ToCameraPos(light.Position) - size / 2).ToPoint(),
                                        size.ToPoint()),
                                    light.ColorModifier * this.LightsModifier);
                            }
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

                this.SpriteBatch.Begin(
                    SpriteSortMode.Deferred, null, SamplerState.PointClamp, null, null, null,
                    Matrix.CreateScale(Interface.Scale));
                if (this.CurrentCutscene == null) {
                    this.Overlay.Draw(this.SpriteBatch);
                }
                if (this.CurrentInterface != null) {
                    this.CurrentInterface.Draw(this.SpriteBatch);
                }
                this.SpriteBatch.End();

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

        public void UpdateLighting() {
            const int eveningTime = 18;
            const int nightTime = 23;
            const int morningTime = 5;
            const int dayTime = 10;

            var nightColor = this.CurrentMap.NightColor;
            var hours = this.CurrentTime.TotalMinutes / 60 % 24;
            if (hours >= nightTime || hours < morningTime) {
                // night
                this.LightsModifier = 1;
                this.DaylightModifier = nightColor;
            } else if (hours >= eveningTime) {
                // evening
                this.LightsModifier = (hours - eveningTime) / (nightTime - eveningTime);
                this.DaylightModifier = nightColor * this.LightsModifier;
            } else if (hours >= dayTime) {
                // day
                this.LightsModifier = 0;
                this.DaylightModifier = Color.Black;
            } else if (hours >= morningTime) {
                // morning
                var rednessMod = (hours - morningTime) / (dayTime - morningTime);
                this.LightsModifier = 1 - rednessMod;
                if (!this.CurrentMap.IsInside) {
                    this.DaylightModifier = new Color(
                                                nightColor.R - (int) (30 * rednessMod),
                                                nightColor.G - (int) (20 * rednessMod),
                                                nightColor.B + (int) (100 * rednessMod))
                                            * this.LightsModifier;
                } else {
                    this.DaylightModifier = nightColor * this.LightsModifier;
                }
            }
        }

        public void AddMap(Map map) {
            this.Maps.Add(map.Name, map);
        }

        public static T LoadContent<T>(string name) {
            return Instance.Content.Load<T>(name);
        }

    }

    public class VirtualTime {

        private const float SecondsPerMinute = 0.25F;

        public float TotalMinutes = 60 * 7;

        public int Minute {
            get { return (int) this.TotalMinutes % 60; }
        }

        public int Hour {
            get { return (int) this.TotalMinutes / 60; }
        }

        public int Day {
            get { return (int) this.TotalMinutes / 60 / 24; }
        }

        public void Update(GameTime time) {
            this.TotalMinutes += (float) time.ElapsedGameTime.TotalSeconds / SecondsPerMinute;
        }

        public string TimeToString() {
            var amPm = this.Hour >= 12 ? " pm" : " am";
            var hour = this.Hour == 0 ? 12 : this.Hour;
            var minute = (this.Minute / 10 * 10).ToString("D2");
            return hour + ":" + minute + amPm;
        }

    }
}