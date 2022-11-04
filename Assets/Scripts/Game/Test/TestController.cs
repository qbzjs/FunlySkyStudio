using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Author:Shaocheng
/// Description:本地测试工具
/// Date: 2022-3-30 19:43:08
/// </summary>
public class TestController:MonoBehaviour
{
    #region Unity本地测试
#if UNITY_EDITOR
    public Button GMBtn;
    private string curSceneName;

    private void Awake()
    {
        LoggerUtils.IsDebug = true;
        if (GMBtn == null)
        {
            GMBtn = gameObject.transform.Find("Gm").GetComponent<Button>();
        }
        curSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        LoggerUtils.Log("curSceneName==>" + curSceneName);
        GMBtn.gameObject.SetActive(true);
        GMBtn.transform.localPosition = new Vector3(-452, 445, 0);
        GMBtn.onClick.AddListener(() => {
            TestPanel.Show();
            TestPanel.Instance.ShowInMain(curSceneName.Equals("Main"), () =>
            {
                GMBtn.gameObject.SetActive(true);
            });
            GMBtn.gameObject.SetActive(false);
        });

        TestNetParams.Inst.LoadConfig();
        
        LoggerUtils.IsDebug = true;
        HttpUtils.IsMaster = true;
        LoggerUtils.Log("force set isMaster true");
    }

#endif
    #endregion
}