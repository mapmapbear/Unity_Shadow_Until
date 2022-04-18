using UnityEngine;

public class SpotLight : MonoBehaviour
{
    [SetProperty("Range")]
    public float _range = 1000;                                                             //聚光灯范围
    [RangeAndSetProperty("SpotAngle", 1, 179)]
    public float _spotAngle = 60;                                                           //聚光灯角度
    public Color _spotColor = Color.red;                                                  //聚光灯颜色
    [SetProperty("Intensity")]
    public float _intensity = 1;                                                            //聚光灯强度
    [RangeAndSetProperty("Atten", -20, 20)]
    public float _atten = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private Vector3 pos;

    private Vector3 rot;
    // Update is called once per frame
    void Update()
    {
        pos = this.gameObject.transform.position;
        rot = -this.gameObject.transform.forward;
        Shader.SetGlobalFloat("_SpotRange", 1000);                                        //把聚光灯范围传入Shader
        Shader.SetGlobalFloat("_SpotAngle", _spotAngle);                                //把聚光灯角度传入Shader
        Shader.SetGlobalColor("_SpotColor", _spotColor);                                //把聚光灯颜色传入Shader
        Shader.SetGlobalFloat("_SpotIntensity", _intensity);                                //把聚光灯强度传入Shader
        Shader.SetGlobalVector("_SpotLightPos", new Vector4(pos.x, pos.y, pos.z, 1));   //把聚光灯位置传入Shader
        Shader.SetGlobalVector("_SpotLightRot", new Vector4(rot.x, rot.y, rot.z, 1));   //把聚光灯光方向传入Shader
        Shader.SetGlobalFloat("_Atten", 1);
    }
}
