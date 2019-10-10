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
        private const string VERSION = "v1.4.0";//版本号
        private const float WIN_WIDTH = 500.0f;
        private const float WIN_HEIGHT = 400.0f;
        private const float SMALL_SPACE = 10.0f;
        private const float SPACE = 20.0f;

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
                PathHelper.SetOutPutPath(ExportSetting.instance.exportDir, selectionObj.name);
                ExportPrefab.Export(selectionObj, PathHelper.OutPath);
                selectionObj.transform.parent = saveParent;
            }
        }
        /**
         * 导出场景
         */
        public static void ExportScene()
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
            Egret3DExportTools.ExportScene.Export(roots, PathHelper.OutPath);
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
        private bool _sceneSetting = false;
        private bool _textureSetting = false;
        private bool _meshSetting = false;

        void OnEnable()
        {
            //加载配置文件
            ExportSetting.Reload(PathHelper.ConfigPath, PathHelper.SaveRootDirectory);
            //初始化一些全局的方法
            SerializeObject.Initialize();
        }

        /**
         * 绘制窗口
         */
        void OnGUI()
        {
            this._scrollPosition = GUILayout.BeginScrollView(this._scrollPosition, GUILayout.Width(WIN_WIDTH), GUILayout.Height(WIN_HEIGHT));
            GUILayout.Space(SMALL_SPACE);
            //------------------------路径选择------------------------
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("根目录");
                var dir = GUILayout.TextField(ExportSetting.instance.rootName);
                if (dir != ExportSetting.instance.rootName)
                {
                    ExportSetting.instance.rootName = dir;
                }
                GUILayout.EndHorizontal();
            }
            {
                GUILayout.BeginHorizontal();
                GUILayout.TextField(ExportSetting.instance.exportDir);
                if (GUILayout.Button("导出路径", GUILayout.Width(100)))
                {
                    ExportSetting.instance.exportDir = EditorUtility.OpenFolderPanel("当前要导出的路径", Application.dataPath, "");
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(SPACE);
            //------------------------辅助选项------------------------
            {
                GUILayout.BeginHorizontal();
                var common = ExportSetting.instance.common;
                common.debugLog = GUILayout.Toggle(common.debugLog, new GUIContent("输出日志", "勾选后，方便查看输出信息"));
                common.jsonFormatting = GUILayout.Toggle(common.jsonFormatting, new GUIContent("Json格式化", "勾选后，格式化导出的Json文件"));
                common.posToZero = GUILayout.Toggle(common.posToZero, new GUIContent("坐标归零", "勾选后，将导出的预制体坐标归零"));
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(SMALL_SPACE);
            {
                this._lightSetting = EditorGUILayout.Foldout(this._lightSetting, "光照设置");
                if (this._lightSetting)
                {
                    GUILayout.BeginVertical();
                    var lightSetting = ExportSetting.instance.light;
                    lightSetting.type = (ExportLightType)EditorGUILayout.EnumPopup(lightSetting.type, GUILayout.MaxWidth(100));
                    GUILayout.EndVertical();
                }
            }

            GUILayout.Space(SMALL_SPACE);
            {
                this._sceneSetting = EditorGUILayout.Foldout(this._sceneSetting, "场景设置");
                if (this._sceneSetting)
                {
                    GUILayout.BeginHorizontal();
                    var sceneSetting = ExportSetting.instance.scene;
                    sceneSetting.lightmap = GUILayout.Toggle(sceneSetting.lightmap, new GUIContent("光照贴图", "勾选后，如果场景有光照贴图，就会导出光照贴图"));
                    sceneSetting.staticBatching = GUILayout.Toggle(sceneSetting.staticBatching, new GUIContent("静态合并", "勾选后，场景加载完毕后会尝试静态合并。"));
                    sceneSetting.fog = GUILayout.Toggle(sceneSetting.fog, new GUIContent("雾", "勾选后，如果场景开启了雾，则会导出对应的参数。"));
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.Space(SMALL_SPACE);
            {
                this._textureSetting = EditorGUILayout.Foldout(this._textureSetting, "图片设置");
                if (this._textureSetting)
                {
                    GUILayout.BeginVertical();
                    var textureSetting = ExportSetting.instance.texture;
                    textureSetting.useOriginalTexture = GUILayout.Toggle(textureSetting.useOriginalTexture, new GUIContent("导出原始图片", "勾选后，jpg和png会直接使用原始图片导出"));
                    textureSetting.useNormalTexture = GUILayout.Toggle(textureSetting.useNormalTexture, new GUIContent("使用Unity法线贴图", "勾选后，使用Unity转换后的法线贴图导出"));
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("JPG导出质量:");
                    textureSetting.jpgQuality = (int)GUILayout.HorizontalSlider(textureSetting.jpgQuality, 0.0f, 100.0f, GUILayout.Width(250));
                    GUILayout.Label(textureSetting.jpgQuality.ToString());
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                }
            }

            GUILayout.Space(SMALL_SPACE);
            {
                this._meshSetting = EditorGUILayout.Foldout(this._meshSetting, "网格设置");
                if (this._meshSetting)
                {
                    GUILayout.BeginHorizontal();
                    var meshSetting = ExportSetting.instance.mesh;
                    meshSetting.uv2 = GUILayout.Toggle(meshSetting.uv2, new GUIContent("UV2", "取消后，不导出UV2"));
                    meshSetting.normal = GUILayout.Toggle(meshSetting.normal, new GUIContent("Normals", "取消后，不导出Normals"));
                    meshSetting.color = GUILayout.Toggle(meshSetting.color, new GUIContent("Colors", "取消后，不导出Colors"));
                    meshSetting.bone = GUILayout.Toggle(meshSetting.bone, new GUIContent("Bones", "取消后，不导出Bones"));
                    meshSetting.tangent = GUILayout.Toggle(meshSetting.tangent, new GUIContent("Tangents", "取消后，不导出Tangents"));
                    GUILayout.EndHorizontal();
                }
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
                        if (GUILayout.Button("预制体导出"))
                        {
                            _frameCount = 0;
                            _isBuzy = true;
                            _info = "导出中...";
                            _curExportType = ExportType.PREFAB;
                        }
                    }
                    else
                    {
                        GUILayout.Label("请选中场景中要导出的对象");
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
                    if (GUILayout.Button("场景导出"))
                    {
                        PathHelper.SetOutPutPath(ExportSetting.instance.exportDir, PathHelper.CurSceneName);

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
            if (GUI.Button(new Rect(WIN_WIDTH - 65, WIN_HEIGHT - 20, 60, 20), "保存配置"))
            {
                ExportSetting.instance.Save(PathHelper.ConfigPath);
                AssetDatabase.Refresh();
            }
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
                        ExportScene();
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
    }
}
