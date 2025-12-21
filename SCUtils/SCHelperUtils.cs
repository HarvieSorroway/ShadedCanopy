using RWCustom;
using SCUtils.RwTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils
{
    public static partial class SCHelperUtils
    {
        //自定义log
        static bool logInit;
        static string path;
        private static int _mainThreadId;

        public static bool IsMainThread
        {
            get { return Thread.CurrentThread.ManagedThreadId == _mainThreadId; }
        }

        public static void Log(object msg)
        {
            if (!logInit)
            {
                path = Application.streamingAssetsPath + Path.DirectorySeparatorChar + "ShadedCanopyLog.txt";
                File.WriteAllText(path, "");
                logInit = true;
            }
            if (IsMainThread)
            {
                UnityEngine.Debug.Log("[ShadedCanopy] " + msg);
            }
            else
            {
                RwTaskHooks.PendingActions.Enqueue(() => UnityEngine.Debug.Log("[ShadedCanopy] " + msg));
            }
            File.AppendAllText(path, msg + "\n");
        }

        public static void Init()
        {
            _mainThreadId = Thread.CurrentThread.ManagedThreadId;
        }

        public class CreatureFollowingLabel: CosmeticSprite  // 跟随生物的任意文本的测试用label
        {
            FLabel label;
            Creature bindCreature;
            public Vector2 posOffset;
            public string text
            {
                get => label.text;
                set => label.text = value;
            }
            public CreatureFollowingLabel(Creature creature, Vector2 ?posOffset=null)
            {
                this.room = creature.room;
                this.bindCreature = creature;
                this.label = new FLabel(Custom.GetFont(), "");
                this.posOffset = posOffset.GetValueOrDefault();
            }
            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[0];
                this.label.RemoveFromContainer();
                rCam.ReturnFContainer("HUD").AddChild(this.label);
            }
            public override void Update(bool eu)
            {
                base.Update(eu);
                if (this.slatedForDeletetion)
                    return;
                if (this.label.container == null && this.room.game.cameras[0].room == this.room)
                {
                    this.room.game.cameras[0].ReturnFContainer("HUD").AddChild(this.label);
                }
                if (this.label.container != null && this.room.game.cameras[0].room != this.room)
                {
                    this.label.RemoveFromContainer();
                }
                if (this.bindCreature.slatedForDeletetion)
                {
                    Log("CreatureFollowingLabel: bindCreature slatedForDeletion, destroying label");
                    Destroy();
                    return;
                }
                if (this.bindCreature.room != this.room)
                {
                    this.Destroy();
                }
                this.pos = this.bindCreature.firstChunk.pos;
                this.lastPos = this.bindCreature.firstChunk.lastPos;
            }
            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                if (this.slatedForDeletetion)
                    return;
                Vector2 viewPos = Vector2.Lerp(lastPos, pos, timeStacker) - camPos + posOffset;
                label.x = viewPos.x;
                label.y = viewPos.y;
            }
            public override void Destroy()
            {
                base.Destroy();
                this.label.RemoveFromContainer();
                this.label = null;
            }
        }
        
        internal class IDLabel : CosmeticSprite
        {
            FLabel label;
            Creature bindCreature;
            string text;

            public IDLabel(Room room, Creature creature)
            {
                this.room = room;
                this.bindCreature = creature;
                var p = creature.abstractCreature.personality;
                text = $"agg:{p.aggression:F2}, brv{p.bravery:F2}, eng:{p.energy:F2}, sym:{p.sympathy:F2}";
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[0];
                label = new FLabel(Custom.GetFont(), text);
                rCam.ReturnFContainer("HUD").AddChild(label);


            }

            public override void Update(bool eu)
            {
                base.Update(eu);
                if (slatedForDeletetion)
                    return;

                if (bindCreature.slatedForDeletetion || bindCreature.room != room)
                {
                    Destroy();
                    return;
                }

                pos = bindCreature.DangerPos + Vector2.up * 80f;
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                if (!sLeaser.deleteMeNextFrame && (base.slatedForDeletetion || room != rCam.room))
                {
                    sLeaser.CleanSpritesAndRemove();
                    Destroy();
                }

                var smoothPos = Vector2.Lerp(lastPos, pos, timeStacker);
                label.SetPosition(smoothPos);
            }

            public override void Destroy()
            {
                if (slatedForDeletetion)
                    return;
                base.Destroy();

                label.RemoveFromContainer();
                bindCreature = null;
            }
        }
    }
    public static partial class SCHelperUtils
    {
        public static float LerpEase(float t)
        {
            return Mathf.Lerp(t, 1f, Mathf.Pow(t, 0.5f));
        }
        public static float EaseOutElastic(float t)
        {
            if (t == 0)
                return 0f;
            if (t == 1)
                return 1f;

            float p = 1f * .3f;
            float a = 1f;
            float s = p / 4;
            return (a * Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 1f - s) * (2 * Mathf.PI) / p) + 1f) * 0.5f + t * 0.5f;
        }
        public static float EaseInOutCubic(float f)
        {
            return f < 0.5 ? 4 * f * f * f : 1 - Mathf.Pow(-2 * f + 2, 3) / 2;
        }

        /// <summary> 获取未初始化的实例 </summary>
        public static T GetUninit<T>()
        {
            return (T)FormatterServices.GetSafeUninitializedObject(typeof(T));
        }
        public static Color GetRGBColor(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static Type[] SafeGetTypes(this Assembly assembly)
        {
            Type[] types = null;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types.Where(i => i != null).ToArray();
            }
            return types;
        }
    }
}
