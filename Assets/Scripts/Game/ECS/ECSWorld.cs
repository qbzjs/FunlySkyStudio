using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entitas;

public class ECSWorld
{
    private int entityCount = 0;
    private List<SceneEntity> entitys = new List<SceneEntity>();
    public SceneEntity NewEntity()
    {
        entityCount++;
        SceneEntity entity = new SceneEntity();
        entity.SetID(entityCount);
        entitys.Add(entity);
        return entity;
    }

    public void DestroyEntity(SceneEntity entity)
    {
        entity.Destroy();
        if (entitys.Contains(entity))
        {
            entityCount--;
            entitys.Remove(entity);
        }
    }

    public SceneEntity CloneEntity(SceneEntity cloneEntity)
    {
        entityCount++;
        SceneEntity entity = new SceneEntity();
        entity.SetID(entityCount);
        entity.Components = cloneEntity.CloneComponent();
        return entity; 
    }

    public SceneEntity NewEntityNoRecord()
    {
        SceneEntity entity = new SceneEntity();
        entity.SetID(-1);
        return entity;
    }


    public void DestoryAllEntity()
    {
        entitys.Clear();
    }
}