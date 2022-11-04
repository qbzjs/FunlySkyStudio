using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DowntownLoadingPanel : BasePanel<DowntownLoadingPanel>
{
    public Text GemText;
    public Text LoadingTips;
    private BudTimer _loadingTimer;

    List<string> LoadingTipsList = new List<string> {
        "10 precious ice gems are scattered around the Great Snowfield",
        "The ice beams scattered across the Great Snowfield will take you to the ice crystal gem's hideout",
        "The road to the ice gems is full of difficulties, use your wisdom and courage to get them!",
        "Precious rewards will be unlocked after collecting all ice gems!"
    };

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
    }

    private void OnBecameVisible()
    {
        TimerManager.Inst.Stop(_loadingTimer);
        LoadingTips.text = LoadingTipsList[Random.Range(0, LoadingTipsList.Count)];
        _loadingTimer = TimerManager.Inst.RunOnce("LoadingTimer", 5f, () => {
            LoadingTips.text = LoadingTipsList[Random.Range(0, LoadingTipsList.Count)];
        });
        GemText.text = GlobalFieldController.CollectIceGem.ToString() + "/" + GlobalFieldController.MaxIceGem.ToString();
    }
}