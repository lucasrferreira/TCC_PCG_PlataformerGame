using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a trigger to cause the level to restart when the player hits the trigger
	/// </summary>
	public class LevelRestarter : MonoBehaviour 
	{
	    protected virtual void OnTriggerEnter2D (Collider2D collider)
		{
			if(collider.tag == "Player")
			{
				LoadingSceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
		}
	}
}