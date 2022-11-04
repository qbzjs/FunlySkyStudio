using System.Collections.Generic;

public static class AdjustItemContextFactory
{
    public static void Create(List<AdjustItemContext> items, EAdjustItemType itemType)
    {
        AdjustItemContext item= Create(itemType);
        if (items!=null)
        {
            items.Add(item);
        }
    }
    public static AdjustItemContext Create(EAdjustItemType itemType)
    {
        AdjustItemContext context = new AdjustItemContext();
        context.mItemType = itemType;
        context.mCurValue = 0;
        switch (itemType)
        {
            case EAdjustItemType.Size:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Size"); 
                break;
            case EAdjustItemType.Up_down:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Up-down");
                break;
            case EAdjustItemType.Left_right:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Left-right");
                break;
            case EAdjustItemType.Front_back:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Front-back");
                break;
            case EAdjustItemType.Spacing:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Spacing");
                break;
            case EAdjustItemType.Vertical:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Vertical");
                break;
            case EAdjustItemType.HorizontalStretch:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Horizontal Stretch");
                break;
            case EAdjustItemType.VerticalStretch:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Vertical Stretch");
                break;
            case EAdjustItemType.Rotation:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Rotation");
                break;
            case EAdjustItemType.X_Rotation:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("X-axis Rotation"); 
                context.mIconName = "XRotation";
                break;
            case EAdjustItemType.Y_Rotation:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Y-axis Rotation");
                context.mIconName = "YRotation";
                break;
            case EAdjustItemType.Z_Rotation:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Z-axis Rotation");
                context.mIconName = "ZRotation";
                break;
            case EAdjustItemType.Hue:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Hue");
                break;
            case EAdjustItemType.Chroma:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Chroma");
                break;
            case EAdjustItemType.Bright:
                context.mTitle = LocalizationConManager.Inst.GetLocalizedText("Bright");
                break;
            default:
                break;
        }
        return context;
    }
}
