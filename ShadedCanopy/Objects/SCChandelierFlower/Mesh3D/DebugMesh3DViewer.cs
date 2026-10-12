using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Objects.SCChandelierFlower.Mesh3D
{
    using Triangle2D = Triangle<Vector2>;
    using Triangle3D = Triangle<Vector3>;
    using Triangle2DVector = List<Triangle<Vector2>>;
    using Triangle3DVector = List<Triangle<Vector3>>;
    internal class DebugMesh3DViewer: CosmeticSprite
    {
        TriangleMesh3D mesh;
        Quaternion rot = Quaternion.identity;
        public float Scale = 1;
        float vanishingZ;
        bool IsInFocus;
        FSprite hintSprite;
        TriangleMesh saveMesh;
        Triangle3DVector renderTriVec;

        public DebugMesh3DViewer(Room room, TriangleMesh3D mesh, Vector2 pos, float vanishingZ = float.NegativeInfinity)
        {
            this.room = room;
            this.mesh = mesh;
            this.pos = pos;
            this.vanishingZ = vanishingZ;
        }
        public override void Update(bool eu)
        {
            base.Update(eu);
            if (IsInFocus)
            {
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    rot = Quaternion.Euler(0, 2, 0) * rot;
                }
                if (Input.GetKey(KeyCode.LeftArrow))
                {
                    rot = Quaternion.Euler(0, -2, 0) * rot;
                }
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    rot = Quaternion.Euler(-2, 0, 0) * rot;
                }
                if (Input.GetKey(KeyCode.DownArrow))
                {
                    rot = Quaternion.Euler(2, 0, 0) * rot;
                }
            }
            TriangleMesh3D copy = this.mesh.Clone();
            copy.Rotate(rot);
            Triangle3DVector triVec = copy.Export();
            for (int i = 0; i < triVec.Count; i++)
            {
                Triangle3D t = triVec[i];
                t.A *= Scale;
                t.B *= Scale;
                t.C *= Scale;
                triVec[i] = t;
            }
            renderTriVec = triVec;
        }
        public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            sLeaser.containers = new FContainer[1];
            sLeaser.containers[0] = new FContainer();
            hintSprite = new FSprite("pixel", true);
            sLeaser.containers[0].AddChild(hintSprite);
            AddToContainer(sLeaser, rCam);
        }
        public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            base.ApplyPalette(sLeaser, rCam, palette);
            this.hintSprite.color = Color.white;
            this.hintSprite.alpha = 0.3f;
        }
        public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner=null)
        {
            if (newContatiner == null)
            {
                newContatiner = rCam.ReturnFContainer("HUD");
            }
            newContatiner.AddChild(sLeaser.containers[0]);
        }
        public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            if (renderTriVec == null)
            {
                return;
            }
            Vector2 offsetPos = this.pos - camPos;
            Triangle3DVector offseted = this.renderTriVec.PositionOffset(offsetPos);
            Triangle2DVector projected = offseted.Projection2D(new Vector3(1366 / 2, 768 / 2, vanishingZ));
            projected.Sort();
            if (saveMesh != null)
            {
                saveMesh.RemoveFromContainer();
            }
            saveMesh = projected.Export();
            sLeaser.containers[0].AddChild(saveMesh);
            this.hintSprite.scale = Scale * 2;
            this.hintSprite.SetPosition(offsetPos);
            Vector2 mousePos = Futile.mousePosition;
            Vector2 mouseDist = mousePos - offsetPos;
            if (Mathf.Abs(mouseDist.x) < Scale && Mathf.Abs(mouseDist.y) < Scale)
            {
                IsInFocus = true;
                this.hintSprite.isVisible = true;
            }
            else
            {
                IsInFocus = false;
                this.hintSprite.isVisible = false;
            }
        }
    }
}
