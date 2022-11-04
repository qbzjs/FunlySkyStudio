using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Text))]
public class TextEllipsis:MonoBehaviour
{
    public int textLimit = 20;
    private Text textComp;

    public void SetText(string val)
    {
        if (textComp == null)
        {
            textComp = this.GetComponent<Text>();
        }
        textComp.text = val.Length > textLimit ? val.Substring(0, textLimit) + "..." : val;
    }
}
