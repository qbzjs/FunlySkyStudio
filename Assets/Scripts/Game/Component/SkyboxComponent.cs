public class SkyboxComponent : IComponent
{
    public int skyboxId;
    public int type; // 1 - gradient, 3 - color 
    public string scol;
    public string ecol;
    public string gcol;
    
    public SkyboxType skyboxType;
    public int dayLength;
    public int daytimeHour;
    public int daytimeMin;
    
    public IComponent Clone()
    {
        SkyboxComponent component = new SkyboxComponent();
        component.skyboxId = skyboxId;
        
        component.type = type;
        component.scol = scol;
        component.ecol = ecol;
        component.gcol = gcol;
        
        component.skyboxType = skyboxType;
        component.dayLength = dayLength;
        component.daytimeHour = daytimeHour;
        component.daytimeMin = daytimeMin;
        return component;
    }

    public BehaviorKV GetAttr()
    {
        return null;
    }
}