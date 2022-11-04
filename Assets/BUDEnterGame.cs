using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BUDEnterGame : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (SceneSelect.Inst == null)
        {
            var enterGamePrefab = Resources.Load<SceneSelect>("EnterGame");
            var enterGame = GameObject.Instantiate<SceneSelect>(enterGamePrefab);
            enterGame.OnStart();
        }
    }
}