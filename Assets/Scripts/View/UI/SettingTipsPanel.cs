
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class SettingTipsPanel : BasePanel<SettingTipsPanel>
{
    private Transform back;
    private Text content;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        back = transform.Find("Back");
        transform.Find("Mask").GetComponent<Button>().onClick.AddListener(() =>
        {
            Hide();
        });
        content = transform.Find("Back/Text").GetComponent<Text>();
    }

    public void AdjustTipsPosition(string text,Transform anchor)
    {
        LocalizationConManager.Inst.SetLocalizedContent(content, text);
        RectTransform rectTrans = GameObject.Find("Canvas").GetComponent<RectTransform>();
        Camera c = rectTrans.GetComponent<Canvas>().worldCamera;
        float factor = 1125f / Screen.height;
        Vector3 worldToScreenPoint = c.WorldToScreenPoint(anchor.position);
        Vector3 real = worldToScreenPoint * factor;
        back.GetComponent<RectTransform>().anchoredPosition = new Vector2(real.x,real.y)+Vector2.down*50;
    }
}
