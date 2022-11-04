using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class IgnoreCantSelectComponent:IComponent
{
    public IComponent Clone()
    {
        return new IgnoreCantSelectComponent();
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}