using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordAnimDataManager : CInstance<SwordAnimDataManager>
{
    public Dictionary<int, string> swordAnim = new Dictionary<int, string>();

    public void InitAnim()
    {
        AddNewData(10009,100026, BundlePart.Hand);
        AddNewData(10007,100020, BundlePart.Hand);
        AddNewData(10008,100021, BundlePart.Hand);
        AddNewData(10010, 100003, BundlePart.Bag);
    }

    private void AddNewData(int emoteId,int swordId,BundlePart part)
    {
        string swordInfo = swordId + "_" + (int)part;
        if (!swordAnim.ContainsKey(emoteId))
        {
            swordAnim.Add(emoteId, swordInfo);
        }
        swordAnim[emoteId] = swordInfo;
    }


    public int FindemoteId(int swordId, BundlePart part)
    {
        foreach (var item in swordAnim)
        {
            string[] swordInfo = item.Value.Split('_');
            if(int.Parse(swordInfo[0]) == swordId && int.Parse(swordInfo[1]) == (int)part)
            {
                return item.Key;
            }
        }
        return 0;
    }
}
