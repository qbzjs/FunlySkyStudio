/// <summary>
/// Author:JayWill
/// Description:Undo/Redo配置表，用反射时使用，实现新的UndoHelper类需要在该表注册相关类名
/// </summary>
public class UndoRedoConfig
{

}

public class UndoHelperName
{
    public static string TransformUndoHelper = "TransformUndoHelper";
    public static string CreateDestroyUndoHelper = "CreateDestroyUndoHelper";
    public static string BaseMaterialUndoHelper = "BaseMaterialUndoHelper";
    public static string TerrainMaterialUndoHelper = "TerrainMaterialUndoHelper";
    public static string TextUndoHelper = "TextUndoHelper";
    public static string MusicBoardUndoHelper = "MusicBoardUndoHelper";
    public static string SkyboxUndoHelper = "SkyboxUndoHelper";
    public static string LockHideUndoHelper = "LockHideUndoHelper";
    public static string UGCClothDrawUndoHelper = "UGCClothDrawUndoHelper";
    public static string CombineUndoHelper = "CombineUndoHelper";
    public static string NewTextUndoHelper = "NewTextUndoHelper";
    public static string WaterCubeUndoHelper = "WaterCubeUndoHelper";
    public static string PGCPlantUndoHelper = "PGCPlantUndoHelper";
    public static string BaseMatColorUndoHelper = "BaseMatColorUndoHelper";
    public static string FireUndoHelper = "FireUndoHelper";
    public static string SnowCubeUndoHelper = "SnowCubeUndoHelper";
    public static string FireworkUndoHelper = "FireworkUndoHelper";
    public static string SeesawUndoHelper = "SeesawUndoHelper";
    public static string UGCClothElementUndoHelper = "UGCClothElementUndoHelper";
    public static string UGCClothesCreateDestroyUndoHelper = "UGCClothesCreateDestroyUndoHelper";
    public static string FishingUndoHelper = "FishingUndoHelper";
    public static string SlideItemUndoHelper = "SlideItemUndoHelper";
    public static string PGCEffectUndoHelper = "PGCEffectUndoHelper";
    public static string FlashlightUndoHelper = "FlashlightUndoHelper";
}


public enum CreateUndoMode
{
    Create,
    Destroy,
    Duplicate,
}

public enum UndoRedoType
{
    Undo,
    Redo,
}

public enum CombineUndoMode
{
    Combine,
    UnCombine
}

public enum UntoState
{
    Begin,
    End,
}
