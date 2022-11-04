using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class that controls the Transform Interactors and provide settings to customize the interactors
/// </summary>
public class TransformInteractorController : CInstance<TransformInteractorController>
{
    public GameObject boundingRectanglePrefab;

    public Interactor interActor;
    public Interactor GetInterActor()
    {
        if (boundingRectanglePrefab == null)
        {
            boundingRectanglePrefab = Resources.Load("Prefabs/UI/Panel/Interactor") as GameObject;
        }

        if (interActor == null)
        {
            interActor = GameObject.Instantiate(boundingRectanglePrefab).GetComponent<Interactor>();
            interActor.transform.parent = MainUGCResPanel.Inst.elementPanel.parent;
            interActor.transform.localScale = Vector3.one;
            interActor.transform.localPosition = Vector3.one;
            interActor.transform.eulerAngles = Vector3.zero;
            interActor.SetUndoRedoAct = MainUGCResPanel.Inst.UpdateUndoBtnView;
            interActor.Init();
        }
        return interActor;
    }


}
