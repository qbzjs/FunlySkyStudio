using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResDetailPanel : MonoBehaviour
{
    public GameObject pgc;
    public Text pgcThemeName;
    public GameObject dc;
    

    public RawImage resCover;
    public Text resName;
    public Text resDesc;
    
    public Button btnExpand;
    
    private const string _stMore = "More";
    private const string _stHide = "Hide";

    private string expand = $"<color=#B5ABFF>... {_stMore}</color> \n \n";
    private string collapse = $"<color=#B5ABFF> {_stHide}</color> \n \n";

    private string rawText;
    private int maxLine = 3;


    public void Refresh()
    {
        LocalizationConManager.Inst.SetSystemTextFont(resName);
        LocalizationConManager.Inst.SetSystemTextFont(resDesc);
        resName.text = "";
        resDesc.text = "";
        var gameMapInfo = GameManager.Inst.gameMapInfo;
        if (gameMapInfo != null)
        {
            LoggerUtils.Log("ResDetailPanel::Refresh set mapinfo");
            if (gameMapInfo.isPGC > 0)
            {
                pgc.SetActive(true);
                if(!string.IsNullOrEmpty(gameMapInfo.bannerName)) LocalizationConManager.Inst.SetLocalizedContent(pgcThemeName, gameMapInfo.bannerName);
            }
            else if (gameMapInfo.isDC > 0)
            {
                dc.SetActive(true);
            }

            if (gameMapInfo.isDC > 0 && gameMapInfo.dcInfo != null)
            {
                StartCoroutine(GameUtils.LoadTexture(gameMapInfo.dcInfo.itemCover, (t) => resCover.texture = t, s=>LoggerUtils.LogError($"ResDetailPanel::Refresh load mapCover {gameMapInfo.dcInfo.itemCover} err") ));
            }
            else
            {
                StartCoroutine(GameUtils.LoadTexture(gameMapInfo.mapCover, (t) => resCover.texture = t, s=>LoggerUtils.LogError($"ResDetailPanel::Refresh load mapCover {gameMapInfo.mapCover} err") ));
            }
            resName.text = DataUtils.FilterNonStandardText(gameMapInfo.mapName);
            rawText = DataUtils.FilterNonStandardText(gameMapInfo.mapDesc);
            resDesc.text = rawText;
            Canvas.ForceUpdateCanvases();
            if (!string.IsNullOrEmpty(rawText))
            {
                var lines = resDesc.cachedTextGenerator.lineCount;
                if (lines > maxLine)
                {
                    SetDescExpand();
                }
            }
        }

    }

    

    private void SetDescExpand()
    {
        CollapseDesc();
    }
    
    private void ExpandDesc()
    {
        resDesc.text = rawText + collapse;
        btnExpand.onClick.RemoveAllListeners();
        btnExpand.onClick.AddListener(CollapseDesc);
    }

    private void CollapseDesc()
    {
        int endIndex = 0;
        for (int i = 0; i < resDesc.cachedTextGenerator.lineCount; i++) 
        {
            if (i == maxLine - 1)
            {
                endIndex = (i == resDesc.cachedTextGenerator.lines.Count - 1) ? resDesc.text.Length 
                    : resDesc.cachedTextGenerator.lines[i + 1].startCharIdx;
            }
        }
        
        resDesc.text = rawText.Substring(0, endIndex) + expand;
        btnExpand.onClick.RemoveAllListeners();
        btnExpand.onClick.AddListener(ExpandDesc);
    }
    

    
}
