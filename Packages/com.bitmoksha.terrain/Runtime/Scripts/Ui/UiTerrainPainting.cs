using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace com.bitmoksha.terrain
{
    /// <summary>
    /// Controls for splat map painting.
    /// </summary>
    public class UiTerrainPainting : MonoBehaviour
    {

        [SerializeField]
        private ScrollView _scrViewPaintControls;
        [SerializeField]
        private Transform _contentRoot;
        [SerializeField]
        private UnityEngine.UI.Button _btnPaintControlsTemplate;

        public void SetupPaintingButtons(SplatCategoryData[] splatCategories, Action<int> onClickAction)
        {
            for (int i = _contentRoot.childCount - 1; i >= 0 ; i--) 
            {
                DestroyImmediate(_contentRoot.GetChild(i).gameObject);
            }
            for(int i = 0; i < splatCategories.Length; i++)
            {
                UnityEngine.UI.Button btn = GameObject.Instantiate(_btnPaintControlsTemplate);
                btn.GetComponent<UnityEngine.UI.Image>().color = splatCategories[i]._color;
                btn.GetComponentInChildren<UnityEngine.UI.Text>().text = splatCategories[i]._name;
                btn.GetComponentInChildren<UnityEngine.UI.Text>().color = splatCategories[i]._color * 2;
                btn.gameObject.SetActive(true);
                btn.transform.SetParent(_contentRoot);
                int idx = i;
                btn.onClick.AddListener(() => onClickAction(idx));
            }
        }
    }
}