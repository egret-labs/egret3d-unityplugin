using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FB.PosePlus;
using Egret3DExportTools;

/**
 * 扩展 Animator 编辑面板
 */
[CustomEditor(typeof(Animator))]
public class Animator_Inspector : Editor
{
    Dictionary<string, float> anipos = new Dictionary<string, float>();
    //DateTime last = DateTime.Now;
    bool bFuncSrc = true;
    bool bFuncSplit = true;
    bool bFuncAni = true;
    int maxBone = 55;
    public UnityEngine.Object objQte;
    public UnityEngine.Object objRole;
    public override void OnInspectorGUI()
    {
        if (this.target == null) return;

        bFuncSrc = GUILayout.Toggle(bFuncSrc, "====原生功能====");
        if (bFuncSrc)
        {
            base.OnInspectorGUI();
            GUILayout.Space(16);
        }
        if (Application.isPlaying)
            return;

        bFuncSplit = GUILayout.Toggle(bFuncSplit, "====骨骼数拆解功能====");
        if (bFuncSplit)
        {
            this.OnGUI_Split();
            GUILayout.Space(16);
        }

        bFuncAni = GUILayout.Toggle(bFuncAni, "====动画功能====");
        if (bFuncAni)
        {
            this.OnGUI_Ani();
            GUILayout.Space(16);
        }

        GUILayout.BeginHorizontal();
        GUILayout.TextArea("主角");
        objRole = EditorGUILayout.ObjectField(objRole, typeof(UnityEngine.Object), true);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.TextArea("QTE");
        objQte = EditorGUILayout.ObjectField(objQte, typeof(UnityEngine.Object), true);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("CompareBones"))
        {
            SkinnedMeshRenderer[] roleMeshs = ((GameObject)objRole).GetComponentsInChildren<SkinnedMeshRenderer>();
            SkinnedMeshRenderer[] qteMeshs = ((GameObject)objQte).GetComponentsInChildren<SkinnedMeshRenderer>();

            Dictionary<string, string> dicQte = new Dictionary<string, string>();

            foreach (SkinnedMeshRenderer mesh in qteMeshs)
            {
                Transform[] bones0 = mesh.bones;
                foreach (Transform tr in bones0)
                {
                    dicQte.Add(tr.name, tr.name);
                }
            }

            foreach (SkinnedMeshRenderer mesh in roleMeshs)
            {
                Transform[] bones0 = mesh.bones;
                foreach (Transform tr in bones0)
                {
                    if (!dicQte.ContainsKey(tr.name))
                    {
                        Debug.LogError("骨骼不匹配：" + tr.name);
                    }
                }
            }
        }
    }


    /*--------动画拆解--------*/
    /**
	 * 骨骼数拆解
	 */
    void OnGUI_Split()
    {
        Animator a = target as Animator;
        SkinnedMeshRenderer[] rs = a.GetComponentsInChildren<SkinnedMeshRenderer>();
        List<Transform> totalbones = new List<Transform>();

        foreach (var r in rs)
        {
            foreach (var b in r.bones)
            {
                if (totalbones.Contains(b) == false)
                    totalbones.Add(b);
            }
        }

        GUILayout.Label("bone for all: " + totalbones.Count);
        maxBone = EditorGUILayout.IntField("max bone for one part: ", maxBone);
        GUILayout.Label("has part: " + rs.Length);
        foreach (var r in rs)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("part bone: " + r.bones.Length + "||" + r.name, GUILayout.Width(250));
            if (r.bones.Length >= maxBone)
            {
                if (GUILayout.Button("split the part", GUILayout.Width(100)))
                {
                    DoSplit(maxBone, r);
                }
            }
            GUILayout.EndHorizontal();
        }
    }

    public static void DoSplit(int maxBone, SkinnedMeshRenderer srcr)
    {
        SkinMesh src = new SkinMesh();
        src.bone = new List<Transform>(srcr.bones);
        src.mesh = srcr.sharedMesh;
        var ss = SplitSkinMesh(maxBone, src);
        if (ss.Count > 0)
        {
            srcr.sharedMesh = ss[0].mesh;
            srcr.bones = ss[0].bone.ToArray();
        }
        for (int i = 1; i < ss.Count; i++)
        {
            GameObject newSplit = new GameObject();
            newSplit.name = srcr.name + "_" + i;
            newSplit.transform.parent = srcr.transform.parent;
            newSplit.transform.localPosition = srcr.transform.localPosition;
            newSplit.transform.localScale = srcr.transform.localScale;
            newSplit.transform.localRotation = srcr.transform.localRotation;

            var nr = newSplit.AddComponent<SkinnedMeshRenderer>();
            nr.rootBone = srcr.rootBone;
            nr.sharedMesh = ss[i].mesh;
            nr.bones = ss[i].bone.ToArray();
            nr.sharedMaterials = srcr.sharedMaterials;
        }
    }

    class SkinMesh
    {
        public List<Transform> bone;
        public Mesh mesh;
    }

    static List<SkinMesh> SplitSkinMesh(int maxBone, SkinMesh srcMesh)
    {
        List<SkinMesh> splitMesh = new List<SkinMesh>();//拆解的模型
        List<int> seeks = new List<int>();//每个子模型的处理进度
        if (srcMesh == null || srcMesh.mesh == null)
        {
            splitMesh.Add(srcMesh);
            return splitMesh;
        }
        Debug.Log("submesh = " + srcMesh.mesh.subMeshCount);
        for (int s = 0; s < srcMesh.mesh.subMeshCount; s++)
        {
            seeks.Add(0);
        }
        for (int loopCount = 0; loopCount < 100; loopCount++)
        {
            var m = new SkinMesh();
            FillSkinMesh(srcMesh, maxBone, seeks, m);
            splitMesh.Add(m);

            bool bexit = true;
            for (int s = 0; s < seeks.Count; s++)
            {
                if (seeks[s] < srcMesh.mesh.GetIndices(s).Length)
                {
                    bexit = false;
                    break;
                }
            }
            if (bexit) break;
        }
        for (int i = 0; i < splitMesh.Count; i++)
        {
            if (splitMesh.Count > 1)
            {
                splitMesh[i].mesh.name = srcMesh.mesh.name + "_" + i;
            }
            else
            {
                splitMesh[i].mesh.name = srcMesh.mesh.name;
            }
        }
        return splitMesh;
    }

    static void FillSkinMesh(SkinMesh srcMesh, int maxBone, List<int> seek, SkinMesh outMesh)
    {
        outMesh.bone = new List<Transform>();
        outMesh.mesh = new Mesh();
        Dictionary<int, int> indexmap = new Dictionary<int, int>();
        Dictionary<int, int> bonemap = new Dictionary<int, int>();
        List<Vector3> poses = new List<Vector3>();
        List<Vector3> normal = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<Vector2> uv2 = null;
        List<Vector2> uv3 = null;
        List<Color32> color = null;
        List<BoneWeight> skininfo = new List<BoneWeight>();
        List<Matrix4x4> tpose = new List<Matrix4x4>();

        List<List<int>> subindexs = new List<List<int>>();
        for (int s = 0; s < seek.Count; s++)
        {
            subindexs.Add(new List<int>());
        }

        //拆分逻辑
        for (int loopcount = 0; loopcount < 100000; loopcount++)//死循环，直到超出限制
        {
            bool bexit = true;
            for (int s = 0; s < seek.Count; s++)
            {
                var subindex = subindexs[s];
                var si = srcMesh.mesh.GetIndices(s);
                if (si.Length > seek[s])//有才处理
                {
                    bexit = false;
                    for (int i = 0; i < 3; i++)//一次处理一个三角形
                    {
                        int srcindex = si[seek[s] + i];
                        if (indexmap.ContainsKey(srcindex) == false)//新顶点
                        {
                            indexmap[srcindex] = poses.Count;
                            poses.Add(srcMesh.mesh.vertices[srcindex]);
                            normal.Add(srcMesh.mesh.normals[srcindex]);
                            if (srcMesh.mesh.uv != null && srcMesh.mesh.uv.Length > 0)
                            {
                                if (uv == null) uv = new List<Vector2>();
                                uv.Add(srcMesh.mesh.uv[srcindex]);
                            }

#if UNITY4
                            if (srcMesh.mesh.uv1 != null && srcMesh.mesh.uv1.Length > 0)
							{
								if (uv2 == null) uv2 = new List<Vector2>();
								uv2.Add(srcMesh.mesh.uv1[srcindex]);
							}
							if (srcMesh.mesh.uv2 != null && srcMesh.mesh.uv2.Length > 0)
							{
								if (uv3 == null) uv3 = new List<Vector2>();
								uv3.Add(srcMesh.mesh.uv2[srcindex]);
							}
#else
                            if (srcMesh.mesh.uv2 != null && srcMesh.mesh.uv2.Length > 0)
                            {
                                if (uv2 == null) uv2 = new List<Vector2>();
                                uv2.Add(srcMesh.mesh.uv2[srcindex]);
                            }
                            if (srcMesh.mesh.uv3 != null && srcMesh.mesh.uv3.Length > 0)
                            {
                                if (uv3 == null) uv3 = new List<Vector2>();
                                uv3.Add(srcMesh.mesh.uv3[srcindex]);
                            }
#endif

                            if (srcMesh.mesh.colors32 != null && srcMesh.mesh.colors32.Length > 0)
                            {
                                if (color == null) color = new List<Color32>();
                                color.Add(srcMesh.mesh.colors32[srcindex]);
                            }

                            BoneWeight bw = srcMesh.mesh.boneWeights[srcindex];
                            bw.boneIndex0 = TestBone(srcMesh, outMesh, tpose, bonemap, bw.boneIndex0);
                            bw.boneIndex1 = TestBone(srcMesh, outMesh, tpose, bonemap, bw.boneIndex1);
                            bw.boneIndex2 = TestBone(srcMesh, outMesh, tpose, bonemap, bw.boneIndex2);
                            bw.boneIndex3 = TestBone(srcMesh, outMesh, tpose, bonemap, bw.boneIndex3);
                            skininfo.Add(bw);
                        }
                        subindex.Add(indexmap[srcindex]);

                    }
                    seek[s] = seek[s] + 3;
                }
            }
            //加上骨骼的处理，终止条件改为骨骼数超越限制
            if (outMesh.bone.Count >= maxBone)
            {
                Debug.LogWarning("==break at:" + maxBone + "/" + outMesh.bone.Count);
                break;
            }
            else if (bexit)
            {
                Debug.LogWarning("==break for end.");
                break;
            }
        }
#if UNITY4
        outMesh.mesh.vertices = poses.ToArray();
		outMesh.mesh.normals = normal.ToArray();
		if (uv != null) outMesh.mesh.uv = uv.ToArray();
		if (uv2 != null) outMesh.mesh.uv1 = uv2.ToArray();
		if (uv3 != null) outMesh.mesh.uv2 = uv3.ToArray();
		if (color != null) outMesh.mesh.colors32 = color.ToArray();
#else
        outMesh.mesh.SetVertices(poses);
        outMesh.mesh.SetNormals(normal);
        if (uv != null) outMesh.mesh.SetUVs(0, uv);
        if (uv2 != null) outMesh.mesh.SetUVs(1, uv2);
        if (uv3 != null) outMesh.mesh.SetUVs(2, uv3);
        if (color != null) outMesh.mesh.SetColors(color);
#endif
        outMesh.mesh.subMeshCount = subindexs.Count;
        for (int i = 0; i < subindexs.Count; i++)
        {
            outMesh.mesh.SetTriangles(subindexs[i].ToArray(), i);
        }

        outMesh.mesh.boneWeights = skininfo.ToArray();

        outMesh.mesh.bindposes = tpose.ToArray();
        Debug.LogWarning("outmest.mesh" + outMesh.mesh.vertexCount + "," + outMesh.bone.Count + "," + outMesh.mesh.bindposes.Length);
        foreach (var b in outMesh.bone)
        {
            Debug.LogWarning("bone=" + b);
        }
    }

    private static int TestBone(SkinMesh srcMesh, SkinMesh outMesh, List<Matrix4x4> tpose, Dictionary<int, int> boneMap, int bone)
    {
        if (bone >= 0)
        {
            if (boneMap.ContainsKey(bone) == false)//addbone
            {
                boneMap[bone] = outMesh.bone.Count;
                outMesh.bone.Add(srcMesh.bone[bone]);
                tpose.Add(srcMesh.mesh.bindposes[bone]);
            }
            return boneMap[bone];
        }
        else
            return -1;
    }


    /*----------动画-----------------*/
    void OnGUI_Ani()
    {
        EditorGUILayout.HelpBox("你只需要把所有的动画拖入一个controller就行了，然后我们就会导出他", MessageType.Info);
        var ani = target as Animator;

        {

            List<UnityEngine.AnimationClip> clips = new List<UnityEngine.AnimationClip>();
#if UNITY4
            UnityEditorInternal.AnimatorController cc = ani.runtimeAnimatorController as UnityEditorInternal.AnimatorController;
#else
            //UnityEditor.Animations.AnimatorController cc = ani.runtimeAnimatorController as UnityEditor.Animations.AnimatorController;
            RuntimeAnimatorController cc = ani.runtimeAnimatorController;
#endif
            if (cc != null)
            {
                FindAllAniInControl(cc, clips);
                if (ani.gameObject.GetComponent<FB.PosePlus.AniPlayer>() == null)
                {
                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("Create FBAni Component(FILL)"))
                    {
                        var ccc = ani.gameObject.AddComponent<FB.PosePlus.AniPlayer>();
                        ccc.InitBone();
                        foreach (var c in clips)
                        {
                            CloneAni(c, c.frameRate);
                        }
                    }
                    if (GUILayout.Button("Create FBAni Component(ONLY)"))
                    {
                        var ccc = ani.gameObject.AddComponent<FB.PosePlus.AniPlayer>();
                        ccc.InitBone();

                    }
                    GUILayout.EndHorizontal();

                }
                GUILayout.Label("拥有动画:" + clips.Count);
                //}
                foreach (var c in clips)
                {
                    if (c == null) continue;
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(c.name + "(" + c.length * c.frameRate + ")" + (c.isLooping ? "loop" : ""));


                    if (GUILayout.Button("Create FBAni", GUILayout.Width(150)))
                    {
                        CloneAni(c, c.frameRate);
                    }
                    GUILayout.EndHorizontal();
                    if (anipos.ContainsKey(c.name) == false)
                    {
                        anipos[c.name] = 0;
                    }
                    float v = anipos[c.name];
                    v = GUILayout.HorizontalSlider(v, 0, c.length);
                    if (v != anipos[c.name])
                    {
                        Debug.Log("setani");
                        anipos[c.name] = v;
                        ani.Play(mapClip2State[c.name], 0, v / c.length);
                        ani.updateMode = AnimatorUpdateMode.UnscaledTime;
                        ani.Update(0);// 0.001f);

                    }
                }
            }
        }
    }

    void CloneAni(UnityEngine.AnimationClip clip, float fps)
    {

        var ani = target as Animator;
        CloneAni(clip, fps, ani);
    }
    //从一个Animator中获取所有的Animation
    public static void CloneAni(UnityEngine.AnimationClip clip, float fps, Animator ani)
    {
        //创建CleanData.Ani
        FB.PosePlus.AniClip _clip = ScriptableObject.CreateInstance<FB.PosePlus.AniClip>();
        _clip.boneinfo = new List<string>();//也增加了每个动画中的boneinfo信息.

        //这里重新检查动画曲线，找出动画中涉及的Transform部分，更精确
        List<Transform> cdpath = new List<Transform>();
        var curveDatas = AnimationUtility.GetCurveBindings(clip);
        //AnimationClipCurveData[] curveDatas = AnimationUtility.GetAllCurves(clip, true);
        foreach (var dd in curveDatas)
        {
            Transform tran = ani.transform.Find(dd.path);
            if (tran == null)
            {
                Debug.LogWarning("trans not found:" + dd.path);
                //丢弃无法被找到的动画通道
            }
            else
            {
                if (cdpath.Contains(tran) == false)
                {
                    _clip.boneinfo.Add(tran.name);
                    cdpath.Add(tran);
                }
            }
        }
        FB.PosePlus.AniPlayer con = ani.gameObject.GetComponent<FB.PosePlus.AniPlayer>();
        foreach (var b in con.bones)
        {
            //if (b.bone.GetComponent<asbone>() != null)
            {
                //特别关注的骨骼
                if (_clip.boneinfo.Contains(b.bone.name) == false)
                {
                    _clip.boneinfo.Add(b.bone.name);
                    cdpath.Add(b.bone);
                }
            }
        }
        Debug.LogWarning("curve got path =" + cdpath.Count);

        string path = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(clip.GetInstanceID()));
        _clip.name = clip.name;
        _clip.frames = new List<FB.PosePlus.Frame>();
        _clip.fps = fps;
        _clip.loop = clip.isLooping;

        float flen = (clip.length * fps);
        int framecount = (int)flen;
        if (flen - framecount > 0.0001) framecount++;
        //if (framecount < 1) framecount = 1;

        for (int i = 0, l = clip.events.Length; i < l; i++)
        {
            var aniEvent = clip.events[i];
            FrameEvent evt = new FrameEvent();
            evt.name = aniEvent.functionName;
            evt.position = aniEvent.time;
            evt.intVariable = aniEvent.intParameter;
            evt.floatVariable = aniEvent.floatParameter;
            evt.stringVariable = aniEvent.stringParameter;

            _clip.events.Add(evt);
        }

        framecount += 1;
        FB.PosePlus.Frame last = null;

        //ani.StartPlayback();
        //逐帧复制
        //ani.Play(_clip.name, 0, 0);
        for (int i = 0; i < framecount; i++)
        {            
            ani.Play(mapClip2State[_clip.name], 0, (i * 1.0f / fps) / clip.length);
            ani.Update(0);
            last = new FB.PosePlus.Frame(_clip, con, last, i, ani.transform, cdpath);

            _clip.frames.Add(last);
        }

        //特殊处理：写文件后，clips中的内容会丢失
        //这里用clipcache保存clips的内容
        //Debug.Log(con.clips[0].name);
        Dictionary<string, int> clipcache = new Dictionary<string, int>();
        if (con.clips != null)
        {
            for (int i = 0; i < con.clips.Count; i++)
            {
                if (con.clips[i])
                {
                    clipcache[con.clips[i].name] = i;
                }
                else
                {
                    con.clips.RemoveAt(i);
                }
            }
        }
        con.clipcache = clipcache;


        string outpath = PathHelper.CheckFileName(path + "/" + ani.gameObject.name + "_" + clip.name + ".FBAni.asset");
        /*FB.PosePlus.AniClip src = null;
        if (Pretreatment.AnimatorCache.ContainsKey(outpath))
        {
            src = Pretreatment.AnimatorCache[outpath];
        }
        else
        {
            AssetDatabase.CreateAsset(_clip, outpath);
            src = AssetDatabase.LoadAssetAtPath(outpath, typeof(FB.PosePlus.AniClip)) as FB.PosePlus.AniClip;
            Pretreatment.AnimatorCache[outpath] = src;
        }*/
        AssetDatabase.CreateAsset(_clip, outpath);
        var src = AssetDatabase.LoadAssetAtPath(outpath, typeof(FB.PosePlus.AniClip)) as FB.PosePlus.AniClip;
        Pretreatment.AnimatorCache[outpath] = src;
        src.clipName = PathHelper.CheckFileName(clip.name);
        
        con.AddAni(src);
    }


    static Dictionary<string, string> mapClip2State = new Dictionary<string, string>();

