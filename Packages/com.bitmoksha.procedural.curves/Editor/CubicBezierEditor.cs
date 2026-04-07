using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace com.bitmoksha.procedural.curves
{
    [CustomEditor(typeof(CubicBezier))]
    public class CubicBezierEditor : Editor
    {
        private int mSelected = -1;
        private int mSelectedSubCurve = -1;
        private bool mCurvesListFoldout = true;
        private bool mLockControlPoints = false;
        private float mCurvePosIndicatorValue = 0.0f;
        private Vector3 mCurvePosIndicatorVec = Vector3.zero;

        public override void OnInspectorGUI()
        {
            CubicBezier targetCurve = target as CubicBezier;
            mLockControlPoints = EditorGUILayout.Toggle("Lock Control Points", mLockControlPoints, GUILayout.Width(128 + 32));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Length: " + targetCurve.curveLength.ToString("0.00"), GUILayout.Width(128 + 32));
            mCurvePosIndicatorValue = EditorGUILayout.Slider("Show Position: ", mCurvePosIndicatorValue, 0.0f, 1.0f);
            mCurvePosIndicatorVec = targetCurve.GetPoint(mCurvePosIndicatorValue);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add Sub-Curve", GUILayout.Width(128))) { targetCurve.AddCurveOriented(); SceneView.RepaintAll(); }
            if (GUILayout.Button("Redraw Curve", GUILayout.Width(128))) { DrawCurve(); SceneView.RepaintAll(); }
            if (GUILayout.Button("Clear Curve", GUILayout.Width(128))) { targetCurve.ClearCurve(); SceneView.RepaintAll(); }
            EditorGUILayout.EndHorizontal();

            mCurvesListFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(mCurvesListFoldout, "Sub-Curves");
            SerializedProperty curvesProp = this.serializedObject.FindProperty("_curve");
            EditorGUI.indentLevel++;
            int prevSelectedSubCurve = mSelectedSubCurve;
            for (int i = 0; i < curvesProp.arraySize; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Length: " + targetCurve.GetSubCurveLength(i).ToString("0.00"), GUILayout.Width(128 + 32));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Delete", GUILayout.Width(64)))
                {
                    targetCurve.RemoveSubCurve(i);
                    Repaint();
                    SceneView.RepaintAll();
                    break;
                }
                bool currSel = mSelectedSubCurve == i;
                bool sel = GUILayout.Toggle(currSel, "Edit", GUILayout.Width(64));
                if (sel)
                {
                    mSelectedSubCurve = i;
                }
                else
                {
                    if (currSel)
                        mSelectedSubCurve = -1;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(curvesProp.GetArrayElementAtIndex(i));
            }
            if (prevSelectedSubCurve != mSelectedSubCurve)
                SceneView.RepaintAll();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        public void OnSceneGUI()
        {
            CubicBezier targetCurve = target as CubicBezier;
            SerializedProperty curvesProp = this.serializedObject.FindProperty("_curve");
            int ctrlId = 0;
            bool modified = false;
            //Color c = Handles.color;
            for (int i = 0; i < curvesProp.arraySize; i++)
            {
                Handles.color = Color.white;
                DrawCurveEndpoints(curvesProp.GetArrayElementAtIndex(i),
                    (i == 0) ? null : curvesProp.GetArrayElementAtIndex(i - 1),
                    (i < curvesProp.arraySize - 1) ? curvesProp.GetArrayElementAtIndex(i + 1) : null, 
                    ref ctrlId
                    );
                SerializedProperty ep00Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("endPoint00");
                //if (i == 0)
                //{
                //    //SerializedProperty ep00Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("endPoint00");
                //    ep00Prop.vector3Value = DrawCurveControlPoint(ep00Prop.vector3Value, ctrlId++);
                //}
                SerializedProperty ep01Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("endPoint01");
                //ep01Prop.vector3Value = DrawCurveControlPoint(ep01Prop.vector3Value, ctrlId++);
                //if (i < curvesProp.arraySize - 1)
                //{
                //    SerializedProperty ep00PropPrev = curvesProp.GetArrayElementAtIndex(i + 1).FindPropertyRelative("endPoint00");
                //    ep00PropPrev.vector3Value = ep01Prop.vector3Value;
                //}

                //if (mSelectedSubCurve != i)
                //    continue;

                Handles.color = Color.cyan;
                SerializedProperty ctrl00Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("control00");
                Vector3 oldPos = ctrl00Prop.vector3Value;
                ctrl00Prop.vector3Value = DrawCurveControlPoint(ctrl00Prop.vector3Value, ctrlId++);
                modified = modified || (Vector3.Distance(oldPos, ctrl00Prop.vector3Value) > 0.01f);

                SerializedProperty ctrl01Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("control01");
                oldPos = ctrl01Prop.vector3Value;
                ctrl01Prop.vector3Value = DrawCurveControlPoint(ctrl01Prop.vector3Value, ctrlId++);
                modified = modified || (Vector3.Distance(oldPos, ctrl01Prop.vector3Value) > 0.01f);

                if (!mLockControlPoints)
                {
                    Handles.DrawLine(targetCurve.transform.TransformPoint(ctrl00Prop.vector3Value),
                        targetCurve.transform.TransformPoint(ctrl01Prop.vector3Value));
                }
                else
                {
                    Handles.DrawLine(targetCurve.transform.TransformPoint(ep00Prop.vector3Value),
                        targetCurve.transform.TransformPoint(ctrl00Prop.vector3Value));
                    Handles.DrawLine(targetCurve.transform.TransformPoint(ctrl01Prop.vector3Value),
                        targetCurve.transform.TransformPoint(ep01Prop.vector3Value));
                }
            }
            if(modified)
            {
                modified = false;
            }

            Handles.color = Color.green;
            Handles.DrawWireCube(mCurvePosIndicatorVec, Vector3.one * HandleUtility.GetHandleSize(mCurvePosIndicatorVec) / 10);

            if (serializedObject.hasModifiedProperties)
            {
                targetCurve.UpdateCurveLengths();
                DrawCurve();
                serializedObject.ApplyModifiedProperties();
            }

            DrawCurve();
        }

        private void DrawCurveEndpoints(SerializedProperty curveData, 
            SerializedProperty prevCurveData, SerializedProperty nextCurveData, ref int ctrlId)
        {
            Handles.color = Color.white;
            SerializedProperty ep00Prop = curveData.FindPropertyRelative("endPoint00");
            Vector3 oldPos = ep00Prop.vector3Value;
            if (prevCurveData == null)
            {
                //SerializedProperty ep00Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("endPoint00");
                ep00Prop.vector3Value = DrawCurveControlPoint(ep00Prop.vector3Value, ctrlId++);
                
                if (mLockControlPoints && Vector3.Distance(oldPos, ep00Prop.vector3Value) > 0.01f)
                {
                    Vector3 delta = ep00Prop.vector3Value - oldPos;
                    SerializedProperty ctrl00Prop = curveData.FindPropertyRelative("control00");
                    ctrl00Prop.vector3Value += delta;
                }
            }
            SerializedProperty ep01Prop = curveData.FindPropertyRelative("endPoint01");
            oldPos = ep01Prop.vector3Value;
            ep01Prop.vector3Value = DrawCurveControlPoint(ep01Prop.vector3Value, ctrlId++);
            if (mLockControlPoints && Vector3.Distance(oldPos, ep01Prop.vector3Value) > 0.01f)
            {
                Vector3 delta = ep01Prop.vector3Value - oldPos;
                SerializedProperty ctrl01Prop = curveData.FindPropertyRelative("control01");
                ctrl01Prop.vector3Value += delta;
                if (nextCurveData != null)
                {
                    SerializedProperty ctrl00Prop = nextCurveData.FindPropertyRelative("control00");
                    ctrl00Prop.vector3Value += delta;
                }
            }
            if (nextCurveData != null)
            {
                SerializedProperty ep00PropPrev = nextCurveData.FindPropertyRelative("endPoint00");
                ep00PropPrev.vector3Value = ep01Prop.vector3Value;
            }
        }

        private Vector3 DrawCurveControlPoint(Vector3 point, int ctrlId)
        {
            CubicBezier targetCurve = target as CubicBezier;
            point = targetCurve.transform.TransformPoint(point);
            if (Handles.Button(point, Quaternion.identity,
                HandleUtility.GetHandleSize(point) / 10, 1, Handles.CubeHandleCap))
            {
                if (mSelected == ctrlId)
                    mSelected = -1;
                else
                    mSelected = ctrlId;
            }
            
            if (mSelected == ctrlId)
                return targetCurve.transform.InverseTransformPoint(Handles.DoPositionHandle(point, Quaternion.identity));
            return targetCurve.transform.InverseTransformPoint(point);
        }

        public void DrawCurve()
        {
            Handles.color = Color.blue;
            CubicBezier targetCurve = target as CubicBezier;
            SerializedProperty curvesProp = this.serializedObject.FindProperty("_curve");
            if (curvesProp.arraySize <= 0)
                return;

            List<Vector3> curvePoints = new List<Vector3>();
            float t = 0;
            float incr = 0.001f;
            Vector3 currPt = targetCurve.GetPoint(t);
            while (t <= (1.0f - incr))
            {
                Vector3 nextPt = targetCurve.GetPoint(t + incr);
                Handles.DrawLine(currPt, nextPt);
                currPt = nextPt;
                t += incr;
            }
        }


    }
}
