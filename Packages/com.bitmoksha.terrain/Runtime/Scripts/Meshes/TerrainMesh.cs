using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;

namespace com.bitmoksha.terrain
{
    public class TerrainMesh : TerrainMeshBase
    {
        private const string SHDR_NAME = "Universal Render Pipeline/Lit";

        [Serializable]
        public enum TextureSizes
        {
            _2x2 = 2,
            _4x4 = 4,
            _8x8 = 8,
            _16x16 = 16,
            _32x32 = 32,
            _64x64 = 64,
            _128x128 = 128,
            _256x256 = 256,
            _512x512 = 512,
            _1024x1024 = 1024,
            _2048x2048 = 2048,
        }


        [SerializeField]
        [Tooltip("Size of the terrain.")]
        protected Vector2 _worldDimensions = Vector2.one;
        [SerializeField]
        [Tooltip("Number of horizontal and vertical vertices")]
        protected Vector2Int _meshDimensions = new Vector2Int(75, 75);
        [SerializeField]
        protected Vector2Int _heightRange = new Vector2Int(0, 5);
        [SerializeField]
        protected TextureSizes _textureSize = TextureSizes._64x64;


        protected Texture2D mSplatMap;
        protected Color[] mSplatColors = null;
        protected int mActiveSplatCategory;
        protected int mRandomSeed;
        protected SplatCategoryData[] mSplatCategories = null;
        protected Vector3[] mPositionsCache;
        protected int[] mIndicesCache;


        public Texture2D splatMap => mSplatMap;

        public void SetSplatCategories(SplatCategoryData[] splatCategories, int activeCategory)
        {
            mSplatCategories = splatCategories;
            mActiveSplatCategory = activeCategory;
        }

        public void PaintAtPosition(Vector3 position)
        {
            if(mSplatCategories == null)
            {
                Debug.LogError("Splat categories not set");
                return;
            }
            Vector2 uv = GetUvForPosition(position);
            int x = Mathf.Clamp((int)(uv.x * mSplatMap.width), 0, mSplatMap.width - 1);
            int y = Mathf.Clamp((int)(uv.y * mSplatMap.height), 0, mSplatMap.height - 1);
            mSplatColors[y * mSplatMap.width + x] =
                mSplatCategories[mActiveSplatCategory]._color;
            mSplatMap.SetPixels(mSplatColors);
            mSplatMap.Apply();
        }

