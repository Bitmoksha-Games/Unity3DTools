using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(CubicBezier))]
public class CubicBezierMeshGenerator : MonoBehaviour
{
    [SerializeField]
    private float _angleIncrement = 1.0f;
    [SerializeField]
    private int _curveDivisions = 10;
    [SerializeField]
    private Material _material;
    [Range(0.0f, 360.0f)]
    [SerializeField]
    private float _startAngle = 0;
    [Range(0.0f, 360.0f)]
    [SerializeField]
    private float _endAngle = 360;
    [SerializeField]
    private bool _invertNormals = false;

    private Vector3 mCurrPoint = Vector3.up * 6 + Vector3.right;
    private List<Vector3> mVertices = new List<Vector3>();
    private List<int> mIndices = new List<int>();
    private List<Vector3> mNormals = new List<Vector3>();
    private List<Vector2> mUv = new List<Vector2>();
    private List<Vector3> mFaceNormals = new List<Vector3>();
    private List<int> mSharedVertexFaces = new List<int>();
    private GameObject mMeshGO;

    public void Start()
    {
#if !UNITY_EDITOR
#endif
    }

    [ContextMenu("Add Curve")]
    public void AddCurve()
    {
        CubicBezier curve = GetComponent<CubicBezier>();
        curve.AddSubCurve(mCurrPoint, mCurrPoint + Vector3.down * 3,
            mCurrPoint + Vector3.down * 1, mCurrPoint + Vector3.down * 2);
        mCurrPoint = mCurrPoint + Vector3.down * 3;
    }

    [ContextMenu("Generate Mesh")]
    public void GenerateMesh()
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        float angleFloat = _startAngle;
        int angleRotCount = 0;
        mVertices.Clear();
        mIndices.Clear();
        mUv.Clear();
        mNormals.Clear();
        mFaceNormals.Clear();
        mSharedVertexFaces.Clear();
        List<Color> colors = new List<Color>();
        CubicBezier curve = GetComponent<CubicBezier>();
        curve.UpdateCurveLengths();

        List<Vector3> curvePoints = new List<Vector3>();
        float curveIncrement = 1.0f / (float)_curveDivisions;

        float t = 0;
        Quaternion rotVal = Quaternion.AngleAxis(angleFloat, Vector3.up);
        for (int i = 0; i < _curveDivisions; i++)
        {
            mVertices.Add(rotVal * curve.GetPoint(t) - transform.position);
            mUv.Add(new Vector2(0, 1.0f - (float)i / _curveDivisions));
            mNormals.Add(new Vector3(0, 0, 0));
            mSharedVertexFaces.Add(0);
            t += curveIncrement;
        }

        angleFloat += _angleIncrement;
        float angleIncrementRatio = (_angleIncrement / (float)360);
        float uvX = angleIncrementRatio;
        angleRotCount += 1;
        float normalMul = _invertNormals ? -1.0f : 1.0f;
        while (angleFloat <= _endAngle)
        {
            Quaternion rotVal1 = Quaternion.AngleAxis(angleFloat, Vector3.up);
            for (int i = 0; i < _curveDivisions; i++)
            {
                mVertices.Add(rotVal1 * mVertices[i]);
                mUv.Add(new Vector2(uvX, 1.0f - (float)i / _curveDivisions));
                mNormals.Add(new Vector3(0, 0, 0));
                mSharedVertexFaces.Add(0);
                int currVertIdx = mVertices.Count - 1;
                if (i > 0)
                {
                    //int v0Idx = (angleRotCount - 1) * _curveDivisions + (i - 1);
                    //int v1Idx = (angleRotCount - 1) * _curveDivisions + (i);
                    //int v2Idx = (angleRotCount) * _curveDivisions + (i);
                    //int v3Idx = (angleRotCount) * _curveDivisions + (i - 1);
                    int v0Idx = currVertIdx - _curveDivisions - 1;
                    int v1Idx = currVertIdx - _curveDivisions;
                    int v2Idx = currVertIdx;
                    int v3Idx = currVertIdx - 1;
                    if(!_invertNormals)
                    {
                        mIndices.Add(v0Idx);
                        mIndices.Add(v1Idx);
                        mIndices.Add(v2Idx);
                        mIndices.Add(v0Idx);
                        mIndices.Add(v2Idx);
                        mIndices.Add(v3Idx);
                    } 
                    else
                    {
                        mIndices.Add(v0Idx);
                        mIndices.Add(v2Idx);
                        mIndices.Add(v1Idx);
                        mIndices.Add(v0Idx);
                        mIndices.Add(v3Idx);
                        mIndices.Add(v2Idx);
                    }

                    Vector3 e1 = mVertices[v0Idx] - mVertices[v1Idx];
                    Vector3 e2 = mVertices[v2Idx] - mVertices[v1Idx];
                    mFaceNormals.Add(Vector3.Cross(e2.normalized, e1.normalized));
                    e1 = mVertices[v0Idx] - mVertices[v2Idx];
                    e2 = mVertices[v3Idx] - mVertices[v2Idx];
                    mFaceNormals.Add(Vector3.Cross(e2.normalized, e1.normalized));
                }
            }
            angleFloat += _angleIncrement;
            angleRotCount += 1;
            uvX += angleIncrementRatio;
        }

