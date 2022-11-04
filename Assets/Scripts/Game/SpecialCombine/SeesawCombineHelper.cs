public class SeesawCombineHelper : CInstance<SeesawCombineHelper>
{

    /**
     * 判断是不是类似跷跷板的Combine类型
     * 暂时没用到，先留着。。。
     */
    public bool IsSeesawCombine(SceneEntity entity)
    {
        if (entity.HasComponent<SeesawComponent>())
        {
            return true;
        }
        return false;
    }
    
    
}