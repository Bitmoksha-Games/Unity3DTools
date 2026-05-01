using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    [Serializable]
    public enum MeshType
    {
        None, 
        Quad, 
        Cube, 
        Grass,
        Isocahedron,
        Terrain, 

        InstancedMeshes = 128,
        InstancedCube,
        InstancedGrass,
    }

    public static class ProceduralMeshFactory 
    {
        public static TerrainMeshBase GetMeshGenerator(MeshType type) 
        {
            switch (type)
            {
                case MeshType.Quad:
                    break;
                case MeshType.Cube:
                    break;
                case MeshType.Grass:
                    break;
                case MeshType.Terrain:
                    break;
                case MeshType.InstancedCube:

                    break;
                default:
                    break;
            }
            return null;
        }

        public static TerrainMeshBase AddMeshGenerator(MeshType type, GameObject go)
        {
            switch (type)
            {
                case MeshType.Quad:
                    break;
                case MeshType.Cube:
                    break;
                case MeshType.Terrain:
                    return go.AddComponent<TerrainMesh>();
                case MeshType.InstancedMeshes:
                    return go.AddComponent<TerrainMeshInstanced>();
                case MeshType.InstancedCube:
                    return go.AddComponent<CubeMeshInstanced>();
                case MeshType.InstancedGrass:
                    return go.AddComponent<GrassMeshInstanced>();
                default:
                    break;
            }
            return null;
        }
    }
}