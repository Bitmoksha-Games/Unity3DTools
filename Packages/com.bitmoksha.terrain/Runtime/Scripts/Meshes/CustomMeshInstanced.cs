using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    public class CustomMeshInstanced : TerrainMeshInstanced
    {
        [SerializeField]
        private Mesh _mesh;

        public override Mesh BuildMesh()
        {
            Mesh mesh = new Mesh()
            {
                name = _mesh.name,
            };

            mesh.vertices = _mesh.vertices;
            mesh.normals = _mesh.normals;
            mesh.uv = _mesh.uv;
            mesh.triangles = _mesh.triangles;
            mesh.tangents = _mesh.tangents;
            mesh.colors = _mesh.colors;
            mesh.bounds = _mesh.bounds;

            return mesh;
        }
    }
}
