using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.Security.AccessControl;
namespace TUT
{
    /// <summary>
    /// 文件相关的辅助类
    /// 
    /// </summary>
    public class TutFileUtil
    {
        public static bool CreateFolder(string path)
        {
            if (!IsFolder(path))
                return false;
            if (Directory.Exists(path))
            {
                return false;
            }

            Directory.CreateDirectory(path);
			return true;
        }

        public static bool IsFolder(string path)
        {
            string tp = path.Replace("\\", "/");
            int las = tp.LastIndexOf("/");
            tp = tp.Substring(las + 1);
            return !tp.Contains(".");
        }

        public static bool FileExist(string path)
        {
            return File.Exists(path);
        }

		public static string GetFileSuffix(string path)
		{
			if (IsFolder(path))
				return string.Empty;	
			return path.Substring (path.LastIndexOf ("."));
		}

        public static string GetFileName(string path)
        {
            if (IsFolder(path))
				return string.Empty;

            string p = path.Replace("\\", "/");
            int las = p.LastIndexOf("/");
            if (p.Contains("."))
            {
                return p.Substring(las+1, p.LastIndexOf(".") - (las+1));
            } else
            {
                return p.Substring(0, las);
            }
        }

        public static string GetFile(string path)
        {
            if (IsFolder(path))
                return string.Empty;
            string p = path.Replace("\\", "/");
            int las = p.LastIndexOf("/");
            return p.Substring(las+1);
        }

        public static string GetPathParent(string path)
        {
            return Path.GetDirectoryName(path);
        }

        public static string GetFilePath(string path)
        {
            string p = path.Replace("\\", "/");
            if (p.EndsWith("/"))
                return path;
            return p.Substring(0, p.LastIndexOf("/"));
        }

        public static string[] GetFiles(string path, SearchOption option = SearchOption.TopDirectoryOnly,string search = null)
        {
            List<string> dirs = new List<string>();
            if (!IsFolder(path))
            {
                for(int i = 0;i<dirs.Count;i++)
                {
                    dirs[i] = dirs[i].Replace("\\","/");
                }
                return dirs.ToArray();
            }

            if (option == SearchOption.TopDirectoryOnly)
            {
                if(!string.IsNullOrEmpty(search))
                    dirs.AddRange(Directory.GetFiles(path,search,option));
                else
                    dirs.AddRange(Directory.GetFiles(path));
                for(int i = 0;i<dirs.Count;i++)
                {
                    dirs[i] = dirs[i].Replace("\\","/");
                }
                return dirs.ToArray();
            }

            if(!string.IsNullOrEmpty(search))
                dirs.AddRange(Directory.GetFiles(path,search,option));
            else
                dirs.AddRange(Directory.GetFiles(path));
            
            foreach (string dir in GetDirectories(path))
            {
                dirs.AddRange(GetFiles(dir, option,search));
            }

            for(int i = 0;i<dirs.Count;i++)
            {
                dirs[i] = dirs[i].Replace("\\","/");
            }
            return dirs.ToArray();
        }

		public static bool CopyFile(string source,string target)
		{
			if(!FileExist(source))
				return false;
			if(FileExist(target))
			{
				DeleteFile(target);
			}
			CreateFolder (GetFilePath (target));
			File.Copy (source, target);		
			return true;
		}

        public static string[] GetDirectories(string path)
        {
            List<string> dirs = new List<string>();
            dirs.AddRange(Directory.GetDirectories(path));
            for(int i = 0;i<dirs.Count;i++)
            {
                dirs[i] = dirs[i].Replace("\\","/");
            }
            return dirs.ToArray();
        }

        public static T ReadJsonFile<T>(string path)
        {
            if (!FileExist(path))
                return default(T);
            if (!FileExist(path))
            {
                return default(T);
            }
            T obj = default(T);
            try
            {
                using (StreamReader reader =  File.OpenText(path))
                {
                    if (reader == null)
                        return default(T);
                    string text = reader.ReadToEnd();
                    obj = LitJson.JsonMapper.ToObject<T>(text);
                    reader.Close();
                }
            } catch (LitJson.JsonException e)
            {
                Debug.LogError(TutNorm.LogErrFormat("Json Parse", "read json failed: " + e.ToString()));
                return default(T);
            } catch (Exception e)
            {
                Debug.LogError(TutNorm.LogErrFormat("Json Parse", "read json failed: " + e.ToString()));
                return default(T);
            }
            return obj;
        }

        public static T ReadJsonString<T>(string text)
        {
            if (string.IsNullOrEmpty(text))
                return default(T);
            T obj = default(T);
            try
            {
                obj = LitJson.JsonMapper.ToObject<T>(text);
            } catch (LitJson.JsonException e)
            {
                Debug.LogError(TutNorm.LogErrFormat("Json Parse", "read json failed: " + e.ToString()));
                return default(T);
            }
            return obj;
        }

        public static bool WriteJsonFile(object obj, string path,bool pretty_print = false)
        {
            try
            {
                CreateFolder(GetFilePath(path));
                if(File.Exists(path))
                    File.SetAttributes(path, FileAttributes.Normal);
                System.IO.TextWriter writer = new System.IO.StreamWriter(path, false);
                LitJson.JsonWriter jw = new JsonWriter(writer as System.IO.TextWriter);   
                jw.PrettyPrint = pretty_print;
                LitJson.JsonMapper.ToJson(obj, jw);
                writer.Close();
            } catch (Exception e)
            {
                Debug.Log(TutNorm.LogErrFormat("Make Json", "write json failed: " + e.ToString()));
                return false;
            }
            return true;
        }



        public static bool WriteTxtFile(string str, string path)
        {
            try
            {
                CreateFolder(GetFilePath(path));
                if(File.Exists(path))
                    File.SetAttributes(path, FileAttributes.Normal);
                System.IO.TextWriter writer = new System.IO.StreamWriter(path, false);
                writer.Write(str);
                writer.Close();
            } catch (Exception e)
            {
                Debug.Log(TutNorm.LogErrFormat("Make Json", "write json failed: " + e.ToString()));
                return false;
            }
            return true;
        }

        public static int GetFileSize(string path)
        {
            if(!File.Exists(path))
                return 0;
            FileInfo fileInfo = new FileInfo(path);
            return Convert.ToInt32(fileInfo.Length);
        }

        public static void DeleteFile(string path)
        {
//            if(!IsFolder(path))
            {
                if(File.Exists(path))
                {
                    File.SetAttributes(path, FileAttributes.Normal);
                    File.Delete(path);
                }
            }
        }

        public static void DeleteFolder(string path)
        {
            if(!IsFolder(path))
                return;
			if(Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path);
                for(int i = 0;i<files.Length;i++)
                {
					File.SetAttributes(path, FileAttributes.Normal);
                    File.Delete(files[i]);
                }
                string[] folders = Directory.GetDirectories(path);
                for(int i = 0;i<folders.Length;i++)
                {
                    DeleteFolder(folders[i]);
                }

				Directory.Delete(path);
            }
        }

        public static double GetFileLastTime(string file)
        {
//            if (!FileExist(file))
//            {
//                return 0;
//            }
            FileInfo info = new FileInfo(file);
            DateTime origin = new DateTime (1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = info.LastWriteTime - origin;
            double clientStamp =  Math.Floor ( diff.TotalSeconds );
            return clientStamp;
        }
    }
}