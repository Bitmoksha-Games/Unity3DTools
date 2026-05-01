using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace com.bitmoksha.terrain
{
    /// <summary>
    /// Camera controller used while creating and viewing the terrain
    /// Allows for panning and contrained orbit view.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class TerrainCameraController : MonoBehaviour
    {

        [Header("Distance (zoom) parameters.")]
        [Tooltip("Default starting distance.")]
        [SerializeField]
        private float _distance = 5;
        [Tooltip("Minimum distance allowed. Controls how far we can zoom in.")]
        [SerializeField]
        private float _minDistance = 2;
        [Tooltip("Maximum distance allowed. Controls how far we can zoom out.")]
        [SerializeField]
        private float _maxDistance = 10;
        [Header("Pitch (look up / down) parameters.")]
        [Tooltip("default pitch.")]
        [SerializeField]
        private float _pitch = 45;
        [Tooltip("Minimum allowed pitch.")]
        [SerializeField]
        private float _minPitch = 20;
        [Tooltip("Maximum allowed pitch.")]
        [SerializeField]
        private float _maxPitch = 70;
        [Header("Control sensitivity parameters.")]
        [Tooltip("Rotation speed.")]
        [SerializeField]
        private float _rotationSpeed = 20;
        [Tooltip("Panning speed.")]
        [SerializeField]
        private float _panningSpeed = 20;
        [Tooltip("Zoom speed.")]
        [SerializeField]
        private float _zoomSpeed = 20;



        private Camera mCamera;
        private float mCurrentPitch;
        private float mCurrentYaw;
        private float mCurrentDistance;
        private Vector3 mReferencePoint;


        public void UpdateOrbit(Vector2 delta)
        {
            mCurrentPitch += delta.y * Time.deltaTime * _rotationSpeed;
            mCurrentYaw += delta.x * Time.deltaTime * _rotationSpeed;
            mCurrentPitch = Mathf.Clamp(mCurrentPitch, _minPitch, _maxPitch);
        }

        public void UpdatePan(Vector2 delta)
        {
            Vector3 fwd = -mCamera.transform.forward;
            fwd.y = 0;
            fwd.Normalize();
            Vector3 rt = -mCamera.transform.right;
            rt.y = 0;
            rt.Normalize();
            Vector3 moveDir = (fwd * delta.y + rt * delta.x).normalized * _panningSpeed * Time.deltaTime;
            mReferencePoint += moveDir;
        }

        public void UpdateZoom(Vector2 delta)
        {
            mCurrentDistance += delta.y * Time.deltaTime * _zoomSpeed;
            mCurrentDistance = Mathf.Clamp(mCurrentDistance, _minDistance, _maxDistance);
        }

        #region _Unity_Methods_
        void Start()
        {
            mCamera = GetComponent<Camera>();
            mCurrentDistance = _distance;
            mCurrentPitch = _pitch;
            mCurrentYaw = 0;
            mReferencePoint = Vector3.zero;
        }

        void Update()
        {
        }

        private void LateUpdate()
        {
            if (mCamera != null)
            {
                mCamera.transform.position = Quaternion.Euler(mCurrentPitch, mCurrentYaw, 0) 
                    * new Vector3(0, 0, -mCurrentDistance) + mReferencePoint;
                mCamera.transform.rotation = Quaternion.Euler(mCurrentPitch, mCurrentYaw, 0);
            }
        }
        #endregion
    }
}