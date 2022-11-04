using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class DowntownTransferData
{
    public int transId;
    public int transType = (int)TransferType.DowntownTransfer;
}

public class TransferComponent : IComponent
{
    public string subMapId = "";
    public int transId;
    public int transType = (int)TransferType.DowntownTransfer;

    public IComponent Clone()
    {
        var comp = new TransferComponent();
        comp.subMapId = subMapId;
        comp.transId = transId;
        comp.transType = transType;
        return comp;
    }

    public BehaviorKV GetAttr()
    {
        DowntownTransferData data = new DowntownTransferData()
        {
            transId = this.transId,
            transType = transType
        };
        return new BehaviorKV
        {
            k = (int)BehaviorKey.DowntownTransfer,
            v = JsonConvert.SerializeObject(data)
        };
    }
}
