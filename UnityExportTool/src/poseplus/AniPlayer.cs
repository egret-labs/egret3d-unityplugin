using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace FB.PosePlus
{


    //新的动画控制器，相比封闭的animator，建立一个更开放自由的模式
    public class AniPlayer : MonoBehaviour
    {
		public static string AvatarInfoPath{
			get{
				return Application.dataPath + "/Egret3DExportTool/AvatarInfos/";
			}
		}
        //public int tagid;
        void Start()
        {
            bPlayRunTime = true;
        }


        public List<FB.PosePlus.AniClip> clips;
        public Dictionary<string, int> clipcache = null;
        public List<BoneInfo> bones;
        Dictionary<string, int> bonecache = null;
        public BoneInfo getbone(string name)
        {
            _checkBone();
            if (bonecache.ContainsKey(name) == false)
            {
                return null;
            }
            return bones[bonecache[name]];
        }
        public void AddBone(string name)
        {
            var binfo = new BoneInfo();
            binfo.tpose = Matrix4x4.identity;
            binfo.bone = this.transform.Find(name);
            if (binfo.bone == null)
                Debug.LogError("No find bone");
            bones.Add(binfo);
            _checkBone();
        }

        void _checkClip()
        {
            if (clipcache == null || clipcache.Count != clips.Count)
            {
                clipcache = new Dictionary<string, int>();
                for (int i = 0; i < clips.Count; i++)
                {
                    // if (clipcache.ContainsKey(clips[i].name))
                    clipcache[clips[i].name] = i;
                }
            }
        }
        void _checkBone()
        {
            if (bonecache == null || bonecache.Count != bones.Count)
            {
                bonecache = new Dictionary<string, int>();
                for (int i = 0; i < bones.Count; i++)
                {
                    // if (clipcache.ContainsKey(clips[i].name))
					if(bones[i]!=null&&bones[i].bone!=null){
                    	bonecache[bones[i].bone.name] = i;
					}
					else{
						Debug.LogError(gameObject.name+"--------有骨骼节点为空");
					}
                }
            }
        }

		public void InitBone(string saveAvatarName = "",string exportAvatarName = "")
        {
            if (bones == null)
                bones = new List<BoneInfo>();

			var ss = this.transform.GetComponentsInChildren<SkinnedMeshRenderer> ();
			foreach (var s in ss) {
				if (s.sharedMesh != null) {
					for (int i = 0; i < s.bones.Length; i++) {
						var b = s.bones [i];
						bool bnew = true;
						foreach (var _b in bones) {
							if (_b.bone == b) {
								bnew = false;
								break;
							}
						}
						if (bnew) {
							var binfo = new BoneInfo ();
							binfo.bone = b;
							binfo.tpose = s.sharedMesh.bindposes [i];
							bones.Add (binfo);
						}
					}
				}
			}

			//骨骼配置文件操作
			if (!string.IsNullOrEmpty (saveAvatarName)||!string.IsNullOrEmpty (exportAvatarName)) {
				//先记录下当前骨骼的信息
				Dictionary<string,string> curDic = new Dictionary<string, string> ();
				for (int i = 0; i < bones.Count; i++) {
					curDic.Add (bones [i].bone.name,bones [i].tpose.ToString ());
				}

				string avatarInfoName = !string.IsNullOrEmpty (saveAvatarName) ? saveAvatarName : exportAvatarName;
				Dictionary<string,string> allDic;
				string infoPath = AvatarInfoPath + avatarInfoName + ".txt";

				if (!string.IsNullOrEmpty (exportAvatarName)) {
					if (File.Exists (infoPath)) {
						string ctx = File.ReadAllText (infoPath);
						try {
							allDic = JsonUtil.DeserializeObject<Dictionary<string,string>> (ctx);
						} catch {
							allDic = new Dictionary<string,string> ();
						}

						//将配置中所有的节点添加进去
						foreach (KeyValuePair<string,string> item in allDic) {
							if (!curDic.ContainsKey (item.Key)) {
								if (item.Key == "shoulder_R") {
									Debug.Log ("aaa");
								}
								var binfo = new BoneInfo ();
								binfo.bone = getTransform (this.transform,item.Key);
								binfo.tpose = getMatrixByString (item.Value);
								bones.Add (binfo);
							}
						}
					}
				}
				else {
					StartCoroutine(WaitForSaveConfig(infoPath,curDic));
				}
					
			}

            //自动添加asbone对象
            var ss2 = this.transform.GetComponentsInChildren<Asbone>();
            foreach (var asb in ss2)
            {
                var b = asb.transform;
                bool bnew = true;
                foreach (var _b in bones)
                {
                    if (_b.bone == b)
                    {
                        bnew = false;
                        break;
                    }
                }
                if (bnew)
                {
                    var binfo = new BoneInfo();
                    binfo.bone = b;
                    binfo.tpose = Matrix4x4.identity;
                    bones.Add(binfo);
                }
            }
        }
        public void AddAni(AniClip clip)
        {
            if (clips == null)
            {
                clips = new List<AniClip>();
            }
            if (clipcache == null) {
                clipcache = new Dictionary<string, int>();
            }
            if (clipcache.ContainsKey(clip.name)) {
                clips[clipcache[clip.name]] = clip;
            } else {
                clips.Add(clip);
                clipcache[clip.name] = clips.Count - 1;
            }
        }

        public void RemoveAni(string name)
        {
            if (clips == null) return;
            int igot = -1;
            if (clipcache.TryGetValue(name, out igot))
            {
                clipcache.Remove(name);
                clips.RemoveAt(igot);
                return;
            }
            return;
        }
        public bool ContainsAni(string name)
        {
            if (clips == null) return false;
            int igot = -1;
            if (clipcache.TryGetValue(name, out igot))
            {
                return true;
            }
            return false;
        }
        public AniClip GetClip(string name)
        {
            if (clips == null || clips.Count == 0) return null;
            //if (clipcache == null || clipcache.Count != clips.Count)
            //{
            //    clipcache = new Dictionary<string, int>();
            //    for (int i = 0; i < clips.Count; i++)
            //    {
            //        // if (clipcache.ContainsKey(clips[i].name))
            //        clipcache[clips[i].name] = i;
            //    }
            //}
            int igot = -1;
            if (clipcache.TryGetValue(name, out igot))
            {
                return clips[igot];
            }
            return null;
        }


        AniClip lastClip = null;//当前剪辑
        bool bPlayRunTime = false;

        int lastframe = -1; //当前帧

        int lastframeid = -1; //step，判断是否需要处理动画
        public int frameid//获取此值作为帧计时器
        {
            get
            {
                return lastframeid;
            }
        }

        bool bLooped = false;
        int startframe;
        int endframe;
        float _crossTimer = -1;
        float _crossTimerTotal = 0;
        Frame crossFrame = null; //用来混合用的帧

        public Frame frameNow
        {
            get;
            private set;
        }
        public void Play(AniClip clip, SubClip clipsub = null, float crosstimer = 0)
        {
            if (clipsub != null)
            {
                bLooped = clipsub.loop;
                startframe = (int)clipsub.startframe;
                endframe = (int)clipsub.endframe;
                if (_fps < 0)
                {
                    _fps = clip.fps;
                }
            }
            else if (clip != null)
            {
                bLooped = clip.loop;
                startframe = 0;
                endframe = (clip.aniFrameCount - 1);
                if (_fps < 0)
                {
                    _fps = clip.fps;
                }
            }
            if (crosstimer <= 0)
            {
                this._crossTimer = -1;
                crossFrame = null;

                lastClip = clip;
                lastframe = startframe;
                SetPose(clip, startframe, true);
                frameNow = lastClip.frames[lastframe];
            }
            else
            {

                if (lastClip != null && lastframe >= 0 && lastframe < lastClip.frames.Count)
                {
                    RecCrossFrame();
                    lastClip = clip;
                    lastframe = startframe;
                }
                else
                {
                    lastClip = clip;
                    lastframe = startframe;
                    SetPose(clip, startframe, true);
                    frameNow = lastClip.frames[lastframe];
                }
                this._crossTimerTotal = this._crossTimer = crosstimer;
            }

        }
        void RecCrossFrame()
        {
            if (this._crossTimer >= 0 && crossFrame != null)
            {
                //Frame f = new Frame();

                float l = 1.0f - _crossTimer / _crossTimerTotal;
                lastClip.frames[lastframe].boneinfo = lastClip.boneinfo;
                lastClip.frames[lastframe].bonehash = lastClip.bonehash;
                crossFrame = Frame.Lerp(crossFrame, lastClip.frames[lastframe], l);

            }
            else
            {
                crossFrame = lastClip.frames[lastframe];
                crossFrame.boneinfo = lastClip.boneinfo;
                crossFrame.bonehash = lastClip.bonehash;
            }
        }
        float timer = 0;
        float _fps = -1;
        float pauseTimer = 0;
        public void _OnUpdate(float delta)
        {
            //帧推行
            if (lastClip == null)
                return;
            //打中暂停机制
            if (pauseframe > 0)
            {
                pauseTimer += delta;
                int pid = (int)((timer + pauseTimer) * _fps);
                if (pid - lastframeid >= pauseframe)
                {
                    pauseframe = 0;
                    pauseTimer = 0;
                }
                else
                {
                    return;
                }
            }

            timer += delta;

            bool crossend = false;
            if (_crossTimer >= 0)
            {
                _crossTimer -= delta;
                if (_crossTimer <= 0)
                    crossend = true;
            }

            int frameid = (int)(timer * _fps);//这里要用一个稳定的fps，就用播放的第一个动画的fps作为稳定fps
            if (frameid == lastframeid)
                return;

            if (frameid > lastframeid + 1)//增加一个限制，不准动画跳帧
            {
                frameid = lastframeid + 1;
                timer = (float)frameid / _fps;
            }
            lastframeid = frameid;





            //帧前行

            int frame = lastframe + 1;
            if (frame > endframe)
            {
                if (bLooped)
                {
                    frame = startframe;

                }
                else
                {
                    frame = endframe;

                }
            }


            //设置动作或者插值
            if (crossend)
            {
                crossFrame = null;
                SetPose(lastClip, frame, true);
                return;
            }
            if (_crossTimer >= 0)
            {
                //_crossTimer -= delta;
                //if (_crossTimer < 0)
                //{
                //    crossFrame = null;
                //    SetPose(lastClip, frame, true);
                //    return;
                //}
                if (crossFrame != null)
                {


                    float l = 1.0f - _crossTimer / _crossTimerTotal;


                    lastframe = frame;
                    frameNow = lastClip.frames[frame];
                    frameNow.boneinfo = lastClip.boneinfo;
                    frameNow.bonehash = lastClip.bonehash;

                    SetPoseLerp(crossFrame, frameNow, l);

                }
            }
            else
            {
                if (frame != lastframe)
                {
                    SetPose(lastClip, frame);
                    frameNow = lastClip.frames[frame];
                }
            }

            //更新角色闪烁

            if (flashTime > 0)
            {
                flashTime--;
                UpdateFlash();
            }
        }

        //int transcode = -1;
        //Transform[] trans = null;
        public void SetPose(AniClip clip, int frame, bool reset = false)
        {
            _checkBone();
            //if (clip.bonehash != transcode)
            //{
            //    trans = new Transform[clip.boneinfo.Count];
            //    for (int i = 0; i < clip.boneinfo.Count; i++)
            //    {
            //        trans[i] = this.transform.Find(clip.boneinfo[i]);
            //    }
            //    transcode = clip.bonehash;
            //}

            bool badd = false;

            if (lastClip == clip && !reset)
            {
                if (lastframe + 1 == frame) badd = transform;
                if (clip.loop && lastframe == clip.frames.Count - 1 && frame == 0)
                    badd = true;
            }

            for (int i = 0; i < clip.boneinfo.Count; i++)
            {
                var bn = clip.boneinfo[i];
                if (this.bonecache.ContainsKey(bn) == false)
                {
                    //if (bn == "CameraPoint")
                    //{
                    //    AddBone(bn);
                    //}
                    //else
                    {
                        continue;
                    }
                }
                clip.frames[frame].bonesinfo[i].UpdateTran(bones[bonecache[bn]].bone, bones[bonecache[bn]].tpose, this.transform, badd);
            }



            lastClip = clip;
            lastframe = frame;

        }
        public void SetPose(Frame frame)
        {
            _checkBone();
            for (int i = 0; i < frame.boneinfo.Count; i++)
            {
                var bname = frame.boneinfo[i];
                if (bonecache.ContainsKey(bname) == false)
                {
                    //Debug.Log("do not have this bone:" + bname);
                }
                else
                {
                    frame.bonesinfo[i].UpdateTran(bones[bonecache[bname]].bone, bones[bonecache[bname]].tpose, this.transform, true);
                }
            }

        }
        public void SetPoseLerp(Frame src, Frame dest, float lerp)
        {
            _checkBone();

            var frame = Frame.Lerp(src, dest, lerp);
            SetPose(frame);

        }


        void Update()
        {
            if (bPlayRunTime)
            {
                _OnUpdate(Time.deltaTime);
            }

        }




        public void Play(string clip, string subclip, float cross)
        {
            if (string.IsNullOrEmpty(clip) == false)
            {

                var _clip = GetClip(clip);
                if (_clip == null)
                {
                    Debug.LogWarning("No clip:" + clip);
                    return;
                }
                SubClip _subclip = null;
                if (string.IsNullOrEmpty(subclip) == false)
                {
                    _subclip = _clip.GetSubClip(subclip);
                }

                //Debug.LogError("_clip = " + _clip);
                Play(_clip, _subclip, cross);
            }
        }


        //bool ispause;
        int pauseframe = 0;
        //int pausecount;
        public bool isPause
        {
            get
            {
                return pauseframe > 0;
            }
        }

        public int totalFrame()
        {
            var res = 0;
            if (this.clips != null)
            {
                for (int i = 0, l = this.clips.Count; i < l; i++)
                {
                    var clip = this.clips[i];
                    res += clip.frames.Count;
                }
            }

            return res;
        }
        public void PlayPause(int frame)
        {
            pauseframe = frame;
            //ispause = true;
            //pausecount = 0;
        }

        Color flashColor;
        int flashTime;
        int flashSpeed;
        public void SetFlash(Color color, int time, int speed)
        {
            flashColor = color;
            flashTime = time;
            flashSpeed = speed;
            UpdateFlash();
        }

        void UpdateFlash()
        {
            Color nc = flashColor;
            if (flashTime > 0)
            {
                float lerp = ((float)(flashTime % flashSpeed)) / (float)(flashSpeed - 1);
                lerp = Mathf.Abs(1 - lerp * 2);
                nc.a = lerp;
            }

            var rs = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
            foreach (var r in rs)
            {
                r.material.SetColor("_RimColor", nc);
            }
        }

		bool MatrixIsIdentity(string mtxStr){
			string MatrixIdentityStr = Matrix4x4.identity.ToString ();
			if (MatrixIdentityStr.Equals (mtxStr))
				return true;
			return false;
		}

		Transform getTransform(Transform tran,string name){
			if (!tran)
				return null;
			Transform tempTran = tran.Find (name);
			if (tempTran)
				return tempTran;
			else {
				for (int i = 0; i < tran.childCount; i++) {
					tempTran = getTransform (tran.GetChild (i),name);
					if (tempTran)
						return tempTran;
				}
			}
			return null;
		}

		Matrix4x4 getMatrixByString(string mtxStr){
			Matrix4x4 result = Matrix4x4.identity;
			string[] strs = mtxStr.Split (new string[]{"\t","\n"},StringSplitOptions.None);
			if (strs.Length < 16)
				return result;
			result.m00 = float.Parse(strs [0]);result.m01 = float.Parse(strs [1]);result.m02 = float.Parse(strs [2]);result.m03 = float.Parse(strs [3]);
			result.m10 = float.Parse(strs [4]);result.m11 = float.Parse(strs [5]);result.m12 = float.Parse(strs [6]);result.m13 = float.Parse(strs [7]);
			result.m20 = float.Parse(strs [8]);result.m21 = float.Parse(strs [9]);result.m22 = float.Parse(strs [10]);result.m23 = float.Parse(strs [11]);
			result.m30 = float.Parse(strs [12]);result.m31 = float.Parse(strs [13]);result.m32 = float.Parse(strs [14]);result.m33 = float.Parse(strs [15]);
			return result;
		}
			
		/// <summary>
		/// Waits for save config.
		/// 解决同步存文件后update不刷新的问题
		/// </summary>
		/// <returns>The for save config.</returns>
		IEnumerator WaitForSaveConfig(string infoPath,Dictionary<string,string> dic){
			yield return 1;
			Dictionary<string,string> allDic;
			if (File.Exists (infoPath)) {
				string ctx = File.ReadAllText (infoPath);
				try {
					allDic = JsonUtil.DeserializeObject<Dictionary<string,string>> (ctx);
				} catch {
					allDic = new Dictionary<string,string> ();
				}

				//遍历当前骨骼信息
				foreach (KeyValuePair<string,string> item in dic) {
					//配置中没有的骨骼，添加进去
					if (!allDic.ContainsKey (item.Key)) {
						allDic.Add (item.Key, item.Value);
					}
					//配置中存在的骨骼，但是tpose无效，用当前的替换
					else {
						if (MatrixIsIdentity (allDic [item.Key]) && MatrixIsIdentity (item.Value)) {
							allDic [item.Key] = item.Value;
						}
					}
				}
			} 
			else {
				allDic = dic;
			}

			string text = JsonUtil.SerializeObject (allDic);
			File.WriteAllText (infoPath,text);
		}
    }
}
