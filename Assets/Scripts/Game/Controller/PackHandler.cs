using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PackHandler : InputHandler
{
    public Action<Touch> OnSelectTarget;
    public override void OnShortTouchEnd(Touch touch)
    {
        OnSelectTarget?.Invoke(touch);
    }

}
