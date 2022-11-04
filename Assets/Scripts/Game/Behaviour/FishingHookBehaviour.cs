public class FishingHookBehaviour : NodeBaseBehaviour
{
    public override void OnInitByCreate()
    {
        if (! gameObject.GetComponent<SpawnPointConstrainer>())
            gameObject.AddComponent<SpawnPointConstrainer>();
    }
}
