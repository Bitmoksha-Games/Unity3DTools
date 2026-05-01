using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    [Serializable]
    public class SplatCategoryData
    {
        public string _name;
        public Color _color;
        public float _sampleDistance = 0.15f;
        public TerrainMeshInstanced _instancedObject;
    }

}