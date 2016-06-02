using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using TUT;
using Rotorz.ReorderableList;

namespace TUT.RSystem
{
	public class RSEdAliasUnit
	{
		public string Alias;
		public string Target;
	}

	public class RSEdAliasData
	{
		public List<RSEdAliasUnit> AliasList = new List<RSEdAliasUnit>();

		private Dictionary<string,RSEdAliasUnit> mAliasMap = new Dictionary<string, RSEdAliasUnit> ();

		private Dictionary<string,List<RSEdAliasUnit>> mTargetMap = new Dictionary<string, List<RSEdAliasUnit>>();

		public void init()
		{
			mAliasMap.Clear ();
			foreach(RSEdAliasUnit unit in AliasList)
			{
				mAliasMap.Add(unit.Alias,unit);
				if(mTargetMap.ContainsKey(unit.Target))
				{
					mTargetMap[unit.Target].Add(unit);
				}
				else
				{
					mTargetMap.Add(unit.Target,new List<RSEdAliasUnit>(){unit});
				}
			}
		}

		public bool ContainsAlias(string alias)
		{
			return mAliasMap.ContainsKey (alias);		
		}

		public bool Add(string alias,string target)
		{
			RSEdAliasUnit unit = null;
			if(ContainsAlias(alias))
			{
				unit = mAliasMap[alias];
				if(unit.Alias == alias && unit.Target == target)
				{
					return false;
				}
				if(mTargetMap.ContainsKey(unit.Target))
				{
					mTargetMap[unit.Target].Remove(unit);
				}

				unit.Target = target;
			}
			else
			{
				unit = new RSEdAliasUnit();
				unit.Alias = alias;
				unit.Target = target;
				AliasList.Add(unit);
				mAliasMap.Add(alias,unit);


			}
			if(mTargetMap.ContainsKey(unit.Target))
			{
				if(!mTargetMap[unit.Target].Contains(unit))
					mTargetMap[unit.Target].Add(unit);
			}
			else
			{
				mTargetMap.Add(unit.Target,new List<RSEdAliasUnit>(){unit});
			}
			return true;
		}

		public RSEdAliasUnit[] getAliasUnitFromPath(string target)
		{
			if(mTargetMap.ContainsKey(target))
			{
				return mTargetMap[target].ToArray();
			}
			return null;
		}

		public void Remove(string alias)
		{
			if(ContainsAlias(alias))
			{
				string target = mAliasMap[alias].Target;
				if(mTargetMap.ContainsKey(target))
				{
					mTargetMap[target].Remove(mAliasMap[alias]);
					if(mTargetMap[target].Count == 0)
					{
						mTargetMap.Remove(target);
					}
				}
				AliasList.Remove(mAliasMap[alias]);
				mAliasMap.Remove(alias);
			}
		}
	}

	[InitializeOnLoad]
	public class RSEdAlias : EditorWindow
	{
		public static readonly string AliasSavePath = "Assets/_oohhoo_data/_configs/Alias/aliasConfig.bytes";
		public static readonly string AliasCSPath = "Assets/_oohhoo_data/_configs/Alias/RA.cs";

		public static readonly string FileStartStr = "using System.Collections;\n" +
						"public class RA : TUT.RSystem.RSAlias\n" +
						"{\n" +
						"\t protected RA(string target):base(target){}\n";

		public static readonly string FileEndStr = "}";

		public static readonly string FileModeStr = "\t public static RA {0} = new RA(\"{1}\");\n";

		private RSEdAliasData mData = null;

		private RSEdAliasData Data
		{
			get
			{
				if(mData == null)
				{
					if(TutFileUtil.FileExist(AliasSavePath))
					{
						mData = TutFileUtil.ReadJsonFile<RSEdAliasData>(AliasSavePath);
					}

					if(mData == null)
					{
						mData = new RSEdAliasData();
						TutFileUtil.WriteJsonFile(mData,AliasSavePath,true);
					}
					else
					{
						mData.init();
					}
				}
				return mData;
			}
		}

