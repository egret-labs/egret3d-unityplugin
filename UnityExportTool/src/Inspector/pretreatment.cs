using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using FB.PosePlus;

public class Pretreatment
{
    static int maxbone = 55;
    public static Dictionary<string, FB.PosePlus.AniClip> AnimatorCache = new Dictionary<string, FB.PosePlus.AniClip>();
    public static Dictionary<string, FB.PosePlus.AniClip> AnimationCacne = new Dictionary<string, FB.PosePlus.AniClip>();

    public static void preAnimator(Animator animator)
    {
        GameObject curObj = animator.gameObject;
        FB.PosePlus.AniPlayer player = curObj.GetComponent<FB.PosePlus.AniPlayer>();
        if (player == null)
        {
            player = curObj.AddComponent<FB.PosePlus.AniPlayer>();
        }
        player.InitBone();

        if (animator.runtimeAnimatorController != null)
        {
            List<AnimationClip> clips = new List<AnimationClip>();
#if UNITY4
            Animator_Inspector.FindAllAniInControl(animator.runtimeAnimatorController as UnityEditorInternal.AnimatorController, clips);
#else
            Animator_Inspector.FindAllAniInControl(animator.runtimeAnimatorController as UnityEditor.Animations.AnimatorController, clips);
#endif
            foreach (var c in clips)
            {
                Animator_Inspector.CloneAni(c, c.frameRate, animator);
            }
        }
    }

    public static void preAnimation(Animation animation)
    {
        GameObject curObj = animation.gameObject;
        FB.PosePlus.AniPlayer player = curObj.GetComponent<FB.PosePlus.AniPlayer>();
        if (player == null)
        {
            player = curObj.AddComponent<FB.PosePlus.AniPlayer>();
        }
        player.InitBone();

        List<AnimationClip> clips = new List<AnimationClip>();
        foreach (AnimationState state in animation)
        {
            clips.Add(state.clip);
        }
        foreach (var c in clips)
        {
            AddClip(c, c.frameRate, animation);
        }
    }

    public static void AddClip(AnimationClip clip, float fps, Animation ani)
    {
        FB.PosePlus.AniClip _clip = ScriptableObject.CreateInstance<FB.PosePlus.AniClip>();
        _clip.boneinfo = new List<string>();//也增加了每个动画中的boneinfo信息.

        //这里重新检查动画曲线，找出动画中涉及的Transform部分，更精确
        List<Transform> cdpath = new List<Transform>();
        var curveDatas = AnimationUtility.GetCurveBindings(clip);
        FB.PosePlus.AniPlayer con = ani.GetComponent<FB.PosePlus.AniPlayer>();

        UpdateCdpath(ani, curveDatas, _clip, con, cdpath);

        string path = System.IO.Path.GetDirectoryName(AssetDatabase.GetAssetPath(clip.GetInstanceID()));
        _clip.name = clip.name;
        _clip.frames = new List<FB.PosePlus.Frame>();
        _clip.fps = fps;
        _clip.loop = clip.isLooping;
        float flen = (clip.length * fps);
        int framecount = (int)flen;
        if (flen - framecount > 0.0001) framecount++;

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
        for (int i = 0; i < framecount; i++)
        {
            ani[_clip.name].time = (i * 1.0f / fps) / clip.length;
            ani[_clip.name].enabled = true;
            ani[_clip.name].weight = 1;
            ani.Sample();
            ani[_clip.name].enabled = false;

            last = new FB.PosePlus.Frame(_clip, con, last, i, ani.transform, cdpath);
            _clip.frames.Add(last);
        }

        Dictionary<string, int> clipcache = new Dictionary<string, int>();
        if (con.clips != null)
        {
            for (int i = 0, l = con.clips.Count; i < l; i++)
            {
                if (con.clips[i])
                {
                    clipcache[con.clips[i].name] = i;
                    Debug.Log(con.clips[i].name);
                }
                else
                {
                    con.clips.RemoveAt(i);
                    i--;
                    l--;
                }
            }
        }

        con.clipcache = clipcache;

        string outpath = path + "/" + clip.name + ".FBAni.asset";
        FB.PosePlus.AniClip src = null;
        /*if (Pretreatment.AnimatorCache.ContainsKey(outpath))
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
        src = AssetDatabase.LoadAssetAtPath(outpath, typeof(FB.PosePlus.AniClip)) as FB.PosePlus.AniClip;
        Pretreatment.AnimatorCache[outpath] = src;

        con.AddAni(src);
    }

    public static void UpdateCdpath(Animation ani, EditorCurveBinding[] curveDatas, FB.PosePlus.AniClip _clip, FB.PosePlus.AniPlayer con, List<Transform> cdpath)
    {
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
    }
}
