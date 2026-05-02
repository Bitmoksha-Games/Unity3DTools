using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    public class CustomMeshInstanced : TerrainMeshInstanced
    {
        [Header("Single Mesh")]
        [SerializeField]
        private Mesh _mesh;
        [Header("Combine Meshes")]
        [SerializeField]
        private Mesh[] _meshes;

        public override Mesh BuildMesh()
        {
            Mesh mesh = new Mesh();
            if (_meshes.Length <= 0)
            {
                mesh.name = _mesh.name;
                mesh.vertices = _mesh.vertices;
                mesh.normals = _mesh.normals;
                mesh.uv = _mesh.uv;
                mesh.triangles = _mesh.triangles;
                mesh.tangents = _mesh.tangents;
                mesh.colors = _mesh.colors;
                mesh.bounds = _mesh.bounds;
            }
            else
            {
                mesh.name = _meshes[0].name;
                CombineInstance[] ci = new CombineInstance[_meshes.Length];
                for (int i = 0; i < _meshes.Length; i++)
                {
                    ci[i].mesh = _meshes[i];
                    ci[i].transform = transform.localToWorldMatrix;
                }
                mesh.CombineMeshes(ci, true);
            }

            return mesh;
        }
    }
}
