using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.bitmoksha.terrain
{
    /// <summary>
    /// Manages the Ui for listing saved terrains and allows selecting a particular terrain to load.
    /// </summary>
    public class UiTerrainsList : MonoBehaviour
    {
        public delegate void TerrainSelectedDelegate(TerrainSaveData terrainData);
        public delegate void HeightmapSelectedDelegate(int index);

        [SerializeField]
        private ScrollRect _scrViewTerrains;
        [SerializeField]
        private Transform _contentParent;
        [SerializeField]
        private GameObject _buttonTemplate;
        [SerializeField]
        private Button _btnCancel;

        private TerrainSelectedDelegate mOnTerrainSelected;
        private HeightmapSelectedDelegate mOnHeightmapSelected;

        public event TerrainSelectedDelegate onTerrainSelected
        {
            add { mOnTerrainSelected += value; }
            remove { mOnTerrainSelected -= value; }
        }

        public event HeightmapSelectedDelegate onHeightmapSelected
        {
            add { mOnHeightmapSelected += value; }
            remove { mOnHeightmapSelected -= value; }
        }


        #region _Unity_Methods_
        void Start()
        {
            _btnCancel.onClick.AddListener(Hide);
        }
        #endregion

        public void ShowSavedTerrains()
        {
            string path = Path.Combine(Application.persistentDataPath,
                "terrain_*" + ".json");
            string[] terrainFilePaths = Directory.GetFiles(Application.persistentDataPath, 
                "terrain_*" + ".json", SearchOption.TopDirectoryOnly);

            for(int i = 0; i < terrainFilePaths.Length; i++)
            {
                string jsonData = File.ReadAllText(terrainFilePaths[i]);
                TerrainSaveData saveData = JsonUtility.FromJson<TerrainSaveData>(jsonData);
                byte[] splatBytes = File.ReadAllBytes(saveData.terrainMesh.splatPath);
                Texture2D splatTex = new Texture2D(2, 2);
                splatTex.LoadImage(splatBytes, true);

                SetupButton(splatTex, 
                    () =>
                    {
                        if (mOnTerrainSelected != null)
                            mOnTerrainSelected(saveData);
                        Hide();
                    });

                Debug.Log("Read data: " + saveData.terrainId);
            }

            gameObject.SetActive(true);
        }

        public void ShowHeightMaps(HeightMapInfo[] heightmapInfo)
        {
            for (int i = 0; i < heightmapInfo.Length; i++)
            {
                int index = i;
                SetupButton(heightmapInfo[i].heightMap,
                    () =>
                    {
                        if (mOnHeightmapSelected != null)
                            mOnHeightmapSelected(index);
                        Hide();
                    });
            }

            gameObject.SetActive(true);
        }

        public void Hide()
        {
            Clear();
            gameObject.SetActive(false);
        }

        public void Clear()
        {
            for(int i = _contentParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(_contentParent.GetChild(i).gameObject);
            }
        }

        private void SetupButton(Texture2D iconTexture, UnityAction action)
        {
            GameObject btnObject = GameObject.Instantiate<GameObject>(_buttonTemplate);
            Image splatImg = null;
            foreach (Transform child in btnObject.transform)
            {
                splatImg = child.GetComponentInChildren<Image>();
                if (splatImg != null)
                {
                    break;
                }
            }
            if (splatImg != null)
                splatImg.sprite = Sprite.Create(iconTexture,
                new Rect(0, 0, iconTexture.width, iconTexture.height), new Vector2(0.5f, 0.5f));
            btnObject.SetActive(true);
            btnObject.transform.SetParent(_contentParent);
            btnObject.GetComponent<Button>().onClick.AddListener(action);
        }
    }
}