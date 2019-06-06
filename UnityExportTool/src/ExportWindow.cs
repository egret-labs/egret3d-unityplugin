using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
namespace Egret3DExportTools
{
    public class ExportWindow : EditorWindow
    {
        private enum ExportType
        {
            NONE, PREFAB, SCENE
        }
        public const string VERSION = "v1.3.9";//版本号
        private const float WIN_WIDTH = 500.0f;
        private const float WIN_HEIGHT = 400.0f;
        private const float SMALL_SPACE = 10.0f;
        private const float SPACE = 20.0f;

        private static GUIStyle _showStyle = new GUIStyle();//Label的样式

        /**
         * 初始化插件窗口
         */
        [MenuItem("Egret3DExportTools/OpenWindow")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow<ExportWindow>(true, "Egret3D Export Tools" + VERSION);
            window.minSize = new Vector2(WIN_WIDTH, WIN_HEIGHT);
            window.maxSize = window.minSize;
            window.Show();
        }
        /**
         * 导出预制体
         */
        public static void ExportPrefabs()
        {
            var selectionObjs = Selection.gameObjects;
            foreach (var selectionObj in selectionObjs)
            {
                //防止egret 序列化报错
                var saveParent = selectionObj.transform.parent;
                selectionObj.transform.parent = null;
                PathHelper.SetOutPutPath(ExportConfig.instance.exportPath, selectionObj.name);
                ExportPrefabTools.ExportPrefab(selectionObj, PathHelper.OutPath);
                selectionObj.transform.parent = saveParent;
            }
        }
        /**
         * 导出场景
         */
        public static void ExportCurScene()
        {
            ExportExtendTools.CleanupMissingScripts();
            //获取场景中的根gameObject
            List<GameObject> roots = new List<GameObject>();
            GameObject[] allObjs = GameObject.FindObjectsOfType<GameObject>();
            for (int i = 0; i < allObjs.Length; i++)
            {
                var tempObj = allObjs[i];
                while (tempObj.transform.parent != null)
                {
                    tempObj = tempObj.transform.parent.gameObject;
                }
                if (!roots.Contains(tempObj))
                {
                    roots.Add(tempObj);
                }
            }
            ExportSceneTools.ExportScene(roots, PathHelper.OutPath);
        }


        //
        private Vector2 _scrollPosition = new Vector2(0, 0);
        private string _info = "就绪";
        private bool _isBuzy = false;
        private int _frameCount = 0;
        private ExportType _curExportType;

        /*导出类型的单选框状态*/
        private bool _resourceToolOpen = false;//资源
        private bool _sceneToolOpen = false;//场景

        private bool _lightSetting = true;
        private bool _meshSetting = false;
        private SerializedObject _serializeObject;
        private SerializedProperty _meshIgnoresProperty;

        void OnEnable()
        {
            _showStyle.fontSize = 15;
            _showStyle.normal.textColor = new Color(1, 0, 0, 1);

            //
            _serializeObject = new SerializedObject(ExportToolsSetting.instance);
            _meshIgnoresProperty = _serializeObject.FindProperty("meshIgnores");
            //

            //加载配置文件
            ExportConfig.Reload(PathHelper.ConfigPath, PathHelper.SaveRootDirectory);
            //初始化一些全局的方法
            SerializeObject.Initialize();
        }

