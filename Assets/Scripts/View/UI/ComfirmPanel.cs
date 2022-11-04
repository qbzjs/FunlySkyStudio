using System;
using UnityEngine;
using UnityEngine.UI;

public class ComfirmPanel:BasePanel<ComfirmPanel>
{
    public Text TitleText;
    public Button DontBtn;
    public Button CancelBtn;
    public Button SaveBtn;
    public GameObject BtnText;
    public Animator Anim;
    public Image MaskImage;
    public Action OnDontClick;
    public Action OnSaveClick;
    
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        DontBtn.onClick.AddListener(()=>
        {
            Hide();
            OnDontClick?.Invoke();
        });
        CancelBtn.onClick.AddListener(Hide);
        SaveBtn.onClick.AddListener(()=>
        {
            OnSaveClick?.Invoke();
        });
    }

    public static void SetAnim(bool isPlay)
    {
        if (Instance != null)
        {
            Instance.BtnText.SetActive(!isPlay);
            Instance.Anim.gameObject.SetActive(isPlay);
            if (isPlay)
            {
                Instance.Anim.Play("SaveAnimtion", 0, 0);
            }
        }
    }

    public void SetText(string val)
    {
        LocalizationConManager.Inst.SetLocalizedContent(TitleText, val);
    }

    public void SetMaskColor(Color color)
    {
        MaskImage.color = color;
    }
}