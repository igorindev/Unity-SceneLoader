using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_2021_2_OR_NEWER
using UnityEditor.Toolbars;
using UnityEditor.Overlays;
#endif

namespace SceneLoader
{
    [InitializeOnLoad, ExecuteInEditMode]
    public class SceneLoaderEditor : EditorWindow
    {
        static SceneLoaderEditor()
        {
            
        }

        public static bool open = false;

        Vector2 scroll = Vector2.zero;
        static string sceneName;
        static string[] allScenes;
        static Texture2D back;
        public static SceneLoaderEditor window;

        static List<Scene> Scenes = new List<Scene>();

        static Texture plus, remove;
        static Rect SceneViewPosition;

        static string path;

        static SceneLoaderGroup scriptableObject;

#if !UNITY_2021_2_OR_NEWER
        static SceneLoaderEditor()
        {
            SceneView.duringSceneGui += OnScene;
        }
        private static void OnScene(SceneView sceneview)
        {
            GUIStyle mystyle = new GUIStyle("Button")
            {
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                padding = new RectOffset(0, 0, 0, 0)
            };
            Handles.BeginGUI();
            if (GUI.Button(new Rect(6, 6, 57, 20), "Scenes", mystyle))
                ShowWindow();

            Handles.EndGUI();
        }
#endif

        public static void ShowWindow()
        {
            if (string.IsNullOrEmpty(path))
            {
                string[] guid = AssetDatabase.FindAssets("t:SceneLoaderGroup");
                path = AssetDatabase.GUIDToAssetPath(guid[0]);
            }
            if (scriptableObject == null)
            {
                scriptableObject = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) as SceneLoaderGroup;
            }
            allScenes = scriptableObject.scenes; 

            plus = scriptableObject.addTex;
            remove = scriptableObject.removeTex;

            window = CreateInstance<SceneLoaderEditor>();

            SceneViewPosition = GetWindow<SceneView>().position;

            window.position = new Rect(SceneViewPosition.x, SceneViewPosition.y + 40f, 240, Mathf.Clamp((32 * allScenes.Length), SceneViewPosition.height - 22, SceneViewPosition.height - 22));
            window.ShowPopup();

