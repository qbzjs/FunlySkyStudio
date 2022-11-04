
//单独的Avatar界面
using RedDot;

namespace AvatarRedDotSystem
{

    public enum ENodeType
    {
        Root,
        //一级
        Face,
        Body,
        //二级
        saves,
        collections,
        news,
        eyes,
        brows,
        nose,
        mouth,
        blush,
        hair,
        skin,
        outfits,//衣服下的子类 ，1、original 2、Marketplace 3、Digital Collectibles
        headwear,
        glasses,
        shoes,
        accessories,
        bag,
        ugcCloth,//Marketplace
        patterns,
        special,
        digitalCollect,//Digital Collectibles
        hand,
        outfitsoriginal,//官方衣服
        handoriginal,//官方手套
        shoesoriginal,//官方鞋子
        bagoriginal = 27,
        acoriginal,
        glassoriginal,
        headweardoriginal,
        rewards,
        my,
        airdrop,
        patternoriginal,
        ugcpattern,
        dcpattern,
        dcbag = 37,
        pgcBackpack = 38,
        pgcCrossbody = 39,
        dcBackpack = 40,
        dcCrossbody = 41,
        effect = 42,
        effectoriginal = 43,
        eyesoriginal = 44,
        dceyes = 45,
    }

    public class AvatarRedDotTreeConstructer : IRedDotTreeConstructer
    {
        public void Construct(RedDotTree tree)
        {
            tree.Construct((int)ENodeType.Root);
        }
    }
    public class AvatarRedDotNodeFactory : RedDotNodeFactoryBase
    {
        protected override void OnInit()
        {
            Register<Node>((int)ENodeType.Root);
        }
    }
    
    
   
}