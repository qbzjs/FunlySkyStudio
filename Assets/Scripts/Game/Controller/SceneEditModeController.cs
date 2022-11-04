using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SceneEditModeController : EditModeController
{
    protected override void SetSelectTarget(SceneEntity entity)
    {
        base.SetSelectTarget(entity);
        ShowTransformAnimPanel(entity);
    }

    private void ShowTransformAnimPanel(SceneEntity entity)
    {
        if (MovePathManager.Inst.IsSelectMovePoint(entity))
        {
            return;
        }
        MovePathManager.Inst.CloseAndSave();
        var gameComp = entity.Get<GameObjectComponent>();
        if (gameComp.modId == (int)GameResType.BornPoint
            || gameComp.modId == (int)GameResType.NudeModel
            || gameComp.modId == (int)GameResType.EditMovePoint
            || gameComp.modId == (int)GameResType.TrapSpawn
            || gameComp.modId == (int)GameResType.SteeringWheel
            || gameComp.modId == (int)GameResType.PVPWaitArea
            || gameComp.modId == (int)GameResType.Parachute
            || gameComp.modId == (int)GameResType.FishingHook
            || gameComp.modId == (int)GameResType.FishingRod
            || gameComp.modId==(int)GameResType.SlidePipe
            )
        {
            return;
        }
        if (gameComp.modId == (int) GameResType.CombEmpty)
        {
            if (gameComp.bindGo.GetComponentInChildren<SteeringWheelBehaviour>())
            {
                return;
            }
        }
        if (entity.HasComponent<ParachuteBagComponent>())
        {
            return;
        }
        if (entity.HasComponent<SeesawComponent>())
        {
            return;
        }
        if (VIPZoneManager.Inst.IsVIPZoneComponent(entity))
        {
            return;
        }
        if (entity.HasComponent<SwingComponent>())
        {
            return;
        }

        if(entity.HasComponent<SlideItemComponent>() || entity.HasComponent<SlidePipeComponent>())
        {
            return;
        }
        PropertiesPanel.Show();
        PropertiesPanel.Instance.SetEntity(entity);
        MovePathManager.Inst.BindEntity(entity);
    }

    public override void DisableAllPanel()
    {
        base.DisableAllPanel();
        PropertiesPanel.Hide();
    }
}
