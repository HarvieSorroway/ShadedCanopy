using Menu.Remix.MixedUI;
using RWCustom;
using SCUtils;
using SCUtils.SCDevTools.NodeTreeManager;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace ShadedCanopy.Iterators.MechIterator
{
    internal class MechIteratorGraphic
    {
        int projPointCount, totSpriteCount;

        public MechIterator mechIterator;

        Point[] points;

        FibonacciSphereCaster sphereCaster;
        List<KarmaRingCaster> ringCasters = new List<KarmaRingCaster>();
        //KarmaRingCaster leftRing, rightRing;
        List<PointCaster> pointCasters = new List<PointCaster>();
        Dictionary<int, PointCaster> casterPointsFirstIndexMap = new Dictionary<int, PointCaster>();

        //动画参数
        float Expand;

        public Vector2 noticedPlayerPos;
        Vector2 lookDir;//-1 ~ 1;

        public AnimationID animationID, nextAnimation;
        Animation currentAnimation;

        public MechIteratorGraphic(MechIterator mechIterator)
        {
            this.mechIterator = mechIterator;
            
            pointCasters.Add(sphereCaster = new FibonacciSphereCaster(this, 2f));
            var ringCaster = new KarmaRingCaster(this, false, 20f, 80f, 120f, 20, 0.8f);
            pointCasters.Add(ringCaster);
            ringCasters.Add(ringCaster);

            ringCaster = new KarmaRingCaster(this, true, 30f, 80f, 100f, 30, 0.5f);
            pointCasters.Add(ringCaster);
            ringCasters.Add(ringCaster);

            ringCaster = new KarmaRingCaster(this, false, 40f, 80f, 85f,40, 0.4f);
            pointCasters.Add(ringCaster);
            ringCasters.Add(ringCaster);

            ringCaster = new KarmaRingCaster(this, true, 50f, 80f, 80f,60, 0.2f);
            pointCasters.Add(ringCaster);
            ringCasters.Add(ringCaster);

            foreach (var caster in pointCasters)
            {
                casterPointsFirstIndexMap.Add(projPointCount, caster);
                projPointCount += caster.PointsCount;
            }

            totSpriteCount = projPointCount * 3;
            points = new Point[projPointCount];

            int casterIndex = 0;
            int casterStartIndex = 0;
            for(int i = 0;i < projPointCount;i++)
            {
                if (casterPointsFirstIndexMap.TryGetValue(i, out var caster))
                {
                    casterIndex = pointCasters.IndexOf(caster);
                    casterStartIndex = i;
                }
                points[i] = new Point(casterIndex, i - casterStartIndex);
            }

            ForceSwitchAnimation(AnimationID.Idle);
        }


        public void Update()
        {
            noticedPlayerPos = mechIterator.room.game.FirstRealizedPlayer.firstChunk.pos;
            AnimationUpdate();

            foreach (var ring in ringCasters)
            {
                ring.rotation = Quaternion.Euler(-90f + lookDir.y * 50f * Expand, 90f + lookDir.x * 50f * Expand, 0);
                //ring.rotation = Quaternion.Euler(mechIterator.RotX, mechIterator.RotY, mechIterator.RotZ);
            }

            foreach (var caster in pointCasters)
            {
                caster.Update();
            }
            for(int i = 0;i < points.Length;i++)
            {
                points[i].Update(this);
            }
        }


        #region DrawFunctions
        public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.sprites = new FSprite[totSpriteCount];
            for (int i = 0; i < projPointCount; i++)
            {
                FSprite sprite = new FSprite(SCResources.Blur80Atlas, true) { shader = rCam.room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName] };
                sprite.color = Color.blue;
                sLeaser.sprites[i] = sprite;
            }
            for (int i = projPointCount; i < projPointCount * 2; i++)
            {
                FSprite sprite = new FSprite(SCResources.Blur40Atlas, true) { shader = rCam.room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName] };
                sprite.color = Color.cyan;
                sLeaser.sprites[i] = sprite;
            }
            for (int i = projPointCount * 2; i < projPointCount * 3; i++)
            {
                FSprite sprite = new FSprite("pixel", true) { shader = rCam.room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName], scale = 2.1f };
                sprite.color = Color.cyan;
                sLeaser.sprites[i] = sprite;
            }
            AddToContainer(sLeaser, rCam, null);
        }

        public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("HUD");
            }
            foreach (var sprite in sLeaser.sprites)
            {
                newContatiner.AddChild(sprite);
            }
        }

        public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
        }

        public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (mechIterator.slatedForDeletetion)
                return;
            for (int i = 0; i < projPointCount; i++)
            {
                sLeaser.sprites[i].SetPosition(points[i].DrawPos(camPos, timeStacker));
                sLeaser.sprites[i].alpha = points[i].alpha;

                sLeaser.sprites[i + projPointCount].SetPosition(points[i].DrawPos(camPos, timeStacker));
                sLeaser.sprites[i + projPointCount].alpha = points[i].alpha;

                sLeaser.sprites[i + projPointCount * 2].SetPosition(points[i].DrawPos(camPos, timeStacker));
                sLeaser.sprites[i + projPointCount * 2].alpha = points[i].alpha;
            }
        }
        #endregion

        #region AnimationFunctions
        public void AnimationUpdate()
        {
            if(currentAnimation.AllowSwitch && nextAnimation != null)
            {
                SwitchAnimation(nextAnimation);
            }
            currentAnimation?.Update();
        }
        public void RequestSwitchAnimation(AnimationID newAnim)
        {
            nextAnimation = newAnim;
        }

        public void ForceSwitchAnimation(AnimationID newAnim)
        {
            SwitchAnimation(newAnim);
        }

        void SwitchAnimation(AnimationID newAnim)
        {
            Animation newAnimation;

            if(newAnim == AnimationID.Idle)
            {
                newAnimation = new IdleAnimation(this);
            }
            else if(newAnim == AnimationID.NoticePlayer)
            {
                newAnimation = new NoticePlayerAnimation(this);
            }
            else if (newAnim == AnimationID.TalkToPlayer)
            {
                newAnimation = new Animation(this);
            }
            else
            {
                SCUtils.SCUtils.Log($"MechIteratorGraphic: Unknown AnimationID {newAnim}");
                return;
            }

            if (currentAnimation != null)
            {
                newAnimation.SwitchAnim(currentAnimation);
            }
            currentAnimation = newAnimation;
            nextAnimation = null;
        }
        #endregion

        #region PointCasters
        internal class Point
        {
            static float FireUpAnimChance = 0.01f, AnimAlpha = 0.1f;

            public int casterIndex, pointIndex;
            public Vector2 pos, lastPos;
            public float alpha;

            bool inAnim;
            float switchPosAnim;
            int noAnimCounter;
            Vector2 animOrigPos, animMidPos;

            public bool ReadyForAnim => !inAnim && noAnimCounter == 0;

            public Point(int initCasterIndex, int initPointIndex)
            {
                this.casterIndex = initCasterIndex;
                this.pointIndex = initPointIndex;
                SCUtils.SCUtils.Log($"Point : {initCasterIndex} - {initPointIndex}");
            }

            public void Update(MechIteratorGraphic graphic)
            {
                lastPos = pos;

                var caster = graphic.pointCasters[casterIndex];
                Vector2 targetPos = caster.Cast(pointIndex) + graphic.mechIterator.pos;
                float targetAlpha = caster.GetAlpha(pointIndex);

                if (inAnim)
                {
                    switchPosAnim = Mathf.Clamp01(switchPosAnim + 1 / 40f);

                    Vector2 a = Vector2.Lerp(animOrigPos, animMidPos, switchPosAnim);
                    Vector2 b = Vector2.Lerp(animMidPos, targetPos, switchPosAnim);
                    targetPos = Vector2.Lerp(a, b, switchPosAnim);
                    targetAlpha = AnimAlpha;

                    if (switchPosAnim == 1f)
                    {
                        inAnim = false;
                        noAnimCounter = 40;
                    }
                }
                
                if(noAnimCounter > 0)
                {
                    noAnimCounter--;
                }
                else
                {
                    if (Random.value < FireUpAnimChance * graphic.Expand)
                    {
                        var randomPoint = graphic.points[Random.Range(0, graphic.points.Length)];
                        if(randomPoint != this && randomPoint.ReadyForAnim)
                        {
                            SwitchCasterWith(randomPoint);
                        }
                    }
                }

                pos = Vector2.Lerp(pos, targetPos, 0.15f);
                alpha = Mathf.Lerp(alpha, targetAlpha, 0.1f);
            }

            public Vector2 DrawPos(Vector2 camPos, float timeStacker)
            {
                return Vector2.Lerp(lastPos, pos, timeStacker) - camPos;
            }

            public void SwitchCasterWith(Point anotherPoint)
            {
                int tempCasterIndex = casterIndex;
                int tempPointIndex = pointIndex;

                casterIndex = anotherPoint.casterIndex;
                pointIndex = anotherPoint.pointIndex;

                anotherPoint.casterIndex = tempCasterIndex;
                anotherPoint.pointIndex = tempPointIndex;

                FireUpAnim();
                anotherPoint.FireUpAnim();
            }

            public void FireUpAnim()
            {
                switchPosAnim = 0f;
                animOrigPos = pos;
                animMidPos = (pos - lastPos) * 40f + pos;
                inAnim = true;
            }
        }

        internal class PointCaster
        {
            public virtual int PointsCount { get; }
            public MechIteratorGraphic graphic;
            public PointCaster(MechIteratorGraphic graphic)
            {
                this.graphic = graphic;
            }

            public virtual void Update()
            {
                throw new NotImplementedException();
            }

            public virtual Vector2 Cast(int i)
            {
                throw new NotImplementedException();
            }

            public virtual float GetAlpha(int i)
            {
                return 1f;
            }
        }

        internal class FibonacciSphereCaster : PointCaster
        {
            static float GoldenRatio = (1f + Mathf.Sqrt(5)) / 2;

            public override int PointsCount => 100;

            float _rad;
            float Rad
            {
                get => _rad;
                set
                {
                    if (_rad != value)
                    {
                        _rad = value;
                        _needRecalc = true;
                    }
                }
            }

            float _extraPush;
            float extraPush
            {
                get => _extraPush;
                set
                {
                    if(_extraPush != value)
                    {
                        _extraPush = value;
                        _needRecalc = true;
                    }
                }
            }
            bool _needRecalc = false;

            List<Vector2> cachedPoints = new List<Vector2>() { Vector2.zero };
            List<float> alphas = new List<float>() { 1f };

            Quaternion rotation;

            public FibonacciSphereCaster(MechIteratorGraphic graphic, float extraPush) : base(graphic)
            {
                Rad = 80f;
            }

            public override void Update()
            {
                //Rad = graphic.mechIterator.FibonacciSphereCastRad;
                extraPush = Mathf.Lerp(0.1f, 10f, graphic.Expand);

                rotation = Quaternion.Euler(Time.time * 10f % 360f, Time.time * 15f % 360f, -Time.time * 23f % 360f);
                _needRecalc = true;
                if (_needRecalc)
                {
                    CaculateCastPoints();
                    _needRecalc = false;
                }
            }

            public override Vector2 Cast(int i)
            {
                if (i < cachedPoints.Count && i >= 0)
                    return cachedPoints[i];
                else
                    return cachedPoints.Last();
            }

            public override float GetAlpha(int i)
            {
                if (i < alphas.Count && i >= 0)
                    return alphas[i];
                else
                    return alphas.Last();
            }

            void CaculateCastPoints()
            {
                cachedPoints.Clear();
                alphas.Clear();
                // 参数合法性校验
                if (PointsCount < 1)
                    throw new ArgumentOutOfRangeException(nameof(PointsCount), "点的数量必须大于等于1");
                if (Rad < 0)
                    throw new ArgumentOutOfRangeException(nameof(Rad), "球面半径必须非负");

                List<Vector3> points = new List<Vector3>();

                
                if (PointsCount == 1)// 特殊场景：仅1个点时，默认放在北极点
                {
                    points.Add(new Vector3(0, Rad, 0));
                }
                else // 斐波那契球面核心逻辑
                {
                    for (int i = 0; i < PointsCount; i++)
                    {
                        // 1. 计算Y轴坐标（单位球面：从+1到-1线性分布）
                        float y = 1 - (i / (float)(PointsCount - 1)) * 2;

                        // 2. 计算垂直于Y轴的圆半径（保证点在单位球面上）
                        float r = Mathf.Sqrt(1 - y * y);

                        // 3. 利用黄金比例计算角度θ，避免分布重复
                        float theta = 2f * Mathf.PI * i / GoldenRatio;

                        // 4. 极坐标转笛卡尔坐标（X/Z轴）
                        float x = r * Mathf.Cos(theta);
                        float z = r * Mathf.Sin(theta);

                        // 5. 缩放到指定球面半径
                        points.Add(rotation * new Vector3(x * Rad, y * Rad, z * Rad));
                    }
                }
               


                foreach(var point in points)
                {
                    Vector2 pos = new Vector2(point.x, point.z);
                    Vector2 posOnRing = pos.normalized * Rad;
                    float pushFactor = Mathf.Pow((Rad - pos.magnitude) * pos.magnitude / (Rad * Rad), 1f/extraPush);
                    cachedPoints.Add(Vector2.Lerp(pos ,posOnRing, pushFactor));
                    alphas.Add(Mathf.Lerp(0.05f, 0.4f + 0.4f * graphic.Expand, Mathf.InverseLerp(-Rad * 0.1f, Rad * 0.1f, point.y)) *(pos.magnitude / Rad));
                }
            }
        }

        internal class KarmaRingCaster : PointCaster
        {
            List<Vector2> cachedPoints = new List<Vector2>() { Vector2.zero };
            List<float> alphas = new List<float>() { 1f };

            //public float zRot;
            public Quaternion rotation;

            int _totPoints = 0;
            public override int PointsCount => _totPoints;

            bool _needRecalc = false;

            float _rad;
            float Rad
            {
                get => _rad;
                set
                {
                    if (_rad != value)
                    {
                        _rad = value;
                        _needRecalc = true;
                    }
                }
            }

            float dist, sphereRad, alpha;
            bool _reverse;


            public KarmaRingCaster(MechIteratorGraphic graphic, bool reverse, float Rad, float sphereRad, float dist, int totPoints, float alpha) : base(graphic)
            {
                this.Rad = Rad;
                this.dist = dist;
                this.sphereRad = sphereRad;
                this._totPoints = totPoints;
                this.alpha = alpha;
                _reverse = reverse;
            }

            void CaculateCastPoints()
            {
                cachedPoints.Clear();
                alphas.Clear();

                // 参数合法性校验
                if (PointsCount < 1)
                    throw new ArgumentOutOfRangeException(nameof(PointsCount), "点的数量必须大于等于1");
                if (Rad < 0)
                    throw new ArgumentOutOfRangeException(nameof(Rad), "球面半径必须非负");

                List<Vector3> points = new List<Vector3>();


                if (PointsCount == 1)// 特殊场景：仅1个点时，默认放在北极点
                {
                    points.Add(new Vector3(Rad, 0, 0));
                }
                else
                {
                    float rad = Mathf.Lerp(sphereRad, Rad, graphic.Expand);
                    for (int i = 0;i < PointsCount; i++)
                    {
                        Vector3 pos = new Vector3(Mathf.Cos(Mathf.PI * i * 2 / (float)PointsCount) * rad, dist, Mathf.Sin(Mathf.PI * i * 2 / (float)PointsCount) * rad);
                        pos = (Quaternion.Euler(0, Time.time * 10f % 360f * (_reverse ? 1f : -1f), 0f)) * pos;
                        pos = rotation * pos;

                        points.Add(pos);
                    }
                }

                foreach (var point in points)
                {
                    Vector2 pos = new Vector2(point.z, point.y);
                    cachedPoints.Add(pos);
                    alphas.Add(0.8f);
                }
            }

            public override void Update()
            {
                _needRecalc = true;
                if (_needRecalc)
                {
                    CaculateCastPoints();
                    _needRecalc = false;
                }
            }
            public override Vector2 Cast(int i)
            {
                if (i < cachedPoints.Count && i >= 0)
                    return cachedPoints[i];
                else
                    return cachedPoints.Last();
            }

            public override float GetAlpha(int i)
            {
                return alpha;
            }
        }

        #endregion

        #region Animations
        internal class Animation
        {
            public virtual bool AllowSwitch => true;

            public MechIteratorGraphic graphic;

            public int timeInAnim;
            public Animation(MechIteratorGraphic graphic)
            {
                this.graphic = graphic;
            }

            public virtual void Update()
            {
                timeInAnim++;
            }

            public virtual void SwitchAnim(Animation lastAnim)
            {

            }
        }

        internal class IdleAnimation : Animation
        {
            float initExpand;

            public override bool AllowSwitch => timeInAnim > 40;

            public IdleAnimation(MechIteratorGraphic graphic) : base(graphic)
            {
                initExpand = graphic.Expand;
            }

   
            public override void SwitchAnim(Animation lastAnim)
            {
            }

            public override void Update()
            {
                base.Update();
                if(timeInAnim <= 40)
                {
                    graphic.Expand = SCUtils.SCUtils.EaseInOutCubic(Mathf.Lerp(initExpand, 0f, timeInAnim / 40f));
                }
                else
                {
                    graphic.lookDir = Vector2.zero;
                    if (Vector2.Distance(graphic.noticedPlayerPos, graphic.mechIterator.pos) < 200f)
                    {
                        graphic.RequestSwitchAnimation(AnimationID.NoticePlayer);
                    }
                }
            }
        }

        internal class NoticePlayerAnimation : Animation
        {
            float initExpand;
            public override bool AllowSwitch => timeInAnim > 40;

            public NoticePlayerAnimation(MechIteratorGraphic graphic) : base(graphic)
            {
                initExpand = graphic.Expand;
            }

            public override void SwitchAnim(Animation lastAnim)
            {
                base.SwitchAnim(lastAnim);
                initExpand = graphic.Expand;
            }

            public override void Update()
            {
                base.Update();
                if (timeInAnim <= 40)
                {
                    graphic.Expand = SCUtils.SCUtils.EaseInOutCubic(Mathf.Lerp(initExpand, 1f, timeInAnim / 40f));
                }
                else
                {
                    if (Vector2.Distance(graphic.noticedPlayerPos, graphic.mechIterator.pos) > 400f)
                    {
                        graphic.RequestSwitchAnimation(AnimationID.Idle);
                    }
                    Vector2 deltaPox = (graphic.noticedPlayerPos - graphic.mechIterator.pos);

                    Vector2 targetLookDir = new Vector2(Mathf.InverseLerp(-500f, 500f, deltaPox.x) * 2f - 1f, Mathf.InverseLerp(-500f, 500f, deltaPox.y) * 2f - 1f);
                    graphic.lookDir = Vector2.Lerp(graphic.lookDir, targetLookDir, 0.25f);
                }
              
            }
        }

        internal class AnimationID : ExtEnum<AnimationID>
        {
            public static readonly AnimationID Idle = new AnimationID("Idle", true);
            public static readonly AnimationID NoticePlayer = new AnimationID("NoticePlayer", true);

            public static readonly AnimationID TalkToPlayer = new AnimationID("TalkToPlayer", true);

            public AnimationID(string value, bool register = false) : base(value, register)
            {
            }
        }
        #endregion

        #region ProjectionText

        [SCDevToolsInspectType("Root.RainWorld.Game.World.Room", "ProjTextLabel")]
        public class ProjTextLabel : CosmeticSprite
        {
            public static Color labelCol = Custom.hexToColor("7BFFF0");

            public FLabel[] rLabels, gLabels, bLabels;
            public Vector2[] rBias, gBias, bBias;
 
            List<Vector2> topLeftRelativePos = new List<Vector2>();

            [SCDevToolsInspectValue]
            [SCDevToolsRangeField(0f, 100f)]
            public float revealProgression;

            bool labelCleaned;
            public ProjTextLabel(Room room, string text, Vector2 topLeft)
            {
                this.room = room;
                this.pos = this.lastPos = topLeft;
                string font = Custom.GetDisplayFont();

                text = LabelTest.GlobalTextModifier(text);

                FLetterQuadLine[] quadLines = Futile.atlasManager.GetFontWithName(font).GetQuadInfoForText(text, new FTextParams());
                foreach(var line in quadLines)
                {
                    foreach(var quad in line.quads)
                    {
                        topLeftRelativePos.Add(quad.rect.position);
                    }
                }

                rLabels = new FLabel[text.Length];
                gLabels = new FLabel[text.Length];
                bLabels = new FLabel[text.Length];

                rBias = new Vector2[text.Length];
                gBias = new Vector2[text.Length];
                bBias = new Vector2[text.Length];

                for(int i = 0;i < text.Length; i++)
                {
                    rLabels[i] = new FLabel(font, text.Substring(i, 1)) { shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName], color = Color.red , alpha = 0f};
                    gLabels[i] = new FLabel(font, text.Substring(i, 1)) { shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName], color = Color.green , alpha = 0f};
                    bLabels[i] = new FLabel(font, text.Substring(i, 1)) { shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName], color = Color.blue , alpha = 0f };

                    rBias[i] = Custom.RNV() * Mathf.Lerp(5f, 8f, Random.value);
                    gBias[i] = Custom.RNV() * Mathf.Lerp(5f, 8f, Random.value);
                    bBias[i] = Custom.RNV() * Mathf.Lerp(5f, 8f, Random.value);
                }
                SCDevNodeTreeManager.Track(this);
            }

            public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
            {
                sLeaser.sprites = new FSprite[rLabels.Length];
                for(int i = 0; i< sLeaser.sprites.Length; i++)
                {
                    sLeaser.sprites[i] = new FSprite(SCResources.Blur80Atlas, true)
                    {
                        shader = room.game.rainWorld.Shaders[SCResources.AdditiveDefaultShaderName],
                        color = Color.blue,
                        alpha = 0f
                    };
                }
                AddToContainer(sLeaser, rCam, null);
            }

            public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
            {
                if (newContatiner == null)
                    newContatiner = rCam.ReturnFContainer("HUD");
                for(int i = 0;i < rLabels.Length;i++)
                {
                    newContatiner.AddChild(sLeaser.sprites[i]);
                    newContatiner.AddChild(rLabels[i]);
                    newContatiner.AddChild(gLabels[i]);
                    newContatiner.AddChild(bLabels[i]);
                }
            }

            public override void Update(bool eu)
            {
                base.Update(eu);
            }

            public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
            {
                base.DrawSprites(sLeaser, rCam, timeStacker, camPos);

                if (slatedForDeletetion)
                {
                    if (!labelCleaned)
                        CleanOutLabels();
                    return;
                }

                Vector2 smoothPos = Vector2.Lerp(lastPos, pos, timeStacker) - camPos;

                for(int i = 0;i < rLabels.Length; i++)
                {
                    float localReveal = Mathf.Clamp01(revealProgression - i);

                    float targetAlphaR, targetAlphaG, targetAlphaB;
                    targetAlphaR = Mathf.Lerp(1f, labelCol.r, localReveal);
                    targetAlphaG = Mathf.Lerp(1f, labelCol.g, localReveal);
                    targetAlphaB = Mathf.Lerp(1f, labelCol.b, localReveal);

                    rLabels[i].alpha = (Random.value < localReveal ? 1f : 0f) * targetAlphaR;
                    rLabels[i].SetPosition(rBias[i] * Mathf.Pow(1f - localReveal, 2f) + topLeftRelativePos[i] + smoothPos);

                    gLabels[i].alpha = (Random.value < localReveal ? 1f : 0f) * targetAlphaG;
                    gLabels[i].SetPosition(gBias[i] * Mathf.Pow(1f - localReveal, 2f) + topLeftRelativePos[i] + smoothPos);


                    bLabels[i].alpha = (Random.value < localReveal ? 1f : 0f) * targetAlphaB;
                    bLabels[i].SetPosition(bBias[i] * Mathf.Pow(1f - localReveal, 2f) + topLeftRelativePos[i] + smoothPos);

                    sLeaser.sprites[i].SetPosition(topLeftRelativePos[i] + smoothPos);
                    sLeaser.sprites[i].alpha = Mathf.Pow((rLabels[i].alpha + gLabels[i].alpha + bLabels[i].alpha) / 3f, 2f);
                }
            }

            public void CleanOutLabels()
            {
                for(int i = 0;i < rLabels.Length; i++)
                {
                    rLabels[i].RemoveFromContainer();
                    gLabels[i].RemoveFromContainer();
                    bLabels[i].RemoveFromContainer();
                }
            }

        }
        #endregion

    }
}
