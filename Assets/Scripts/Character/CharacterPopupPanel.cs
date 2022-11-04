using UnityEngine.UI;

/// <summary>
/// Author:WenJia
/// Description:Avatar 删除搭配弹窗
/// Date: 2022/5/6 17:22:39
/// </summary>


public class CharacterPopupPanel : BasePanel<CharacterPopupPanel>
{
    public Button CancelBtn, DelBtn, MaskBtn;
    private RoleMatchItem item;

    private void Start()
    {
        CancelBtn.onClick.AddListener(OnCancelBtnClick);
        MaskBtn.onClick.AddListener(OnCancelBtnClick);
        DelBtn.onClick.AddListener(OnDeleteBtnClick);
    }

    public void OnCancelBtnClick()
    {
        Hide();
    }

    public void OnDeleteBtnClick()
    {
        CharacterTipDialogPanel.Show();
        CharacterTipDialogPanel.Instance.SetTitle("Are you sure you want to delete this outfit?", "Delete");
        CharacterTipDialogPanel.Instance.RightBtnClickAct = () => { item.SendCancelMatchCollection(); };
        Hide();
    }

    public void SetItemData(RoleMatchItem matchItem)
    {
        item = matchItem;
    }
}
