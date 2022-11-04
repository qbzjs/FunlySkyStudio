using SavingData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PropEditModePanel:EditModePanel<PropEditModePanel>
{
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        PlayBtn.gameObject.SetActive(false);
    }

    protected override void SaveMapCover(Action<string> success, Action<string> fail)
    {
        base.SaveMapCover(success,fail);
        GameManager.Inst.gameMapInfo.dataType = (int)MapSaveType.Prop;
        var isHasSetCover = GameManager.Inst.gameMapInfo.mapStatus.isSetCover;
        if (isHasSetCover)
        {
            EditModeController.SaveMapJson((fileName) =>
            {
                OnSaveMapJsonSuccess(fileName,success,fail);
            }, fail);
        }
        else
        {
            EditModeController.SaveResMapCover((fileName) =>
            {
                OnSaveMapCoverSuccess(fileName,success,fail);
            }, fail);
        }
    }

    private void OnSaveMapCoverSuccess(string fileName,Action<string> success,Action<string> fail)
    {
        EditModeController.SaveMapJson((fileName) =>
        {
            OnSaveMapJsonSuccess(fileName, success, fail);
        }, fail);
    }

    private void OnSaveMapJsonSuccess(string fileName, Action<string> success,Action<string> fail)
    {
        EditModeController.SavePropJson((fileName) =>
        {
            OnSavePropsSuccess(fileName, success,fail);
        }, fail);
    }


    private void OnSavePropsSuccess(string fileName, Action<string> success, Action<string> fail)
    {
        var optType = string.IsNullOrEmpty(GameManager.Inst.gameMapInfo.mapId) ? OperationType.ADD : OperationType.UPDATE;
        DataUtils.SetMapInfoLocal(optType);
        DataUtils.SetConfigLocal(CoverType.PNG);
        success?.Invoke("Save Success");
    }

    protected override void OnCloseClick()
    {
        base.OnCloseClick();
        ComfirmPanel.Instance.SetText("Do you want to save this prop?");
    }
}