using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

/// <summary>
/// Author:Meimei-LiMei
/// Description:精灵图集管理
/// Date: 2022/8/16 20:59:4
/// </summary>
public class SpriteAtlasManager : CInstance<SpriteAtlasManager>
{
    private SpriteAtlas avatarCommonAtlas;
    public SpriteAtlas AvatarCommonAtlas
    {
        get
        {
            if (avatarCommonAtlas == null)
            {
                avatarCommonAtlas = ResManager.Inst.LoadRes<SpriteAtlas>("Atlas/AtlasAB/AvatarCommon");
            }
            return avatarCommonAtlas;
        }
    }

    public Sprite GetAvatarCommonSprite(string spriteName)
    {
        var sprite = AvatarCommonAtlas.GetSprite(spriteName);
        return sprite;
    }
}
