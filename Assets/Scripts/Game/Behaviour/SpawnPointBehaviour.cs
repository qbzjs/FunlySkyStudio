using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class SpawnPointBehaviour : NodeBaseBehaviour
{
    public TextMeshPro numText;
    public int id;
    public int hpValue;
    public Animator anim;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        numText = transform.GetComponentInChildren<TextMeshPro>();
        anim = transform.GetComponent<Animator>();
    }


    public void SetNumText(int num)
    {
        numText.text = num + "";
        gameObject.name = "Spanw_" + num;
        id = num;
    }
}
