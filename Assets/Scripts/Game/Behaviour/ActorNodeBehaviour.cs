using System;
using UnityEngine;

/// <summary>
/// 模型承载节点,用以挂载NodeBaseBehaviour相关逻辑
/// 模型置于子层级
/// 如：滑梯SlidePipeBehaviour、SlideItemBehaviour
/// </summary>
public class ActorNodeBehaviour : NodeBaseBehaviour 
{
    public GameObject assetObj;//模型节点
}
