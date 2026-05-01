using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal.VR;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.bitmoksha.terrain
{
    /// <summary>
    /// Controller for the terrain editor.
    /// Manages editor states(movement, painting, sculpting)
    /// Holds individual terrain creation modules(mesh, ui)
    /// </summary>
    public class TerrainEditorController : MonoBehaviour
    {
        /// <summary>
        /// Interaction states for the editor.
        /// </summary>
        public enum TerrainEditorState
        {
            Pan, 
            Orbit, 
            Zoom,
            Paint, 
            Sculpt
        }

        [SerializeField]
        private TerrainMesh _terrainMesh;
        [SerializeField]
        private UiTerrainCreation _terrainCreationUi;
        [SerializeField]
        private UiTerrainPainting _terrainPaintingUi;
        [SerializeField]
        private SplatCategoryData[] _splatCategories;
        [SerializeField]
        private TerrainCameraController _terrainCameraController;
        [SerializeField]
        private Button _btnSave;
        [SerializeField]
        private Button _btnLoad;
        [SerializeField] 
        private UiTerrainsList _terrainListUi;
        [SerializeField]
        private TerrainElementsSpawner _spawner;

        private TerrainEditorState mCurrentEditorState;
        private Vector3 mLastMousePosition;
        private bool mIsMouseDragging;
        private TerrainSaveData mSaveData;

        internal SplatCategoryData[] splatCategories => _splatCategories;


        private void OnPaintColorSelected(int stateIdx)
        {
            mCurrentEditorState = TerrainEditorState.Paint;
            _terrainMesh.OnPaintColorSelected(stateIdx);
        }

        #region _Unity_Methods_
        void Start()
        {
            mCurrentEditorState = TerrainEditorState.Pan;
            _terrainMesh.SetSplatCategories(_splatCategories, 0);
            _terrainMesh.Initialize();
            _terrainPaintingUi.SetupPaintingButtons(_splatCategories,
                OnPaintColorSelected);
            _terrainCreationUi.onOrbitClick += () => mCurrentEditorState = TerrainEditorState.Orbit;
            _terrainCreationUi.onPanClick += () => mCurrentEditorState = TerrainEditorState.Pan;
            _terrainCreationUi.onZoomClick += () => mCurrentEditorState = TerrainEditorState.Zoom;
            _terrainCreationUi.onSpawnClick += OnSpawnClicked;
            _terrainCreationUi.onClearClick += OnClearClicked;

            _btnSave.onClick.AddListener(OnSaveTerrainClicked);
            _btnLoad.onClick.AddListener(OnLoadTerrainClicked);

            _terrainListUi.onTerrainSelected += 
                (saveData) => _terrainMesh.Initialize(saveData.terrainMesh);
        }

        // Update is called once per frame
        void Update()
        {
            switch(mCurrentEditorState)
            {
                case TerrainEditorState.Pan:
                    HandleCameraPan();
                    break;
                case TerrainEditorState.Orbit:
                    HandleCameraOrbit();
                    break;
                case TerrainEditorState.Zoom:
                    HandleCameraZoom();
                    break;
                case TerrainEditorState.Paint:
                    HandleTerrainPaint();
                    break;
                case TerrainEditorState.Sculpt:
                    break;
            }
            
        }
        #endregion

        void HandleCameraOrbit()
        {
            if (!mIsMouseDragging)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    mLastMousePosition = Input.mousePosition;
                    mIsMouseDragging = true;
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    mIsMouseDragging = false;
                }
                else
                {
                    Vector3 delta = Input.mousePosition - mLastMousePosition;
                    _terrainCameraController.UpdateOrbit(delta);
                    mLastMousePosition = Input.mousePosition;
                }
            }
        }

        void HandleCameraPan()
        {
            if (!mIsMouseDragging)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    mLastMousePosition = Input.mousePosition;
                    mIsMouseDragging = true;
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    mIsMouseDragging = false;
                }
                else
                {
                    Vector3 delta = Input.mousePosition - mLastMousePosition;
                    _terrainCameraController.UpdatePan(delta);
                    mLastMousePosition = Input.mousePosition;
                }
            }
        }

        void HandleCameraZoom()
        {
            if (!mIsMouseDragging)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    mLastMousePosition = Input.mousePosition;
                    mIsMouseDragging = true;
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    mIsMouseDragging = false;
                }
                else
                {
                    Vector3 delta = Input.mousePosition - mLastMousePosition;
                    _terrainCameraController.UpdateZoom(delta);
                    mLastMousePosition = Input.mousePosition;
                }
            }
        }

        void HandleTerrainPaint()
        {
            bool isPointerOverGO = EventSystem.current.IsPointerOverGameObject();
            //|| EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
            if (Input.GetMouseButton(0)
                && !isPointerOverGO)
            {
                RaycastHit rcHit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out rcHit))
                {
                    _terrainMesh.PaintAtPosition(rcHit.point);
                    //Debug.Log("Point test: " + rcHit.point + ", Cell: " + _terrainMesh.GetTriangleForPosition(rcHit.point));
                }
            }
        }

        void OnSaveTerrainClicked()
        {
            if (mSaveData == null) mSaveData = new TerrainSaveData();
            mSaveData.terrainMesh = _terrainMesh.meshConfig as TerrainMeshData;
            mSaveData.terrainMesh.splatPath = Path.Combine(Application.dataPath,
                "terrain_splat_" + mSaveData.terrainId + ".png");
            byte[] splatData = _terrainMesh.splatMap.EncodeToPNG();
            File.WriteAllBytes(mSaveData.terrainMesh.splatPath, splatData);
            Debug.Log("Save Terrain: " + mSaveData.GetSaveString());
            string path = Path.Combine(Application.dataPath, 
                "terrain_" + mSaveData.terrainId + ".json");
            File.WriteAllText(path, mSaveData.GetSaveString());
            
            Debug.Log("Saved to: " + path);
        }

        void OnLoadTerrainClicked()
        {
            string path = Path.Combine(Application.dataPath,
                "terrain_*" + ".json");
            string[] terrainFilePaths = Directory.GetFiles(Application.dataPath, "terrain_*" + ".json", SearchOption.TopDirectoryOnly);
            Debug.Log("Found terrain files: " + string.Join(", ", terrainFilePaths));
            _terrainListUi.Show();
        }

        void OnSpawnClicked()
        {
            _spawner.SpawnElementsInstanced(_splatCategories);
        }

        void OnClearClicked()
        {
            _spawner.ClearElementsInstanced(_splatCategories);
        }
    }
}