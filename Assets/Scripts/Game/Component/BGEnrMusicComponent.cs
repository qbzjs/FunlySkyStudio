
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BGEnrMusicComponent:IComponent
{
    public int enrMusicId;
    public IComponent Clone()
    {
        return null;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}