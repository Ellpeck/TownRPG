using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using TownRPG.Interfaces;
using TownRPG.Maps.Objects;

namespace TownRPG.Main {
    public class Cutscene {

        private readonly Queue<CutsceneInstruction> instructions;
        private CutsceneInstruction currInst;

        public Cutscene(params CutsceneInstruction[] instructions) {
            this.instructions = new Queue<CutsceneInstruction>(instructions);
        }

        public void Update(GameTime time) {
            if (this.instructions.Count > 0 || this.currInst != null) {
                if (this.currInst == null) {
                    this.currInst = this.instructions.Dequeue();
                    this.currInst.Init();
                }
                this.currInst.Update(time);
                if (this.currInst.IsDone()) {
                    this.currInst = null;
                }
            } else {
                GameImpl.Instance.CurrentCutscene = null;
            }
        }

    }

    public abstract class CutsceneInstruction {

        public virtual void Update(GameTime time) {
        }

        public virtual void Init() {
        }

        public abstract bool IsDone();

    }

    public class CameraPos : CutsceneInstruction {

        private readonly Vector2 position;
        private readonly float speed;
        private readonly bool waitForDone;
        private readonly bool snap;
        private bool done;

        public CameraPos(Vector2 position, float speed, bool snap, bool waitForDone) {
            this.position = position;
            this.speed = speed;
            this.snap = snap;
            this.waitForDone = waitForDone;
        }

        public override void Init() {
            var cam = GameImpl.Instance.Camera;
            cam.PanToPosition = this.position;
            cam.FollowedObject = null;
            cam.PanSpeed = this.speed;
            if (this.waitForDone) {
                cam.PanCallback = () => { this.done = true; };
            } else {
                this.done = true;
            }
            if (this.snap) {
                cam.FixPosition();
            }
        }

        public override bool IsDone() {
            return this.done;
        }

    }

    public class CameraObject : CutsceneInstruction {

        private readonly MapObject obj;
        private readonly float speed;
        private readonly bool waitForDone;
        private readonly bool snap;
        private bool done;

        public CameraObject(MapObject obj, bool snap, bool waitForDone = false, float speed = 0) {
            this.obj = obj;
            this.speed = speed;
            this.snap = snap;
            this.waitForDone = waitForDone;
        }

        public override void Init() {
            var cam = GameImpl.Instance.Camera;
            cam.FollowedObject = this.obj;
            cam.PanToPosition = null;
            cam.PanSpeed = this.speed;
            if (this.waitForDone) {
                cam.PanCallback = () => this.done = true;
            } else {
                this.done = true;
            }
            if (this.snap) {
                cam.FixPosition();
            }
        }

        public override bool IsDone() {
            return this.done;
        }

    }

    public class CharacterPath : CutsceneInstruction {

        private readonly Character character;
        private readonly Point position;
        private readonly float speed;
        private readonly bool waitForDone;
        private bool done;

        public CharacterPath(Character character, Point position, float speed, bool waitForDone) {
            this.character = character;
            this.position = position;
            this.speed = speed;
            this.waitForDone = waitForDone;
        }

        public override void Init() {
            this.character.IsCutsceneRoute = true;
            this.character.PathfindTo(this.position, () => {
                this.character.IsCutsceneRoute = false;
                this.character.NoClip = false;
                if (this.waitForDone) {
                    this.done = true;
                }
            }, this.speed);
            if (!this.waitForDone) {
                this.done = true;
            }
            this.character.NoClip = true;
        }

        public override bool IsDone() {
            return this.done;
        }

    }

    public class CharacterFace : CutsceneInstruction {

        private readonly Character character;
        private readonly int direction;
        private readonly MapObject other;

        public CharacterFace(Character character, int direction = -1, MapObject other = null) {
            this.character = character;
            this.direction = direction;
            this.other = other;
        }

        public override void Init() {
            if (this.direction == -1) {
                this.character.StopAndFace(this.other);
            } else {
                this.character.Direction = this.direction;
            }
        }

        public override bool IsDone() {
            return true;
        }

    }

    public class Wait : CutsceneInstruction {

        private double secondsToWait;

        public Wait(double seconds) {
            this.secondsToWait = seconds;
        }

        public override void Update(GameTime time) {
            this.secondsToWait -= time.ElapsedGameTime.TotalSeconds;
        }

        public override bool IsDone() {
            return this.secondsToWait <= 0;
        }

    }

    public class Fade : CutsceneInstruction {

        private readonly bool waitForDone;
        private readonly float speed;
        private bool done;

        public Fade(float speed, bool waitForDone) {
            this.speed = speed;
            this.waitForDone = waitForDone;
        }

        public override void Init() {
            GameImpl.OnFaded callback;
            if (this.waitForDone) {
                callback = () => this.done = true;
            } else {
                callback = null;
                this.done = true;
            }
            GameImpl.Instance.Fade(false, this.speed, callback);
        }

        public override bool IsDone() {
            return this.done;
        }

    }

    public class ShowInterface : CutsceneInstruction {

        private readonly Interface inter;

        public ShowInterface(Interface inter) {
            this.inter = inter;
        }

        public override void Init() {
            GameImpl.Instance.SetInterface(this.inter);
        }

        public override bool IsDone() {
            return GameImpl.Instance.CurrentInterface == null;
        }

    }
}