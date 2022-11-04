using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class IOSInterface
{
#if UNITY_IPHONE
    [DllImport("__Internal")]
    public static extern void sendMessageToClient(string key,string msg);
#endif
}
