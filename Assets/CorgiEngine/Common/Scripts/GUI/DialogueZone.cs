using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	

	[RequireComponent(typeof(Collider2D))]

	/// <summary>
	/// Add this class to an empty component. It will automatically add a boxcollider2d, set it to "is trigger". Then customize the dialogue zone
	/// through the inspector.
	/// </summary>

	public class DialogueZone : ButtonActivated 
	{	
		[Header("Dialogue Look")]
		/// the color of the text background.
		public Color TextBackgroundColor=Color.black;
		/// the color of the text
		public Color TextColor=Color.white;
		/// if true, the dialogue box will have a small, downward pointing arrow
		public bool ArrowVisible=true;
		
		[Space(10)]
		
		[Header("Dialogue Speed (in seconds)")]
		/// the duration of the in and out fades
		public float FadeDuration=0.2f;
		/// the time between two dialogues 
		public float TransitionTime=0.2f;
			
		[Space(10)]	
		[Header("Dialogue Position")]
		/// the distance from the top of the box collider the dialogue box should appear at
		public float DistanceFromTop=0;
		
		[Space(10)]	
		[Header("Player Movement")]
		public bool CanMoveWhileTalking=true;
		[Header("Press button to go from one message to the next ?")]
		public bool ButtonHandled=true;
		/// duration of the message. only considered if the box is not button handled
		[Header("Only if the dialogue is not button handled :")]
		[Range (1, 100)]
		public float MessageDuration=3f;
		
		[Space(10)]
		
		[Header("Activations")]
		/// true if can be activated more than once
		public bool ActivableMoreThanOnce=true;
		/// if the zone is activable more than once, how long should it remain inactive between up times ?
		[Range (1, 100)]
		public float InactiveTime=2f;
		
		[Space(10)]
		
		/// the dialogue lines
		[Multiline]
		public string[] Dialogue;

	    /// private variables
	    protected DialogueBox _dialogueBox;
	    protected bool _activated=false;
	    protected bool _playing=false;
	    protected int _currentIndex;
	    protected bool _activable=true;

		/// <summary>
	    /// Determines whether this instance can show button prompt.
	    /// </summary>
	    /// <returns><c>true</c> if this instance can show prompt; otherwise, <c>false</c>.</returns>
	    public override bool CanShowPrompt()
	    {
			if ( (_buttonA==null) && _activable && !_playing )
	    	{
	    		return true;
	    	}
	    	return false;
	    }
	

	    /// <summary>
	    /// Initializes the dialogue zone
	    /// </summary>
		protected override void OnEnable () 
		{		
			base.OnEnable();
			_currentIndex=0;					
		}

		/// <summary>
		/// When the button is pressed we start the dialogue
		/// </summary>
		public override void TriggerButtonAction()
		{
			if (_playing && !ButtonHandled)
			{
				return;
			}
			StartDialogue();
		}
			
		/// <summary>
		/// When triggered, either by button press or simply entering the zone, starts the dialogue
		/// </summary>
		public virtual void StartDialogue()
		{
			// if the button A prompt is displayed, we hide it
			if (_buttonA!=null)
				Destroy(_buttonA);
		
			// if the dialogue zone has no box collider, we do nothing and exit
			if (_collider==null)
				return;	
			
			// if the zone has already been activated and can't be activated more than once.
			if (_activated && !ActivableMoreThanOnce)
				return;
				
			// if the zone is not activable, we do nothing and exit
			if (!_activable)
				return;
			
			// if the player can't move while talking, we notify the game manager
			if (!CanMoveWhileTalking)
			{
				GameManager.Instance.FreezeCharacter();
			}
										
			// if it's not already playing, we'll initialize the dialogue box
			if (!_playing)
			{	
				// we instantiate the dialogue box
				GameObject dialogueObject = (GameObject)Instantiate(Resources.Load("GUI/DialogueBox"));
				_dialogueBox = dialogueObject.GetComponent<DialogueBox>();		
				// we set its position
				_dialogueBox.transform.position=new Vector2(_collider.bounds.center.x,_collider.bounds.max.y+DistanceFromTop); 
				// we set the color's and background's colors
				_dialogueBox.ChangeColor(TextBackgroundColor,TextColor);
				// if it's a button handled dialogue, we turn the A prompt on
				_dialogueBox.ButtonActive(ButtonHandled);
				// if we don't want to show the arrow, we tell that to the dialogue box
				if (!ArrowVisible)
					_dialogueBox.HideArrow();			
				
				// the dialogue is now playing
				_playing=true;
			}
			// we start the next dialogue
			StartCoroutine(PlayNextDialogue());
		}

	    /// <summary>
	    /// Plays the next dialogue in the queue
	    /// </summary>
	    protected virtual IEnumerator PlayNextDialogue()
		{		
			// we check that the dialogue box still exists
			if (_dialogueBox == null) 
			{
				yield return null;
			}
			// if this is not the first message
			if (_currentIndex!=0)
			{
				// we turn the message off
				_dialogueBox.FadeOut(FadeDuration);	
				// we wait for the specified transition time before playing the next dialogue
				yield return new WaitForSeconds(TransitionTime);
			}	
			
			// if we've reached the last dialogue line, we exit
			if (_currentIndex>=Dialogue.Length)
			{
				_currentIndex=0;
				Destroy(_dialogueBox.gameObject);
				_collider.enabled=false;
				// we set activated to true as the dialogue zone has now been turned on		
				_activated=true;
				// we let the player move again
				if (!CanMoveWhileTalking)
				{
					GameManager.Instance.CanMove=true;
				}
				if ((_character!=null))
				{				
					_character.BehaviorState.InButtonActivatedZone=false;
					_character.BehaviorState.ButtonActivatedZone=null;
				}
				// we turn the zone inactive for a while
				if (ActivableMoreThanOnce)
				{
					_activable=false;
					_playing=false;
					StartCoroutine(Reactivate());
				}
				else
				{
					gameObject.SetActive(false);
				}
				yield break;
			}

			// we check that the dialogue box still exists
			if (_dialogueBox.DialogueText!=null)
			{
				// every dialogue box starts with it fading in
				_dialogueBox.FadeIn(FadeDuration);
				// then we set the box's text with the current dialogue
				_dialogueBox.DialogueText.text=Dialogue[_currentIndex];
			}
			
			_currentIndex++;
			
			// if the zone is not button handled, we start a coroutine to autoplay the next dialogue
			if (!ButtonHandled)
			{
				StartCoroutine(AutoNextDialogue());
			}
		}

	    protected virtual IEnumerator AutoNextDialogue()
		{
			// we wait for the duration of the message
			yield return new WaitForSeconds(MessageDuration);
			StartCoroutine(PlayNextDialogue());
		}

	    protected virtual IEnumerator Reactivate()
		{
			yield return new WaitForSeconds(InactiveTime);
			_collider.enabled=true;
			_activable=true;
			_playing=false;
			_currentIndex=0;

			if (AlwaysShowPrompt)
			{
				ShowPrompt();
			}
			
		}			    
	}
}