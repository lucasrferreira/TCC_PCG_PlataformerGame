using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a trigger and it will send your player to the next level
	/// </summary>
	public class FinishLevel : ButtonActivated 
	{
		public string LevelName;

		/// <summary>
		/// When the button is pressed we start the dialogue
		/// </summary>
		public override void TriggerButtonAction()
		{
			GoToNextLevel();
		}			

	    public virtual void GoToNextLevel()
	    {
	        LoadingSceneManager.LoadScene(LevelName);
	    }
	}
}