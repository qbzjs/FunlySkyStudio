/// <summary>
/// Author:zhouzihan
/// Description:水方块管理器（处理检测）
/// Date: #CreateTime#
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterCubeManager : ManagerInstance<WaterCubeManager>, IManager
{
    public void Clear()
    {
    }

    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
    }
    public override void Release()
    {
        base.Release();
        vectors.Clear();
        UpVectors.Clear();
        DownVectors.Clear();
    }

    public bool IsContains(Collider collider, Vector3 point)
    {
        var dir = collider.transform.position - point;
        Ray ray = new Ray(point, dir);
        RaycastHit[] hit = Physics.RaycastAll(ray, Vector3.Distance(point, collider.transform.position),  LayerMask.GetMask("WaterCube"));
        if (hit.Length==0)
        {
            return true;
        }
        else
        {
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].collider == collider)
                {
                    return  false;
                }
                
            }
            return true;
        }
       


    }

    List<Vector3> vectors = new List<Vector3>();
    List<Vector3> UpVectors = new List<Vector3>();
    List<Vector3> DownVectors = new List<Vector3>();

    public bool ContainsPlayer(List<Collider> colliders, PlayerBaseControl player, bool isSwimming)
    {
        if (!isSwimming)
        {
            
            vectors = GetCornersForBoxCollider(player);
            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = vectors.Count - 1; j >= 0; j--)
                {
                    if (IsContains(colliders[i], vectors[j]))
                    {
                        vectors.Remove(vectors[j]);
                    }

                }
            }
            return vectors.Count == 0;
        }
        else
        {
            UpVectors = GetUpCornersForBoxCollider(player);
            DownVectors = GetDownCornersForBoxCollider(player);
            for (int i = 0; i < colliders.Count; i++)
            {
                for (int j = UpVectors.Count - 1; j >= 0; j--)
                {
                    if (IsContains(colliders[i], UpVectors[j]))
                    {
                        UpVectors.Remove(UpVectors[j]);
                    }

                }
                for (int j = DownVectors.Count - 1; j >= 0; j--)
                {
                    if (IsContains(colliders[i], DownVectors[j]))
                    {
                        DownVectors.Remove(DownVectors[j]);
                    }
                }
            }

            if (DownVectors.Count == 0 && UpVectors.Count != 0)
            {
                PlayerSwimControl.Inst.swimLimite = PlayerSwimControl.SwimLimite.upLimit;
            }
            else if (UpVectors.Count == 0 && DownVectors.Count != 0)
            {
                PlayerSwimControl.Inst.swimLimite = PlayerSwimControl.SwimLimite.downLimit;
            }
            else
            {
                PlayerSwimControl.Inst.swimLimite = PlayerSwimControl.SwimLimite.nonLimit;
            }


            return DownVectors.Count == 0 || UpVectors.Count == 0;
        }
        
    }
    Vector3 size;
    Vector3 center;
    public List<Vector3> GetCornersForBoxCollider(PlayerBaseControl player)
    {
        List<Vector3> verts = new List<Vector3>();
        if (PlayerControlManager.Inst)
        {
            size = PlayerControlManager.Inst.waterTriggerSize;
            center = PlayerControlManager.Inst.waterTriggerCenter;
            verts.Add(player.transform.TransformPoint(center + new Vector3(-size.x, -size.y, -size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(size.x, -size.y, -size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(size.x, -size.y, size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(-size.x, -size.y, size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(-size.x, size.y, -size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(size.x, size.y, -size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(size.x, size.y, size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(-size.x, size.y, size.z) * 0.5f));
        }

        return verts;
    }

    public List<Vector3> GetUpCornersForBoxCollider(PlayerBaseControl player)
    {
        List<Vector3> verts = new List<Vector3>();
        if (PlayerControlManager.Inst)
        {
            size = PlayerControlManager.Inst.waterTriggerSize;
            center = PlayerControlManager.Inst.waterTriggerCenter;
            verts.Add(player.transform.TransformPoint(center + new Vector3(-size.x, size.y, -size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(size.x, size.y, -size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(size.x, size.y, size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(-size.x, size.y, size.z) * 0.5f));

        }
        return verts;
    }

    public List<Vector3> GetDownCornersForBoxCollider(PlayerBaseControl player)
    {
        List<Vector3> verts = new List<Vector3>();
        if (PlayerControlManager.Inst)
        {
            size = PlayerControlManager.Inst.waterTriggerSize;
            center = PlayerControlManager.Inst.waterTriggerCenter;
            verts.Add(player.transform.TransformPoint(center + new Vector3(-size.x, -size.y, -size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(size.x, -size.y, -size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(size.x, -size.y, size.z) * 0.5f));
            verts.Add(player.transform.TransformPoint(center + new Vector3(-size.x, -size.y, size.z) * 0.5f));
        }
        return verts;
    }
}
