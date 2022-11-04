using UnityEngine;
using UnityEngine.UI;
public class AlarmTips : BasePanel<AlarmTips>
{
    public GameObject TipsBg;

    public Text tipsText;

    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
    }

    public static void ShowTips(string msg) {
        Show();
        Instance.LocalShow(msg);
    }

    private void LocalShow(string msg)
    {
        CancelInvoke("LocalHide");
        tipsText.text = msg;
        Invoke("LocalHide",3.5f);
    }

    private void LocalHide() {
        Hide();
    }

}
