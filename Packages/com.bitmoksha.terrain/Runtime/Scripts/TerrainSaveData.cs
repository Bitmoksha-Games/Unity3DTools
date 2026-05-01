using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{

    [Serializable]
    public class TerrainSaveData
    {
        public string terrainId;
        public TerrainMeshData terrainMesh;


        public TerrainSaveData()
        {
            if (string.IsNullOrEmpty(terrainId)) terrainId = Guid.NewGuid().ToString();
        }

        public string GetSaveString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [Serializable]
    public class TerrainMeshBaseData
    {
    }

    [Serializable]
    public class TerrainMeshData: TerrainMeshBaseData
    {
        public Vector2 worldDimensions;
        public Vector2Int meshDimensions;
        public int randomSeed;
        public Vector2Int heightRange;
        public string splatPath;
    }

}