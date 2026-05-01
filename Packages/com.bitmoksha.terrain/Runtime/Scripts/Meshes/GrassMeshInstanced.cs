using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    public class GrassMeshInstanced : TerrainMeshInstanced
    {
        [SerializeField]
        [Range(1, 4)]
        private int _segments = 1;
        [SerializeField]
        private Vector2 _size = Vector2.one; // new Vector2(0.01f, 0.1f);
        [SerializeField]
        private Texture2D _testWindTexture;
        [SerializeField]
        [Range(0.0f, 2.0f)]
        private float _curvature = 1.0f;

        public override Mesh BuildMesh()
        {
            int nVertices = _segments * 2 + 1;
            int nTriangles = _segments * 2 - 1;
            Mesh mesh = new Mesh()
            {
                name = "Grass"
            };
            Vector3[] positions = new Vector3[nVertices];
            Vector3[] normals = new Vector3[nVertices];
            Vector2[] uvs = new Vector2[nVertices];
            Vector4[] tangents = new Vector4[nVertices];
            for (int i = 0; i < _segments; i++)
            {
                float dy = (float)i / (float)(_segments);
                float y = _size.y * dy;
                float x = _size.x * 0.5f * Mathf.Pow((1.0f - dy), _curvature);
                positions[i * 2] = new Vector3(-x, y, 0);
                normals[i * 2] = Vector3.back;
                uvs[i * 2] = new Vector2(-x / _size.x + 0.5f, y / _size.y);
                tangents[i * 2] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
                positions[i * 2 + 1] = new Vector3(x, y, 0);
                normals[i * 2 + 1] = Vector3.back;
                uvs[i * 2 + 1] = new Vector2(x / _size.x + 0.5f, y / _size.y);
                tangents[i * 2 + 1] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
            }
            positions[positions.Length - 1] = new Vector3(0, _size.y, 0);
            normals[positions.Length - 1] = Vector3.back;
            uvs[positions.Length - 1] = new Vector2(0.5f, 1f);
            int[] triangles = new int[nTriangles * 3];
            int idx = 0;
            for (int i = 0; i < _segments; i++)
            {
                if (i == _segments - 1)
                {
                    triangles[idx++] = i * 2;
                    triangles[idx++] = (i + 1) * 2;
                    triangles[idx++] = i * 2 + 1;
                }
                else
                {
                    triangles[idx++] = i * 2;
                    triangles[idx++] = (i + 1) * 2;
                    triangles[idx++] = i * 2 + 1;
                    triangles[idx++] = i * 2 + 1;
                    triangles[idx++] = (i + 1) * 2;
                    triangles[idx++] = (i + 1) * 2 + 1;
                }
            }

            mesh.vertices = positions;
            mesh.uv = uvs;
            mesh.normals = normals;
            mesh.triangles = triangles;
            mesh.tangents = tangents;

            //Debug.Log("Returning Mesh: Vertices=" + string.Join(",", positions) 
            //    + "\nTriangles=" + string.Join(",", triangles)
            //     + "\nuvs=" + string.Join(",", uvs));

            return mesh;
        }
    }
}
