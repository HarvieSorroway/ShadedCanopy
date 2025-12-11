using SCUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.FlashingEffect.ForeObjectMask
{
    internal class ForeObjectMask
    {
        internal FContainer container;
        internal bool Show;

        List<CopiedSpriteLeaser> copiedSpriteLeasers = new List<CopiedSpriteLeaser>();

        public ForeObjectMask(RoomCamera roomCamera)
        {
            container = new FContainer();
            var firstContainer = roomCamera.ReturnFContainer("Shadows");
            Futile.stage.AddChild(container);
            container.MoveInFrontOfOtherNode(firstContainer);

            SCHelperUtils.Log("ForeObjectMask Init");
        }

        public void DrawUpdate(RoomCamera roomCamera, float timeStacker)
        {
            Show = !Input.GetKey(KeyCode.RightShift);
            for (int i = copiedSpriteLeasers.Count - 1; i >= 0; i--)
            {
                copiedSpriteLeasers[i].Update(timeStacker, roomCamera);
                if (copiedSpriteLeasers[i].deleteMeNextFrame)
                {
                    copiedSpriteLeasers.RemoveAt(i);
                }
            }
        }

        public void ClearAllSprites()
        {
            container.RemoveAllChildren();
            container.RemoveFromContainer();
            copiedSpriteLeasers.Clear();
        }

        public void CopySLeaser(RoomCamera.SpriteLeaser spriteLeaser)
        {
            copiedSpriteLeasers.Add(new CopiedSpriteLeaser(this,spriteLeaser));
        }

        class CopiedSpriteLeaser
        {
            WeakReference<RoomCamera.SpriteLeaser> bindSpriteLeaser;
            FSprite[] sprites;
            ForeObjectMask mask;

            public bool deleteMeNextFrame;

            public CopiedSpriteLeaser(ForeObjectMask mask, RoomCamera.SpriteLeaser spriteLeaser)
            {
                this.mask = mask;
                bindSpriteLeaser = new WeakReference<RoomCamera.SpriteLeaser>(spriteLeaser);
                sprites = new FSprite[spriteLeaser.sprites.Length];

                SCHelperUtils.Log($"CopiedSpriteLeaser copy from : {spriteLeaser.drawableObject}");

                for(int i = 0;i < sprites.Length; i++)
                {
                    if (spriteLeaser.sprites[i] is TriangleMesh t)
                    {
                        sprites[i] = new TriangleMesh(t.element.name, t.triangles, t.customColor, false);
                    }
                    else if (spriteLeaser.sprites[i] is CustomFSprite c)
                    {
                        sprites[i] = new CustomFSprite(c.element.name);
                    }
                    else if (spriteLeaser.sprites[i] is WaterTriangleMesh w)
                    {
                        sprites[i] = new WaterTriangleMesh(w.element.name, w.triangles, w.customColor);
                    }
                    else
                        sprites[i] = new FSprite(spriteLeaser.sprites[i].element.name, spriteLeaser.sprites[i]._facetTypeQuad);

                    mask.container.AddChild(sprites[i]);
                    SyncSprite(i, spriteLeaser);
                }
            }

            void SyncSprite(int i, RoomCamera.SpriteLeaser spriteLeaser)
            {
                Vector2 bias = mask.Show ? Vector2.zero : new Vector2(Input.mousePosition.x - Screen.width / 2f, Input.mousePosition.y - Screen.height / 2f);
                if(spriteLeaser.sprites[i] is TriangleMesh t)
                {
                    var nT = sprites[i] as TriangleMesh;
                    for (int ii = 0;ii < t.vertices.Length; ii++)
                    {
                        nT.MoveVertice(ii, t.vertices[ii] + bias);
                        nT.UVvertices[ii] = t.UVvertices[ii];
                        nT.customColor = t.customColor;
                        if (t.customColor)
                        {
                            nT.verticeColors[ii] = t.verticeColors[ii];
                            //nT.verticeColors[ii] = Color.white;
                        }
                    }
                    if(!t.customColor)
                    {
                        nT.color = t.color;
                        //nT.color = Color.white;
                    }
                    nT.alpha = t.alpha;
                }
                else if (spriteLeaser.sprites[i] is WaterTriangleMesh w)
                {
                    var nW = sprites[i] as WaterTriangleMesh;
                    for(int ii = 0;ii < w.vertices.Length; ii++)
                    {
                        nW.MoveVertice(ii, w.vertices[ii] + bias);
                        if (w.customColor)
                        {
                            nW.verticeColors[ii] = w.verticeColors[ii];
                        }
                    }
                    if (!w.customColor)
                    {
                        nW.color = w.color;
                    }
                    nW.alpha = w.alpha;
                }
                else if (spriteLeaser.sprites[i] is CustomFSprite c)
                {
                    var nC = sprites[i] as CustomFSprite;
                    for(int ii = 0;ii < 4; ii++)
                    {
                        nC.MoveVertice(ii, c.vertices[ii] + bias);
                        nC.verticeColors[ii] = c.verticeColors[ii];
                        //nC.verticeColors[ii] = Color.white;
                    }
                    nC.alpha = c.alpha;
                }
                else
                {
                    sprites[i].SetPosition(spriteLeaser.sprites[i].GetPosition() + bias);
                    sprites[i].scaleX = spriteLeaser.sprites[i].scaleX;
                    sprites[i].scaleY = spriteLeaser.sprites[i].scaleY;
                    sprites[i].anchorX = spriteLeaser.sprites[i].anchorX;
                    sprites[i].anchorY = spriteLeaser.sprites[i].anchorY;
                    sprites[i].rotation = spriteLeaser.sprites[i].rotation;
                    sprites[i].color = spriteLeaser.sprites[i].color;
                    //sprites[i].color = Color.white;
                    sprites[i].alpha = spriteLeaser.sprites[i].alpha;
                }

                if (sprites[i].element.name != spriteLeaser.sprites[i].element.name)
                {
                    sprites[i].element = spriteLeaser.sprites[i].element;
                }
                if (sprites[i].shader != spriteLeaser.sprites[i].shader)
                {
                    sprites[i].shader = spriteLeaser.sprites[i].shader;
                }
                sprites[i].isVisible = spriteLeaser.sprites[i].isVisible /*&& mask.Show*/;
            }

            public void Update(float timeStacker, RoomCamera rCam)
            {
                if(!bindSpriteLeaser.TryGetTarget(out var sleaser) || sleaser.deleteMeNextFrame)
                {
                    CleanSpritesAndRemove();
                    return;
                }

                if (deleteMeNextFrame)
                    return;

                for(int i = 0;i < sprites.Length; i++)
                {
                    SyncSprite(i, sleaser);
                }
            }

            public void RemoveAllSpritesFromContainer()
            {
                for (int i = 0; i < this.sprites.Length; i++)
                {
                    this.sprites[i].RemoveFromContainer();
                }
            }
            public void CleanSpritesAndRemove()
            {
                deleteMeNextFrame = true;
                RemoveAllSpritesFromContainer();
            }
        }
    }
}
