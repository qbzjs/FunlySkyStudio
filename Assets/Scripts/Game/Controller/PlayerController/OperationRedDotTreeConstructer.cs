using RedDot;
namespace OperationRedDotSystem
{
    public enum ENodeType
    {
        Root,
        CameraMode, // 拍照模式按钮
        SelfieMode, // 自拍模式按钮
        GlobalSetting, // 全局设置按钮
        GraphicsBtn, // 设置画面分类按钮
        SoundBtn,//设置声音分类按钮
    }

    
    public class OperationRedDotNodeFactory : RedDotNodeFactoryBase
    {
        protected override void OnInit()
        {
            Register<Node>((int)ENodeType.Root);
        }
    }
    public class OperationRedDotTreeConstructer : IRedDotTreeConstructer
    {
        public void Construct(RedDotTree tree)
        {
            tree.Construct((int)ENodeType.Root);
        }
    }
}
