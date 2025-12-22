using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SCUtils
{
    public class CustomUVFSprite : FSprite
    {
        public Vector2[] customUVs;
        public Vector2[] vertices;

        public CustomUVFSprite(string elementName, bool quadType = true) : base(elementName, quadType)
        {
            customUVs = new Vector2[4];
            vertices = new Vector2[4];
        }
        public CustomUVFSprite(FAtlasElement element, bool quadType = true) : base(element, quadType)
        {
            customUVs = new Vector2[4];
            vertices = new Vector2[4];
        }

        public override void PopulateRenderLayer()
        {
            if (_isOnStage && _firstFacetIndex != -1)
            {
                _isMeshDirty = false;
                Vector3[] vertices = _renderLayer.vertices;
                Vector2[] uvs = _renderLayer.uvs;
                Color[] colors = _renderLayer.colors;
                if (_facetTypeQuad)
                {
                    int num = _firstFacetIndex * 4;
                    int num2 = num + 1;
                    int num3 = num + 2;
                    int num4 = num + 3;
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num], this.vertices[0], _meshZ);
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num2], this.vertices[1], _meshZ);
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num3], this.vertices[2], _meshZ);
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num4], this.vertices[3], _meshZ);
                    uvs[num] = customUVs[0];
                    uvs[num2] = customUVs[1];
                    uvs[num3] = customUVs[2];
                    uvs[num4] = customUVs[3];
                    colors[num] = _alphaColor;
                    colors[num2] = _alphaColor;
                    colors[num3] = _alphaColor;
                    colors[num4] = _alphaColor;
                }
                else
                {
                    int num5 = _firstFacetIndex * 3;
                    int num6 = num5 + 1;
                    int num7 = num5 + 2;
                    int num8 = num5 + 3;
                    int num9 = num5 + 4;
                    int num10 = num5 + 5;
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num5], this.vertices[0], _meshZ);
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num6], this.vertices[1], _meshZ);
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num7], this.vertices[2], _meshZ);
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num8], this.vertices[0], _meshZ);
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num9], this.vertices[2], _meshZ);
                    _concatenatedMatrix.ApplyVector3FromLocalVector2(ref vertices[num10], this.vertices[3], _meshZ);
                    uvs[num5] = customUVs[0];
                    uvs[num6] = customUVs[1];
                    uvs[num7] = customUVs[2];
                    uvs[num8] = customUVs[0];
                    uvs[num9] = customUVs[2];
                    uvs[num10] = customUVs[3];
                    colors[num5] = _alphaColor;
                    colors[num6] = _alphaColor;
                    colors[num7] = _alphaColor;
                    colors[num8] = _alphaColor;
                    colors[num9] = _alphaColor;
                    colors[num10] = _alphaColor;
                }

                _renderLayer.HandleVertsChange();
            }
        }
    
        public void SetCustomUVs(Vector2 uv0, Vector2 uv1, Vector2 uv2, Vector2 uv3)
        {
            customUVs[0] = uv0;
            customUVs[1] = uv1;
            customUVs[2] = uv2;
            customUVs[3] = uv3;
            _isMeshDirty = true;
        }

        public void MoveVertice(int i, Vector2 pos)
        {
            vertices[i] = pos;
            _isMeshDirty = true;
        }

        public void MoveVertices(Vector2 pos0, Vector2 pos1, Vector2 pos2, Vector2 pos3)
        {
            vertices[0] = pos0;
            vertices[1] = pos1;
            vertices[2] = pos2;
            vertices[3] = pos3;
            _isMeshDirty = true;
        }
    }
}
