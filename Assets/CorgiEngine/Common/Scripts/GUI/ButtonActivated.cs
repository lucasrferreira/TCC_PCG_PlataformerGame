using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	[RequireComponent(typeof(Collider2D))]
	/// <summary>
	/// Extend this class to activate something when a button is pressed in a certain zone
	/// </summary>
	public class ButtonActivated : MonoBehaviour 
	{
		[Header("Activation")]
		/// if true, the zone will activate whether the button is pressed or not
		public bool AutoActivation=false;
		/// Set this to true if you want the CharacterBehaviorState to be notified of the player's entry into the zone.
		public bool ShouldUpdateState=true;

		[Header("Visual Prompt")]
		/// If true, the "buttonA" prompt will always be shown, whether the player is in the zone or not.
		public bool AlwaysShowPrompt=true;
		/// If true, the "buttonA" prompt will be shown when a player is colliding with the zone
		public bool ShowPromptWhenColliding=true;
		/// the position of the actual buttonA prompt relative to the object's center
		public Vector3 PromptRelativePosition = Vector3.zero;

		protected GameObject _buttonA;
	    protected Collider2D _collider;
	    protected CharacterBehavior _character;

	    /// <summary>
	    /// Determines whether this instance can show button prompt.
	    /// </summary>
	    /// <returns><c>true</c> if this instance can show prompt; otherwise, <c>false</c>.</returns>
	    public virtual bool CanShowPrompt()
	    {
	    	if (_buttonA==null)
	    	{
	    		return true;
	    	}
	    	return false;
	    }


		protected virtual void OnEnable()
		{
			_collider = (Collider2D)GetComponent<Collider2D>();
			if (AlwaysShowPrompt)
			{
				ShowPrompt();
			}
		}

		/// <summary>
		/// Override this to trigger an action when the main button is pressed within the zone
		/// </summary>
		public virtual void TriggerButtonAction()
		{

		}

		/// <summary>
	    /// Shows the button A prompt.
	    /// </summary>
	    public virtual void ShowPrompt()
		{
			// we add a blinking A prompt to the top of the zone
			_buttonA = (GameObject)Instantiate(Resources.Load("GUI/ButtonA"));			
			_buttonA.transform.position=_collider.bounds.center + PromptRelativePosition; 
			_buttonA.transform.parent = transform;
			_buttonA.GetComponent<SpriteRenderer>().material.color=new Color(1f,1f,1f,0f);
			StartCoroutine(MMFade.FadeSprite(_buttonA.GetComponent<SpriteRenderer>(),0.2f,new Color(1f,1f,1f,1f)));	
		}

	    /// <summary>
	    /// Hides the button A prompt.
	    /// </summary>
		public virtual IEnumerator HidePrompt()
		{	
			StartCoroutine(MMFade.FadeSprite(_buttonA.GetComponent<SpriteRenderer>(),0.2f,new Color(1f,1f,1f,0f)));	
			yield return new WaitForSeconds(0.3f);
			Destroy(_buttonA);
		}

		/// <summary>
		/// Triggered when something collides with the button activated zone
		/// </summary>
		/// <param name="collider">Something colliding with the water.</param>
		protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			// we check that the object colliding with the water is actually a corgi controller and a character
			CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
			if (character==null)
				return;		
			CorgiController controller = collider.GetComponent<CorgiController>();
			if (controller==null)
				return;	
			if (character.tag!="Player")
				return;	

			if (AutoActivation)
			{
				TriggerButtonAction();
			}
				

			if (ShouldUpdateState)
			{
				_character=character;
				_character.BehaviorState.InButtonActivatedZone=true;
				_character.BehaviorState.ButtonActivatedZone=this;
			}

			// if we're not already showing the prompt and if the zone can be activated, we show it
			if (CanShowPrompt() && ShowPromptWhenColliding)
			{
				ShowPrompt();	
			}
		}

		/// <summary>
		/// Triggered when something exits the water
		/// </summary>
		/// <param name="collider">Something colliding with the dialogue zone.</param>
		protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			// we check that the object colliding with the water is actually a corgi controller and a character
			CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
			if (character==null)
				return;		
			CorgiController controller = collider.GetComponent<CorgiController>();
			if (controller==null)
				return;
			if (character.tag!="Player")
				return;		

			if ((_buttonA!=null) && !AlwaysShowPrompt)
			{
				StartCoroutine(HidePrompt());	
			}	

			if (ShouldUpdateState)
			{
				_character=character;
				_character.BehaviorState.InButtonActivatedZone=false;
				_character.BehaviorState.ButtonActivatedZone=null;		
			}
		}
	}
}
