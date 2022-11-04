using System;
public class GameInfo : CInstance<GameInfo>
{
    #if UNITY_EDITOR
    public string myUid = "test555";
    #else
    public string myUid;
    #endif
}