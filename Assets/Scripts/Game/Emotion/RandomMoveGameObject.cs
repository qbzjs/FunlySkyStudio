using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMoveGameObject : MonoBehaviour
{
    public bool isChangeTexture;


    public void ChangeTexture(string name, int id)
    {

        Texture texture = (Texture)Resources.Load("Texture/Emo/" + name + "/" + id);
        MeshRenderer render = GetComponent<MeshRenderer>();
        if (render != null)
        {
            render.material.mainTexture = texture;
            return;
        }

        SkinnedMeshRenderer skinnedRender = GetComponent<SkinnedMeshRenderer>();
        LoggerUtils.Log(skinnedRender);
        if (skinnedRender != null)
        {
            skinnedRender.material.mainTexture = texture;
            return;
        }

    }
}