#if UNITY4
    public static void FindAllAniInControl(UnityEditorInternal.AnimatorController control, List<AnimationClip> list)
	{
		mapClip2State.Clear();
		for (int i = 0; i < control.layerCount; i++)
		{
			var layer = control.GetLayer(i);
			FindAllAniInControlMachine(layer.stateMachine, list);
		}
	}
	static void FindAllAniInControlMachine(UnityEditorInternal.StateMachine machine, List<AnimationClip> list)
	{
		for (int i = 0; i < machine.stateCount; i++)
		{
			var s = machine.GetState(i);
			var m = s.GetMotion();
			if (m != null)
			{
				if (list.Contains(m as AnimationClip) == false)
				{
					list.Add(m as AnimationClip);
					if (m.name != s.name)
					{
						//Debug.LogWarning("发现一个问题，clipname 和 state name 不相等 " + m.name + "=>" + s.name);
					}
					mapClip2State[m.name] = s.name;//建立一张转换表，把clip的名字转换为state的名字，以正常clone
					
				}
			}
		}
		for (int i = 0; i < machine.stateMachineCount; i++)
		{
			var m = machine.GetStateMachine(i);
			FindAllAniInControlMachine(m, list);
		}
	}
#else
    public static void FindAllAniInControl(RuntimeAnimatorController control, List<UnityEngine.AnimationClip> list)
    {
        mapClip2State.Clear();
        if (control == null)
        {
            return;
        }
        UnityEditor.Animations.AnimatorControllerLayer[] layers = null;

        if (control is UnityEditor.Animations.AnimatorController)
        {
            layers = (control as UnityEditor.Animations.AnimatorController).layers;
        }
        if (layers == null)
        {
            foreach (var clip in control.animationClips)
            {
                list.Add(clip);
                mapClip2State[clip.name] = clip.name;
            }            
            return;
        }
        for (int i = 0; i < layers.Length; i++)
        {
            var layer = layers[i];
            FindAllAniInControlMachine(layer.stateMachine, list);
        }
    }
    static void FindAllAniInControlMachine(UnityEditor.Animations.AnimatorStateMachine machine, List<UnityEngine.AnimationClip> list)
    {
        for (int i = 0; i < machine.states.Length; i++)
        {
            var s = machine.states[i].state;
            var m = s.motion;
            if (m != null)
            {
                if (list.Contains(m as UnityEngine.AnimationClip) == false)
                {
                    list.Add(m as UnityEngine.AnimationClip);
                    if (m.name != s.name)
                    {
                        //Debug.LogWarning("发现一个问题，clipname 和 state name 不相等 " + m.name + "=>" + s.name);
                    }
                    mapClip2State[m.name] = s.name;//建立一张转换表，把clip的名字转换为state的名字，以正常clone

                }
            }
        }
        for (int i = 0; i < machine.stateMachines.Length; i++)
        {
            var m = machine.stateMachines[i].stateMachine;
            FindAllAniInControlMachine(m, list);
        }
    }
#endif
}


















