#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public static class DemoLauncher
{
    const string ScenePath = "Assets/Scenes/AIrport navigation.unity";
    const string SessionKey = "AirportDemo_AutoPlayed_v3";

    static DemoLauncher()
    {
        EditorApplication.update += WaitAndLaunch;
    }

    static void WaitAndLaunch()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        EditorApplication.update -= WaitAndLaunch;

        if (EditorApplication.isPlayingOrWillChangePlaymode) return;

        // Ensure URP pipeline asset is assigned so materials render properly
        var urpAsset = AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Assets/Settings/PC_RPAsset.asset");
        if (urpAsset != null && GraphicsSettings.defaultRenderPipeline == null)
        {
            GraphicsSettings.defaultRenderPipeline = urpAsset;
            QualitySettings.renderPipeline = urpAsset;
        }

        if (!SessionState.GetBool(SessionKey, false))
        {
            SessionState.SetBool(SessionKey, true);
            OpenAndPlay();
        }
    }

    [MenuItem("Airport Demo/Open Scene & Play Demo %#p")]
    public static void OpenAndPlay()
    {
        if (System.IO.File.Exists(ScenePath))
        {
            var active = EditorSceneManager.GetActiveScene();
            if (!string.Equals(active.path, ScenePath, StringComparison.OrdinalIgnoreCase))
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }
        }
        Debug.Log("[DemoLauncher] Automatically entering Play Mode for the Airport Navigation Demo...");
        var gameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        if (gameViewType != null)
        {
            EditorWindow.GetWindow(gameViewType);
        }
        EditorApplication.isPlaying = true;
    }
}
#endif
