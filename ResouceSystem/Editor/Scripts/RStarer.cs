
using UnityEngine;
using UnityEditor;
using System.Collections;
using TUT;

namespace TUT.RSystem
{
    [InitializeOnLoad]
    public class RStarter
    {
        static RStarter()
        {
            EditorApplication.projectWindowChanged += OnProjectWindowChanged;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
        }

        static void OnProjectWindowChanged()
        {
        
        }

        static void OnProjectWindowItemOnGUI(string guid, Rect selectionRect)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            RSInfo info = RSEdManifest.GetInfo(path);
            if (info != null)
            {
                switch (info.rstype)
                {
                    case RSType.RT_BUNDLE:
					if(RSInspector.LimitedSuffixs.isOnlyExtralLocalAsset(path))
					{
						DrawIconForProjectItem(RSEdConst.nil_icon, selectionRect, -5, 5);
					}
					else
					DrawIconForProjectItem(RSEdConst.bld_icon, selectionRect, -5, 5);
                        break;
                    case RSType.RT_RESOURCES:
						if(RSInspector.LimitedSuffixs.isNoSupportLocalAsset(path) || 
						   RSInspector.LimitedSuffixs.isOnlyExternalAsset(path) ||
						   RSInspector.LimitedSuffixs.isOnlyExtralLocalAsset(path))
						{
							DrawIconForProjectItem(RSEdConst.nil_icon, selectionRect, -5, 5);
//							Debug.LogError("No Support Local Asset : "+ path);
						}
						else
							DrawIconForProjectItem(RSEdConst.res_icon, selectionRect, -5, 5);
					break;
				case RSType.RT_STREAM:
					if(RSInspector.LimitedSuffixs.isOnlyExternalAsset(path))
					{
						DrawIconForProjectItem(RSEdConst.nil_icon, selectionRect, -5, 5);
					}
					else
                        DrawIconForProjectItem(RSEdConst.stm_icon, selectionRect, -5, 5);
                        break;
                    case RSType.RT_NIL:
                        DrawIconForProjectItem(RSEdConst.nil_icon, selectionRect, -5, 5);
                        break;
                }
            } else
            {
                if (RSInfo.isResTypeFromPath(path))
                {
					if(RSInspector.LimitedSuffixs.isNoSupportLocalAsset(path) || 
					   RSInspector.LimitedSuffixs.isOnlyExternalAsset(path) ||
					   RSInspector.LimitedSuffixs.isOnlyExtralLocalAsset(path))
					{
						DrawIconForProjectItem(RSEdConst.nil_icon, selectionRect, -5, 5);
//						Debug.LogError("No Support Local Asset : "+ path);
					}
					else
                    	DrawIconForProjectItem(RSEdConst.res_icon, selectionRect, -5, 5);
                }
            }
        }

        static void DrawIconForProjectItem(Texture tex, Rect draw_rect, float offset_x, float offset_y)
        {
            Rect tar_rect = draw_rect;
            tar_rect.x += offset_x;
            tar_rect.y += offset_y;
            GUI.Label(tar_rect, tex);
        }
    }
}