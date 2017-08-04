#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// This class adds names for each LevelMapPathElement next to it on the scene view, for easier setup
	/// </summary>
	[CustomEditor(typeof(PathMovement))]
	[InitializeOnLoad]
	public class PathMovementEditor : Editor 
	{		
		/// <summary>
		/// OnSceneGUI, draws repositionable handles at every point in the path, for easier setup
		/// </summary>
		public void OnSceneGUI()
	    {
			Handles.color=Color.green;
			PathMovement t = (target as PathMovement);

			if (t._OriginalTransformPositionSet==false)
			{
				return;
			}

			for (int i=0;i<t.Path.Count;i++)
			{
	       		EditorGUI.BeginChangeCheck();

				Vector3 oldPoint = t._OriginalTransformPosition+t.Path[i];
				GUIStyle style = new GUIStyle();

				// draws the path item number
		        style.normal.textColor = Color.yellow;	 
				Handles.Label(t._OriginalTransformPosition+t.Path[i]+(Vector3.down*0.4f)+(Vector3.right*0.4f), ""+i,style);

				// draws a movable handle
				Vector3 newPoint = Handles.FreeMoveHandle(oldPoint, Quaternion.identity,.5f,new Vector3(.25f,.25f,.25f),Handles.CircleCap);

				// records changes
				if (EditorGUI.EndChangeCheck())
		        {
		            Undo.RecordObject(target, "Free Move Handle");
					t.Path[i] = newPoint - t._OriginalTransformPosition;
		        }
			}	        
	    }
	}
}

#endif