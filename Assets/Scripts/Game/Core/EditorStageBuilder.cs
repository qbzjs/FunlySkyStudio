using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Scripts.Game.Core;
using UnityEngine;
//Edit-Mode GameObject Save Position
public class EditorStageBuilder : InstMonoBehaviour<EditorStageBuilder>
{
    private List<GameObject> eGameObjects = new List<GameObject>();
    public GameObject CreateGameObject(Vector3 pos,GameObject prefab)
    {
        var temp = SceneBuilder.Inst.CreateMovementPoint(pos, prefab,this.transform);
        eGameObjects.Add(temp.gameObject);
        return temp.gameObject;
    }

    public void HideAllGameObject()
    {
        eGameObjects.ForEach(x=>x.SetActive(false));
    }
}