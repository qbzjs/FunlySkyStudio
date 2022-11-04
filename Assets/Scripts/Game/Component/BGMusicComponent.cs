using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BGMusicComponent : IComponent
{
    public string bgName;
    public string bgUrl;
    public string bgPath;
    public int musicType;
    public int musicId;
    public IComponent Clone()
    {
        return null;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}