        /**
         * 绘制窗口
         */
        void OnGUI()
        {
            var setting = ExportToolsSetting.instance;
            this._scrollPosition = GUILayout.BeginScrollView(this._scrollPosition, GUILayout.Width(WIN_WIDTH), GUILayout.Height(400));
            GUILayout.Space(SMALL_SPACE);
            //------------------------目录选择------------------------
            {
                GUILayout.Label("当前导出路径");
                GUILayout.BeginHorizontal();
                GUILayout.TextField(ExportConfig.instance.exportPath);
                if (GUILayout.Button("选择目录", GUILayout.Width(100)))
                {
                    ExportConfig.instance.exportPath = EditorUtility.OpenFolderPanel("当前要导出的路径", Application.dataPath, "");
                    ExportConfig.instance.Save(PathHelper.ConfigPath);
                    AssetDatabase.Refresh();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(SPACE);
            //------------------------根目录------------------------
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("根目录");
                var dir = GUILayout.TextField(ExportConfig.instance.rootDir);
                if(dir != ExportConfig.instance.rootDir)
                {
                    ExportConfig.instance.rootDir = dir;
                    Debug.Log("根目录：" + dir);
                    ExportConfig.instance.Save(PathHelper.ConfigPath);
                    AssetDatabase.Refresh();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(SPACE);
            //------------------------辅助选项------------------------
            {
                GUILayout.BeginHorizontal();
                setting.debugLog = GUILayout.Toggle(setting.debugLog, new GUIContent("输出日志", "勾选后，方便查看输出信息"));
                setting.prefabResetPos = GUILayout.Toggle(setting.prefabResetPos, new GUIContent("坐标归零", "勾选后，将导出的预制体坐标归零"));
                setting.exportOriginalImage = GUILayout.Toggle(setting.exportOriginalImage, new GUIContent("导出原始图片", "勾选后，jpg和png会直接使用原始图片导出"));
                // ExportToolsSetting.unityNormalTexture = GUILayout.Toggle(ExportToolsSetting.unityNormalTexture, new GUIContent("使用Unity法线贴图", "勾选后，时使用Unity转换后的法线贴图导出"));

                GUILayout.EndHorizontal();
            }
            GUILayout.Space(SPACE);
            this._lightSetting = EditorGUILayout.Foldout(this._lightSetting, "光照设置");
            if (this._lightSetting)
            {
                GUILayout.BeginVertical();
                setting.lightType = (ExportLightType)EditorGUILayout.EnumPopup(setting.lightType, GUILayout.MaxWidth(100));
                GUILayout.EndVertical();
            }
            GUILayout.Space(SMALL_SPACE);
            this._meshSetting = EditorGUILayout.Foldout(this._meshSetting, "网格设置");
            if (this._meshSetting)
            {
                GUILayout.BeginHorizontal();
                setting.enableNormals = GUILayout.Toggle(setting.enableNormals, new GUIContent("Normals", "取消后，不导出Normals"));
                setting.enableColors = GUILayout.Toggle(setting.enableColors, new GUIContent("Colors", "取消后，不导出Colors"));
                setting.enableBones = GUILayout.Toggle(setting.enableBones, new GUIContent("Bones", "取消后，不导出Bones"));
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                this._serializeObject.Update();
                if (this._meshIgnoresProperty != null)
                {
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(this._meshIgnoresProperty, new GUIContent("忽略对象:", "在忽略列表中的对象网格属性全部导出"), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        _serializeObject.ApplyModifiedProperties();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(SPACE);
            //------------------------主功能------------------------
            {
                //资源导出
                _resourceToolOpen = GUILayout.Toggle(_resourceToolOpen, "--------资源导出工具--------");
                if (_resourceToolOpen)
                {
                    GUILayout.Space(SPACE);
                    GUILayout.BeginHorizontal();
                    if (Selection.activeGameObject)
                    {
                        if (GUILayout.Button("导出当前选中对象"))
                        {
                            _frameCount = 0;
                            _isBuzy = true;
                            _info = "导出中...";
                            _curExportType = ExportType.PREFAB;
                        }
                    }
                    else
                    {
                        GUILayout.Label("请选中场景中要导出的对象", _showStyle);
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(SPACE);
                }
                //场景导出
                _sceneToolOpen = GUILayout.Toggle(_sceneToolOpen, "--------场景导出工具--------");
                if (_sceneToolOpen)
                {
                    GUILayout.Space(SPACE);
                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("导出当前场景"))
                    {
                        PathHelper.SetOutPutPath(ExportConfig.instance.exportPath, PathHelper.CurSceneName);

                        _frameCount = 0;
                        _info = "导出中...";
                        _isBuzy = true;
                        _curExportType = ExportType.SCENE;
                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(SPACE);
                }
            }

            GUILayout.EndScrollView();
            GUI.Label(new Rect(0, WIN_HEIGHT - 15, WIN_WIDTH, 15), "状态：" + _info);
        }

        void OnInspectorUpdate()
        {
            this.Repaint();
        }

        void Update()
        {
            if (!_isBuzy)
            {
                return;
            }

            _frameCount++;
            //第二帧再处理，保证能播起来
            if (_frameCount == 2)
            {
                switch (_curExportType)
                {
                    case ExportType.PREFAB:
                        ExportPrefabs();
                        break;
                    case ExportType.SCENE:
                        ExportCurScene();
                        break;
                    default:
                        break;
                }

                //导出完毕后，恢复初始值
                _frameCount = 0;
                _isBuzy = false;
                _curExportType = ExportType.NONE;
                _info = "就绪";
            }
        }


        // public static void BakeSkinnedMeshRenderer()
        // {
        //     var selection = Selection.activeGameObject;
        //     if (selection == null)
        //     {
        //         return;
        //     }
        //     var skinned = selection.GetComponentInChildren<SkinnedMeshRenderer>();
        //     if (skinned == null || skinned.sharedMesh == null)
        //     {
        //         return;
        //     }
        //     //
        //     var mesh = new Mesh();
        //     skinned.BakeMesh(mesh);
        //     var url = UnityEditor.AssetDatabase.GetAssetPath(skinned.sharedMesh);
        //     string name = selection.name + ".asset";
        //     url = url.Substring(0, url.LastIndexOf("/") + 1) + name;
        //     AssetDatabase.CreateAsset(mesh, url);
        // }
    }
}
