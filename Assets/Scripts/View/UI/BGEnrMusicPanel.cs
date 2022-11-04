using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGEnrMusicPanel : InfoPanel<BGEnrMusicPanel>
{
    public Transform ItemParent;
    public Sprite[] iconSprites;
    private string[] enrNames = { "No ambience","Rainy","Snowy", "Forests", "Summer night", "Park", "Calm ocean waves","Home","Seaside"};
    private SceneEntity entity;
    private BGMusicBehaviour bgBehv;
    private List<CommonSelectItem> allSelect;
    private int lastIndex = 0;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        allSelect = new List<CommonSelectItem>();
        entity = SceneBuilder.Inst.BGMusicEntity;
        var bindGo = entity.Get<GameObjectComponent>().bindGo;
        bgBehv = bindGo.GetComponent<BGMusicBehaviour>();
        var itemPrefab = ResManager.Inst.LoadResNoCache<GameObject>(GameConsts.PanelPath + "BGEnrMusicItem");
        for (int i = 0; i < enrNames.Length; i++)
        {
            int index = i;
            var itemGo = GameObject.Instantiate(itemPrefab, ItemParent);
            var itemScript = itemGo.GetComponent<CommonSelectItem>();
            itemScript.SetText(enrNames[i]);
            itemScript.SetIcon(i == 0 ? iconSprites[0] : iconSprites[1]);
            itemScript.AddClick(() =>
            OnSelect(index));
            allSelect.Add(itemScript);
        }
        allSelect[0].SetSelectState(true);
    }


    public override void OnDialogBecameVisible()
    {
        base.OnDialogBecameVisible();
        var comp = entity.Get<BGEnrMusicComponent>();
        int index = Array.FindIndex(GameConsts.ambineMusicIds, x => x == comp.enrMusicId);
        OnUISelect(index);
        SetAnim(index, false);
    }
    private void OnSelect(int index)
    {
        OnUISelect(index);
        int id = GameConsts.ambineMusicIds[index];
        entity.Get<BGEnrMusicComponent>().enrMusicId = id;
        if (id == 0)
        {
            bgBehv.StopEnr();
        }
        else
        {
            bgBehv.PlayEnr();
        }
    }

    private void OnUISelect(int index)
    {
        //if (lastIndex == index)
        //{
        //    return;
        //}
        SetAnim(lastIndex, false);
        SetAnim(index, true);
        allSelect[lastIndex].SetSelectState(false);
        allSelect[index].SetSelectState(true);
        lastIndex = index;
    }


    private void SetAnim(int index,bool isAnim)
    {
        if (index != 0)
        {
            allSelect[index].SetAnim(isAnim);
        }
    }

    public override void OnBackPressed()
    {
        base.OnBackPressed();
        bgBehv.StopEnr();
    }

    private void OnDisable()
    {
        bgBehv.StopEnr();
    }
}
