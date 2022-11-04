using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Author:LiShuZhan
/// Description:排行榜个人信息子物体
/// Date: 2022.04.14
/// </summary>
public class LeaderBoardItem : MonoBehaviour
{
    public SuperTextMesh level;
    public SuperTextMesh userName;
    public SuperTextMesh lname;
    public SuperTextMesh score;
    public SpriteRenderer icon;
    public SpriteRenderer unIcon;
    public GameObject superIcon;
    public GameObject selfIcon;
    public GameObject iconGroup;
    public GameObject notIconGroup;

    public string uid;
}
