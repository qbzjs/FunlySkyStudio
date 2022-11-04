using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ElementButtonManager : CInstance<ElementButtonManager>
{
    private bool _isCanClickElement = false;
    public bool isCanClickElement
    {
        get
        {
            return _isCanClickElement;
        }
        set
        {
            UGCClothesInputReceiver.Inst.enabled = !value;
            _isCanClickElement = value;
        }
    }

    public void OnClick(BehaviourType type, ElementBaseBehaviour behav = null)
    {
        if (!isCanClickElement)
        {
            return;
        }
        switch (type)
        {
            case BehaviourType.Photo:
            case BehaviourType.Text:
                if (behav != null)
                {
                    behav.OnClick(behav.gameObject);
                }
                break;
            case BehaviourType.noneArea:
                if (TransformInteractorController.Inst.interActor)
                {
                    TransformInteractorController.Inst.interActor.ResetInfo();
                }
                break;
        }
    }
}
