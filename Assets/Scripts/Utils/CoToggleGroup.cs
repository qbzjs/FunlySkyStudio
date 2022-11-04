using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoToggleGroup : MonoBehaviour
{
    public CoToggle current;

    public void TurnOn(CoToggle target)
    {
        if(target != current)
        {
            if(current) current.IsOn = false;
            current = target;
        }
    }
}
