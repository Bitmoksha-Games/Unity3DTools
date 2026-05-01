using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    /// <summary>
    /// Base class for all procedural mesh generators.
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public abstract class TerrainMeshBase : MonoBehaviour
    {
        [SerializeField]
        private bool _generateOnStart = false;
        [SerializeField]
        private Material _materialOverride = null;

        protected Mesh mMesh;
        protected Material mMaterial;
        protected TerrainMeshBaseData mMeshConfig;
        protected bool mCustomRendering = false;

        public Bounds terrainBounds => GetComponent<MeshRenderer>().bounds;
        public TerrainMeshBaseData meshConfig => mMeshConfig;

        public bool pIsInstanced => mCustomRendering;


        public abstract void BeginInitialization(out bool useCustomRendering);
        public abstract Mesh BuildMesh();
        public abstract Material BuildMaterial();
        public abstract void FinishInitialization();
        protected abstract void DoCustomRender();

        public virtual void Initialize(TerrainMeshData fromSaveData = null)
        {
            mMeshConfig = fromSaveData;
            BeginInitialization(out mCustomRendering);
            mMesh = BuildMesh();
            if (_materialOverride != null)
                mMaterial = _materialOverride;
            else
                mMaterial = BuildMaterial();
            if (!mCustomRendering)
            {
                GetComponent<MeshRenderer>().material = mMaterial;
                GetComponent<MeshFilter>().mesh = mMesh;
                gameObject.AddComponent<BoxCollider>();
            }
            FinishInitialization();
        }

        public string PopulateSaveData(TerrainMeshData meshConfig)
        {
            return JsonUtility.ToJson(mMeshConfig);
        }

        #region _Unity_Methods_
        void Start()
        {
            if (_generateOnStart)
            {
                Initialize();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (mCustomRendering)
                DoCustomRender();
        }
        #endregion
    }
}