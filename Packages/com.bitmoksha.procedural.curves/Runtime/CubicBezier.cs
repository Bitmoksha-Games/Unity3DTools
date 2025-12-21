using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.bitmoksha.procedural.curves
{
    /// <summary>
    /// Provides API for creating, managing and using cubic bezier curves.
    /// A single full curve consists of multiple individual sub-curves.
    /// </summary>
    public class CubicBezier : MonoBehaviour
    {
        /// <summary>
        /// Contains curve data. End-points and control points.
        /// A curve can contain multiple sub-curve
        /// </summary>
        [Serializable]
        private class CurveData
        {
            public Vector3 endPoint00;
            public Vector3 endPoint01;
            public Vector3 control00;
            public Vector3 control01;
            private float mCurveLength;

            public float curveLength
            {
                get
                {
                    if (mCurveLength <= 0) ComputeCurveLength();
                    return mCurveLength;
                }
            }

            public CurveData(Vector3 ep00, Vector3 ep01, Vector3 ctrl00, Vector3 ctrl01)
            {
                endPoint00 = ep00;
                endPoint01 = ep01;
                control00 = ctrl00;
                control01 = ctrl01;
                mCurveLength = 0;
                ComputeCurveLength();
            }

            public void ComputeCurveLength()
            {
                Vector3 pt = endPoint00;
                float incr = 0.001f;
                float t = 0.0f + incr;
                float l = 0.0f;
                while (t <= 1.0f)
                {
                    Vector3 nextPt = GetPoint(t);
                    float dist = Vector3.Distance(nextPt, pt);
                    l += dist;
                    pt = nextPt;
                    t += incr;
                }
                mCurveLength = l;
            }

            public Vector3 GetPoint(float t)
            {
                t = Mathf.Clamp01(t);

                return Mathf.Pow(1.0f - t, 3) * endPoint00
                    + 3 * Mathf.Pow(1.0f - t, 2) * t * control00
                    + 3 * Mathf.Pow(t, 2) * (1.0f - t) * control01
                    + Mathf.Pow(t, 3) * endPoint01;
            }
        }

        [SerializeField]
        private List<CurveData> _curve = new List<CurveData>();

        // Curve length in meters
        public float curveLength 
        {
            get
            {
                if (mCurveLength <= 0) UpdateCurveLengths();
                return mCurveLength;
            }
        } 

        private float mCurveLength;


        /// <summary>
        /// Add a sub-curve.
        /// </summary>
        /// <param name="endPoint00">The curve begins at this point.</param>
        /// <param name="endPoint01">The curve ends at this point.</param>
        /// <param name="control00">Controls the shape of the curve.</param>
        /// <param name="control01">Controls the shape of the curve.</param>
        public void AddSubCurve(Vector3 endPoint00, Vector3 endPoint01, Vector3 control00, Vector3 control01)
        {
            _curve.Add(new CurveData(
                (_curve.Count <= 0) ? endPoint00 : _curve[_curve.Count - 1].endPoint01,
                endPoint01, control00, control01));
        }

        /// <summary>
        /// Removes the sub-curve at the specified index.
        /// </summary>
        /// <param name="idx">Sub-curve index.</param>
        public void RemoveSubCurve(int idx)
        {
            _curve.RemoveAt(idx);
            if (idx > 0 && _curve.Count > idx)
            {
                _curve[idx].endPoint00 = _curve[idx - 1].endPoint01;
            }
            UpdateCurveLengths();
        }

        /// <summary>
        /// Erase / empty the curve. Removes all sub-curves.
        /// </summary>
        public void ClearCurve()
        {
            _curve.Clear();
            UpdateCurveLengths();
        }

        /// <summary>
        /// Force recalculation of the curve length.
        /// </summary>
        public void UpdateCurveLengths()
        {
            mCurveLength = 0;
            for (int i = 0; i < _curve.Count; i++)
            {
                _curve[i].ComputeCurveLength();
                mCurveLength += _curve[i].curveLength;
            }
        }

        /// <summary>
        /// Get the position of a point on the curve at the specified relative distance.
        /// </summary>
        /// <param name="t">[0,1] - The relative distance on the curve. 0 = starting point, 1 = end point.</param>
        /// <param name="inheritTransformPos">Returns the global position of the curve points related to the gameobject containing the curve.</param>
        /// <returns></returns>
        public Vector3 GetPoint(float t, bool inheritTransformPos = true)
        {
            if (_curve.Count <= 0)
                return transform.position;

            t = Mathf.Clamp01(t);
            int selectedSubCurve = -1;
            float reqdCurvePoint = t * curveLength;
            float totalSubCurveLength = 0;
            for (int i = 0; i < _curve.Count; i++)
            {
                if ((totalSubCurveLength + _curve[i].curveLength) >= reqdCurvePoint)
                {
                    selectedSubCurve = i;
                    break;
                }
                totalSubCurveLength += _curve[i].curveLength;
            }
            selectedSubCurve = Mathf.Clamp(selectedSubCurve, 0, _curve.Count - 1);
            float reqdT = (reqdCurvePoint - totalSubCurveLength) / _curve[selectedSubCurve].curveLength;
            reqdT = Mathf.Clamp01(reqdT);
            return _curve[selectedSubCurve].GetPoint(reqdT)
                + ((inheritTransformPos) ? transform.position : Vector3.zero);
        }

        /// <summary>
        /// Add a sub-curve, default as a straight line.
        /// </summary>
        public void AddCurveStraight()
        {
            Vector3 offset = Vector3.zero;
            if (_curve.Count > 0)
                offset = _curve[_curve.Count - 1].endPoint01;
            AddSubCurve(Vector3.zero + offset, Vector3.right * 3 + offset, Vector3.right + offset, Vector3.right * 2 + offset);
            UpdateCurveLengths();
        }

        /// <summary>
        /// Draw the curve (uses a linerenderer).
        /// </summary>
        public void DrawCurve()
        {
            if (_curve.Count <= 0)
                return;

            List<Vector3> curvePoints = new List<Vector3>();
            for (int i = 0; i < _curve.Count; ++i)
            {
                float t = 0.0f;
                while (t <= 1.0f)
                {
                    Vector3 pt = _curve[i].GetPoint(t);
                    curvePoints.Add(pt);
                    t += 0.01f;
                }
            }

            LineRenderer lr = GetComponent<LineRenderer>();
            if (lr == null)
            {
                lr = gameObject.AddComponent<LineRenderer>();
            }
            lr.positionCount = curvePoints.Count;
            lr.SetPositions(curvePoints.ToArray());
            lr.widthMultiplier = 0.1f;
        }

        /// <summary>
        /// Get the length of the specified sub-curve.
        /// </summary>
        /// <param name="idx">Index of the sub-curve.</param>
        /// <returns></returns>
        public float GetSubCurveLength(int idx)
        {
            if (idx >= 0 && idx < _curve.Count)
                return _curve[idx].curveLength;
            return 0;
        }
    }
}