            Scenes = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scenes.Add(SceneManager.GetSceneAt(i));
            }
        }

        void OnEnable()
        {
            back = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            back.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f));
            back.Apply();
        }

        void OnLostFocus()
        {
            Close();
        }

        void OnGUI()
        {
            GUIStyle b = new GUIStyle(GUI.skin.textArea);
            b.normal.background = back;

            GUIStyle mystyle = new GUIStyle("Button")
            {
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                padding = new RectOffset(0, 0, 0, 0)
            };

            using (new GUI.GroupScope(new Rect(0, 0, Screen.width, Screen.height), b))
            {
                GUILayout.Space(8);
                using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(225)))
                {
                    GUILayout.Space(13);

                    GUILayout.Label("Scenes", EditorStyles.boldLabel);

                    if (GUILayout.Button(EditorGUIUtility.IconContent("d__Popup", "Edit Scenes"), mystyle, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        Selection.SetActiveObjectWithContext(AssetDatabase.LoadAssetAtPath<ScriptableObject>(path), null);
                        GetWindow(System.Type.GetType("UnityEditor.InspectorWindow, UnityEditor"));
                    }
                    if (GUILayout.Button(EditorGUIUtility.IconContent("d_winbtn_win_close", "Edit Scenes"), mystyle, GUILayout.Width(20), GUILayout.Height(20)))
                    {
                        Close();
                    }
                }

                GUI.Label(new Rect(15, 40, 80, 20), "Single", EditorStyles.boldLabel);
                GUI.Label(new Rect(170, 40, 80, 20), "Additive", EditorStyles.boldLabel);

                if (Application.isPlaying)
                {
                    GUI.Label(new Rect(15, 70, 1000, 1000), "Disabled during PlayMode", EditorStyles.largeLabel);
                }
                else
                {
                    if (allScenes.Length > 0)
                    {
                        {
                            using (var ScrollView = new GUI.ScrollViewScope(new Rect(10, 65, 220, Screen.height - 70), scroll, new Rect(0, 32.5f, 0, allScenes.Length * 32.5f)))
                            {
                                scroll = ScrollView.scrollPosition;

                                for (int i = 0; i < allScenes.Length; i++)
                                {
                                    using (new GUILayout.HorizontalScope())
                                    {
                                        sceneName = Path.GetFileNameWithoutExtension(allScenes[i]);

                                        if (GUILayout.Button(sceneName, GUILayout.Width(130), GUILayout.Height(30)))
                                        {
                                            OpenScene(allScenes[i]);
                                        }

                                        GUILayout.Space(20);

                                        bool isLoaded = false;
                                        int id = 0;
                                        sceneName = Path.GetFileNameWithoutExtension(allScenes[i]);
                                        for (int j = 0; j < Scenes.Count; j++)
                                        {
                                            if (Scenes[j].name == sceneName)
                                            {
                                                isLoaded = true;
                                                id = j;
                                            }
                                        }

                                        if (!isLoaded)
                                        {
                                            if (GUILayout.Button(plus, GUILayout.Width(30), GUILayout.Height(30)))
                                            {
                                                OpenSceneAdd(allScenes[i]);
                                            }
                                        }
                                        else
                                        {
                                            if (GUILayout.Button(remove, GUILayout.Width(30), GUILayout.Height(30)))
                                            {
                                                RemoveScene(id);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                Texture2D ss = new Texture2D(1, 1);

                GUI.DrawTexture(new Rect(-1, 0, 242, window.position.height + 2), ss, ScaleMode.StretchToFill, false, 0, new Color(0.33f, 0.33f, 0.33f, 1f), new Vector4(4, 0, 4, 4), 0); ;
                GUI.DrawTexture(new Rect(1, 1, 238, window.position.height - 2), ss, ScaleMode.StretchToFill, true, 0, new Color(1, 1, 1, 0.7f), 1f, 4);
            }
        }

        void OpenScene(string path)
        {
            if (EditorSceneManager.EnsureUntitledSceneHasBeenSaved("The Current Scene is Untitled"))
            {
                if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(Scenes.ToArray()))
                {
                    Scenes = new List<Scene>();
                    Scenes.Add(EditorSceneManager.OpenScene(path));
                }
            }
            else
            {
                if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(Scenes.ToArray()))
                {
                    Scenes = new List<Scene>();
                    Scenes.Add(EditorSceneManager.OpenScene(path));
                }
            }
        }
        void OpenSceneAdd(string path)
        {
            Scenes.Add(EditorSceneManager.OpenScene(path, OpenSceneMode.Additive));
        }
        void RemoveScene(int id)
        {
            bool noUntitledSceneFound = EditorSceneManager.EnsureUntitledSceneHasBeenSaved("The Current Scene is Untitled");

            if (EditorSceneManager.SaveModifiedScenesIfUserWantsTo(new Scene[1] { Scenes[id] }))
            {
                if (EditorSceneManager.CloseScene(Scenes[id], true))
                {
                    Scenes.RemoveAt(id);
                }
            }
        }

        static string ReadSetupData(ScriptableObject obj)
        {
            MonoScript ms = MonoScript.FromScriptableObject(obj);
            string path = AssetDatabase.GetAssetPath(ms);

            FileInfo fi = new FileInfo(path);
            path = fi.Directory.ToString();
            path.Replace('\\', '/');

            Debug.Log(path);
            return path;
        }
    }
    public static class SceneLoaderFunctions
    {
        public static System.Type[] GetAllDerivedTypes(this System.AppDomain aAppDomain, System.Type aType)
        {
            var result = new List<System.Type>();
            var assemblies = aAppDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsSubclassOf(aType))
                        result.Add(type);
                }
            }
            return result.ToArray();
        }

        public static Rect GetEditorMainWindowPos()
        {
            var containerWinType = System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(ScriptableObject)).Where(t => t.Name == "ContainerWindow").FirstOrDefault();
            if (containerWinType == null)
                throw new System.MissingMemberException("Can't find internal type ContainerWindow. Maybe something has changed inside Unity");
            var showModeField = containerWinType.GetField("m_ShowMode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var positionProperty = containerWinType.GetProperty("position", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (showModeField == null || positionProperty == null)
                throw new System.MissingFieldException("Can't find internal fields 'm_ShowMode' or 'position'. Maybe something has changed inside Unity");
            var windows = Resources.FindObjectsOfTypeAll(containerWinType);
            foreach (var win in windows)
            {
                var showmode = (int)showModeField.GetValue(win);
                if (showmode == 4) // main window
                {
                    var pos = (Rect)positionProperty.GetValue(win, null);
                    return pos;
                }
            }
            throw new System.NotSupportedException("Can't find internal main window. Maybe something has changed inside Unity");
        }
    }

#if UNITY_2021_2_OR_NEWER
    [EditorToolbarElement(id, typeof(SceneView))]
    class SceneLoaderOverlay : EditorToolbarButton, IAccessContainerWindow
    {
        public const string id = "ToolBar/SceneLoader";
        public EditorWindow containerWindow { get; set; }

        public SceneLoaderOverlay()
        {
            text = "";
            icon = EditorGUIUtility.IconContent("winbtn_win_restore_h", "").image as Texture2D;

            tooltip = "Open a menu that shows all assigned scenes, allowing to load then.";
            clicked += OnClick;
        }

        void OnClick()
        {
            SceneLoaderEditor.ShowWindow();
        }
    }

    [Overlay(typeof(SceneView), "Scene Loader")]
    public class SceneLoaderToolBar : ToolbarOverlay
    {
        SceneLoaderToolBar() : base(SceneLoaderOverlay.id) { }
    }
#endif
}