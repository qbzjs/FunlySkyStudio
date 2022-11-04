using System;
using System.Collections;
using System.Collections.Generic;
using BudEngine.NetEngine;
using UnityEngine;

/// <summary>
/// Author:Meimei-LiMei
/// Description:分队模式标志
/// Date: 2022/6/29 18:20:35
/// </summary>
public class TeamLogo : MonoBehaviour
{
    public SpriteRenderer logoImg;
    public GameObject nickGO;
    public SuperTextMesh nickTMP;
    public Action<bool> voiceActive;
    public VoiceItemPos voiceItem;
    private Vector3 origNickPos;
    private float nNamePosOff = 0.02f;
    private float nHeartPosOff = 0.06f;
    private string nBlank = "     ";   
    public Transform heartTF;
    public void Awake()
    {
        if (voiceItem != null)
        {
            voiceActive = voiceItem.SetPos;
        }
    }
    public void SetVisiable(bool state)
    {
        origNickPos = nickGO.transform.localPosition;
        if (state)
        {
            SwitchBlankToName(true);
            gameObject.SetActive(true);
            Vector3 logoPos = heartTF.localPosition;
            logoPos.x = (nickTMP.preferredWidth) / 20 - nHeartPosOff;
            heartTF.localPosition = logoPos;
        }
        else
        {
            gameObject.SetActive(false);
            nickGO.transform.localPosition = origNickPos;
            SwitchBlankToName(false);
        }
        if (voiceActive != null)
        {
            voiceActive(state);
        }
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
    private readonly Dictionary<TeamColor, string> logoSpritesDic = new Dictionary<TeamColor, string>()
    {
        {TeamColor.Green, "ic_teamgreen"},
        {TeamColor.Blue, "ic_teamblue"},
        {TeamColor.Red, "ic_teamred"}
    };
    public void SetTeamLogoColor(int memberState)
    {
        switch ((TeamMemberState)memberState)
        {
            case TeamMemberState.Normal:
            case TeamMemberState.Self:
                SetColor(TeamColor.Green);
                break;
            case TeamMemberState.TeamMate:
                SetColor(TeamColor.Blue);
                break;
            case TeamMemberState.Enem:
                SetColor(TeamColor.Red);
                break;
        }
    }
    public void SetColor(TeamColor color)
    {
        logoImg.sprite = ResManager.Inst.GetGameAtlasSprite(logoSpritesDic[color]);
    }
    public void ExitShowTeamLogo()
    {
        SetVisiable(false);
        this.gameObject.SetActive(false);
    }

}
