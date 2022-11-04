/// <summary>
/// Author:MeiMei—LiMei
/// Description:音效道具Manager,管理Node数
/// Date: 2022-01-13
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : ManagerInstance<SoundManager>, IManager
{
    public int MaxCount = 10;//最大数量
    public float soundVolume; // 声音按钮音量

    public List<NodeBaseBehaviour> soundBevs = new List<NodeBaseBehaviour>();
    public override void Release()
    {
        base.Release();
        Clear();
    }
    public void Clear()
    {
        if (soundBevs != null)
        {
            soundBevs.Clear();
        }
    }
    public void RemoveNode(NodeBaseBehaviour behaviour)
    {
        OnRemoveNode(behaviour);
    }
    public bool IsOverMaxSoundCount()//是否达到最大数量
    {
        if (soundBevs.Count>= MaxCount)
        {
            return true;
        }
        return false;
    }
    public void OnHandleClone(NodeBaseBehaviour sourceBev, NodeBaseBehaviour newBev)
    {
        if (newBev.entity.HasComponent<SoundComponent>())
        {
            AddSound(newBev);
        }
    }
    public bool IsCanCloneSound(GameObject curTarget)//是否能克隆
    {
        if (curTarget.GetComponentInChildren<SoundButtonBehaviour>()!=null)
        {
            int CombineCount = curTarget.GetComponentsInChildren<SoundButtonBehaviour>().Length;
            if (CombineCount>1)
            {
                if (CombineCount+soundBevs.Count>MaxCount)
                {
                    TipPanel.ShowToast("You can only add up to 10 sound buttons");
                    return false;
                }
            }
            else
            {
                if (IsOverMaxSoundCount())
                {
                    TipPanel.ShowToast("You can only add up to 10 sound buttons");
                    return false;
                }
            }    
        }
        return true;
    }

    public void AddSound(NodeBaseBehaviour b)
    {
        if (!soundBevs.Contains(b))
        {
            soundBevs.Add(b);
            LoggerUtils.Log("soundCount:" + soundBevs.Count);
        }
        else
        {
            LoggerUtils.Log("[AssSound] sid already exist");
        }
    }
    public void OnRemoveNode(NodeBaseBehaviour b)
    {
        GameObjectComponent goCmp = b.entity.Get<GameObjectComponent>();

        if (goCmp.modelType == NodeModelType.Sound)
        {
            soundBevs.Remove(b);
        }
    }

    public void RevertNode(NodeBaseBehaviour behaviour)
    {
        var goCmp = behaviour.entity.Get<GameObjectComponent>();
        if (goCmp.modelType == NodeModelType.Sound)
        {
            if(!soundBevs.Contains(behaviour))
            {
                soundBevs.Add(behaviour);
            }
            
        }
    }
    
    public void AudioStop()
    {
        for (int i = 0; i < soundBevs.Count; i++)
        {
            var sound = soundBevs[i] as SoundButtonBehaviour;
            if (sound != null)
            {
                if (sound.importASource.clip != null)
                {
                    sound.Stop();
                }
            }
        }                 
    }
    public string[] GetAudioList()
    {
        List<string> audioList=new List<string>();
        for (int i = 0; i < soundBevs.Count; i++)
        {
            var comp = soundBevs[i].entity.Get<SoundComponent>();
            if (comp.soundUrl!=null)
            {
                audioList.Add(comp.soundUrl);
            }  
        }
        LoggerUtils.Log("GetAudioList" + audioList.Count);
        GameManager.Inst.gameMapInfo.audioList = audioList.ToArray();
        return audioList.ToArray();
    }

    public void SetSoundVolume(float volume)
    {
        soundVolume = volume;
        foreach (var soundBev in soundBevs)
        {
            var soundBtn = soundBev as SoundButtonBehaviour;
            soundBtn.SetSoundVolume(volume);
        }
    }
}
