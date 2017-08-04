using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Simple start screen class.
	/// </summary>
	public class StartScreen : MonoBehaviour
	{
		public string FirstLevel;

		public float AutoSkipDelay = 0f;

		protected float _delayAfterClick = 1f;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start()
		{	
			GUIManager.Instance.SetHUDActive (false);
			GUIManager.Instance.FaderOn (false, 1f);

			if (AutoSkipDelay > 1f)
			{
				_delayAfterClick = AutoSkipDelay;
				StartCoroutine (LoadFirstLevel ());
			}
		}

		/// <summary>
		/// During update we simply wait for the user to press the "jump" button.
		/// </summary>
		protected virtual void Update()
		{
			if (!Input.GetButtonDown ("Jump"))
				return;
			
			ButtonPressed ();
		}

		public virtual void ButtonPressed()
		{
			GUIManager.Instance.FaderOn (true, _delayAfterClick);
			// if the user presses the "Jump" button, we start the first level.
			StartCoroutine (LoadFirstLevel ());
		}

		protected virtual IEnumerator LoadFirstLevel()
		{
			yield return new WaitForSeconds (_delayAfterClick);
			LoadingSceneManager.LoadScene (FirstLevel);
		}
	}
}