        /// <summary>
        /// Returns the triangle index for the the provided position
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public int GetTriangleForPosition(Vector3 position)
        {
            TerrainMeshData terrainData = (TerrainMeshData)mMeshConfig;
            float meshDx = terrainData.worldDimensions.x / ((float)terrainData.meshDimensions.x - 1);
            float meshDy = terrainData.worldDimensions.y / ((float)terrainData.meshDimensions.y - 1);
            float xCellF = ((position.x + terrainData.worldDimensions.x / 2f) / meshDx);
            float yCellF = ((position.z + terrainData.worldDimensions.y / 2f) / meshDy);
            int xCell = (int)xCellF;
            int yCell = (int)yCellF;
            int cell = yCell * (terrainData.meshDimensions.x - 1) + xCell;

            // Check 2D distance from opposing vertices in the triangles.
            // This works for now because we do not manipulate the vertices in the xz-plane.
            // Will stop working if we start manipulating the vertex in the xz-plane.
            Vector3 p1 = mPositionsCache[mIndicesCache[cell * 6]];
            Vector3 p2 = mPositionsCache[mIndicesCache[cell * 6 + 5]];
            Vector2 p1xz = new Vector2(p1.x, p1.z);
            Vector2 p2xz = new Vector2(p2.x, p2.z);
            Vector2 positionXz = new Vector2(position.x, position.z);

            // 2 triangles per quad (cell)
            if (Vector2.SqrMagnitude(p1xz - positionXz) > Vector2.SqrMagnitude(p2xz - positionXz))
                return cell * 2 + 1;
            return cell * 2;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetHeightAtPosition(Vector3 position)
        {
            int tri = GetTriangleForPosition(position);
            Vector3 p1 = mPositionsCache[mIndicesCache[tri * 3]];
            Vector3 p2 = mPositionsCache[mIndicesCache[tri * 3 + 1]];
            Vector3 p3 = mPositionsCache[mIndicesCache[tri * 3 + 2]];
            float d = (p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z);
            float dInv = 1f / d;
            float a = (p2.z - p3.z) * (position.x - p3.x) + (p3.x - p2.x) * (position.z - p3.z);
            a *= dInv;
            float b = (p3.z - p1.z) * (position.x - p3.x) + (p1.x - p3.x) * (position.z - p3.z);
            b *= dInv;
            float c = 1f - a - b;
            return (a * p1.y + b * p2.y + c * p3.y);
        }

        public Vector2 GetUvForPosition(Vector3 position)
        {
            TerrainMeshData terrainData = (TerrainMeshData)mMeshConfig;
            return new Vector2(
                (position.x + terrainData.worldDimensions.x / 2) / terrainData.worldDimensions.x,
                (position.z + terrainData.worldDimensions.y / 2) / terrainData.worldDimensions.y);
        }

        public SplatCategoryData GetCategoryAtUv(Vector2 uv)
        {
            if (mSplatCategories == null)
            {
                return null;
            }
            int x = (int)(uv.x * mSplatMap.width);
            int y = (int)(uv.y * mSplatMap.height);
            Color c = mSplatColors[y * mSplatMap.width + x];
            for (int i = 0; i < mSplatCategories.Length; i++)
            {
                if (mSplatCategories[i]._color == c)
                    return mSplatCategories[i];
            }
            return null;
        }

        public SplatCategoryData GetCategoryAtPosition(Vector3 position)
        {
            if (mSplatCategories == null)
            {
                return null;
            }
            Vector2 uv = GetUvForPosition(position);
            int x = Mathf.Clamp((int)(uv.x * mSplatMap.width), 0, mSplatMap.width - 1);
            int y = Mathf.Clamp((int)(uv.y * mSplatMap.height), 0, mSplatMap.height - 1);
            Color c = mSplatColors[y * mSplatMap.width + x];
            for (int i = 0; i < mSplatCategories.Length; i++)
            {
                if (ColorUtilites.CheckEquality(mSplatCategories[i]._color, c))
                    return mSplatCategories[i];
            }

            return null;
        }

        public void OnPaintColorSelected(int idx)
        {
            mActiveSplatCategory = idx;
        }

        public void SetHeightmap(Texture2D heightMap, Texture2D heightMapAlbedo)
        {
            Color[] colors = heightMap.GetPixels();
            TerrainMeshData terrainData = (TerrainMeshData)mMeshConfig;
            int rows = terrainData.meshDimensions.x;
            int cols = terrainData.meshDimensions.y;
            int numVertices = rows * cols;
            Vector3[] positions = mMesh.vertices;
            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < cols; x++)
                {
                    int texX = (int)((float)x / (float)cols * heightMap.width);
                    int texY = (int)((float)z / (float)rows * heightMap.height);
                    float y = colors[texY * heightMap.width + texX].r
                        * (terrainData.heightRange.y - terrainData.heightRange.x)
                        + terrainData.heightRange.x;
                    Vector3 pos = positions[z * cols + x];
                    pos.y = y;
                    positions[z * cols + x] = pos;
                }
            }
            mMesh.vertices = positions;
            mPositionsCache = positions;
            GetComponent<MeshRenderer>().material.mainTexture = heightMapAlbedo;
        }

