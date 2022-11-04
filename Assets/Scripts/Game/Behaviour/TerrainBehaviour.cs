using UnityEngine;

public class TerrainBehaviour:NodeBaseBehaviour
{
    private Renderer terRender;
    public override void OnInitByCreate()
    {
        terRender = this.GetComponentInChildren<Renderer>();
    }

    public void SetMat(Material mat)
    {
#if UNITY_ANDROID
        var tag = mat.GetTag("RenderType",false);
        string shaderStr = "Custom/CustomDiffuse"; 
        if(tag == "Transparent")shaderStr = "Custom/CustomDiffuseAlpha";
        Material matTemp = new Material(Shader.Find(shaderStr));
        matTemp.name = mat.name;
        matTemp.SetTexture("_MainTex", mat.mainTexture);
        matTemp.SetColor("_Color", mat.GetColor("_Color"));
        matTemp.SetTexture("_BumpMap", mat.GetTexture("_BumpMap"));
        matTemp.SetTextureScale("_MainTex",mat.GetTextureScale("_MainTex"));
        mat = matTemp;
#endif
        terRender.material = mat;
    }
    public void SetUGCMat(Texture tex)
    {
        Material matTemp;
#if UNITY_ANDROID
        string shaderStr = "Custom/CustomDiffuse";
        matTemp = new Material(Shader.Find(shaderStr));
        matTemp.SetTexture("_MainTex", tex);
#else
        matTemp = ResManager.Inst.LoadRes<Material>(GameConsts.TerrainMatPath + "Ground_0");
        matTemp.SetTexture("_MainTex", tex);
#endif
        terRender.material = matTemp;
    }
    public void ExpandTextureScale(int param){
        var scale = terRender.material.GetTextureScale("_MainTex");
        terRender.material.SetTextureScale("_MainTex", scale * param);
    }

    public void SetTexture(Texture tex)
    {
        terRender.material.SetTexture("_MainTex", tex);
    }

    public void SetColor(Color color)
    {
        var curColor = terRender.material.GetColor("_Color");
        color.a = curColor.a;
        terRender.material.SetColor("_Color", color);
    }
}