        for(int i = 0; i < mFaceNormals.Count; ++i)
        {
            int baseIdx = i * 3;
            Vector3 faceNormal = mFaceNormals[i];
            mNormals[mIndices[baseIdx]] += faceNormal;
            mSharedVertexFaces[mIndices[baseIdx]] += 1;
            ++baseIdx;
            mNormals[mIndices[baseIdx]] += faceNormal;
            mSharedVertexFaces[mIndices[baseIdx]] += 1;
            ++baseIdx;
            mNormals[mIndices[baseIdx]] += faceNormal;
            mSharedVertexFaces[mIndices[baseIdx]] += 1;
        }

        for(int i = 0; i < mNormals.Count; ++i)
        {
            mNormals[i] = mNormals[i] / mSharedVertexFaces[i];
        }

        //string msg = "Total vertices: " + m_vertices.Count + "\n";
        //for (int i = 0; i < m_vertices.Count; i++)
        //{
        //    msg += m_vertices[i] + "\n";
        //}
        //msg += "-----\nTotal Indices: " + m_indices.Count + "\n-----\n";
        //for (int i = 0; i < m_indices.Count; i += 3)
        //{
        //    msg += m_indices[i] + ", " + m_indices[i + 1] + ", " + m_indices[i + 2] + "\n";
        //}
        //Debug.Log(msg);

        //msg = "UV: " + m_uv.Count + "\n";
        //for(int i = 0; i < m_uv.Count; ++i)
        //{
        //    msg += m_uv[i] + "\n";
        //}
        //Debug.Log(msg);

        //msg = "Normals: " + m_normals.Count + "\n";
        //for (int i = 0; i < m_normals.Count; ++i)
        //{
        //    msg += m_normals[i] + "\n";
        //}
        //Debug.Log(msg);

        if (mMeshGO == null)
        {
            mMeshGO = new GameObject("BezierMesh", typeof(MeshRenderer), typeof(MeshFilter));
            mMeshGO.transform.SetParent(transform);
            mMeshGO.transform.localPosition = Vector3.zero;
            mMeshGO.transform.localRotation = Quaternion.identity;
            mMeshGO.transform.localScale = Vector3.one;
        }
        MeshFilter mf = mMeshGO.GetComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.vertices = mVertices.ToArray();
        mesh.triangles = mIndices.ToArray();
        mesh.uv = mUv.ToArray();
        mesh.normals = mNormals.ToArray();
        mf.mesh = mesh;
        MeshRenderer mr = mMeshGO.GetComponent<MeshRenderer>();
        mr.sharedMaterial = _material;

        sw.Stop();
        Debug.Log("Time Taken: " + sw.Elapsed + "s (" + sw.ElapsedMilliseconds + "ms)");
    }

    public void OnDrawGizmosSelected()
    {
        //for(int i = 0; i < m_vertices.Count; i++)
        //{
        //    Gizmos.DrawCube(m_vertices[i], Vector3.one * 0.1f);
        //}
    }
}