        public override Mesh BuildMesh()
        {
            if (mMeshConfig == null)
            {
                mMeshConfig = new TerrainMeshData()
                {
                    meshDimensions = _meshDimensions, 
                    randomSeed = UnityEngine.Random.Range(0, 1000000), 
                    worldDimensions = _worldDimensions, 
                    heightRange = _heightRange
                };
            }
            TerrainMeshData terrainData = (TerrainMeshData)mMeshConfig;
            Mesh mesh = new Mesh()
            {
                name = "Terrain"
            };
            int rows = terrainData.meshDimensions.x;
            int cols = terrainData.meshDimensions.y;
            int numVertices = rows * cols;
            Vector2 resolution = new Vector2((float)terrainData.worldDimensions.x / (cols - 1), 
                (float)terrainData.worldDimensions.y / (rows - 1));
            float startX = -((rows - 1) * resolution.x) / 2;
            float startZ = -((cols - 1) * resolution.y) / 2;
            int numTriangles = (rows - 1) * (cols - 1) * 2;

            // Vertices
            float xPos = startX;
            float zPos = startZ;
            Vector3[] positions = new Vector3[numVertices];
            Vector3[] normals = new Vector3[numVertices];
            Vector2[] uvs = new Vector2[numVertices];
            int[] indices = new int[numTriangles * 3];
            
            System.Random rnd = new System.Random(terrainData.randomSeed);
            float rndX = rnd.Next() % terrainData.worldDimensions.x;
            float rndY = rnd.Next() % terrainData.worldDimensions.y;
            for (int z = 0; z < rows; z++) 
            {
                xPos = startX;
                for (int x = 0; x < cols; x++)
                {
                    // Positions
                    //float y = Mathf.PerlinNoise((float)x / (float)cols, (float)z / (float)rows) * 5;
                    float y = Mathf.PerlinNoise((float)x / (float)cols + rndX, 
                        (float)z / (float)rows + rndY) 
                        * (terrainData.heightRange.y - terrainData.heightRange.x) 
                        + terrainData.heightRange.x;
                    positions[z * cols + x] = new Vector3(xPos,y * 1, zPos);
                    xPos += resolution.x;
                    // UV
                    uvs[z * cols + x] = new Vector2((float)x / (float)(cols - 1), 
                        (float)z / (float)(rows - 1));
                    // Normals
                    normals[z * cols + x] = Vector3.up;
                }
                zPos += resolution.y;
            }

            mesh.vertices = positions;
            mesh.normals = normals;
            mesh.uv = uvs;
            int indexId = 0;
            for (int z = 0; z < rows - 1; z++)
            {
                xPos = startX;
                for (int x = 0; x < cols - 1; x++)
                {
                    indices[indexId++] = z * cols + x;
                    indices[indexId++] = (z + 1) * cols + x;
                    indices[indexId++] = z * cols + x + 1;
                    indices[indexId++] = z * cols + x + 1;
                    indices[indexId++] = (z + 1) * cols + x;
                    indices[indexId++] = (z + 1) * cols + x + 1;
                }
                zPos += resolution.y;
            }
            //Debug.Log(string.Join(", ", indices));
            //Debug.Log(string.Join(", ", uvs));
            //Debug.Log(string.Join(", ", positions));

            mesh.triangles = indices;
            int numFaces = indices.Length / 3;
            Vector3[] faceNormals = new Vector3[numFaces];
            for(int i = 0; i < numFaces; i++)
            {
                faceNormals[i] = Vector3.Cross(positions[indices[i * 3 + 2]] - positions[indices[i * 3 + 1]],
                    positions[indices[i * 3]] - positions[indices[i * 3 + 1]]).normalized;
            }
            for(int i = 0; i < normals.Length; i++)
            {
                Vector3 normal = Vector3.zero;
                int numSharedFaces = 0;
                for(int j = 0; j < indices.Length; j++)
                {
                    if (indices[j] == i)
                    {
                        numSharedFaces++;
                        normal += faceNormals[j / 3];
                    }
                }
                normals[i] = normal / numSharedFaces;
            }
            mesh.normals = normals;

            mPositionsCache = positions;
            mIndicesCache = indices;
            return mesh;
        }

        public override Material BuildMaterial()
        {
            if (mSplatCategories == null)
            {
                return null;
            }
            return new Material(Shader.Find(SHDR_NAME));
        }

        public override void BeginInitialization(out bool useCustomRendering)
        {
            useCustomRendering = false;
        }

        public override void FinishInitialization()
        {
            mActiveSplatCategory = 0;
            BuildSplatMap();
            DestroyImmediate(gameObject.GetComponent<BoxCollider>());
            RefreshMeshCollider();
        }

        public void RefreshMeshCollider()
        {
            MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
            if(meshCollider == null)
            {
                meshCollider = gameObject.AddComponent<MeshCollider>();
            }
            // Trying to force cooking the collider data. 
            // Seems to be required for IL2CPP builds as raycasting for drawing the splat doesn't
            // seem to work otherwise.
            meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation |
                              MeshColliderCookingOptions.EnableMeshCleaning |
                              MeshColliderCookingOptions.WeldColocatedVertices |
                              MeshColliderCookingOptions.UseFastMidphase;
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mMesh;
        }

        protected override void DoCustomRender()
        {

        }

        private void BuildSplatMap()
        {
            if (mSplatCategories == null)
            {
                return;
            }
            TerrainMeshData terrainData = (TerrainMeshData)mMeshConfig;
            if(!string.IsNullOrEmpty(terrainData.splatPath))
            {
                byte[] splatBytes = File.ReadAllBytes(terrainData.splatPath);
                mSplatMap = new Texture2D(2, 2);
                mSplatMap.LoadImage(splatBytes, false);
                mSplatColors = mSplatMap.GetPixels();
                mSplatMap.Apply();
                GetComponent<MeshRenderer>().material.mainTexture = mSplatMap;
                return;
            }

            mSplatMap = new Texture2D((int)_textureSize, (int)_textureSize, TextureFormat.ARGB32, 0, true);
            //mSplatMap.filterMode = FilterMode.Point;
            mSplatColors = mSplatMap.GetPixels();
            for (int i = 0; i < mSplatColors.Length; i++) 
            {
                mSplatColors[i] = mSplatCategories[0]._color;
            }
            mSplatMap.SetPixels(mSplatColors);
            mSplatMap.Apply();
            GetComponent<MeshRenderer>().material.mainTexture = mSplatMap;
        }
    }
}
