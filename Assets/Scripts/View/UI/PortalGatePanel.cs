using Entitas;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class PortalGatePanel:InfoPanel<PortalGatePanel>
{
    public RawImage mapImage;
    public GameObject AddPanel;
    public GameObject HasPanel;
    public Button AddButton;
    public Button CloseBtn;
    public Text AddTipText;
    public Text BgNameText;
    private PortalGateData mInfo;
    private SceneEntity pEntity;
    private PortalGateBehaviour pBehv;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        AddButton.onClick.AddListener(OnAddClick);
        CloseBtn.onClick.AddListener(OnCloseClick);
        mapImage.gameObject.SetActive(false);
    }

    public void SetEntity(SceneEntity entity)
    {
        pEntity = entity;
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        pBehv = bindGo.GetComponent<PortalGateBehaviour>();
        var comp = entity.Get<PortalGateComponent>();
        ShowPanel(string.IsNullOrEmpty(comp.mapName));
        if (!string.IsNullOrEmpty(comp.mapName))
        {
            BgNameText.text = comp.mapName;
            StartCoroutine(ResManager.Inst.GetTexture(comp.pngUrl, GetTexSuccess, GetTexFail));
        }
    }

    private void ShowPanel(bool isAdd)
    {
        AddPanel.SetActive(isAdd);
        HasPanel.SetActive(!isAdd);
    }

    private void OnAddClick()
    {
        MobileInterface.Instance.AddClientRespose(MobileInterface.skipNative, GetMapInfo);
        MobileInterface.Instance.SkipNative(2);
        //OnTest();
    }

    private void OnTest()
    {
        PortalGateData data = new PortalGateData();
        data.mapId = "1231232";
        data.mapName = "123123";
        data.pngUrl = "https://cdn.joinbudapp.com/UgcImage/500x330/1450306028838199296_1636648728.jpg";
        GetMapInfo(JsonConvert.SerializeObject(data));
    }

    private void GetMapInfo(string content)
    {
        ShowPanel(false);
        mapImage.gameObject.SetActive(false);
        mInfo = JsonConvert.DeserializeObject<PortalGateData>(content);
        BgNameText.text = mInfo.mapName;
        pEntity.Get<PortalGateComponent>().mapName = mInfo.mapName;
        pEntity.Get<PortalGateComponent>().diyMapId = mInfo.mapId;
        pEntity.Get<PortalGateComponent>().pngUrl = mInfo.pngUrl;
        StartCoroutine(ResManager.Inst.GetTexture(mInfo.pngUrl, GetTexSuccess, GetTexFail));
    }

    private void GetTexSuccess(Texture2D tex)
    {
        mapImage.gameObject.SetActive(true);
        mapImage.texture = tex;
    }

    private void GetTexFail()
    {
        TipPanel.ShowToast("Oops! Something went wrong. Please try again!");
    }

    private void OnCloseClick()
    {
        ShowPanel(true);
        var comp = pEntity.Get<PortalGateComponent>();
        comp.mapName = string.Empty;
        comp.diyMapId = string.Empty;
        comp.mapName = string.Empty;
        mapImage.texture = null;
        mapImage.gameObject.SetActive(false);
    }
}