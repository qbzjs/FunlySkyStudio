using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[CreateAssetMenu(menuName = "ResourceLibrary/ColorLibrary")]
public class ColorLibrary : Library<Color>
{

    //string colorString
    //    = "@[@(0xF06078),@(0x3BEEE8),@(0x8475FF),@(0x8548BB),@(0xB983F0),@(0xC62B2A),@(0xCF4B38),@(0xE90432),@(0xFB4667),@(0xD2434D),@(0xBE8C8D),@(0xDCB8B4),@(0xE9A5A1),@(0xFFCFD0),@(0xC94189),@(0xD5557B),@(0xDC799F),@(0xE9A6BB),@(0xEDC1C0),@(0xFFCCDC),@(0x9A4216),@(0xD36500),@(0xEF7A35),@(0xE39600),@(0xF4AF20),@(0xF1BD41),@(0xF4D302),@(0xFDEB00),@(0xFFF09D),@(0x007350),@(0x566E36),@(0x568066),@(0x04974A),@(0x4EA22A),@(0x8DB823),@(0x7FC9A5),@(0xB4D7C6),@(0xCCDCCC),@(0xCBEECA),@(0x004C68),@(0x076393),@(0x2C5EB9),@(0x648AC7),@(0x648AC7),@(0xAFEEEF),@(0xC1EDEE),@(0xCAFFFE),@(0x6A3785),@(0x6E6FAB),@(0x9B90C1),@(0xB999C4),@(0xC7AFD0),@(0xCBCCFF),@(0x2B2B2B),@(0x4C4C4E),@(0xA8A8A8),@(0xE2E2E2),@(0xFFFFFF),@(0x411B23),@(0x623D33),@(0x9B8779),@(0xA88754),@(0xD0B58C),@(0xF0E8D6),]";

    /*public void Set()
    {
        List<Color> list = new List<Color>();
        string[] colorList = colorString.Split(',');
        for (int i = 0; i < colorList.Length - 1; ++i)
        {
            int xInd = colorList[i].IndexOf('x');
            string colorStr = colorList[i].Substring(xInd + 1, 6);
            Color color = GameData.DataUtils.DeSerializeColor(colorStr);
            list.Add(color);
        }
        List = list;
    }*/

    [SerializeField]
    bool reset = false;

    void Set()
    {
        string[] colors = File.ReadAllLines(Application.dataPath + "/LocalSaving/colors.txt");
        List<Color> list = new List<Color>();
        LoggerUtils.Log(colors.Length);
        for(int i = 0; i < colors.Length; ++i)
        {
            string color = colors[i];
            if (color == "") continue;
            bool isCus = ColorUtility.TryParseHtmlString("#"+color.Substring(1,6), out Color res);
            if (isCus)
            {
                list.Add(res);
            }
            else
            {
                LoggerUtils.Log(i + "miss" + color);
            }

        }
        List = list;
    }


    //private void OnEnable()
    //{
    //    if (reset)
    //    {
    //        Set();
    //    }

    //}

    public List<Color> List
    {
        set
        {
            source = value;
        }
        get
        {
            return source;
        }
    }
}
