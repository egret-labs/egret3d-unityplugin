using System;
using System.Collections.Generic;
using UnityEngine;

namespace FB.PosePlus
{

    public interface IPlayAni
	{
		//float aniFps
		//{
		//    get;
		//}
		bool aniLoop
		{
			get;
		}
		int aniFrameCount
		{
			get;
		}
		Frame GetFrame(int frame);
		
	}
	[Serializable]
	public class BoneInfo
	{
		[SerializeField]
		public Transform bone;
		[SerializeField]
		public Matrix4x4 tpose;
	}
	[Serializable]
	public class SubClip
	{
		[SerializeField]
		public String name = "noname";
		[SerializeField]
		public bool loop;
		[SerializeField]
		public uint startframe;
		[SerializeField]
		public uint endframe;
	}

    [Serializable]
    public class FrameEvent
    {
        public string name;
        public float position;
        public int intVariable;
        public float floatVariable;
        public string stringVariable;
    }

	public class AniClip : ScriptableObject, IPlayAni
	{
        public string clipName = "";
		public List<string> boneinfo = new List<string>();
		public List<Frame> frames = new List<Frame>();
		
		public List<SubClip> subclips = new List<SubClip>();

        public List<FrameEvent> events = new List<FrameEvent>();
		
		Dictionary<string, int> subclipcache = null;
		public SubClip GetSubClip(string name)
		{
			if (subclips == null || subclips.Count == 0) return null;
			if (subclipcache == null || subclipcache.Count != subclips.Count)
			{
				subclipcache = new Dictionary<string, int>();
				for (int i = 0; i < subclips.Count; i++)
				{
					subclipcache[subclips[i].name] = i;
				}
			}
			int igot = 0;
			if (subclipcache.TryGetValue(name, out igot))
			{
				return subclips[igot];
			}
			return null;
		}
		public float fps = 24.0f;
		public bool loop;
		
		[NonSerialized]
		int _bonehash = -1;
		public int bonehash
		{
			get
			{
				if (_bonehash == -1)
				{
					string name = "";
					foreach (var s in boneinfo)
					{
						name += s + "|";
					}
					_bonehash = name.GetHashCode();
				}
				return _bonehash;
			}
			
		}
		
		public bool aniLoop
		{
			get { return loop; }
			set { loop = value; }
		}
		
		public int aniFrameCount
		{
			get { return frames.Count; }
		}
		
		public Frame GetFrame(int frame)
		{
			return frames[frame];
		}
		
		public void ResetLerpFrameSegment(int frame)
		{
			//搜索开始与结束帧
			if (frames[frame].key) return;
			if (frame <= 0 || frame >= frames.Count - 1) return;
			int ibegin = frame;
			for (; ibegin >= 0; ibegin--)
			{
				if (frames[ibegin].key)
				{
					break;
				}
			}
			if (ibegin == frame) return;
			int iend = frame;
			for (; iend < frames.Count; iend++)
			{
				if (frames[iend].key)
				{
					break;
				}
			}
			if (iend == frame) return;
			//找到最近两个关键帧之间 插值
			for (int i = ibegin + 1; i < iend; i++)
			{
				float d1 = (i - ibegin);
				float d2 = (iend - i);
				float lerp = d1 / (d1 + d2);
				frames[i] = Frame.Lerp(frames[ibegin], frames[iend], lerp);
				//frames[i].lerp = lerp;
				frames[i].fid = i;
			}
		}
		
		public void ResetLerpFrameAll()
		{
			for (int ibegin = 0; ibegin < frames.Count - 1; ibegin++)
			{
				if (frames[ibegin].key)
				{
					if (frames[ibegin + 1].key)
					{
						//下一帧就是关键帧，没法玩，坑了
						continue;
					}
					for (int iend = ibegin + 2; iend < frames.Count; iend++)
					{
						if (frames[iend].key)
						{
							//发现一个需要计算的
							//Debug.LogWarning("find need calc:" + ibegin + "-" + iend);
							for (int i = ibegin + 1; i < iend; i++)
							{
								float d1 = (i - ibegin);
								float d2 = (iend - i);
								float lerp = d1 / (d1 + d2);
								frames[i] = Frame.Lerp(frames[ibegin], frames[iend], lerp);
								//frames[i].lerp = lerp;
								frames[i].fid = i;
							}
							break;
						}
					}
				}
			}
			for (int i = 0; i < frames.Count; i++)
			{
				if (i == 0 && !this.loop) continue;
				int ilast = i - 1;
				if (ilast < 0) ilast = frames.Count - 1;
			}
		}
		
		public bool MatchBone(AniClip clip)
		{
			Dictionary<int, int> boneconvert = new Dictionary<int, int>();
			for (int i = 0; i < clip.boneinfo.Count; i++)
			{
				bool bhave = false;
				
				for (int j = 0; j < this.boneinfo.Count; j++)
				{
					
					if (this.boneinfo[j] == clip.boneinfo[i])
					{
						boneconvert[j] = i;
						bhave = true;
					}
				}
				if (bhave == false)
				{//需要增加骨骼了，呵呵了
					Debug.LogError("bone need add:" + clip.boneinfo[i]);
					Debug.LogWarning("目前不能处理这种情况，将另一个动画放在前面试试");
					return false;
				}
				//if donthave
			}
			for (int i = 0; i < this.boneinfo.Count; i++)
			{
				if (boneconvert.ContainsKey(i) == false)
				{
					Debug.LogWarning("bone need delete:" + this.boneinfo[i]);
				}
			}
			this.boneinfo = new List<string>(clip.boneinfo);
			
			foreach (var f in frames)
			{
				List<PoseBoneMatrix> list = new List<PoseBoneMatrix>();
				for (int i = 0; i < this.boneinfo.Count; i++)
				{
					list.Add(null);
				}
				foreach (var c in boneconvert)
				{
					list[c.Value] = f.bonesinfo[c.Key];
				}
				
				f.bonesinfo = list;
			}
			return true;
		}	
	}	
}
