using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class MovePathManager:CInstance<MovePathManager>
{
    private SceneEntity curEntity;
    private GameObject pointPrefab;
    private List<GameObject> nousePoints = new List<GameObject>();
    private List<GameObject> usePoints = new List<GameObject>();

    public void Init()
    {
        pointPrefab = ResManager.Inst.LoadResNoCache<GameObject>(GameConsts.SpecialModelPath + "MovePoint");
    }


    public void CloseAndSave()
    {
        if(curEntity == null || usePoints.Count == 0)
            return;
        var moveComp = curEntity.Get<MovementComponent>();
        if (moveComp.pathPoints.Count != usePoints.Count)
        {
            LoggerUtils.Log("move points error");
            return;
        }
        moveComp.pathPoints.Clear();
        usePoints.ForEach(x=> moveComp.pathPoints.Add(x.transform.position));
        ReleaseAllPoints();
        curEntity = null;
    }

    public bool IsSelectMovePoint(SceneEntity entity)
    {
        var go = entity.Get<GameObjectComponent>().bindGo;
        return usePoints.Contains(go);
    }

    public void AddMovePoint(Vector3 pos,int count)
    {
        var node = GetPoint(pos);
        var textNode = node.GetComponentInChildren<TextMeshPro>();
        textNode.text = count.ToString();
    }

    public void SubMovePoint()
    {
        var lastPoints = usePoints.Last();
        lastPoints.gameObject.SetActive(false);
        usePoints.Remove(lastPoints);
        nousePoints.Add(lastPoints);
    }

    public void BindEntity(SceneEntity entity)
    {
        curEntity = entity;
        var paths = curEntity.Get<MovementComponent>().pathPoints;
        for (var i = 0; i < paths.Count; i++)
        {
            var node = GetPoint(paths[i]);
            var textNode = node.GetComponentInChildren<TextMeshPro>();
            textNode.text = (i + 1).ToString();
        }
    }

    public void ReleaseAllPoints()
    {
        usePoints.ForEach(x =>
        {
            x.SetActive(false);
        });
        nousePoints.AddRange(usePoints);
        usePoints.Clear();
    }

    private GameObject GetPoint(Vector3 pos)
    {
        GameObject point = null;
        if (nousePoints.Count > 0)
        {
            point = nousePoints.First();
            point.transform.position = pos;
            point.SetActive(true);
            nousePoints.Remove(point);
        }
        else
        {
            point = EditorStageBuilder.Inst.CreateGameObject(pos, pointPrefab);
        }
        usePoints.Add(point);
        return point;
    }

}