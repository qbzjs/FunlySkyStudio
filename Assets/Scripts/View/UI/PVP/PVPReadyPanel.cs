using System;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;
using UnityEngine;
public class PVPReadyPanel: BasePVPGamePanel
    {
        public Text ReadyTimeText;
        public Button roomPlayers;
        public Action ReadyEndAction;
        private int waitTime = 3;
        private int readyGoTime = 5;
        public override void Enter(PVPGameConnectEnum connect)
        {
            PVPWaitAreaManager.Inst.IsPVPGameStart = false;
            Invoke("ReadyCountDown", waitTime);
        }

        /// <summary>
        /// 准备倒计时
        /// </summary>
        private void ReadyCountDown()
        {
            if(!gameObject.activeInHierarchy)
                return;
            ReadyTimeText.gameObject.SetActive(true);
            roomPlayers.gameObject.SetActive(false);
            StartCoroutine("ReadyTimeCount");
        }


        private IEnumerator ReadyTimeCount()
        {
            int ReadyTime = readyGoTime;
            while (ReadyTime >= 0)
            {
                ReadyTimeText.transform.localScale = new Vector3(0, 0, 0);
                ReadyTimeText.text = ReadyTime.ToString();
                if (ReadyTime < 1)
                {
                    ReadyTimeText.text = "GO";
                    yield return new WaitForSeconds(0.8f);
                    ReadyTimeText.gameObject.SetActive(false);
                    if (GlobalFieldController.CurGameMode == GameMode.Play)
                    {
                        ReadyEndAction?.Invoke();
                    }
                }
                ReadyTime--;
                var sequence = DOTween.Sequence();
                sequence.Append(ReadyTimeText.transform.DOScale(1.2f, 0.3f))
                    .Append(ReadyTimeText.transform.DOScale(1.15f, 0.1f));
                yield return new WaitForSeconds(1);
            }
        }

        public override void Leave()
        {
            ReadyTimeText.gameObject.SetActive(false);
            CancelInvoke("ReadyCountDown");
            StopCoroutine("ReadyTimeCount");
        }
        
    }