		private void SaveData()
		{
			string file = FileStartStr;
			foreach(RSEdAliasUnit unit in Data.AliasList)
			{
				file += string.Format(FileModeStr,unit.Alias,unit.Target);
			}
			file += FileEndStr;
			TutFileUtil.WriteTxtFile (file, AliasCSPath);
			TutFileUtil.WriteJsonFile(Data,AliasSavePath,true);
			AssetDatabase.Refresh ();
		}

		[MenuItem ("Assets/TUT Asset Alias",false,1112)]
		[MenuItem ("Window/TUT Asset Alias &a",false,1112)]
		static void InitWindow()
		{
			RSEdAlias window = (RSEdAlias)EditorWindow.GetWindow (typeof(RSEdAlias), false, "Asset Alias");
			window.Show();
		}

		private string mSelectedPath = string.Empty;
		private RSEdAliasUnit[] mSelectedUnits =  null;
		void OnInspectorUpdate()
		{
			RefrushSelectObj();
			Repaint();
		}

		private Object mSelectObj = null;
		void RefrushSelectObj()
		{
			if(Selection.activeObject != null)
			{
				if(mSelectObj != Selection.activeObject)
				{
					mSelectObj = Selection.activeObject;
					mSelectedPath = AssetDatabase.GetAssetPath(mSelectObj);
					mSelectedUnits = Data.getAliasUnitFromPath(mSelectedPath);
				}

			}
			else
			{
				Reset();
			}
		}

		void Reset()
		{
			mSelectedPath = string.Empty;
			mSelectObj = null;
		}

		void OnLostFocus()
		{

		}

		string mNewTarget = string.Empty;
		void OnGUI()
		{
			string path = string.Empty;
			if(string.IsNullOrEmpty(mSelectedPath))
			{
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.LabelField("New Target:");
				mNewTarget = EditorGUILayout.TextField(mNewTarget);
				mSelectedUnits = Data.getAliasUnitFromPath(mNewTarget);
				EditorGUILayout.EndHorizontal ();
				path = mNewTarget;
			}
			else
			{
				path = mSelectedPath;
			}
			 
			OnGUISelectPath (path);
			OnGuiData (path);
			EditorGUILayout.BeginHorizontal ();
			if(GUILayout.Button("SAVE"))
			{
				SaveData();
			}
			EditorGUILayout.EndHorizontal ();
		}

		Vector2 mSelectPathSVPos = Vector2.zero;
		int mSelectedAliasIndex = -1;
		string mSelectedNewAlias = string.Empty;
		Vector2 mSelectSVPos = Vector2.zero;

		void OnGUISelectPath(string path)
		{
			mSelectSVPos = EditorGUILayout.BeginScrollView (mSelectSVPos,GUILayout.Height(120));
			EditorGUILayout.BeginVertical ();
			SetLabel ("Selected path: "+path, RSEdConst.Skin.customStyles [2]);

			EditorGUILayout.BeginHorizontal (GUILayout.Width(400));
			SetLabel ("New Alias for this paht: ", RSEdConst.Skin.customStyles [0]);
			mSelectedNewAlias = EditorGUILayout.TextArea (mSelectedNewAlias);
			if(!string.IsNullOrEmpty(mSelectedNewAlias))
			{
				mSelectedNewAlias =mSelectedNewAlias.Replace(" ","");
				if(GUILayout.Button("Add"))
				{
					if(Data.Add(mSelectedNewAlias,path))
					{
						mSelectedAliasIndex = -1;
						mSelectedUnits = Data.getAliasUnitFromPath(path);
						mSelectedNewAlias = string.Empty;
					}
					else
					{
						this.ShowNotification(new GUIContent("The Alias Repeat"));
					}

				}
			}

			EditorGUILayout.EndHorizontal ();

			mSelectPathSVPos = EditorGUILayout.BeginScrollView (mSelectPathSVPos);
			if(mSelectedUnits == null || mSelectedUnits.Length == 0)
			{
				mSelectedAliasIndex = -1;
				SetLabel ("Aliases: null", RSEdConst.Skin.customStyles [0]);
			}
			else
			{
				SetLabel ("Aliases: ", RSEdConst.Skin.customStyles [0]);
				List<string> aliases = new List<string>();
				foreach(RSEdAliasUnit unit in mSelectedUnits)
				{
					aliases.Add(unit.Alias);
				}
				int index = GUILayout.Toolbar (mSelectedAliasIndex, aliases.ToArray());
				if(mSelectedAliasIndex != index)
				{
					Data.Remove( mSelectedUnits[index].Alias);
					mSelectedAliasIndex = -1;
					mSelectedUnits = Data.getAliasUnitFromPath(path);
					mSelectedNewAlias = string.Empty;
				}
			}
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndVertical ();
			EditorGUILayout.EndScrollView ();
		}

