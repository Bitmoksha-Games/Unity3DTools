using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using static com.bitmoksha.terrain.CubeMeshInstanced;

namespace com.bitmoksha.terrain
{
    public class TerrainMeshInstanced : TerrainMeshBase
    {

        private const string PARAMNAME_OBJECT_TO_WORLD = "_ObjectToWorld";
        private const string PARAMNAME_NUM_INSTANCES = "_NumInstances";
        private const string PARAMNAME_PER_INSTANCE_DATA = "_PerInstanceData";
        private const string SHDR_NAME = "BitMoksha/Unlit/ShdrInstancedMeshGen";

        [Serializable]
        public class InstanceConfig
        {
            public Vector3 positionRandomMax = Vector3.zero;
            public Vector3 positionRandomMin = Vector3.zero;
            public Quaternion rotationRandomMax = Quaternion.identity;
            public Quaternion rotationRandomMin = Quaternion.identity;
            public Vector3 scaleRandomMax = Vector3.one;
            public Vector3 scaleRandomMin = Vector3.one;
        }

        [SerializeField]
        protected int _instanceCount = 1;
        [SerializeField]
        protected InstanceConfig _instanceConfig;

        [StructLayout(LayoutKind.Sequential)]
        protected struct InstanceData
        {
            public Matrix4x4 transformMatrix;

            public static int Size()
            {
                return (sizeof(float) * 4 * 4);
            }
        }

        protected ComputeBuffer mInstanceBuffer;
        protected RenderParams mRenderParams;
        protected int mInstanceCount;

        protected virtual void SetupInstanceData()
        {
            if (mMaterial == null)
                return;
            if (_instanceConfig == null) return;
            if (_instanceCount < 1)
                _instanceCount = 1;
            mRenderParams = new RenderParams(mMaterial);
            mRenderParams.matProps = new MaterialPropertyBlock();
            mRenderParams.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
            mRenderParams.matProps.SetMatrix(PARAMNAME_OBJECT_TO_WORLD, transform.localToWorldMatrix);
            mRenderParams.matProps.SetFloat(PARAMNAME_NUM_INSTANCES, _instanceCount);

            InstanceData[] instanceData = new InstanceData[_instanceCount];
            for (int i = 0; i < _instanceCount; i++) 
            {
                Vector3 position = transform.position + new Vector3(i, 0, 0) 
                    + Vector3.Lerp(_instanceConfig.positionRandomMin, 
                    _instanceConfig.positionRandomMax, UnityEngine.Random.Range(0f, 1f));
                Quaternion rotation = Quaternion.Lerp(_instanceConfig.rotationRandomMin, 
                    _instanceConfig.rotationRandomMax, UnityEngine.Random.Range(0f, 1f));
                Vector3 scale = Vector3.Lerp(_instanceConfig.scaleRandomMin,
                    _instanceConfig.scaleRandomMax, UnityEngine.Random.Range(0f, 1f));
                instanceData[i] = new InstanceData
                {
                    transformMatrix = Matrix4x4.TRS(position, rotation, scale)
                };
            }
            mInstanceBuffer = new ComputeBuffer(_instanceCount, InstanceData.Size());
            mInstanceBuffer.SetData(instanceData);
            mRenderParams.matProps.SetBuffer(PARAMNAME_PER_INSTANCE_DATA, mInstanceBuffer);
            mInstanceCount = _instanceCount;
        }

        public virtual void SetSampledPositions(List<Vector3> positions)
        {
            if (mMaterial == null)
            {
                Debug.Log("Null?");
                return;
            }
            mInstanceCount = positions.Count;
            if (mInstanceCount < 1)
                return;
            mRenderParams = new RenderParams(mMaterial);
            mRenderParams.matProps = new MaterialPropertyBlock();
            mRenderParams.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
            mRenderParams.matProps.SetMatrix(PARAMNAME_OBJECT_TO_WORLD, transform.localToWorldMatrix);
            mRenderParams.matProps.SetFloat(PARAMNAME_NUM_INSTANCES, mInstanceCount);

            InstanceData[] instanceData = new InstanceData[mInstanceCount];
            for (int i = 0; i < mInstanceCount; i++)
            {
                Vector3 position = positions[i]
                    + Vector3.Lerp(_instanceConfig.positionRandomMin,
                    _instanceConfig.positionRandomMax, UnityEngine.Random.Range(0f, 1f));
                //Debug.Log(position);
                Quaternion rotation = Quaternion.Lerp(_instanceConfig.rotationRandomMin,
                    _instanceConfig.rotationRandomMax, UnityEngine.Random.Range(0f, 1f));
                Vector3 scale = Vector3.Lerp(_instanceConfig.scaleRandomMin,
                    _instanceConfig.scaleRandomMax, UnityEngine.Random.Range(0f, 1f));
                instanceData[i] = new InstanceData
                {
                    transformMatrix = Matrix4x4.TRS(position, rotation, scale)
                };
            }
            mInstanceBuffer = new ComputeBuffer(mInstanceCount, InstanceData.Size());
            mInstanceBuffer.SetData(instanceData);
            mRenderParams.matProps.SetBuffer(PARAMNAME_PER_INSTANCE_DATA, mInstanceBuffer);
            Debug.Log(gameObject.name + " instances to render: " + mInstanceCount);
        }

        public override void BeginInitialization(out bool useCustomRendering)
        {
            useCustomRendering = true;
        }

        public override Material BuildMaterial()
        {
            //Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            Material mat = new Material(Shader.Find(SHDR_NAME));
            return mat;
        }

        public override Mesh BuildMesh()
        {
            Mesh mesh = new Mesh()
            {
                name = "InstancedMesh"
            };

            mesh.vertices = new Vector3[3] {
                new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(0f, 0.5f, 0f)
            };
            int[] indices = new int[3] {0, 2, 1 };
            mesh.triangles = indices;
            mesh.uv = new Vector2[3]
            {
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 1f)
            };
            Vector3[] normals = new Vector3[3];
            for(int i = 0; i < mesh.vertices.Length; i++)
            {
                normals[i] = Vector3.forward;
            }
            mesh.normals = normals;

            return mesh;
        }

        public override void FinishInitialization()
        {
            SetupInstanceData();
        }

        protected override void DoCustomRender()
        {
            if (mRenderParams.matProps == null) return;
            //RenderParams renderParams = new RenderParams(mMaterial);
            ////mRenderParams.worldBounds = gameObject.GetComponent<BoxCollider>().bounds;
            //renderParams.matProps = new MaterialPropertyBlock();
            //renderParams.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
            //renderParams.matProps.SetMatrix("_ObjectToWorld", transform.localToWorldMatrix);
            //renderParams.matProps.SetFloat("_NumInstances", 10.0f);
            //mRenderParams.material = mMaterial;
            //mRenderParams.matProps.SetFloat("_NumInstances", _instanceCount);
            //Debug.Log("Rendering " + mInstanceCount + " " + gameObject.name + " instances.");
            mRenderParams.matProps.SetMatrix(PARAMNAME_OBJECT_TO_WORLD, transform.localToWorldMatrix);
            Graphics.RenderMeshPrimitives(mRenderParams, mMesh, 0, mInstanceCount);
        }

        private void OnEnable()
        {
            SetupInstanceData();
        }

        private void OnDisable()
        {
            
        }
    }
}
