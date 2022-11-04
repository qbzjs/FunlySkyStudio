using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlyPermisionPanel : InfoPanel<FlyPermisionPanel>
{
    public Toggle ToggleFlyPermision;

    public void Start()
    {
        ToggleFlyPermision.onValueChanged.AddListener(OnToggleClick);

        if (SceneBuilder.Inst.CanFlyEntity.Get<CanFlyComponent>().canFly != 1)
        {
            ToggleFlyPermision.isOn = true;
        }
    }

    private void OnToggleClick(bool isToggle)
    {
        SceneBuilder.Inst.CanFlyEntity.Get<CanFlyComponent>().canFly = isToggle ? 0 : 1;
    }
}
