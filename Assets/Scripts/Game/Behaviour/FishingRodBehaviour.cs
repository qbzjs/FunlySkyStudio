/// <summary>
/// Author: Tee Li
/// 日期：2022/8/30
/// 鱼竿行为
/// </summary>

public class FishingRodBehaviour : NodeBaseBehaviour
{
    public override void OnInitByCreate()
    {
        if (! gameObject.GetComponent<SpawnPointConstrainer>())
            gameObject.AddComponent<SpawnPointConstrainer>();
    }
}
