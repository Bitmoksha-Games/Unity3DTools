using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    [Serializable]
    public struct LightSaveData
    {
        public Vector3 position;
        public Quaternion rotation;
        public Color color;
        public float intensity;
        public LightType type;
    }
}