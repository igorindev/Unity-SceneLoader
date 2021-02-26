#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class SceneLoader : EditorWindow
{
    Vector2 scroll = Vector2.zero;
    string path;

    [MenuItem("Scenes/Open Scenes Window")]
    static void Create()
    {
        var window = GetWindow<SceneLoader>("Scene Loader");
        window.position = new Rect(0, 0, 340, 600);
        window.Show();
    }

    void OnGUI()
    {
        GUI.BeginGroup(new Rect(0, 0, Screen.width, Screen.height));

        EditorBuildSettingsScene[] allScenes = EditorBuildSettings.scenes;

        GUI.Label(new Rect(15, 10, 1000, 1000), "Scenes (In Build Settings)", EditorStyles.largeLabel);

        GUI.Label(new Rect(15, 40, 80, 20), "Single", EditorStyles.boldLabel);
        GUI.Label(new Rect(170, 40, 80, 20), "Additive", EditorStyles.boldLabel);

        if (Application.isPlaying)
        {
            GUI.Label(new Rect(15, 70, 1000, 1000), "Disabled during PlayMode", EditorStyles.largeLabel);
        }
        else
        {
            GUILayout.BeginArea(new Rect(10, 60, 1000, 1000));
            scroll = EditorGUILayout.BeginScrollView(scroll, false, true, GUILayout.Width(Screen.width - 15), GUILayout.Height(Screen.height - 100));
            for (int i = 0; i < allScenes.Length; i++)
            {
                path = Path.GetFileNameWithoutExtension(allScenes[i].path);

                if (GUILayout.Button(path, GUILayout.Width(130), GUILayout.Height(30)))
                {
                    OpenScene(allScenes[i].path);
                }
            }

            GUILayout.BeginArea(new Rect(160, 2, 50, 1000));
            for (int i = 0; i < allScenes.Length; i++)
            {
                path = Path.GetFileNameWithoutExtension(allScenes[i].path);

                if (GUILayout.Button("+", GUILayout.Width(30), GUILayout.Height(30)))
                {
                    OpenSceneAdd(allScenes[i].path);
                }
            }
            GUILayout.EndArea();

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        GUI.EndGroup();
    }

    void OpenScene(string path)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path);
        }
    }
    void OpenSceneAdd(string path)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
        }
    }
}
#endif