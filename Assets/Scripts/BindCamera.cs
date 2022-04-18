using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine.Rendering;

public class BindCamera : MonoBehaviour
{
    
    public enum PCFState
    {
        NO_PCF,
        PCF_2X2,
        PCF_4X4,
        PCF_8X8,
        PCF_16X16
    }

    [SetProperty("PcfState")] public PCFState pcfState = PCFState.PCF_2X2; //PCF状态

    public PCFState PcfState
    {
        get { return pcfState; }
        set
        {
            if (value == PCFState.NO_PCF)
            {
                Shader.EnableKeyword("NO_PCF");
                Shader.DisableKeyword("PCF_2X2");
                Shader.DisableKeyword("PCF_4X4");
                Shader.DisableKeyword("PCF_8X8");
                Shader.DisableKeyword("PCF_16X16");
            }
            else if (value == PCFState.PCF_2X2)
            {
                Shader.DisableKeyword("NO_PCF");
                Shader.EnableKeyword("PCF_2X2");
                Shader.DisableKeyword("PCF_4X4");
                Shader.DisableKeyword("PCF_8X8");
                Shader.DisableKeyword("PCF_16X16");
            }
            else if (value == PCFState.PCF_4X4)
            {
                Shader.DisableKeyword("NO_PCF");
                Shader.DisableKeyword("PCF_2X2");
                Shader.EnableKeyword("PCF_4X4");
                Shader.DisableKeyword("PCF_8X8");
                Shader.DisableKeyword("PCF_16X16");
            }
            else if (value == PCFState.PCF_8X8)
            {
                Shader.DisableKeyword("NO_PCF");
                Shader.DisableKeyword("PCF_2X2");
                Shader.DisableKeyword("PCF_4X4");
                Shader.EnableKeyword("PCF_8X8");
                Shader.DisableKeyword("PCF_16X16");
            }
            else if (value == PCFState.PCF_16X16)
            {
                Shader.DisableKeyword("NO_PCF");
                Shader.DisableKeyword("PCF_2X2");
                Shader.DisableKeyword("PCF_4X4");
                Shader.DisableKeyword("PCF_8X8");
                Shader.EnableKeyword("PCF_16X16");
            }
            pcfState = value;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        Init();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Init();
    }

    void Update()
    {
        DrawShadow();
    }

    void AWake()
    {
        
    }

//    private Camera viewCamera;
    private Camera lightCamera;
    private Camera spotCamera;
    private RenderTexture shadowMap;
    private RenderTexture SpotshadowMap;
    public float shadowMapSize = 2048;
    public GameObject DirLightGameObject;
    public GameObject SpotLightGameObject;

    private void CreateLightCamera()
    {
        GameObject goLightCamera = new GameObject("DirLight Camera");
        lightCamera = goLightCamera.AddComponent<Camera>();
        lightCamera.transform.SetParent(DirLightGameObject.transform);
        lightCamera.backgroundColor = Color.black;
        lightCamera.transform.localPosition = Vector3.zero;
        lightCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
        lightCamera.transform.localScale = Vector3.one;
        lightCamera.clearFlags = CameraClearFlags.SolidColor;
        lightCamera.orthographic = true;
        lightCamera.enabled = true;
        lightCamera.orthographicSize = 10f;
        lightCamera.nearClipPlane = 0.3f;
        lightCamera.farClipPlane = 50;

        GameObject goSpotCamera = new GameObject("SpotLight Camera");
        spotCamera = goSpotCamera.AddComponent<Camera>();
        spotCamera.transform.SetParent(SpotLightGameObject.transform);
        spotCamera.transform.localPosition = Vector3.zero;
        spotCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
        spotCamera.transform.localScale = Vector3.one;
        spotCamera.clearFlags = CameraClearFlags.SolidColor;
        spotCamera.nearClipPlane = 0.3f;
        spotCamera.farClipPlane = 50f;
        spotCamera.enabled = true;
        spotCamera.backgroundColor = Color.black;
    }

    private void Init()
    { 
        shadowMap = new RenderTexture((int)shadowMapSize, (int)shadowMapSize, 24, RenderTextureFormat.Shadowmap);
        SpotshadowMap = new RenderTexture((int)shadowMapSize, (int)shadowMapSize, 24, RenderTextureFormat.Shadowmap);
        PcfState = pcfState;
        CreateLightCamera();
    }


    private void DrawShadow()
    {
        lightCamera.targetTexture = shadowMap;
        spotCamera.targetTexture = SpotshadowMap;
        Shader shader = Shader.Find("Shader/CastShadow");
       
        Shader.SetGlobalTexture("_gShadowMapTexture", lightCamera.targetTexture);
        Shader.SetGlobalTexture("_gSpotShadowMapTexture", spotCamera.targetTexture);
        
        lightCamera.SetReplacementShader(shader, "RenderType");
        spotCamera.SetReplacementShader(shader, "RenderType");
        
        lightCamera.cullingMask = 1 << 4;
        spotCamera.cullingMask = 1 << 4;
        MatrixToShader(lightCamera, "_gWorldToShadow");
        MatrixToShader(spotCamera, "_gSpotWorldToShadow");
    }

    private void UpdateCamearArgs(Camera ca)
    {
        ca.transform.localPosition = DirLightGameObject.transform.localPosition;
        ca.transform.localRotation = DirLightGameObject.transform.localRotation;
    }

    public void MatrixToShader(Camera lightCamera, string shadowName)
    {
        Matrix4x4 worldToView = lightCamera.worldToCameraMatrix;
        Matrix4x4 projection = GL.GetGPUProjectionMatrix(lightCamera.projectionMatrix, false);
        //Debug.Log("WorldToView" + worldToView + "Projection" + projection);
        Shader.SetGlobalMatrix(shadowName, projection * worldToView);
    }

    public static GameObject getScenceAABBBounds(List<GameObject> go, GameObject AABBBounds)
    {
        Vector3 center = Vector3.zero;
        Vector3 size = Vector3.zero;
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        for (int i = 0; i < go.Count; ++i)
        {
            min = ComputeVectorMin(go[i].transform.position - go[i].transform.localScale / 2, min);
            max = ComputeVectorMax(go[i].transform.position - go[i].transform.localScale / 2, max);
            center = new Vector3((min.x + max.x) / 2, (min.y + max.y) / 2, (min.z + max.z) / 2);
        }

        float xSize = max.x - min.x;
        float ySize = max.y - min.y;
        float zSize = max.z - min.z;
        size = new Vector3(xSize, ySize, zSize);

        Renderer[] renders = new Renderer[go.Count];
        for (int i = 0; i > go.Count; ++i)
            renders[i] = go[i].GetComponent<Renderer>();
        Bounds bounds = new Bounds(center, size);
        foreach (Renderer child in renders)
            bounds.Encapsulate(child.bounds);
        AABBBounds.transform.position = bounds.center;
        AABBBounds.transform.localScale = bounds.size;

        if (AABBBounds.GetComponent<BoxCollider>() == null)
            AABBBounds.AddComponent<BoxCollider>();
        return AABBBounds;
    }

    public static Vector3 ComputeVectorMin(Vector3 v1, Vector3 min)
    {
        Vector3 temp;
        temp.x = v1.x < min.x ? v1.x : min.x;
        temp.y = v1.y < min.y ? v1.y : min.y;
        temp.z = v1.z < min.z ? v1.z : min.z;
        return temp;
    }

    public static Vector3 ComputeVectorMax(Vector3 v1, Vector3 max)
    {
        Vector3 temp;
        temp.x = v1.x > max.x ? v1.x : max.x;
        temp.y = v1.y > max.y ? v1.y : max.y;
        temp.z = v1.z > max.z ? v1.z : max.z;
        return temp;
    }

    public static void OrthogonalLightCameraSelfAdaption(Camera lightCamera, GameObject light, List<Vector3> vertexs,
        float shadowResolution, float farClipPlane)
    {
        lightCamera.orthographicSize = 1;
        lightCamera.ResetProjectionMatrix();
        List<Vector3> lightSpaceVertexs = new List<Vector3>();
        Vector3 minValue = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 maxValue = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (int i = 0; i < vertexs.Count; ++i)
        {
            lightSpaceVertexs.Add(light.transform.worldToLocalMatrix.MultiplyPoint(vertexs[i]));
            minValue = ComputeVectorMin(lightSpaceVertexs[i], minValue);
            maxValue = ComputeVectorMax(lightSpaceVertexs[i], maxValue);
        }

        float radius = ComputeSphereRadius(lightSpaceVertexs, lightSpaceVertexs.Count);
        Vector3 controlRotatingShadowWithoutJitterValue =
            new Vector3(radius + radius * 0.45f, radius + radius * 0.45f, radius + radius * 0.45f);
        Vector3 CRSWJV = controlRotatingShadowWithoutJitterValue;
        float f = 1f / (float) shadowResolution;
        Shader.SetGlobalFloat("_shadowMapSize", f);
        Vector3 v = new Vector3(f, f, f);
        
        Vector3 eachPixelIsInWorldSpaceUnitSize = Vector3.Scale(CRSWJV * 2 , v);

        minValue.x /= eachPixelIsInWorldSpaceUnitSize.x;
        minValue.y /= eachPixelIsInWorldSpaceUnitSize.y;
        minValue.z /= eachPixelIsInWorldSpaceUnitSize.z;
        minValue = ComputeFloor(CRSWJV);
        CRSWJV = Vector3.Scale(CRSWJV, eachPixelIsInWorldSpaceUnitSize);
        lightCamera.transform.localPosition = new Vector3((minValue.x + maxValue.x) / 2, (minValue.y + maxValue.y) / 2, (minValue.z + maxValue.z) / 2);
        lightCamera.nearClipPlane = -farClipPlane * 2f;
        lightCamera.farClipPlane = farClipPlane * 2f;

        Vector3 scale;
        scale.x = 2f / CRSWJV.x;
        Matrix4x4 croppedMatrix = Matrix4x4.identity;
        croppedMatrix.m00 = scale.x;
        croppedMatrix.m11 = scale.x;
        croppedMatrix.m22 = 1;
        Matrix4x4 projectionMatrix = lightCamera.projectionMatrix;
        lightCamera.projectionMatrix = croppedMatrix * projectionMatrix;
    }

    public static float ComputeSphereRadius(List<Vector3> vertexs, int vertex_Count)
    {
        Vector3 total = Vector3.zero;
        Vector3 center = Vector3.zero;
        float[] distance = new float[vertex_Count];
        for (int i = 0; i < vertex_Count; ++i) total += vertexs[i];
        center = total / vertex_Count;
        for (int i = 0; i < vertex_Count; ++i)
        {
            distance[i] = Vector3.Distance(center, vertexs[i]);
        }

        return Sort(distance)[0];
    }

    public static float[] Sort(float[] f)
    {
        for (int i = 0; i < f.Length; i++)
        {
            for (int j = 0; j < f.Length - 1; j++)
            {
                if (f[j] < f[j + 1])
                {
                    float temp = f[j];
                    f[j] = f[j + 1];
                    f[j + 1] = temp;
                }
            }
        }
        return f;
    }

    public static Vector3 ComputeFloor(Vector3 vector3)
    {
        Vector3 temp;
        temp.x = (float) Mathf.Floor(vector3.x);
        temp.y = (float) Mathf.Floor(vector3.y);
        temp.z = (float) Mathf.Floor(vector3.z);
        return temp;
    }

}