
using RedDot;

namespace EmoRedDotSystem
{
    public enum ENodeType
    {
        Root,
        Emo, //入口节点，主界面面板
        Emoji, //Emoji 情绪表情
        SingleEmo, //单人动作表情
        DoubleEmo, //双人交互动作表情
        StateEmo, //状态表情
        EmojiItem,
        SingleEmoItem,
        DoubleEmoItem,
        CollectEmoItem,
        StateEmoItem,
    }
    public class EmoRedDotTreeConstructer : IRedDotTreeConstructer
    {
        public void Construct(RedDotTree tree)
        {
            tree.Construct((int)ENodeType.Root);
        }
    }
    public class EmoRedDotNodeFactory : RedDotNodeFactoryBase
    {
        protected override void OnInit()
        {
            Register<Node>((int)ENodeType.Root);

        }
    }
}
