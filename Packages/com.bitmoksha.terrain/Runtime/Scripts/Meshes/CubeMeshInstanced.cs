using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    public class CubeMeshInstanced : TerrainMeshInstanced
    {
        [Serializable]
        public enum UVWrapMode
        {
            PerSide, 
            CubeMap
        }

        [Header("Cube settings")]
        [SerializeField]
        private UVWrapMode _uvWrapMode;

        public override Mesh BuildMesh()
        {
            Mesh mesh = new Mesh()
            {
                name = "Cube"
            };

            float vPositionOffset = 0.5f;
            Vector3 p = new Vector3(vPositionOffset, vPositionOffset, vPositionOffset);
            Vector3 p0 = new Vector3(-1, -1, -1); Vector3 p1 = new Vector3(1, -1, -1);
            Vector3 p2 = new Vector3(-1, 1, -1); Vector3 p3 = new Vector3(1, 1, -1);
            Vector3 p4 = new Vector3(1, -1, 1); Vector3 p5 = new Vector3(-1, -1, 1);
            Vector3 p6 = new Vector3(1, 1, 1); Vector3 p7 = new Vector3(-1, 1, 1);
            mesh.vertices = new Vector3[4 * 6] {
                Vector3.Scale(p, p0), Vector3.Scale(p, p1), Vector3.Scale(p, p2), Vector3.Scale(p, p3),     // Front
                Vector3.Scale(p, p4), Vector3.Scale(p, p5), Vector3.Scale(p, p6), Vector3.Scale(p, p7),     // Back
                Vector3.Scale(p, p5), Vector3.Scale(p, p0), Vector3.Scale(p, p7), Vector3.Scale(p, p2),     // Left
                Vector3.Scale(p, p1), Vector3.Scale(p, p4), Vector3.Scale(p, p3), Vector3.Scale(p, p6),     // Right
                Vector3.Scale(p, p2), Vector3.Scale(p, p3), Vector3.Scale(p, p7), Vector3.Scale(p, p6),     // Top
                Vector3.Scale(p, p5), Vector3.Scale(p, p4), Vector3.Scale(p, p0), Vector3.Scale(p, p1),     // Bottom
            };
            int[] indices = new int[12 * 3];
            for(int i = 0; i < 6; i++)
            {
                int voffset = i << 2;
                int ioffset = i * 6;
                indices[ioffset++] = voffset; indices[ioffset++] = voffset + 2; indices[ioffset++] = voffset + 1;
                indices[ioffset++] = voffset + 1; indices[ioffset++] = voffset + 2; indices[ioffset++] = voffset + 3;
            }
            mesh.triangles = indices;
            switch(_uvWrapMode)
            {
                case UVWrapMode.PerSide:
                    mesh.uv = new Vector2[4 * 6]
                    {
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1),     // Front
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1),     // Back
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1),     // Left
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1),     // Right
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1),     // Top
                        new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1),     // Bottom
                    };
                    break;
                case UVWrapMode.CubeMap:
                    mesh.uv = new Vector2[4 * 6]
                    {
                        new Vector2(1f/4f, 2f/3f), new Vector2(2f/4f, 2f/3f), new Vector2(1f/4f, 1f/3f), new Vector2(2f/4f, 1f/3f),     // Front
                        new Vector2(3f/4f, 2f/3f), new Vector2(1f, 2f/3f), new Vector2(3f/4f, 1f/3f), new Vector2(1f, 1f/3f),           // Back
                        new Vector2(0f, 2f/3f), new Vector2(1f/4f, 2f/3f), new Vector2(0f, 1f/3f), new Vector2(1f/4f, 1f/3f),           // Left
                        new Vector2(2f/4f, 2f/3f), new Vector2(3f/4f, 2f/3f), new Vector2(2f/4f, 1f/3f), new Vector2(3f/4f, 1f/3f),     // Right
                        new Vector2(1f/4f, 1f/3f), new Vector2(2f/4f, 1f/3f), new Vector2(1f/4f, 0f), new Vector2(2f/4f, 0f),           // Top
                        new Vector2(1f/4f, 1f), new Vector2(2f/4f, 1f), new Vector2(1f/4f, 2f/3f), new Vector2(2f/4f, 2f/3f),           // Bottom
                    };
                    break;
            }
            Vector3[] normals = new Vector3[4 * 6];
            for(int i = 0; i < mesh.vertices.Length; i++)
            {
                normals[i] = mesh.vertices[i].normalized;
            }
            mesh.normals = normals;

            return mesh;
        }
    }
}
