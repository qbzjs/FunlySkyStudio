using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TeamColor
{
    Green = 1,
    Red = 2,
    Blue = 3,
}

/// <summary>
/// Author: 熊昭
/// Description: 角色对战状态
/// Date: 2022-04-07 21:20:46
/// </summary>
public class BattleState : MonoBehaviour
{
    public GameObject nickGO;
    public Transform heartTF;
    public SpriteRenderer heartImg;
    public SpriteRenderer heartbgImg;
    public SpriteRenderer barImg;
    public SuperTextMesh nickTMP;
    public VoiceItemPos voiceItem;
    public VerifyItemPos verifyItemPos;

    private Vector3 origNickPos;
    private float maxHeart;
    private float maxBar;

    public string nBlank = "     ";           //s:5          t:8
    public float nHeartPosOff = 0.75f;   //s:0.03f    t:0.05f
    public float  nNamePosOff = 0.02f; 
    private float heartIconSrcY = float.MinValue; 

    public Tweener tweener1;
    public Tweener tweener2;
    public Action<bool> heartActive;
    private readonly Dictionary<TeamColor, string> HpSpriteDic = new Dictionary<TeamColor, string>()
    {
        {TeamColor.Green, "ic_bargreen"},
        {TeamColor.Blue, "ic_barblue"},
        {TeamColor.Red, "ic_barred"}
    };
    private readonly Dictionary<TeamColor, string> HeartSpriteDic = new Dictionary<TeamColor, string>()
    {
        {TeamColor.Green, "ic_heartgreen"},
        {TeamColor.Blue, "ic_heartblue"},
        {TeamColor.Red, "ic_heartred"}
    };
    public void Awake()
    {
        if (voiceItem!=null)
        {
            heartActive = voiceItem.SetPos;
        }

        GlobalSettingManager.Inst.OnShowUserNameChange += AdjustHeartIcon;
    }
    private void InitValue()
    {
        if (maxHeart != 0 && maxBar != 0)
        {
            return;
        }
        origNickPos = nickGO.transform.localPosition;
        maxHeart = heartImg.size.y;
        maxBar = barImg.size.x;
    }
    public void SetColor(TeamColor color)
    {
        barImg.sprite = ResManager.Inst.GetGameAtlasSprite(HpSpriteDic[color]);
        heartImg.sprite = ResManager.Inst.GetGameAtlasSprite(HeartSpriteDic[color]);
        heartbgImg.sprite = ResManager.Inst.GetGameAtlasSprite(HeartSpriteDic[color] + "_bg");
    }
    private void OnDestroy()
    {
        if (tweener1 != null)
        {
            tweener1.Kill();
            tweener1 = null;
        }
        if (tweener2 != null)
        {
            tweener2.Kill();
            tweener2 = null;
        }
        DOTween.Kill(heartImg.size);
        DOTween.Kill(barImg.size);
        if (GlobalSettingManager.Inst.OnShowUserNameChange != null)
            GlobalSettingManager.Inst.OnShowUserNameChange -= AdjustHeartIcon;
    }

    private void SwitchBlankToName(bool isAdd)
    {
        if (!isAdd && nickTMP.text.StartsWith(nBlank))
        {
            nickTMP.text = nickTMP.text.Remove(0, nBlank.Length);
        }
        if (isAdd && !nickTMP.text.StartsWith(nBlank))
        {
            nickTMP.text = nBlank + nickTMP.text;
        }
    }

    public void SetVisiable(bool state)
    {
       
        InitValue();
        if (state)
        {
            gameObject.SetActive(true);
            //更改名字高度位置
            Vector3 nickPos = nickGO.transform.localPosition;
            nickPos.y = transform.localPosition.y + nNamePosOff;
            nickGO.transform.localPosition = nickPos;
            //增加名称占位空格
            SwitchBlankToName(true);
            //调整图标位置
            RefreshHeartIconPosition();
            //调整认证图标位置
            RefreshVerifyIconPosition();
        }
        else
        {
            gameObject.SetActive(false);
            //还原名字位置
            nickGO.transform.localPosition = origNickPos;
            //还原名称
            SwitchBlankToName(false);
            //调整认证图标位置
            RefreshVerifyIconPosition();
        }
        if (heartActive != null)
        {
            heartActive(state);
        }
    }

    private void RefreshVerifyIconPosition()
    {
        verifyItemPos.RefreshPos();
    }

    void OnEnable()
    {
        RefreshHeartIconPosition();
    }

    public void RefreshHeartIconPosition()
    {
        AdjustHeartIcon(GlobalSettingManager.Inst.IsShowUserName());
    }

    private void AdjustHeartIcon(bool showUserName)
    {
        CoroutineManager.Inst.StartCoroutine(CAdjustHeartIcon(showUserName));
    }

    private IEnumerator CAdjustHeartIcon(bool showUserName)
    {
        if (Math.Abs(heartIconSrcY - float.MinValue) < 0.1f)
        {
            heartIconSrcY = heartTF.localPosition.y;
        }
        if (showUserName)
        {
            yield return null;
            Vector3 heartPos = heartTF.localPosition;
            heartPos.x = (nickTMP.preferredWidth) / 20 - nHeartPosOff;
            heartPos.y = heartIconSrcY;
            heartTF.localPosition = heartPos;
        }
        else
        {
            Vector3 pos = heartTF.localPosition;
            pos.x = 0.402f;
            pos.y = heartIconSrcY - 0.13f;
            heartTF.localPosition = pos;
        }
    }

    public void SetValue(float percent)
    {
        //heartSize
        Vector2 heartTar = heartImg.size;
        heartTar.y = percent * maxHeart;
        tweener1 = DOTween.To(() => heartImg.size, x => heartImg.size = x, heartTar, 0.6f).SetEase(Ease.OutQuart);

        //barSize
        Vector2 barTar = barImg.size;
        barTar.x = percent * maxBar;
        tweener2 = DOTween.To(() => barImg.size, x => barImg.size = x, barTar, 0.6f).SetEase(Ease.OutQuart);
    }

    public void ResetValue()
    {
        //heartSize
        Vector2 heartTar = heartImg.size;
        heartTar.y = maxHeart;
        heartImg.size = heartTar;

        //barSize
        Vector2 barTar = barImg.size;
        barTar.x = maxBar;
        barImg.size = barTar;
    }
}
