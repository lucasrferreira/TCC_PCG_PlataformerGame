using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{	
	public class HipsterGUI : MonoBehaviour 
	{

		// Use this for initialization
		void Start ()
	    {
	        GUIManager.Instance.SetAvatarActive(false);
	    }
	}
}