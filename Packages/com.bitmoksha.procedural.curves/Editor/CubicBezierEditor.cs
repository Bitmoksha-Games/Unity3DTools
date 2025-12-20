using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CubicBezier))]
public class CubicBezierEditor : Editor
{
    private int mSelected = -1;
    private int mSelectedSubCurve = -1;
    private bool mCurvesListFoldout = true;
    private float mCurvePosIndicatorValue = 0.0f;
    private Vector3 mCurvePosIndicatorVec = Vector3.zero;

    public override void OnInspectorGUI()
    {
        CubicBezier targetCurve = target as CubicBezier;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Length: " + targetCurve.curveLength.ToString("0.00"), GUILayout.Width(128 + 32));
        mCurvePosIndicatorValue = EditorGUILayout.Slider("Show Position: ", mCurvePosIndicatorValue, 0.0f, 1.0f);
        mCurvePosIndicatorVec = targetCurve.GetPoint(mCurvePosIndicatorValue);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Add Sub-Curve", GUILayout.Width(128))) targetCurve.AddCurveStraight();
        if (GUILayout.Button("Redraw Curve", GUILayout.Width(128))) DrawCurve();
        if (GUILayout.Button("Clear Curve", GUILayout.Width(128))) targetCurve.ClearCurve();
        EditorGUILayout.EndHorizontal();

        mCurvesListFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(mCurvesListFoldout, "Sub-Curves");
        SerializedProperty curvesProp = this.serializedObject.FindProperty("_curve");
        EditorGUI.indentLevel++;
        for (int i = 0; i < curvesProp.arraySize; i++)
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Length: " + targetCurve.GetSubCurveLength(i).ToString("0.00"), GUILayout.Width(128 + 32));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Delete", GUILayout.Width(64)))
            {
                targetCurve.RemoveSubCurve(i);
                Repaint();
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
        EditorGUI.indentLevel--;
        EditorGUILayout.EndFoldoutHeaderGroup();
    }

    public void OnSceneGUI()
    {
        CubicBezier targetCurve = target as CubicBezier;
        SerializedProperty curvesProp = this.serializedObject.FindProperty("_curve");
        int ctrlId = 0;
        //Color c = Handles.color;
        for (int i = 0; i < curvesProp.arraySize; i++)
        {            Handles.color = Color.white;
            if(i == 0)
            {
                SerializedProperty ep00Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("endPoint00");
                ep00Prop.vector3Value = DrawCurveControlPoint(ep00Prop.vector3Value, ctrlId++);
                if(i > 0)
                {
                    SerializedProperty ep01PropNext = curvesProp.GetArrayElementAtIndex(i - 1).FindPropertyRelative("endPoint01");
                    ep01PropNext.vector3Value = ep00Prop.vector3Value;
                }
            }
            SerializedProperty ep01Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("endPoint01");
            ep01Prop.vector3Value = DrawCurveControlPoint(ep01Prop.vector3Value, ctrlId++);
            if (i < curvesProp.arraySize - 1)
            {
                SerializedProperty ep00PropPrev = curvesProp.GetArrayElementAtIndex(i + 1).FindPropertyRelative("endPoint00");
                ep00PropPrev.vector3Value = ep01Prop.vector3Value;
            }

            if (mSelectedSubCurve != i)
                continue;

            Handles.color = Color.cyan;
            SerializedProperty ctrl00Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("control00");
            ctrl00Prop.vector3Value = DrawCurveControlPoint(ctrl00Prop.vector3Value, ctrlId++);

            SerializedProperty ctrl01Prop = curvesProp.GetArrayElementAtIndex(i).FindPropertyRelative("control01");
            ctrl01Prop.vector3Value = DrawCurveControlPoint(ctrl01Prop.vector3Value, ctrlId++);

            Handles.DrawLine(ctrl00Prop.vector3Value, ctrl01Prop.vector3Value);
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

    private Vector3 DrawCurveControlPoint(Vector3 point, int ctrlId)
    {
        if (Handles.Button(point, Quaternion.identity,
            HandleUtility.GetHandleSize(point) / 10, 1, Handles.CubeHandleCap))
        {
            if (mSelected == ctrlId)
                mSelected = -1;
            else
                mSelected = ctrlId;
        }
        if(mSelected == ctrlId)
            return Handles.DoPositionHandle(point, Quaternion.identity);
        return point;
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
        while(t <= (1.0f - incr))
        {
            Vector3 nextPt = targetCurve.GetPoint(t + incr);
            Handles.DrawLine(currPt, nextPt);
            currPt = nextPt;
            t += incr;
        }
    }


}
