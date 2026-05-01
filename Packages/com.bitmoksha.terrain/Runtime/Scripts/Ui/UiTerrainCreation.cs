using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.bitmoksha.terrain
{
    /// <summary>
    /// Handles user interface for terrain creation and viewing.
    /// </summary>
    public class UiTerrainCreation : MonoBehaviour
    {
        [SerializeField]
        private Button _btnSpawn;
        [SerializeField]
        private Button _btnClear;
        [SerializeField]
        private Button _btnOrbit;
        [SerializeField]
        private Button _btnPan;
        [SerializeField]
        private Button _btnZoom;

        public event Action onOrbitClick 
        {
            add { mOnOrbitClick += value; }
            remove { mOnOrbitClick -= value; }
        }
        public event Action onPanClick
        {
            add { mOnPanClick += value; }
            remove { mOnPanClick -= value; }
        }
        public event Action onZoomClick
        {
            add { mOnZoomClick += value; }
            remove { mOnZoomClick -= value; }
        }
        public event Action onSpawnClick
        {
            add { mOnSpawnClick += value; }
            remove { mOnSpawnClick -= value; }
        }
        public event Action onClearClick
        {
            add { mOnClearClick += value; }
            remove { mOnClearClick -= value; }
        }


        private Action mOnOrbitClick;
        private Action mOnPanClick;
        private Action mOnZoomClick;
        private Action mOnSpawnClick;
        private Action mOnClearClick;

        #region _Unity_Methods_
        private void Start() 
        {
            _btnSpawn.onClick.AddListener(() => { if (mOnSpawnClick != null) mOnSpawnClick(); });
            _btnClear.onClick.AddListener(() => { if (mOnClearClick != null) mOnClearClick(); });
            _btnOrbit.onClick.AddListener(() => { if (mOnOrbitClick != null) mOnOrbitClick(); });
            _btnPan.onClick.AddListener(() => { if (mOnPanClick != null) mOnPanClick(); });
            _btnZoom.onClick.AddListener(() => { if (mOnZoomClick != null) mOnZoomClick(); });
        }
        #endregion

    }
}