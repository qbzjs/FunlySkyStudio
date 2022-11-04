using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirLightBehaviour : NodeBaseBehaviour
{
    private Light curLight;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        curLight = this.GetComponentInChildren<Light>();
    }

    public void SetIntensity(float inten)
    {
        curLight.intensity = inten;
    }


    public void SetAngleX(float angleX)
    {
        Vector3 angles = curLight.transform.rotation.eulerAngles;
        angles.x = angleX;
        curLight.transform.rotation = Quaternion.Euler(angles);
    }

    public void SetAngleY(float angleY)
    {
        Vector3 angles = curLight.transform.rotation.eulerAngles;
        angles.y = angleY;
        curLight.transform.rotation = Quaternion.Euler(angles);
    }

    public void SetColor(Color color)
    {
        curLight.color = color;
    }


}
