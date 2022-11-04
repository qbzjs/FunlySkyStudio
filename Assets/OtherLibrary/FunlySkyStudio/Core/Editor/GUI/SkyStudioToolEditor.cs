using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Funly.SkyStudio
{
  [InitializeOnLoad]
  public class SkyStudioToolEditor : Editor
  {
    static SkyStudioToolEditor()
    {
      #if UNITY_2019_1_OR_NEWER
        SceneView.duringSceneGui += OnSceneGUI;
      #else
        SceneView.onSceneGUIDelegate += OnSceneGUI;
      #endif
    }

    public static void OnSceneGUI(SceneView sceneView)
    {
      SpherePointGUI.RenderSpherePointSceneSelection();
    }
  }
}
