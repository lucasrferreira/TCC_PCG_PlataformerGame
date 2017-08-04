using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// This persistent singleton handles the inputs and sends commands to the player
	/// </summary>
	public class InputManager : Singleton<InputManager>
	{
		public enum InputForcedMode { None, Mobile, Desktop }
		[Information("If you check Auto Mobile Detection, the engine will automatically switch to mobile controls when your build target is Android or iOS. You can also force mobile or desktop (keyboard, gamepad) controls using the dropdown below.\nNote that if you don't need mobile controls and/or GUI this component can also work on its own, just put it on an empty GameObject instead.",InformationAttribute.InformationType.Info,false)]
		public bool AutoMobileDetection = true;
		public InputForcedMode ForcedMode;
		public bool IsMobile { get; protected set; }
		[HideInInspector]
		public bool InDialogueZone;
	    protected static CharacterBehavior _player;
	    protected static CorgiController _controller;

	    protected float _horizontalMove = 0;
	    protected float _verticalMove = 0;

	    /// <summary>
	    /// We get the player from its tag.
	    /// </summary>
	    protected virtual void Start()
		{
			InDialogueZone = false;
			if (GameManager.Instance.Player!=null)
			{
				_player = GameManager.Instance.Player;
				if (GameManager.Instance.Player.GetComponent<CorgiController>() != null)	
				{
					_controller = GameManager.Instance.Player.GetComponent<CorgiController>();
				}
			}

			if (GUIManager.Instance!=null)
			{
				GUIManager.Instance.SetMobileControlsActive(false);
				IsMobile=false;
				if (AutoMobileDetection)
				{
					#if UNITY_ANDROID || UNITY_IPHONE
						GUIManager.Instance.SetMobileControlsActive(true);
						IsMobile = true;
					 #endif
				}
				if (ForcedMode==InputForcedMode.Mobile)
				{
					GUIManager.Instance.SetMobileControlsActive(true);
					IsMobile = true;
				}
				if (ForcedMode==InputForcedMode.Desktop)
				{
					GUIManager.Instance.SetMobileControlsActive(false);
					IsMobile = false;					
				}
			}
		}

	    /// <summary>
	    /// At update, we check the various commands and send them to the player.
	    /// </summary>
	    protected virtual void Update()
		{		

			// if we can't get the player, we do nothing
			if (_player == null) 
			{
				if (GameManager.Instance.Player!=null)
				{
					if (GameManager.Instance.Player.GetComponent<CharacterBehavior> () != null)
						_player = GameManager.Instance.Player;
						_controller = GameManager.Instance.Player.GetComponent<CorgiController>();
				}
				else
				{
					return;
				}
			}
			
			if ( Input.GetButtonDown("Pause"))
			{
				Pause();
			}
				
			if (GameManager.Instance.Paused)
			{
				return;	
			}
						
			SetMovement();
			
			if ((Input.GetButtonDown("Run")||Input.GetButton("Run")) )
			{
				Run();
			}				
			
			if (Input.GetButtonUp("Run"))
			{
				RunStop();
			}	

			if (Input.GetButtonDown ("Jump")) 
			{
				JumpPressedDown();
			}			
							
			if (Input.GetButtonUp("Jump"))
			{
				JumpReleased();
			}
						
			if ( Input.GetButtonDown("Dash") )
			{
				Dash();
			}						
			
			if (Input.GetButtonDown("Fire"))
			{
				ShootOnce();
			}	
			
			if (Input.GetButton("Fire")) 
			{
				ShootStart();
			}				
			
			if (Input.GetButtonUp("Fire"))
			{
				ShootStop();
			}				

			if (Input.GetButtonDown("Melee"))
			{
				Melee();
			}

			if ((Input.GetButtonDown("Jetpack")||Input.GetButton("Jetpack")) )
			{
				Jetpack();
			}	
			
			if (Input.GetButtonUp("Jetpack"))
			{
				JetpackStop();
			}	

				
		}

		public virtual void SetMovement()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			if (!IsMobile)
			{
				_player.SetHorizontalMove(Input.GetAxis("Horizontal"));
				_player.SetVerticalMove(Input.GetAxis("Vertical"));	
				if (_player.GetComponent<CharacterShoot>()!=null)
				{
					_player.GetComponent<CharacterShoot>().SetHorizontalMove(Input.GetAxis ("Horizontal"));
					_player.GetComponent<CharacterShoot>().SetVerticalMove(Input.GetAxis ("Vertical"));
				}
			}
		}

		public virtual void SetMovement(Vector2 movement)
		{
			MMEventManager.TriggerFloatEvent("HorizontalMovement",movement.x);
			MMEventManager.TriggerFloatEvent("VerticalMovement",movement.y);
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			if (IsMobile)
			{
				_player.SetHorizontalMove(movement.x);
				_player.SetVerticalMove(movement.y);	
				if (_player.GetComponent<CharacterShoot>()!=null)
				{
					_player.GetComponent<CharacterShoot>().SetHorizontalMove(movement.x);
					_player.GetComponent<CharacterShoot>().SetVerticalMove(movement.y);
				}
			}
		}	

		public virtual void JumpPressedDown()
		{
			MMEventManager.TriggerEvent("Jump");

			if (_player==null) { return; }

			// if the player is in a dialogue zone, we handle it
			if ((_player.BehaviorState.InButtonActivatedZone)
			    &&(_player.BehaviorState.ButtonActivatedZone!=null)
			    &&(!_player.BehaviorState.IsDead)
			    &&(!_player.BehaviorState.Firing)
			    &&(!_player.BehaviorState.Dashing))
			{
				_player.BehaviorState.ButtonActivatedZone.TriggerButtonAction();
			}
			
			if (!_player.BehaviorState.InButtonActivatedZone)
			{
				_player.JumpStart ();
			}
		}

		public virtual void JumpReleased()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			_player.JumpStop();
		}

		public virtual void Dash()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			_player.Dash();
		}

		public virtual void Run()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			_player.RunStart();		
		}

		public virtual void RunStop()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			_player.RunStop();	
		}

		public virtual void Jetpack()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			if (_player.GetComponent<CharacterJetpack>()!=null)
			{
				_player.GetComponent<CharacterJetpack>().JetpackStart();
			}
		}

		public virtual void JetpackStop()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			if (_player.GetComponent<CharacterJetpack>()!=null)
			{
				_player.GetComponent<CharacterJetpack>().JetpackStop();
			}
		}

		public virtual void Melee()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			if (_player.GetComponent<CharacterMelee>() != null) 
			{				
				_player.GetComponent<CharacterMelee>().Melee();
			}
		}

		public virtual void ShootOnce()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			if (_player.GetComponent<CharacterShoot>() != null) 
			{
				_player.GetComponent<CharacterShoot>().ShootOnce();		
			}
		}

		public virtual void ShootStart()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			if (_player.GetComponent<CharacterShoot>() != null) 
			{
				_player.GetComponent<CharacterShoot>().ShootStart();
			}
		}

		public virtual void ShootStop()
		{
			if (!GameManager.Instance.CanMove || _player==null) { return; }
			if (_player.GetComponent<CharacterShoot>() != null) 
			{
				_player.GetComponent<CharacterShoot>().ShootStop();
			}
		}

		public virtual void Pause()
		{
			GameManager.Instance.Pause();
		}
	}
}