		Vector2 mAllAliasSVPos = Vector2.zero;
		void OnGuiData(string path)
		{
			EditorGUILayout.BeginVertical ();
			SetLabel ("Alias List: ", RSEdConst.Skin.customStyles [2]);
			mAllAliasSVPos = EditorGUILayout.BeginScrollView (mAllAliasSVPos);
			for(int i = 0;i<Data.AliasList.Count;)
			{
				if(OnGUIDataUnit(i,Data.AliasList[i],path))
				{
					i++;
				}
			}
			EditorGUILayout.EndScrollView ();
			EditorGUILayout.EndVertical ();
		}

		bool OnGUIDataUnit(int index ,RSEdAliasUnit unit,string path)
		{
			bool remove = false;
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Box (index.ToString ());
			if(GUILayout.Button(unit.Alias))
			{
				if(string.IsNullOrEmpty(mSelectedPath))
				{
					mNewTarget = unit.Target;
				}
				else
				{
					mSelectedPath = unit.Target;
				}


				mSelectedAliasIndex = -1;
				mSelectedUnits = Data.getAliasUnitFromPath(path);
				mSelectedNewAlias = string.Empty;
			}
			if(GUILayout.Button(unit.Target))
			{
				if(Data.Add(unit.Alias,path))
				{
					mSelectedAliasIndex = -1;
					mSelectedUnits = Data.getAliasUnitFromPath(path);
					mSelectedNewAlias = string.Empty;
				}
				else
				{
					this.ShowNotification(new GUIContent("The Path Repeat"));
				}
			}
			if(GUILayout.Button("DEL"))
			{
				Data.Remove( unit.Alias );
				mSelectedAliasIndex = -1;
				mSelectedUnits = Data.getAliasUnitFromPath(path);
				mSelectedNewAlias = string.Empty;
				remove = true;
			}
			EditorGUILayout.EndHorizontal ();
			return !remove;
		}

		void SetLabel(string str, GUIStyle style = null, int width = 0,bool text_mode = false)
		{
			if (EditorGUIUtility.isProSkin)
			{
				GUI.color = Color.white;
			} else
			{
				GUI.color = Color.black;
			}
			if (style != null && width == 0)
			{
				if(text_mode)
					EditorGUILayout.TextField(str,style);
				else
					GUILayout.Label(str, style);
			} else
				if (style == null && width > 0)
			{
				if(text_mode)
					EditorGUILayout.TextField(str,GUILayout.Width(width));
				else
					GUILayout.Label(str, GUILayout.Width(width)); 
			} else
				if (style != null && width > 0)
			{
				if(text_mode)
					EditorGUILayout.TextField(str, style, GUILayout.Width(width));
				else
					GUILayout.Label(str, style, GUILayout.Width(width));
			} else
			{
				if(text_mode)
					EditorGUILayout.TextField(str);
				else
					GUILayout.Label(str);
			}
			GUI.color = Color.white;
		}

		void DrawError()
		{
			GUI.skin = RSEdConst.Skin;
			GUILayout.BeginVertical(GUILayout.Width(400));
			GUILayout.BeginHorizontal();
			GUILayout.Label(new GUIContent(RSEdConst.err_icon),GUILayout.Width(80),GUILayout.Height(80));
			SetLabel("\nPLEASE SELECT A FILE\n\t\t OR A FOLDER....", RSEdConst.Skin.customStyles [2],50);
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUI.skin = null;
		}
	}
}



