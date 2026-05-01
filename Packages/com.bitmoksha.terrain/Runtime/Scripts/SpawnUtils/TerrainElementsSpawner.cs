using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace com.bitmoksha.terrain
{
    /// <summary>
    /// Contains functions to sample points and spawn object instances on the terrain.
    /// Supports only instanced mesh objects.
    /// ToDo:
    /// - Currently the sampling distance is common for all Splat categories. 
    ///     This needs to be separate for each splat category.
    /// </summary>
    public class TerrainElementsSpawner : MonoBehaviour
    {
        [SerializeField]
        private TerrainMesh _terrain;
        [SerializeField]
        private float _defaultSampleDistance = 0.15f;

        #region _Unity_Methods_
        #endregion

        //public void SpawnElements(SplatCategoryData[] splatCategoryData)
        //{
        //    for (int i = 0; i < splatCategoryData.Length; i++)
        //    {
        //        SplatCategoryData splatData = splatCategoryData[i];
        //        Transform elementsRoot = transform.Find(splatData._name);
        //        GameObject elementsRootGO = (elementsRoot == null) 
        //            ? CreateElementsRootObject(splatData) 
        //            : elementsRoot.gameObject;
        //    }
        //    Bounds terrainBounds = _terrain.terrainBounds;
        //    Debug.Log(terrainBounds + " " + terrainBounds.min + ", " + terrainBounds.max);
        //    float xPos = terrainBounds.min.x;
        //    float zPos = terrainBounds.min.z;
        //    while(zPos < terrainBounds.max.z)
        //    {
        //        xPos = terrainBounds.min.x;
        //        while (xPos < terrainBounds.max.x)
        //        {
        //            float x = xPos + Random.Range(_sampleDistance * 0.2f, _sampleDistance * 0.8f);
        //            float z = zPos + Random.Range(_sampleDistance * 0.2f, _sampleDistance * 0.8f);
        //            Vector3 position = new Vector3(
        //                x,
        //                0,
        //                z);
        //            SplatCategoryData splatData = _terrain.GetCategoryAtPosition(position);
        //            if(splatData != null && splatData._instancedObject != null)
        //            {
        //                GameObject go = GameObject.Instantiate(splatData._instancedObject.gameObject);
        //                go.transform.SetParent(transform.Find(splatData._name));
        //                position.y = _terrain.GetHeightAtPosition(new Vector3(xPos, 0, zPos));
        //                go.transform.position = position;
        //                go.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 90f), 0);
        //            }
        //            xPos += _sampleDistance;
        //        }
        //        zPos += _sampleDistance;
        //    }
        //}

        //public void ClearElements(SplatCategoryData[] splatCategoryData)
        //{
        //    for (int i = 0; i < splatCategoryData.Length; i++)
        //    {
        //        SplatCategoryData splatData = splatCategoryData[i];
        //        Transform elementsRoot = transform.Find(splatData._name);
        //        if(elementsRoot != null)
        //        {
        //            ClearElementsRoot(elementsRoot);
        //        }
        //    }
        //}

        public void SpawnElementsInstanced(SplatCategoryData[] splatCategoryData)
        {
            for (int i = 0; i < splatCategoryData.Length; i++)
            {
                SpawnElementsForSplatCategory(splatCategoryData[i]);
            }
            //Dictionary<string, MeshGeneratorInstanced> meshGenerators = new Dictionary<string, MeshGeneratorInstanced>();
            //Dictionary<string, List<Vector3>> positions = new Dictionary<string, List<Vector3>>();
            //for (int i = 0; i < splatCategoryData.Length; i++)
            //{
            //    SplatCategoryData splatData = splatCategoryData[i];
            //    Transform elementsRoot = transform.Find(splatData._name);
            //    if(splatData._instancedObject != null)
            //    {
            //        GameObject go = GameObject.Instantiate<GameObject>(splatData._instancedObject.gameObject);
            //        go.name = splatData._name;
            //        go.transform.SetParent(transform);
            //        meshGenerators.Add(splatData._name, go.GetComponent<MeshGeneratorInstanced>());
            //    }
            //    positions.Add(splatData._name, new List<Vector3>());
            //}
            //Bounds terrainBounds = _terrain.terrainBounds;
            //float xPos = terrainBounds.min.x;
            //float zPos = terrainBounds.min.z;
            //while (zPos < terrainBounds.max.z)
            //{
            //    xPos = terrainBounds.min.x;
            //    while (xPos < terrainBounds.max.x)
            //    {
            //        float x = xPos + Random.Range(_sampleDistance * 0.2f, _sampleDistance * 0.8f);
            //        float z = zPos + Random.Range(_sampleDistance * 0.2f, _sampleDistance * 0.8f);
            //        Vector3 position = new Vector3(
            //            x,
            //            0,
            //            z);
            //        SplatCategoryData splatData = _terrain.GetCategoryAtPosition(position);
            //        if (splatData != null)
            //        {
            //            position.y = _terrain.GetHeightAtPosition(new Vector3(xPos, 0, zPos));
            //            positions[splatData._name].Add(position);
            //        }
            //        xPos += _sampleDistance;
            //    }
            //    zPos += _sampleDistance;
            //}
            //for (int i = 0; i < splatCategoryData.Length; i++)
            //{
            //    if (positions.Count <= 0) continue;
            //    if (!meshGenerators.ContainsKey(splatCategoryData[i]._name)
            //        || meshGenerators[splatCategoryData[i]._name] == null) continue;
            //    SplatCategoryData splatData = splatCategoryData[i];
            //    meshGenerators[splatCategoryData[i]._name].Initialize();
            //    meshGenerators[splatCategoryData[i]._name].SetSampledPositions(positions[splatData._name]);
            //}
        }

        private void SpawnElementsForSplatCategory(SplatCategoryData splatData)
        {
            TerrainMeshInstanced meshGenerator = null;
            Transform elementsRoot = transform.Find(splatData._name);
            if (splatData._instancedObject != null)
            {
                GameObject go = GameObject.Instantiate<GameObject>(splatData._instancedObject.gameObject);
                go.name = splatData._name;
                go.transform.SetParent(transform);
                meshGenerator = go.GetComponent<TerrainMeshInstanced>();
            }
            List<Vector3> positions = new List<Vector3>();
            Bounds terrainBounds = _terrain.terrainBounds;
            float xPos = terrainBounds.min.x;
            float zPos = terrainBounds.min.z;
            float sampleDistance = (splatData._sampleDistance > 0) ? splatData._sampleDistance : _defaultSampleDistance;
            while (zPos < terrainBounds.max.z)
            {
                xPos = terrainBounds.min.x;
                while (xPos < terrainBounds.max.x)
                {
                    float x = xPos + Random.Range(sampleDistance * 0.2f, sampleDistance * 0.8f);
                    float z = zPos + Random.Range(sampleDistance * 0.2f, sampleDistance * 0.8f);
                    Vector3 position = new Vector3(
                        x,
                        0,
                        z);
                    SplatCategoryData splatDataAtPos = _terrain.GetCategoryAtPosition(position);
                    if (splatDataAtPos != null && splatDataAtPos._color == splatData._color)
                    {
                        position.y = _terrain.GetHeightAtPosition(new Vector3(xPos, 0, zPos));
                        positions.Add(position);
                    }
                    xPos += sampleDistance;
                }
                zPos += sampleDistance;
            }
            if (positions.Count <= 0) return;
            if (meshGenerator == null) return;
            meshGenerator.Initialize();
            meshGenerator.SetSampledPositions(positions);
        }

        public void ClearElementsInstanced(SplatCategoryData[] splatCategoryData)
        {
            for (int i = 0; i < splatCategoryData.Length; i++)
            {
                SplatCategoryData splatData = splatCategoryData[i];
                Transform elementsRoot = transform.Find(splatData._name);
                if (elementsRoot != null)
                {
                    ClearElementsRoot(elementsRoot);
                    GameObject.DestroyImmediate(elementsRoot.gameObject);
                }
            }
        }

        private GameObject CreateElementsRootObject(SplatCategoryData splatData)
        {
            GameObject elementsRootGO = new GameObject(splatData._name);
            elementsRootGO.transform.SetParent(transform);
            elementsRootGO.transform.localPosition = Vector3.zero;
            elementsRootGO.transform.localRotation = Quaternion.identity;
            elementsRootGO.transform.localScale = Vector3.one;
            return elementsRootGO;
        }

        private void ClearElementsRoot(Transform elementsRoot)
        {
            for (int j = elementsRoot.childCount - 1; j >= 0; j--)
            {
                GameObject.DestroyImmediate(elementsRoot.GetChild(j).gameObject);
            }
        }
    }
}