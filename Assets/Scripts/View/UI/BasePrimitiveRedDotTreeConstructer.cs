using RedDot;
namespace BasePrimitiveRedDotSystem
{
    public enum ENodeType
    {
        Root,
        Character,
        General,
        GamePlay,
        Scene,
        PrimitiveItem, //入口节点，主界面面板
    }
    public class BasePrimitiveRedDotNodeFactory : RedDotNodeFactoryBase
    {
        protected override void OnInit()
        {
            Register<Node>((int)ENodeType.Root);
        }
    }
    public class BasePrimitiveRedDotTreeConstructer : IRedDotTreeConstructer
        {
            public void Construct(RedDotTree tree)
            {
                tree.Construct((int)ENodeType.Root);
            }
        }
}
