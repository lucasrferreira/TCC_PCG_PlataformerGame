using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{
	public class PauseButton : MonoBehaviour
	{
	    public virtual void PauseButtonAction()
	    {
	        GameManager.Instance.Pause();
	    }	
	}
}