using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TUT
{
    public class TutProfiler
    {
        private static UsageMemoryDetail mLastMemoryDetail = null;

		private static Dictionary<int,UsageFragment> mFragment = new Dictionary<int, UsageFragment> ();

		public class UsageFragment
		{
			public int insId;
			public string name;
			public int size;

			public UsageFragment(int id,string _name,int _size)
			{
				insId = id;
				name = _name;
				size = _size;
			}

			public override string ToString ()
			{
				string result = " [ "+name +" => "+ insId.ToString() +" Detail=> ";
				result+= TutProfiler.toMemoryString(size)+"]";
				return result;
			}
		}

        public class UsageMemoryDetail
        {
            public int t_textrue_size;
            public int t_audio_size;
            public int t_mesh_size;
            public int t_material_size;
            public int t_animation_size;
			public int t_font_size;
			public int t_shader_size;
			public int t_particle_size;
            public int total_size;

			public List<UsageFragment> fragments = new List<UsageFragment> ();

            public UsageMemoryDetail Subtract(UsageMemoryDetail detail)
            {
                UsageMemoryDetail result = new UsageMemoryDetail();
                result.t_textrue_size = t_textrue_size - detail.t_textrue_size;
                result.t_audio_size = t_audio_size - detail.t_audio_size;
                result.t_mesh_size = t_mesh_size - detail.t_mesh_size;
                result.t_material_size = t_material_size - detail.t_material_size;
                result.t_animation_size = t_animation_size - detail.t_animation_size;
				result.t_font_size = t_font_size - detail.t_font_size;
				result.t_shader_size = t_shader_size - detail.t_shader_size;
				result.t_particle_size = t_particle_size - detail.t_particle_size;
                result.total_size = total_size - detail.total_size;
                return result;
            }

            public override string ToString()
            {
                string result = " [ Usage Memory Detail=> ";
                if (t_textrue_size > 0)
                    result += " textrue: " + TutProfiler.toMemoryString(t_textrue_size);
                if (t_animation_size > 0)
                    result += " animation: " + TutProfiler.toMemoryString(t_animation_size);
                if (t_audio_size > 0)
                    result += " audio: " + TutProfiler.toMemoryString(t_audio_size);
                if (t_mesh_size > 0)
                    result += " mesh: " + TutProfiler.toMemoryString(t_mesh_size);
                if (t_material_size > 0)
                    result += " material: " + TutProfiler.toMemoryString(t_material_size);
				if (t_font_size > 0)
					result += " font: " + TutProfiler.toMemoryString(t_font_size);
				if (t_shader_size > 0)
					result += " shader: " + TutProfiler.toMemoryString(t_shader_size);
				if (t_particle_size > 0)
					result += " particle: " + TutProfiler.toMemoryString(t_particle_size);
				if (total_size > 0)
					result += " total: " + TutProfiler.toMemoryString(total_size);
                else
                    result += " nil ";
				result += "]"+'\n';

				if(fragments.Count != 0)
				{
					for(int i = 0;i<fragments.Count;i++)
					{
						result += fragments[i].ToString()+'\n';
					}
				}
                return result;
            }
        }

        public static UsageMemoryDetail GetAssetUsageMemory()
        {
			List<UsageFragment> frags = new List<UsageFragment> ();
            UsageMemoryDetail detail = new UsageMemoryDetail();
            Object[] assets = Resources.FindObjectsOfTypeAll(typeof(Texture));
			int size = -1;
			UsageFragment frag = null;
            for (int i = 0; i<assets.Length; i++)
            {
				size = Profiler.GetRuntimeMemorySize(assets [i]);
				frag = new UsageFragment(assets[i].GetInstanceID(),assets[i].name,size);
				frags.Add(frag);
				detail.t_textrue_size += size;
            }
            detail.total_size += detail.t_textrue_size;
            assets = Resources.FindObjectsOfTypeAll(typeof(AudioClip));
            for (int i = 0; i<assets.Length; i++)
            {
				size = Profiler.GetRuntimeMemorySize(assets [i]);
				frag = new UsageFragment(assets[i].GetInstanceID(),assets[i].name,size);
				frags.Add(frag);
				detail.t_audio_size += size;
            }
            detail.total_size += detail.t_audio_size;
            assets = Resources.FindObjectsOfTypeAll(typeof(Mesh));
            for (int i = 0; i<assets.Length; i++)
            {
				size = Profiler.GetRuntimeMemorySize(assets [i]);
				frag = new UsageFragment(assets[i].GetInstanceID(),assets[i].name,size);
				frags.Add(frag);
				detail.t_mesh_size += size;
            }
            detail.total_size += detail.t_mesh_size;
            assets = Resources.FindObjectsOfTypeAll(typeof(Material));
            for (int i = 0; i<assets.Length; i++)
            {
				size = Profiler.GetRuntimeMemorySize(assets [i]);
				frag = new UsageFragment(assets[i].GetInstanceID(),assets[i].name,size);
				frags.Add(frag);
				detail.t_material_size += size;
            }
            detail.total_size += detail.t_material_size;
            assets = Resources.FindObjectsOfTypeAll(typeof(AnimationClip));
            for (int i = 0; i<assets.Length; i++)
            {
				size = Profiler.GetRuntimeMemorySize(assets [i]);
				frag = new UsageFragment(assets[i].GetInstanceID(),assets[i].name,size);
				frags.Add(frag);
				detail.t_animation_size += size;
            }

			assets = Resources.FindObjectsOfTypeAll(typeof(Font));
			for (int i = 0; i<assets.Length; i++)
			{
				size = Profiler.GetRuntimeMemorySize(assets [i]);
				frag = new UsageFragment(assets[i].GetInstanceID(),assets[i].name,size);
				frags.Add(frag);
				detail.t_font_size += size;
			}
			detail.total_size += detail.t_font_size;

			assets = Resources.FindObjectsOfTypeAll(typeof(Shader));
			for (int i = 0; i<assets.Length; i++)
			{
				size = Profiler.GetRuntimeMemorySize(assets [i]);
				frag = new UsageFragment(assets[i].GetInstanceID(),assets[i].name,size);
				frags.Add(frag);
				detail.t_shader_size += size;
			}
			detail.total_size += detail.t_shader_size;

			assets = Resources.FindObjectsOfTypeAll(typeof(ParticleSystem));
			for (int i = 0; i<assets.Length; i++)
			{
				size = Profiler.GetRuntimeMemorySize(assets [i]);
				frag = new UsageFragment(assets[i].GetInstanceID(),assets[i].name,size);
				frags.Add(frag);
				detail.t_particle_size += size;
			}
			detail.total_size += detail.t_particle_size;

			for(int i = 0;i<frags.Count;i++)
			{
				if(!mFragment.ContainsKey(frags[i].insId))
				{
					detail.fragments.Add(frags[i]);
				}
			}
			mFragment.Clear();
			for(int i = 0;i<frags.Count;i++)
			{
				mFragment.Add(frags[i].insId,frags[i]);
			}

            return detail;
        }

        public static UsageMemoryDetail GetMemorySample()
        {
            UsageMemoryDetail detail = GetAssetUsageMemory();
            if (mLastMemoryDetail == null)
            {
                mLastMemoryDetail = detail;
                return mLastMemoryDetail;
            }
            UsageMemoryDetail result = detail.Subtract(mLastMemoryDetail);
            mLastMemoryDetail = detail;
            return result;
        }

        public static string toMemoryString(int size)
        {
            if (size < 0)
                return "";
            int b = size % 1024;
            int kb = size / 1024;
            int mb = kb / 1024;
            kb = kb % 1024;
            string result = string.Empty;
            if (mb > 0)
            {
                result += mb.ToString()+"."+kb.ToString() + " MB ";
            }
            else
            if (kb > 0)
            {
                result += kb.ToString() +"."+b.ToString() + " KB ";
            }
            else
            if (b > 0)
            {
                result += b.ToString() + " B ";
            }
            if (string.IsNullOrEmpty(result))
                result = "nil";
            return result;
        }
    }
}
