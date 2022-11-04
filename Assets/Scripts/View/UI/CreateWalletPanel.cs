using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

public class ImportWalletReq
{
    public string mnomonic;
    public int shouldCheck;
}
public class ImportWalletRes
{
    public int isSuccess;
    public string walletAddress;
    public string rmsg;
}
public class CreateWalletRes
{
    public int isSuccess;
    public string walletAddress;
}

public class CreateWalletPanel : BasePanel<CreateWalletPanel>
{
    public Button closeBtn;
    public GameObject createTipsGo;
    public GameObject importTipsGo;
    public GameObject importingGo;
    public GameObject createMask;
    public Button createBtn;
    public Button importWalletBtn;
    public Button confirmBtn;
    public Button cancelBtn;
    public Button inputBtn;
    public Text inputTxt;
    public Button pasteBtn;
    public Button importBtn;
    public GameObject createSuc;
    public Button okBtn;
    public Animator creatAnim;
    public Animator importAnim;

    public Action createSucAct;
    public Action hidePanelAct;
    private string enterText = "Enter your BUD wallet secret recovery phrase...";
    private string EnterText { get { return LocalizationConManager.Inst.GetLocalizedText(enterText); } }


    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        closeBtn.onClick.AddListener(OnCloseBtnClick);
        createBtn.onClick.AddListener(OnCreateBtnClick);
        importWalletBtn.onClick.AddListener(OnImportWalletBtnClick);
        confirmBtn.onClick.AddListener(OnConfirmBtnClick);
        cancelBtn.onClick.AddListener(OnCancelBtnClisck);
        inputBtn.onClick.AddListener(OnInputBtnClick);
        pasteBtn.onClick.AddListener(OnPasteBtnClick);
        importBtn.onClick.AddListener(OnImportBtnClick);
        okBtn.onClick.AddListener(OnOkBtnClick);
    }
    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        createTipsGo.SetActive(true);
        importTipsGo.SetActive(false);
        importingGo.SetActive(false);
        createSuc.SetActive(false);
        createMask.SetActive(false);
        SetInputText(EnterText);

    }
    public void SetAction(Action sucAct, Action hideAct)
    {
        this.createSucAct = sucAct;
        this.hidePanelAct = hideAct;
    }
    /// <summary>
    /// 关闭按钮
    /// </summary>
    public void OnCloseBtnClick()
    {
        HidePanel();
    }
    /// <summary>
    /// 创建钱包Loading
    /// </summary>
    /// <param name="isVisiable"></param>
    private void ShowCreateLoadAnim(bool isVisiable)
    {
        creatAnim.gameObject.SetActive(isVisiable);
        createMask.SetActive(isVisiable);
        createBtn.GetComponentInChildren<Text>(true).gameObject.SetActive(!isVisiable);
    }
    /// <summary>
    /// 创建钱包
    /// </summary>
    private void OnCreateBtnClick()
    {
        MobileInterface.Instance.AddClientRespose(MobileInterface.createWalletInterface, (response) => BindWalletUtils.OnBindWalletResponse(response, OnCreateWalletSuccess, OnCreateWalletFail));
        MobileInterface.Instance.MobileSendMsgBridge(MobileInterface.createWalletInterface, "");
        ShowCreateLoadAnim(true);//显示loading
    }
    /// <summary>
    /// 创建钱包成功
    /// </summary>
    private void OnCreateWalletSuccess()
    {
        createSuc.gameObject.SetActive(true);
        createTipsGo.SetActive(false);
        ShowCreateLoadAnim(false);
        MobileInterface.Instance.DelClientResponse(MobileInterface.createWalletInterface);
    }
    /// <summary>
    /// 创建钱包失败
    /// </summary>
    private void OnCreateWalletFail()
    {
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
        ShowCreateLoadAnim(false);
        MobileInterface.Instance.DelClientResponse(MobileInterface.createWalletInterface);
    }
    /// <summary>
    /// 导入钱包Click
    /// </summary>
    private void OnImportWalletBtnClick()
    {
        createTipsGo.SetActive(false);
        importTipsGo.SetActive(true);
    }
    /// <summary>
    /// 确认导入钱包Click
    /// </summary>
    private void OnConfirmBtnClick()
    {
        importTipsGo.SetActive(false);
        importingGo.SetActive(true);
        ShowImportBtn(false);
    }
    /// <summary>
    /// 取消导入Click
    /// </summary>
    private void OnCancelBtnClisck()
    {
        HidePanel();
    }
    /// <summary>
    /// 设置输入框内容
    /// </summary>
    /// <param name="str"></param>
    private void SetInputText(string str)
    {
        inputTxt.text = str;
    }
    /// <summary>
    /// 粘贴按钮Click
    /// </summary>
    private void OnPasteBtnClick()
    {
        var str = GUIUtility.systemCopyBuffer;
        SetInputText(str);//设置输入文案
        ShowImportBtn(true);//显示import按钮
        SetImpurtBtnState(0);//设置import按钮状态
        CheckInputLegal(str);//检查助记词合法
    }
    /// <summary>
    /// 粘贴按钮与导入按钮互斥
    /// </summary>
    /// <param name="isVisiable"></param>
    private void ShowImportBtn(bool isVisiable)
    {
        importBtn.gameObject.SetActive(isVisiable);
        pasteBtn.gameObject.SetActive(!isVisiable);
    }
    /// <summary>
    /// 点击钱包地址输入框
    /// </summary>
    private void OnInputBtnClick()
    {
        var str = inputTxt.text == EnterText ? "" : inputTxt.text;
        KeyBoardInfo keyBoardInfo = new KeyBoardInfo
        {
            type = 0,
            placeHolder = "",
            inputMode = 0,
            maxLength = 200,
            inputFlag = 0,
            lengthTips = LocalizationConManager.Inst.GetLocalizedText("Oops! Exceed limit:("),
            defaultText = str,
            returnKeyType = (int)ReturnType.Done
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.showKeyboard, OnInputDone);
        MobileInterface.Instance.ShowKeyboard(JsonUtility.ToJson(keyBoardInfo));
    }
    /// <summary>
    /// 输入完成
    /// </summary>
    /// <param name="str"></param>
    private void OnInputDone(string str)
    {
        SetInputText(str);
        SetImpurtBtnState(0);
        CheckInputLegal(str);
        ShowImportBtn(true);
        MobileInterface.Instance.DelClientResponse(MobileInterface.showKeyboard);
    }
    /// <summary>
    /// 检测输入是否合法
    /// </summary>
    private void CheckInputLegal(string str)
    {
        MobileInterface.Instance.AddClientRespose(MobileInterface.importWalletInterface, CheckLegalCallBack);
        ImportWalletReq importWalletReq = new ImportWalletReq
        {
            mnomonic = str,
            shouldCheck = 0
        };
        MobileInterface.Instance.MobileSendMsgBridge(MobileInterface.importWalletInterface, JsonConvert.SerializeObject(importWalletReq));
    }
    /// <summary>
    /// 输入合法检测回调
    /// </summary>
    /// <param name="response"></param>
    private void CheckLegalCallBack(string response)
    {
        MobileInterface.Instance.DelClientResponse(MobileInterface.importWalletInterface);
        var importWalletRes = JsonConvert.DeserializeObject<ImportWalletRes>(response);
        SetImpurtBtnState(importWalletRes.isSuccess);
    }
    /// <summary>
    /// 设置Import按钮状态(输入不合法时变灰)
    /// /// </summary>
    /// <param name="isActive"></param>
    private void SetImpurtBtnState(int isActive)
    {
        importBtn.GetComponentInChildren<Text>().color = isActive == 1 ? new Color32(0, 0, 0, 255) : new Color32(158, 158, 158, 255);
        importBtn.interactable = isActive == 1;
    }
    /// <summary>
    /// Import按钮点击事件
    /// </summary>
    private void OnImportBtnClick()
    {
        ShowImportLoadAnim(true);
        ImportWalletReq importWalletReq = new ImportWalletReq
        {
            mnomonic = inputTxt.text,
            shouldCheck = 1
        };
        MobileInterface.Instance.AddClientRespose(MobileInterface.importWalletInterface, ImportWalletCallBack);
        MobileInterface.Instance.MobileSendMsgBridge(MobileInterface.importWalletInterface, JsonConvert.SerializeObject(importWalletReq));
    }
    /// <summary>
    /// 导入钱包回调
    /// </summary>
    private void ImportWalletCallBack(string response)
    {
        var importWalletRes = JsonConvert.DeserializeObject<ImportWalletRes>(response);
        if (importWalletRes.isSuccess == 1)
        {
            HttpUtils.tokenInfo.walletAddress = importWalletRes.walletAddress;
            importingGo.gameObject.SetActive(false);
            createSuc.gameObject.SetActive(true);
        }
        else
        {
            TipPanel.ShowToast(importWalletRes.rmsg);
        }
        ShowImportLoadAnim(false);
        MobileInterface.Instance.DelClientResponse(MobileInterface.importWalletInterface);

    }
    /// <summary>
    /// 导入钱包Loading
    /// </summary>
    /// <param name="isVisiable"></param>
    private void ShowImportLoadAnim(bool isVisiable)
    {
        importAnim.gameObject.SetActive(isVisiable);
        createMask.SetActive(isVisiable);
        importBtn.GetComponentInChildren<Text>(true).gameObject.SetActive(!isVisiable);
    }
    /// <summary>
    /// 创建完成点击OK
    /// </summary>
    private void OnOkBtnClick()
    {
        HidePanel();
        createSucAct?.Invoke();
    }
    public void HidePanel()
    {
        Hide();
        hidePanelAct?.Invoke();
    }
}
