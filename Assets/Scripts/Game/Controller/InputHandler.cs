using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler
{
    public virtual void OnTouchBegin(Touch touch) { }

    public virtual void OnShortTouchEnd(Touch touch) { }

    public virtual void OnTouchStay(Touch touch) { }

    public virtual void OnLongTouchEnd(Touch touch) { }

    public virtual void OnMovementTouchStay(Touch touch) { }

    public virtual void OnMultipleTouchesBegin(Touch[] touches) { }

    public virtual void OnMultipleTouchesStay(Touch[] touches) { }

    public virtual bool OnDragJoyStick(Touch touche)
    {
        return false;
    }
}
