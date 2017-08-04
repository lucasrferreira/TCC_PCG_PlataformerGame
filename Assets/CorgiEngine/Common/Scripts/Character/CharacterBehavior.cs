using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// This class will pilot the CorgiController component of your character.
	/// This is where you'll implement all of your character's game rules, like jump, dash, shoot, stuff like that.
	/// </summary>
	public class CharacterBehavior : MonoBehaviour,CanTakeDamage
	{		
		/// the current health of the character
		public int Health {get; set; }	
		
		/// the various states of the character
		public CharacterBehaviorState BehaviorState { get; protected set; }
		/// the default parameters of the character
		public CharacterBehaviorParameters DefaultBehaviorParameters;	
		/// the current behavior parameters (they can be overridden at times)
		public CharacterBehaviorParameters BehaviorParameters{get{return _overrideBehaviorParameters ?? DefaultBehaviorParameters;}}
		/// the permissions associated to the character
		public CharacterBehaviorPermissions Permissions ; 
		
		[Space(10)]	
		[Header("Particle Effects")]
		/// the effect that will be instantiated everytime the character touches the ground
		public ParticleSystem TouchTheGroundEffect;
		/// the effect that will be instantiated everytime the character touches the ground
		public ParticleSystem HurtEffect;
		
		[Space(10)]	
		[Header("Sounds")]
		// the sound to play when the player jumps
		public AudioClip PlayerJumpSfx;
		// the sound to play when the player gets hit
		public AudioClip PlayerHitSfx;
		
		/// is true if the character can jump
		public bool JumpAuthorized 
		{ 
			get 
			{ 
				if ( (BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhere) ||  (BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhereAnyNumberOfTimes) )
					return true;
				
				if (BehaviorParameters.JumpRestrictions == CharacterBehaviorParameters.JumpBehavior.CanJumpOnGround)
					return _controller.State.IsGrounded;
				
				return false; 
			}
		}

	    // associated gameobjects and positions
	    protected CameraController _sceneCamera;
		protected CorgiController _controller;

	    protected Animator _animator;
	    protected CharacterJetpack _jetpack;
	    protected CharacterShoot _shoot;
	    protected Color _initialColor;
	    protected Vector3 _initialScale;

	    // storage for overriding behavior parameters
	    protected CharacterBehaviorParameters _overrideBehaviorParameters;
	    // storage for original gravity and timer
	    protected float _originalGravity;

	    // the current normalized horizontal speed
	    protected float _normalizedHorizontalSpeed;

	    // pressure timed jumps
	    protected float _jumpButtonPressTime = 0;
	    protected bool _jumpButtonPressed=false;
	    protected bool _jumpButtonReleased=false;

	    // duration (in seconds) we need to disable collisions when jumping
	    public float OneWayPlatformsJumpCollisionOffDuration { get; protected set; }		
		public float MovingPlatformsJumpCollisionOffDuration { get; protected set; }

	    // true if the player is facing right
	    protected bool _isFacingRight=true;

		protected Vector3 _leftOne = new Vector3(-1,1,1);

	    // INPUT AXIS
	    protected float _horizontalMove;
	    protected float _verticalMove;
		
		/// <summary>
		/// Initializes this instance of the character
		/// </summary>
		protected virtual void Awake()
		{		
			BehaviorState = new CharacterBehaviorState();
			_sceneCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
			_controller = GetComponent<CorgiController>();
			_jetpack = GetComponent<CharacterJetpack>();
			_shoot = GetComponent<CharacterShoot> ();
	        _initialScale = transform.localScale;
	        Health =BehaviorParameters.MaxHealth;

			OneWayPlatformsJumpCollisionOffDuration=0.3f;
			MovingPlatformsJumpCollisionOffDuration=0.05f;
			
			if (GetComponent<Renderer>()!=null)
			{
				if (GetComponent<Renderer>().material.HasProperty("_Color"))
				{
					_initialColor=GetComponent<Renderer>().material.color;
				}
				else
				{
					_initialColor=Color.red;
				}
			}
		}

	    /// <summary>
	    /// Further initialization
	    /// </summary>
	    protected virtual void Start()
		{
	        // we get the animator
	        if (GetComponent<Animator>() != null)
	        {
	            _animator = GetComponent<Animator>();
	        }
			// if the width of the character is positive, then it is facing right.
			_isFacingRight = transform.localScale.x > 0;
			
			_originalGravity = _controller.Parameters.Gravity;
			
			// we initialize all the controller's states with their default values.
			BehaviorState.Initialize();
			BehaviorState.NumberOfJumpsLeft=BehaviorParameters.NumberOfJumps;
			

			BehaviorState.CanJump=true;		
		}
		
		/// <summary>
		/// This is called every frame.
		/// </summary>
		protected virtual void Update()
		{		
			EveryFrame();
			// if the character became grounded this frame, we reset the doubleJump flag so he can doubleJump again
			if (_controller.State.JustGotGrounded)
			{
				BehaviorState.NumberOfJumpsLeft=BehaviorParameters.NumberOfJumps;		
			}	
		}

		/// <summary>
		/// We do this every frame. This is separate from Update for more flexibility.
		/// </summary>
		protected virtual void EveryFrame()
		{
			// we send our various states to the animator.		
			if (BehaviorParameters.UseDefaultMecanim)
			{
				// we send our various states to the animator.      
				UpdateAnimator ();
			}
			// if the character is not dead
			if (!BehaviorState.IsDead)
			{
				_controller.GravityActive(true);
										
				// we handle horizontal and vertical movement				
				HorizontalMovement();
				VerticalMovement();			

				// we reset all the BehaviorStates that need resetting (triggers mostly)
				BehaviorState.UpdateReset();

				// if we're grounded, we reset the jumping states
				if (_controller.State.JustGotGrounded)
				{
					BehaviorState.Jumping=false;
					BehaviorState.DoubleJumping=false;
				}
							
				// ladder climbing and wall clinging
				ClimbLadder();
				Grip();
				WallClinging ();
				Dangling();
				
				// If the character is dashing, we cancel the gravity
				if (BehaviorState.Dashing) 
				{
					_controller.GravityActive(false);
					_controller.SetVerticalForce(0);
				}	

				// if the character can jump we handle press time controlled jumps
				if (Permissions.JumpEnabled)
				{
	                // If the user releases the jump button and the character is jumping up and enough time since the initial jump has passed, then we make it stop jumping by applying a force down.
	                if ( (_jumpButtonPressTime!=0) 
					    && (Time.time - _jumpButtonPressTime >= BehaviorParameters.JumpMinimumAirTime) 
					    && (_controller.Speed.y > Mathf.Sqrt(Mathf.Abs(_controller.Parameters.Gravity))) 
					    && (_jumpButtonReleased)
					    && (!_jumpButtonPressed||BehaviorState.Jetpacking))
					{
						_jumpButtonReleased=false;	
						if (BehaviorParameters.JumpIsProportionalToThePressTime)	
						{	
							_jumpButtonPressTime=0;
							_controller.SetVerticalForce(0);
						}
					}
				}

				//we reset the jump button released flag
				if (_jumpButtonReleased && _controller.Speed.y<=0)
				{
					_jumpButtonReleased=false;
				}
			}
			else
			{	
				// if the character is dead, we prevent it from moving horizontally		
				_controller.SetHorizontalForce(0);
			}
		}


	    /// <summary>
	    /// This is called at Update() and sets each of the animators parameters to their corresponding State values
	    /// </summary>
	    protected virtual void UpdateAnimator()
		{	
			if (_animator!= null)
	        { 
			    MMAnimator.UpdateAnimatorBool(_animator,"Grounded",_controller.State.IsGrounded);
				MMAnimator.UpdateAnimatorFloat(_animator,"Speed",Mathf.Abs(_normalizedHorizontalSpeed));
				MMAnimator.UpdateAnimatorFloat(_animator,"vSpeed",_controller.Speed.y);
				MMAnimator.UpdateAnimatorBool(_animator,"Running",BehaviorState.Running);
				MMAnimator.UpdateAnimatorBool(_animator,"Dashing",BehaviorState.Dashing);
				MMAnimator.UpdateAnimatorBool(_animator,"Crouching",BehaviorState.Crouching);
				MMAnimator.UpdateAnimatorBool(_animator,"LookingUp",BehaviorState.LookingUp);
				MMAnimator.UpdateAnimatorBool(_animator,"WallClinging",BehaviorState.WallClinging);
				MMAnimator.UpdateAnimatorBool(_animator,"Jetpacking",BehaviorState.Jetpacking);
				MMAnimator.UpdateAnimatorBool(_animator,"Diving",BehaviorState.Diving);
				MMAnimator.UpdateAnimatorBool(_animator,"LadderClimbing",BehaviorState.LadderClimbing);
				MMAnimator.UpdateAnimatorFloat(_animator,"LadderClimbingSpeed",BehaviorState.LadderClimbingSpeed);
				MMAnimator.UpdateAnimatorBool(_animator,"FiringStop",BehaviorState.FiringStop);
				MMAnimator.UpdateAnimatorBool(_animator,"Firing",BehaviorState.Firing);
				MMAnimator.UpdateAnimatorInteger(_animator,"FiringDirection",BehaviorState.FiringDirection);
				MMAnimator.UpdateAnimatorBool(_animator,"MeleeAttacking",BehaviorState.MeleeAttacking);
				MMAnimator.UpdateAnimatorBool(_animator,"Jumping",BehaviorState.Jumping);
				MMAnimator.UpdateAnimatorBool(_animator,"DoubleJumping",BehaviorState.DoubleJumping);
				MMAnimator.UpdateAnimatorBool(_animator,"JustStartedJumping",BehaviorState.JustStartedJumping);
				MMAnimator.UpdateAnimatorBool(_animator,"CollidingLeft",_controller.State.IsCollidingLeft);
				MMAnimator.UpdateAnimatorBool(_animator,"CollidingRight",_controller.State.IsCollidingRight);
				MMAnimator.UpdateAnimatorBool(_animator,"Dangling",BehaviorState.Dangling);
	        }
	    }
		
		
		/// <summary>
		/// Sets the horizontal move value.
		/// </summary>
		/// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
		public virtual void SetHorizontalMove(float value)
		{
			_horizontalMove=value;
		}
		
		/// <summary>
		/// Sets the vertical move value.
		/// </summary>
		/// <param name="value">Vertical move value, between -1 and 1
		public virtual void SetVerticalMove(float value)
		{
			_verticalMove=value;
		}

	    /// <summary>
	    /// Called at Update(), handles horizontal movement
	    /// </summary>
	    protected virtual void HorizontalMovement()
		{	
			// if movement is prevented, we exit and do nothing
			if (!BehaviorState.CanMoveFreely)
				return;				
			
			// If the value of the horizontal axis is positive, the character must face right.
			if (_horizontalMove>0.1f)
			{
				_normalizedHorizontalSpeed = _horizontalMove;
				if (!_isFacingRight)
					Flip();
			}		
			// If it's negative, then we're facing left
			else if (_horizontalMove<-0.1f)
			{
				_normalizedHorizontalSpeed = _horizontalMove;
				if (_isFacingRight)
					Flip();
			}
			else
			{
				_normalizedHorizontalSpeed=0;
			}


			// we pass the horizontal force that needs to be applied to the controller.
			float movementFactor = _controller.State.IsGrounded ? _controller.Parameters.SpeedAccelerationOnGround : _controller.Parameters.SpeedAccelerationInAir;
			float newHorizontalForce;

			// if we're in smooth movement mode, we lerp the speed, otherwise we just apply it
			if (BehaviorParameters.SmoothMovement)
				newHorizontalForce = Mathf.Lerp(_controller.Speed.x, _normalizedHorizontalSpeed * BehaviorParameters.MovementSpeed, Time.deltaTime * movementFactor);
			else
				newHorizontalForce = _normalizedHorizontalSpeed * BehaviorParameters.MovementSpeed;

			// if we have a friction above 1 (mud, water, stuff like that), we divide our speed by that friction
			if (_controller.Friction>1)
			{
				newHorizontalForce = newHorizontalForce/_controller.Friction;
			}

			// if we have a low friction (ice, marbles...) we lerp the speed accordingly
			if (_controller.Friction<1 && _controller.Friction > 0)
			{
				newHorizontalForce = Mathf.Lerp(_controller.Speed.x, newHorizontalForce, Time.deltaTime * _controller.Friction * 10);
			}

			// we set our newly computed speed to the controller
			_controller.SetHorizontalForce(newHorizontalForce);
		}

	    /// <summary>
	    /// Called at Update(), handles vertical movement
	    /// </summary>
	    protected virtual void VerticalMovement()
		{
			
			// Looking up
			if ( (_verticalMove>BehaviorParameters.VerticalThreshold) && (_controller.State.IsGrounded) )
			{
				BehaviorState.LookingUp = true;	
				if (_sceneCamera!=null)
				{	
					_sceneCamera.LookUp();
				}
			}
			else
			{
				BehaviorState.LookingUp = false;
				if (_sceneCamera!=null)
				{
					_sceneCamera.ResetLookUpDown();
				}
			}
		
			// Manages the ground touching effect
			if (_controller.State.JustGotGrounded)
			{
	            if (TouchTheGroundEffect != null)
	            {
	                Instantiate(TouchTheGroundEffect, _controller.BottomPosition, transform.rotation);
	            }
			}
			
			// if the character is not in a position where it can move freely, we do nothing.
			if (!BehaviorState.CanMoveFreely)
				return;
			
			// Crouch Detection : if the player is pressing "down" and if the character is grounded and the crouch action is enabled
			if ( (_verticalMove<-BehaviorParameters.VerticalThreshold) && (_controller.State.IsGrounded) && (Permissions.CrouchEnabled) )
			{
				BehaviorState.Crouching = true;
				_controller.ResizeCollider(BehaviorParameters.CrouchedBoxColliderSize);
				BehaviorParameters.MovementSpeed = BehaviorParameters.CrouchSpeed;
				BehaviorState.Running=false;
				if (_sceneCamera!=null)
				{
					_sceneCamera.LookDown();			
				}
			}
			else
			{	
				// if the character is currently crouching, we'll check if it's in a tunnel
				if (BehaviorState.Crouching)
				{	
					// we cast a raycast above to see if we have room enough to go back to normal size
					bool headCheck = _controller.CanGoBackToOriginalSize();

					// if the character is not crouched anymore, we set 
					if (!headCheck)
					{
						if (!BehaviorState.Running)
						{
							BehaviorParameters.MovementSpeed = BehaviorParameters.WalkSpeed;
						}
						BehaviorState.Crouching = false;
						_controller.ResetColliderSize();
						BehaviorState.CanJump=true;
					}
					else
					{
						
						BehaviorState.CanJump=false;
					}
				}
			}
			
			if (BehaviorState.CrouchingPreviously!=BehaviorState.Crouching)
			{
				Invoke ("RecalculateRays",Time.deltaTime*10);		
			}
			
			BehaviorState.CrouchingPreviously=BehaviorState.Crouching;
			
			
		}
		
		/// <summary>
		/// Use this method to force the controller to recalculate the rays, especially useful when the size of the character has changed.
		/// </summary>
		public virtual void RecalculateRays()
		{
			_controller.SetRaysParameters();
		}

		/// <summary>
		/// Casts a ray in front of the character and going downwards. If the ray hits nothing, we're close to an edge and start dangling.
		/// </summary>
		protected virtual void Dangling()
		{
			// if dangling is disabled or if we're not grounded, we do nothing and exit
			if (!Permissions.DanglingEnabled || !_controller.State.IsGrounded)
			{
				BehaviorState.Dangling=false;
				return;
			}

			Vector3 raycastOrigin=transform.position + BehaviorParameters.DanglingRaycastOrigin;
			if (!_isFacingRight) { raycastOrigin=transform.position + Vector3.Scale(BehaviorParameters.DanglingRaycastOrigin,_leftOne); }

			RaycastHit2D hit = MMDebug.RayCast (raycastOrigin,Vector3.down,BehaviorParameters.DanglingRaycastLength,_controller.PlatformMask | _controller.OneWayPlatformMask | _controller.MovingOneWayPlatformMask,Color.black,_controller.Parameters.DrawRaycastsGizmos);			

			if (hit)
			{
				BehaviorState.Dangling=false;				
			}
			else
			{
				BehaviorState.Dangling=true;				
			}
		}
		
		/// <summary>
		/// Causes the character to start running.
		/// </summary>
		public virtual void RunStart()
		{		
			// if the Run action is enabled in the permissions, we continue, if not we do nothing
			if (!Permissions.RunEnabled)
				return;
			// if the character is not in a position where it can move freely, we do nothing.
			if (!BehaviorState.CanMoveFreely)
				return;
			
			// if the player presses the run button and if we're on the ground and not crouching and we can move freely, 
			// then we change the movement speed in the controller's parameters.
			if (_controller.State.IsGrounded && !BehaviorState.Crouching)
			{
				BehaviorParameters.MovementSpeed = BehaviorParameters.RunSpeed;
				BehaviorState.Running=true;
			}
		}
		
		/// <summary>
		/// Causes the character to stop running.
		/// </summary>
		public virtual void RunStop()
		{
			// if the run button is released, we revert back to the walking speed.
			BehaviorParameters.MovementSpeed = BehaviorParameters.WalkSpeed;
			BehaviorState.Running=false;
		}
		
		/// <summary>
		/// Causes the character to start jumping.
		/// </summary>
		public virtual void JumpStart()
		{
			
			// if the Jump action is enabled in the permissions, we continue, if not we do nothing. If the player is dead, we do nothing.
			if (!Permissions.JumpEnabled  || !JumpAuthorized || BehaviorState.IsDead || _controller.State.IsCollidingAbove)
				return;
			
			// we check if the character can jump without conflicting with another action
			if (_controller.State.IsGrounded 
			    || BehaviorState.LadderClimbing 
			    || BehaviorState.WallClinging 
			    || BehaviorState.NumberOfJumpsLeft>0) 	
				BehaviorState.CanJump=true;
			else
				BehaviorState.CanJump=false;
						
			// if the player can't jump, we do nothing. 
			if ( (!BehaviorState.CanJump) && !(BehaviorParameters.JumpRestrictions==CharacterBehaviorParameters.JumpBehavior.CanJumpAnywhereAnyNumberOfTimes) )
				return;
			
			// if the character is standing on a one way platform and is also pressing the down button,
			if (_verticalMove<-BehaviorParameters.VerticalThreshold && _controller.State.IsGrounded)
			{
				if (_controller.StandingOn.layer==LayerMask.NameToLayer("OneWayPlatforms") || _controller.StandingOn.layer==LayerMask.NameToLayer("MovingOneWayPlatforms"))
				{
					// we make it fall down below the platform by moving it just below the platform
					_controller.transform.position=new Vector2(transform.position.x,transform.position.y-0.1f);
					// we turn the boxcollider off for a few milliseconds, so the character doesn't get stuck mid platform
					StartCoroutine(_controller.DisableCollisionsWithOneWayPlatforms(OneWayPlatformsJumpCollisionOffDuration));
					_controller.DetachFromMovingPlatform();
					return;
				}
			}

			// if the character is standing on a moving platform and not pressing the down button,
			if (_verticalMove>=-BehaviorParameters.VerticalThreshold && _controller.State.IsGrounded)
			{
				if ( _controller.StandingOn.layer==LayerMask.NameToLayer("MovingPlatforms") || _controller.StandingOn.layer==LayerMask.NameToLayer("MovingOneWayPlatforms"))
				{
					// we turn the boxcollider off for a few milliseconds, so the character doesn't get stuck mid air
					StartCoroutine(_controller.DisableCollisionsWithMovingPlatforms(MovingPlatformsJumpCollisionOffDuration));
					_controller.DetachFromMovingPlatform();
				}
			}

			// if we're still here, the jump will happen
			if (BehaviorState.NumberOfJumpsLeft==BehaviorParameters.NumberOfJumps)
			{
				BehaviorState.Jumping=true;
			}
			else
			{
				BehaviorState.Jumping=true;
				BehaviorState.DoubleJumping=true;
			}

			// we trigger the behavior state that will allow us to detect (in animation for example) when the jump exactly started
			BehaviorState.JustStartedJumping=true;
			
			// we decrease the number of jumps left
			BehaviorState.NumberOfJumpsLeft=BehaviorState.NumberOfJumpsLeft-1;
			BehaviorState.LadderClimbing=false;
			BehaviorState.Gripping=false;
			BehaviorState.CanMoveFreely=true;
			_controller.GravityActive(true);
			
			_jumpButtonPressTime=Time.time;
			_jumpButtonPressed=true;
			_jumpButtonReleased=false;

			// we play the jump sound
			FXPlayJumpSound();
			
			// wall jump
			float wallJumpDirection;
			if (BehaviorState.WallClinging)
			{
				
				// If the character is colliding to the right with something (probably the wall)
				if (_controller.State.IsCollidingRight)
				{
					wallJumpDirection=-1f;
				}
				else
				{					
					wallJumpDirection=1f;
				}
				_controller.SlowFall(0f);
				Vector2 walljumpVector = new Vector2(
										wallJumpDirection*BehaviorParameters.WallJumpForce.x,
										Mathf.Sqrt( 2f * BehaviorParameters.WallJumpForce.y * Mathf.Abs(_controller.Parameters.Gravity))
				);
				_controller.AddForce(walljumpVector);
				BehaviorState.WallClinging=false;
				return;
			}

			_controller.SetVerticalForce(Mathf.Sqrt( 2f * BehaviorParameters.JumpHeight * Mathf.Abs(_controller.Parameters.Gravity) ));
			
				
			
		}
		
		/// <summary>
		/// Causes the character to stop jumping.
		/// </summary>
		public virtual void JumpStop()
		{
			_jumpButtonPressed=false;
			_jumpButtonReleased=true;
		}
		
		/// <summary>
		/// Called at update, handles gripping to Grip components (ropes, etc)
		/// </summary>
		protected virtual void Grip()
		{
			// if we're gripping to something, we disable the gravity
			if (BehaviorState.Gripping)
			{	
				_controller.GravityActive(false);		
			}
		}

		
		/// <summary>
		/// Called at Update(), handles the climbing of ladders
		/// </summary>	
		protected virtual void ClimbLadder()
		{
			// if the character is colliding with a ladder
			if (BehaviorState.LadderColliding)
			{
				// if the player is pressing the up key and not yet climbing a ladder, and not colliding with the top platform and not jetpacking
				if (_verticalMove>BehaviorParameters.VerticalThreshold && !BehaviorState.LadderClimbing && !BehaviorState.LadderTopColliding  && !BehaviorState.Jetpacking)
				{			
					// then the character starts climbing
					BehaviorState.LadderClimbing=true;
					_controller.CollisionsOn();
					
					// it can't move freely anymore
					BehaviorState.CanMoveFreely=false;
					// we make it stop shooting
					if (_shoot!=null)
						_shoot.ShootStop();
					// we initialize the ladder climbing speed to zero
					BehaviorState.LadderClimbingSpeed=0;
					// we make sure the controller won't move
					_controller.SetHorizontalForce(0);
					_controller.SetVerticalForce(0);
					// we disable the gravity
					_controller.GravityActive(false);
				}			
				
				// if the character is climbing the ladder (which means it previously connected with it)
				if (BehaviorState.LadderClimbing)
				{
					// we prevent it from shooting
					BehaviorState.CanShoot=false;
					// we disable the gravity
					_controller.GravityActive(false);
					
					if (!BehaviorState.LadderTopColliding)
						_controller.CollisionsOn();
					
					// we set the vertical force according to the ladder climbing speed
					_controller.SetVerticalForce(_verticalMove * BehaviorParameters.LadderSpeed);
					// we set pass that speed to the climbing speed state.
					BehaviorState.LadderClimbingSpeed=Mathf.Abs(_verticalMove);				
				}
				
				if (!BehaviorState.LadderTopColliding)
				{
					_controller.CollisionsOn();
				}
				
				// if the character is grounded AND climbing
				if (BehaviorState.LadderClimbing && _controller.State.IsGrounded && !BehaviorState.LadderTopColliding)
				{			
					// we make it stop climbing, it has reached the ground.
					BehaviorState.LadderColliding=false;
					BehaviorState.LadderClimbing=false;
					BehaviorState.CanMoveFreely=true;
					BehaviorState.LadderClimbingSpeed=0;	
					_controller.GravityActive(true);			
				}			
			}
			
			// If the character is colliding with the top of the ladder and is pressing down and is not on the ladder yet and is standing on the ground, we make it go down.
			if (BehaviorState.LadderTopColliding && _verticalMove<-BehaviorParameters.VerticalThreshold && !BehaviorState.LadderClimbing && _controller.State.IsGrounded)
			{			
				_controller.CollisionsOff();
				// we force its position to be a bit lower
				transform.position=new Vector2(transform.position.x,transform.position.y-0.1f);
				// we initiate the climbing.
				BehaviorState.LadderClimbing=true;
				BehaviorState.CanMoveFreely=false;
				BehaviorState.LadderClimbingSpeed=0;			
				_controller.SetHorizontalForce(0);
				_controller.SetVerticalForce(0);
				_controller.GravityActive(false);
			}		
		}
		
		/// <summary>
		/// Causes the character to dash or dive (depending on the vertical movement at the start of the dash)
		/// </summary>
		public virtual void Dash()
		{	
			// declarations	
			float _dashDirection;
			float _boostForce;
					
			// if the Dash action is enabled in the permissions, we continue, if not we do nothing
			if (!Permissions.DashEnabled || BehaviorState.IsDead)
				return;
			// if the character is not in a position where it can move freely, we do nothing.
			if (!BehaviorState.CanMoveFreely)
				return;
			
			
			// If the user presses the dash button and is not aiming down
			if (_verticalMove>-0.8) 
			{	
				// if the character is allowed to dash
				if (BehaviorState.CanDash)
				{
					// we set its dashing state to true
					BehaviorState.Dashing=true;
					
					// depending on its direction, we calculate the dash parameters to apply				
					if (_isFacingRight) { _dashDirection=1f; } else { _dashDirection = -1f; }
					_boostForce=_dashDirection*BehaviorParameters.DashForce;
					BehaviorState.CanDash = false;
					// we launch the boost corountine with the right parameters
					StartCoroutine( Boost(BehaviorParameters.DashDuration,_boostForce,0,"dash") );
				}			
			}
			// if the user presses the dash button and is aiming down
			if (_verticalMove<-0.8) 
			{
				_controller.CollisionsOn();
				// we start the dive coroutine
				StartCoroutine(Dive());
			}		
			
		}
		
		/// <summary>
		/// Coroutine used to move the player in a direction over time
		/// </summary>
		protected virtual IEnumerator Boost(float boostDuration, float boostForceX, float boostForceY, string name) 
		{
			float time = 0f; 
			
			// for the whole duration of the boost
			while(boostDuration > time) 
			{
				// we add the force passed as a parameter
				if (boostForceX!=0)
				{
					_controller.AddForce(new Vector2(boostForceX,0));
				}
				if (boostForceY!=0)
				{
					_controller.AddForce(new Vector2(0,boostForceY));
				}
				time+=Time.deltaTime;
				// we keep looping for the duration of the boost
				yield return 0; 
			}
			// once the boost is complete, if we were dashing, we make it stop and start the dash cooldown
			if (name=="dash")
			{
				BehaviorState.Dashing=false;
				_controller.GravityActive(true);
				yield return new WaitForSeconds(BehaviorParameters.DashCooldown); 
				BehaviorState.CanDash = true; 
			}	
			if (name=="wallJump")
			{
				// so far we do nothing, but you could use it to trigger a sound or an effect when walljumping
			}		
		}

	    /// <summary>
	    /// Coroutine used to make the player dive vertically
	    /// </summary>
	    protected virtual IEnumerator Dive()
		{	
			// Shake parameters : intensity, duration (in seconds) and decay
			Vector3 ShakeParameters = new Vector3(1.5f,0.5f,1f);
			BehaviorState.Diving=true;
			// while the player is not grounded, we force it to go down fast
			while (!_controller.State.IsGrounded)
			{
				_controller.SetVerticalForce(-Mathf.Abs(_controller.Parameters.Gravity)*2);
				yield return 0; //go to next frame
			}
			
			// once the player is grounded, we shake the camera, and restore the diving state to false
			_sceneCamera.Shake(ShakeParameters);		
			BehaviorState.Diving=false;
		}

	    /// <summary>
	    /// Makes the player stick to a wall when jumping
	    /// </summary>
	    protected virtual void WallClinging()
		{
			// if the wall clinging action is enabled in the permissions, we continue, if not we do nothing
			if (!Permissions.WallClingingEnabled)
				return;
				
			if (!_controller.State.IsCollidingLeft && !_controller.State.IsCollidingRight)
			{
				BehaviorState.WallClinging=false;
			}
			
			// if the character is not in a position where it can move freely, we do nothing.
			if (!BehaviorState.CanMoveFreely)
				return;
			
			// if the character is in the air and touching a wall and moving in the opposite direction, then we slow its fall
			
			if((!_controller.State.IsGrounded) && ( ( (_controller.State.IsCollidingRight) && (_horizontalMove>0.1f) )	|| 	( (_controller.State.IsCollidingLeft) && (_horizontalMove<-0.1f) )	))
			{
				if (_controller.Speed.y<0)
				{
					BehaviorState.WallClinging=true;
					_controller.SlowFall(BehaviorParameters.WallClingingSlowFactor);
				}
			}
			else
			{
				BehaviorState.WallClinging=false;
				_controller.SlowFall(0f);
			}
		}

	    /// <summary>
	    /// makes the character colliding again with layer 12 (Projectiles) and 13 (Enemies)
	    /// </summary>
	    /// <returns>The layer collision.</returns>
	    protected virtual IEnumerator ResetLayerCollision(float delay)
		{
			yield return new WaitForSeconds (delay);
			Physics2D.IgnoreLayerCollision(9,12,false);
			Physics2D.IgnoreLayerCollision(9,13,false);
		}
		
		/// <summary>
		/// Kills the character, sending it in the air
		/// </summary>
		public virtual void Kill()
		{
			// we make our handheld device vibrate
			#if UNITY_ANDROID || UNITY_IPHONE
				Handheld.Vibrate();
			#endif
			// we make it ignore the collisions from now on
			_controller.CollisionsOff();
			GetComponent<Collider2D>().enabled=false;
			// we set its dead state to true
			BehaviorState.IsDead=true;
			// we set its health to zero (useful for the healthbar)
			Health=0;
			// we send it in the air
			_controller.ResetParameters();
			ResetParameters();
			_controller.SetForce(BehaviorParameters.DeathForce);		
		}
		
		/// <summary>
		/// Called to disable the player (at the end of a level for example. 
		/// It won't move and respond to input after this.
		/// </summary>
		public virtual void Disable()
		{
			enabled=false;
			_controller.enabled=false;
			GetComponent<Collider2D>().enabled=false;		
		}
		
		/// <summary>
		/// Makes the player respawn at the location passed in parameters
		/// </summary>
		/// <param name="spawnPoint">The location of the respawn.</param>
		public virtual void RespawnAt(Transform spawnPoint)
		{
			// we make sure the character is facing right
			if(!_isFacingRight)
			{
				Flip ();
			}
			// we raise it from the dead (if it was dead)
			BehaviorState.IsDead=false;
			// we re-enable its 2D collider
			GetComponent<Collider2D>().enabled=true;
			// we make it handle collisions again
			_controller.CollisionsOn();
			transform.position=spawnPoint.position;
			Health=BehaviorParameters.MaxHealth;
		}
		
		/// <summary>
		/// Called when the player takes damage
		/// </summary>
		/// <param name="damage">The damage applied.</param>
		/// <param name="instigator">The damage instigator.</param>
		public virtual void TakeDamage(int damage,GameObject instigator)
		{
			// we play the sound the player makes when it gets hit
			FXPlayHitSound();
					
			// When the character takes damage, we create an auto destroy hurt particle system
	        if (HurtEffect!= null)
	        { 
	    		Instantiate(HurtEffect,transform.position,transform.rotation);
	        }
	        // we prevent the character from colliding with layer 12 (Projectiles) and 13 (Enemies)
	        Physics2D.IgnoreLayerCollision(9,12,true);
			Physics2D.IgnoreLayerCollision(9,13,true);
			StartCoroutine(ResetLayerCollision(0.5f));
			// We make the character's sprite flicker
			if (GetComponent<Renderer>()!=null)
			{
				Color flickerColor = new Color32(255, 20, 20, 255); 
				StartCoroutine(MMImage.Flicker(GetComponent<Renderer>(),flickerColor,0.05f,0.5f));	
			}	
			// we decrease the character's health by the damage
			Health -= damage;
			if (Health<=0)
			{
				LevelManager.Instance.KillPlayer();
			}
		}
		
		/// <summary>
		/// Called when the character gets health (from a stimpack for example)
		/// </summary>
		/// <param name="health">The health the character gets.</param>
		/// <param name="instigator">The thing that gives the character health.</param>
		public virtual void GiveHealth(int health,GameObject instigator)
		{
			// this function adds health to the character's Health and prevents it to go above MaxHealth.
			Health = Mathf.Min (Health + health,BehaviorParameters.MaxHealth);
		}
		
		/// <summary>
		/// Flips the character and its dependencies (jetpack for example) horizontally
		/// </summary>
		protected virtual void Flip()
		{
			// Flips the character horizontally
			transform.localScale = new Vector3(-transform.localScale.x,transform.localScale.y,transform.localScale.z);
			_isFacingRight = transform.localScale.x > 0;
			
			// we flip the emitters individually because they won't flip otherwise.
			
			if (_jetpack!=null)
			{
				if (_jetpack.Jetpack!=null)
					_jetpack.Jetpack.transform.eulerAngles = new Vector3(_jetpack.Jetpack.transform.eulerAngles.x,_jetpack.Jetpack.transform.eulerAngles.y+180,_jetpack.Jetpack.transform.eulerAngles.z);
			}
			if (_shoot != null) 
			{
				_shoot.Flip();		
			}
		}

	   protected virtual void FXPlayJumpSound()
	   {
			if (PlayerJumpSfx!=null)
			{
				SoundManager.Instance.PlaySound(PlayerJumpSfx,transform.position);
			}
	   }

	   protected virtual void FXPlayHitSound()
	   {
			if (PlayerHitSfx!=null)
			{
				SoundManager.Instance.PlaySound(PlayerHitSfx,transform.position);
			}
	   }		
		
		public virtual void ResetParameters()
		{
			_overrideBehaviorParameters = DefaultBehaviorParameters;
		}
		
		/// <summary>
		/// Called when the character collides with something else
		/// </summary>
		/// <param name="other">The other collider.</param>
		public void OnTriggerEnter2D(Collider2D collider)
		{
			
			var parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
			if (parameters == null)
				return;
			// if the other collider has behavior parameters, we override ours with them.
			_overrideBehaviorParameters = parameters.BehaviorParameters;
		}	
		
		/// <summary>
		/// Called when the character is colliding with something else
		/// </summary>
		/// <param name="other">The other collider.</param>
		public virtual void OnTriggerStay2D( Collider2D collider )
		{
		}	
		
		/// <summary>
		/// Called when the character exits a collider
		/// </summary>
		/// <param name="collider">The other collider.</param>
		public virtual void OnTriggerExit2D(Collider2D collider)
		{		
			
			var parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
			if (parameters == null)
				return;
			
			// if the other collider had behavior parameters, we reset ours
			_overrideBehaviorParameters = null;
		}	
	}
}