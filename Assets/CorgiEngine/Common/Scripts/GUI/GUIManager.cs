using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Handles all GUI effects and changes
	/// </summary>
	public class GUIManager : Singleton<GUIManager> 
	{
		/// the game object that contains the heads up display (avatar, health, points...)
		public GameObject HUD;
		/// the pause screen game object
		public GameObject PauseScreen;	
		/// the time splash gameobject
		public GameObject TimeSplash;
		/// The mobile buttons
		public CanvasGroup Buttons;
		/// The mobile movement pad
		public CanvasGroup Joystick;
		/// the points counter
		public Text PointsText;
		/// the level display
		public Text LevelText;
		/// the screen used for all fades
		public Image Fader;
		/// the jetpack bar
		public GameObject JetPackBar;

		protected float _initialJoystickAlpha;
		protected float _initialButtonsAlpha;

		protected override void Awake()
		{
			base.Awake();

			if (Joystick!=null)
			{
				_initialJoystickAlpha=Joystick.alpha;
			}
			if (Buttons!=null)
			{
				_initialButtonsAlpha=Buttons.alpha;
			}
		}

	    /// <summary>
	    /// Initialization
	    /// </summary>
	    protected virtual void Start()
		{
			RefreshPoints();
		}

	    /// <summary>
	    /// Sets the HUD active or inactive
	    /// </summary>
	    /// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
	    public virtual void SetHUDActive(bool state)
	    {
	        if (HUD!= null)
	        { 
	            HUD.SetActive(state);
	        }
	        if (PointsText!= null)
	        { 
	            PointsText.enabled = state;
	        }
	        if (LevelText!= null)
	        { 
	            LevelText.enabled = state;
	        }
	    }

	    /// <summary>
	    /// Sets the avatar active or inactive
	    /// </summary>
	    /// <param name="state">If set to <c>true</c> turns the HUD active, turns it off otherwise.</param>
	    public virtual void SetAvatarActive(bool state)
	    {
	        if (HUD != null)
	        {
	            HUD.SetActive(state);
	        }
	    }

	    public virtual void SetMobileControlsActive(bool state)
		{
			if (Joystick!=null)
			{
				Joystick.gameObject.SetActive(state);
				if (state)
				{
					Joystick.alpha=_initialJoystickAlpha;
				}
				else
				{
					Joystick.alpha=0;
				}
			}
			if (Buttons!=null)
			{
				Buttons.gameObject.SetActive(state);
				if (state)
				{
					Buttons.alpha=_initialButtonsAlpha;
				}
				else
				{
					Buttons.alpha=0;
				}
			}
		}

		/// <summary>
		/// Sets the pause.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public virtual void SetPause(bool state)
		{
	        if (PauseScreen!= null)
	        { 
	    		PauseScreen.SetActive(state);
	        }
	    }

		/// <summary>
		/// Sets the jetpackbar active or not.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, sets the pause.</param>
		public virtual void SetJetpackBar(bool state)
		{
	        if (JetPackBar != null)
	        { 
			    JetPackBar.SetActive(state);
	        }
	    }

		/// <summary>
		/// Sets the time splash.
		/// </summary>
		/// <param name="state">If set to <c>true</c>, turns the timesplash on.</param>
		public virtual void SetTimeSplash(bool state)
		{
	        if (TimeSplash != null)
	        {
	            TimeSplash.SetActive(state);
	        }
		}
		
		/// <summary>
		/// Sets the text to the game manager's points.
		/// </summary>
		public virtual void RefreshPoints()
		{
	        if (PointsText!= null)
	        { 
	    		PointsText.text="$"+GameManager.Instance.Points.ToString("000000");
	        }
	    }
		
		/// <summary>
		/// Sets the level name in the HUD
		/// </summary>
		public virtual void SetLevelName(string name)
		{
	        if (LevelText!= null)
	        { 
	    		LevelText.text=name;
	        }
	    }
		
		/// <summary>
		/// Fades the fader in or out depending on the state
		/// </summary>
		/// <param name="state">If set to <c>true</c> fades the fader in, otherwise out if <c>false</c>.</param>
		public virtual void FaderOn(bool state,float duration)
		{
	        if (Fader!= null)
	        { 
			    Fader.gameObject.SetActive(true);
			    if (state)
				    StartCoroutine(MMFade.FadeImage(Fader,duration, new Color(0,0,0,1f)));
			    else
				    StartCoroutine(MMFade.FadeImage(Fader,duration,new Color(0,0,0,0f)));
	        }
	    }
		

	}
}