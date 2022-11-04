#if UNITY_EDITOR
using System.Collections;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;


[GlobalConfig("Assets/Editor/Config/"), HideMonoScript]
public class ABUpLoadConfig : GlobalConfig<ABUpLoadConfig>
{
    [LabelText("当前环境"), ValueDropdown("env")]
    public string currentEnv;

    [HideInInspector]
    public ValueDropdownList<string> env = new ValueDropdownList<string>()
    {
        {"Master","U3D/Res/Master"},
        {"Alpha","U3D/Res/Alpha"},
        //{"Prod","U3D/Res/Prod"}
    };


    [LabelText("版本号"), InfoBox("大版本号和端上保持一致，小版本按照实际配置更新次数填写，格式必须为*.*.*", InfoMessageType.Warning)]
    public string[] version;

    [LabelText("通用上传路径")]
    public string[] generalUploadPaths;

}
#endif
