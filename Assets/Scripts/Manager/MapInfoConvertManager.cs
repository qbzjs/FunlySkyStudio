using System.Collections;
using System.Collections.Generic;
using SavingData;
using UnityEngine;

public class MapInfoConvertManager : CInstance<MapInfoConvertManager>
{
    public MapInfo ConvertDowntownInfoToMapInfo(DowntownInfo downtownInfo)
    {
        MapInfo mapInfo = new MapInfo()
        {
            mapId = downtownInfo?.downtownId,
            mapCover = downtownInfo?.downtownCover,
            mapName = downtownInfo?.downtownName,
            mapJson = downtownInfo?.downtownJson,
            mapDesc = downtownInfo?.downtownDesc,
            //editorVersion = downtownInfo.editorVersion,
            renderList = downtownInfo.renderList,
            maxPlayer = 16,
        };
        return mapInfo;
    }

    public UgcUntiyMapDataInfo ConvertDowntownDataInfoToMapDataInfo(DowntownDataInfo downtownDataInfo)
    {
        UgcUntiyMapDataInfo untiyMapDataInfo = new UgcUntiyMapDataInfo()
        {
            mapId = downtownDataInfo?.downtownId,
            mapName = downtownDataInfo?.downtownName,
            draftPath = downtownDataInfo?.draftPath
        };
        return untiyMapDataInfo;
    }
}
