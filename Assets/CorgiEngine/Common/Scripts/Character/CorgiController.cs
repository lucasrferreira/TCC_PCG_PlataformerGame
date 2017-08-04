using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	[RequireComponent(typeof(BoxCollider2D))]

	// DISCLAIMER : this controller's been built from the ground up for the Corgi Engine. It takes clues and inspirations from various methods and articles freely 
	// available online. Special thanks to @prime31 for his talent and patience, Yoann Pignole, Mysteriosum and Sebastian Lague, among others for their great articles
	// and tutorials on raycasting. If you have questions or suggestions, feel free to contact me at unitysupport@reuno.net

	/// <summary>
	/// The character controller that handles the character's gravity and collisions.
	/// It requires a Collider2D and a rigidbody to function.
	/// </summary>
	public class CorgiController : MonoBehaviour 
	{
		/// the various states of our character
		public CorgiControllerState State { get; protected set; }
		/// the initial parameters
		public CorgiControllerParameters DefaultParameters;
		/// the current parameters
		public CorgiControllerParameters Parameters{get{return _overrideParameters ?? DefaultParameters;}}
		
		[Space(10)]	
		[Header("Collision Masks")]
		/// The layer mask the platforms are on
		public LayerMask PlatformMask=0;
		/// The layer mask the moving platforms are on
		public LayerMask MovingPlatformMask=0;
		/// The layer mask the one way platforms are on
		public LayerMask OneWayPlatformMask=0;
		/// The layer mask the moving one way platforms are on
		public LayerMask MovingOneWayPlatformMask=0;
		/// gives you the object the character is standing on
		public GameObject StandingOn { get; protected set; }	
		/// the current velocity of the character
		public Vector2 Speed { get{ return _speed; } }
		/// the value of the forces applied at one point in time 
		public Vector2 ForcesApplied { get; protected set; }
		
		[Space(10)]	
		[Header("Raycasting")]
		/// the number of rays cast horizontally
		public int NumberOfHorizontalRays = 8;
		/// the number of rays cast vertically
		public int NumberOfVerticalRays = 8;
		/// a small value added to all raycasts to accomodate for edge cases	
		public float RayOffset=0.05f; 
		
		public Vector3 ColliderCenter {get
			{
				Vector3 colliderCenter = Vector3.Scale(transform.localScale, _boxCollider.offset);
				return colliderCenter;
			}}
		public Vector3 ColliderPosition {get
			{
				Vector3 colliderPosition = transform.position + ColliderCenter;
				return colliderPosition;
			}}
		public Vector3 ColliderSize {get
			{
				Vector3 colliderSize = Vector3.Scale(transform.localScale, _boxCollider.size);
				return colliderSize;
			}}
		public Vector3 BottomPosition {get
			{
				Vector3 colliderBottom = new Vector3(ColliderPosition.x,ColliderPosition.y - (ColliderSize.y / 2),ColliderPosition.z);
				return colliderBottom;
			}}
		public float Friction { get
			{
				return _friction;
			}}

	    // parameters override storage
	    protected CorgiControllerParameters _overrideParameters;
	    // private local references			
	    protected Vector2 _speed;
	    protected float _friction=0;
	    protected float _fallSlowFactor;
	    protected Vector2 _externalForce;
	    protected Vector2 _newPosition;
	    protected Transform _transform;
	    protected BoxCollider2D _boxCollider;
	    protected GameObject _lastStandingOn;
	    protected LayerMask _platformMaskSave;
		protected PathMovement _movingPlatform=null;
	    protected float _movingPlatformCurrentGravity;
		protected bool _gravityActive=true;

	    protected const float _largeValue=500000f;
	    protected const float _smallValue=0.0001f;
	    protected const float _obstacleHeightTolerance=0.05f;
		protected const float _movingPlatformsGravity=-500;

	    protected Vector2 _originalColliderSize;
	    protected Vector2 _originalColliderOffset;

	    // rays parameters
	    protected Rect _rayBoundsRectangle;

	    protected List<RaycastHit2D> _contactList;

	    /// <summary>
	    /// initialization
	    /// </summary>
	    protected virtual void Awake()
		{
			// we get the various components
			_transform=transform;
			_boxCollider = (BoxCollider2D)GetComponent<BoxCollider2D>();
			_originalColliderSize = _boxCollider.size;
			_originalColliderOffset = _boxCollider.offset;
			
			// we test the boxcollider's x offset. If it's not null we trigger a warning.
			if (_boxCollider.offset.x!=0)
			{
				Debug.LogWarning("The boxcollider for "+gameObject.name+" should have an x offset set to zero. Right now this may cause issues when you change direction close to a wall.");
			}

			// raycast list and state init
			_contactList = new List<RaycastHit2D>();
			State = new CorgiControllerState();
			
			// we add the edge collider platform and moving platform masks to our initial platform mask so they can be walked on	
			_platformMaskSave = PlatformMask;	
			PlatformMask |= OneWayPlatformMask;
			PlatformMask |= MovingPlatformMask;
			PlatformMask |= MovingOneWayPlatformMask;
			
			State.Reset();
			SetRaysParameters();
		}

	    /// <summary>
	    /// Use this to add force to the character
	    /// </summary>
	    /// <param name="force">Force to add to the character.</param>
	    public virtual void AddForce(Vector2 force)
		{
			_speed += force;	
			_externalForce += force;
		}
		
		/// <summary>
		///  use this to set the horizontal force applied to the character
		/// </summary>
		/// <param name="x">The x value of the velocity.</param>
		public virtual void AddHorizontalForce(float x)
		{
			_speed.x += x;
			_externalForce.x += x;
		}
		
		/// <summary>
		///  use this to set the vertical force applied to the character
		/// </summary>
		/// <param name="y">The y value of the velocity.</param>
		public virtual void AddVerticalForce(float y)
		{
			_speed.y += y;
			_externalForce.y += y;
		}
		
		/// <summary>
		/// Use this to set the force applied to the character
		/// </summary>
		/// <param name="force">Force to apply to the character.</param>
		public virtual void SetForce(Vector2 force)
		{
			_speed = force;
			_externalForce = force;	
		}
		
		/// <summary>
		///  use this to set the horizontal force applied to the character
		/// </summary>
		/// <param name="x">The x value of the velocity.</param>
		public virtual void SetHorizontalForce (float x)
		{
			_speed.x = x;
			_externalForce.x = x;
		}
		
		/// <summary>
		///  use this to set the vertical force applied to the character
		/// </summary>
		/// <param name="y">The y value of the velocity.</param>
		public virtual void SetVerticalForce (float y)
		{
			_speed.y = y;
			_externalForce.y = y;
			
		}

	    /// <summary>
	    /// This is called every frame
	    /// </summary>
	    protected virtual void Update()
		{	
			EveryFrame();
		}

		/// <summary>
		/// Every frame, we apply the gravity to our character, then check using raycasts if an object's been hit, and modify its new position 
	    /// accordingly. When all the checks have been done, we apply that new position. 
		/// </summary>
		protected virtual void EveryFrame()
		{
			_contactList.Clear();

			if (_gravityActive)
			{
				_speed.y += (Parameters.Gravity + _movingPlatformCurrentGravity) * Time.deltaTime;
			}
					
			if (_fallSlowFactor!=0)
			{
				_speed.y*=_fallSlowFactor;
			}

			// we initialize our newposition, which we'll use in all the next computations			
			_newPosition=Speed * Time.deltaTime;
						
			State.WasGroundedLastFrame = State.IsCollidingBelow;
			State.WasTouchingTheCeilingLastFrame = State.IsCollidingAbove;
			State.Reset(); 

			// we initialize our rays
			SetRaysParameters();

			HandleMovingPlatforms();

			// we store our current speed for use in moving platforms mostly
			ForcesApplied = _speed;

			// we cast rays on all sides to check for slopes and collisions
			CastRaysToTheSides();
			CastRaysBelow();	
			CastRaysAbove();

			// we move our transform to its next position
			_transform.Translate(_newPosition,Space.World);			

			SetRaysParameters();	
						
			// we compute the new speed
			if (Time.deltaTime > 0)
			{
				_speed = _newPosition / Time.deltaTime;			
			}

			// we apply our slope speed factor based on the slope's angle
			if (State.IsGrounded)
			{
				_speed.x *= Parameters.SlopeAngleSpeedFactor.Evaluate(State.BelowSlopeAngle * Mathf.Sign(_speed.y));
			}

			if (!State.OnAMovingPlatform)				
			{
				// we make sure the velocity doesn't exceed the MaxVelocity specified in the parameters
				_speed.x = Mathf.Clamp(_speed.x,-Parameters.MaxVelocity.x,Parameters.MaxVelocity.x);
				_speed.y = Mathf.Clamp(_speed.y,-Parameters.MaxVelocity.y,Parameters.MaxVelocity.y);
			}

			
			// we change states depending on the outcome of the movement
			if( !State.WasGroundedLastFrame && State.IsCollidingBelow )
				State.JustGotGrounded=true;
				
			if (State.IsCollidingLeft || State.IsCollidingRight || State.IsCollidingBelow || State.IsCollidingRight)
			{
				OnCorgiColliderHit();
			}			
			
			_externalForce.x=0;
			_externalForce.y=0;
		}

		/// <summary>
		/// If the CorgiController is standing on a moving platform, we match its speed
		/// </summary>
		protected virtual void HandleMovingPlatforms()
		{
			if (_movingPlatform!=null)			
			{
				State.OnAMovingPlatform=true;
				_transform.Translate(_movingPlatform.CurrentSpeed*Time.deltaTime);

				GravityActive(false);

				_movingPlatformCurrentGravity=_movingPlatformsGravity;
				_newPosition.y = _movingPlatform.CurrentSpeed.y*Time.deltaTime;		

				_speed = - _newPosition / Time.deltaTime;	
				SetRaysParameters();
			}
		}

		/// <summary>
		/// Disconnects the CorgiController from its current moving platform.
		/// </summary>
		public virtual void DetachFromMovingPlatform()
		{
			State.OnAMovingPlatform=false;
			_movingPlatform=null;
			_movingPlatformCurrentGravity=0;
		}

	    /// <summary>
	    /// Casts rays to the sides of the character, from its center axis.
	    /// If we hit a wall/slope, we check its angle and move or not according to it.
	    /// </summary>
	    protected virtual void CastRaysToTheSides() 
		{			
			float movementDirection=1;	
			if ((_speed.x < 0) || (_externalForce.x<0))
				movementDirection = -1;
			
			float horizontalRayLength = Mathf.Abs(_speed.x*Time.deltaTime) + _rayBoundsRectangle.width/2 + RayOffset*2;
			
			Vector2 horizontalRayCastFromBottom=new Vector2(_rayBoundsRectangle.center.x,
			                                                _rayBoundsRectangle.yMin+_obstacleHeightTolerance);										
			Vector2 horizontalRayCastToTop=new Vector2(	_rayBoundsRectangle.center.x,
			                                           _rayBoundsRectangle.yMax-_obstacleHeightTolerance);				
			
			RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfHorizontalRays];	
					
			for (int i=0; i<NumberOfHorizontalRays;i++)
			{	
				Vector2 rayOriginPoint = Vector2.Lerp(horizontalRayCastFromBottom,horizontalRayCastToTop,(float)i/(float)(NumberOfHorizontalRays-1));
				
				if ( State.WasGroundedLastFrame && i == 0 )			
					hitsStorage[i] = MMDebug.RayCast (rayOriginPoint,movementDirection*(Vector2.right),horizontalRayLength,PlatformMask,Color.red,Parameters.DrawRaycastsGizmos);	
				else
					hitsStorage[i] = MMDebug.RayCast (rayOriginPoint,movementDirection*(Vector2.right),horizontalRayLength,PlatformMask & ~OneWayPlatformMask & ~MovingOneWayPlatformMask,Color.red,Parameters.DrawRaycastsGizmos);			
				
				if (hitsStorage[i].distance >0)
				{						
					float hitAngle = Mathf.Abs(Vector2.Angle(hitsStorage[i].normal, Vector2.up));		
					
					State.LateralSlopeAngle = hitAngle	;					
					
					if (hitAngle > Parameters.MaximumSlopeAngle)
					{														
						if (movementDirection < 0)		
							State.IsCollidingLeft=true;
						else
							State.IsCollidingRight=true;						
						
						State.SlopeAngleOK=false;
						
						if (movementDirection<=0)
						{
							_newPosition.x = -Mathf.Abs(hitsStorage[i].point.x - horizontalRayCastFromBottom.x) 
								+ _rayBoundsRectangle.width/2 
									+ RayOffset*2;
						}
						else
						{						
							_newPosition.x = Mathf.Abs(hitsStorage[i].point.x - horizontalRayCastFromBottom.x) 
								- _rayBoundsRectangle.width/2 
									- RayOffset*2;						
						}					
						
						_contactList.Add(hitsStorage[i]);
						_speed = new Vector2(0, _speed.y);
						break;
					}
				}						
			}
			
			
		}

	    /// <summary>
	    /// Every frame, we cast a number of rays below our character to check for platform collisions
	    /// </summary>
	    protected virtual void CastRaysBelow()
		{
			_friction=0;

			if (_newPosition.y < -_smallValue)
			{
				State.IsFalling=true;
			}
			else
			{
				State.IsFalling = false;
			}
			
			if ((Parameters.Gravity > 0) && (!State.IsFalling))
				return;
			
			float rayLength = _rayBoundsRectangle.height/2 + RayOffset ; 	

			if (State.OnAMovingPlatform)
			{
				rayLength*=2;
			}	

			if (_newPosition.y<0)
			{
				rayLength+=Mathf.Abs(_newPosition.y);
			}			
			
			Vector2 verticalRayCastFromLeft=new Vector2(_rayBoundsRectangle.xMin+_newPosition.x,
			                                            _rayBoundsRectangle.center.y+RayOffset);	
			Vector2 verticalRayCastToRight=new Vector2(	_rayBoundsRectangle.xMax+_newPosition.x,
			                                           _rayBoundsRectangle.center.y+RayOffset);					
			
			RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfVerticalRays];
			float smallestDistance=_largeValue; 
			int smallestDistanceIndex=0; 						
			bool hitConnected=false; 		
			
			for (int i=0; i<NumberOfVerticalRays;i++)
			{			
				Vector2 rayOriginPoint = Vector2.Lerp(verticalRayCastFromLeft,verticalRayCastToRight,(float)i/(float)(NumberOfVerticalRays-1));
				
				if ((_newPosition.y>0) && (!State.WasGroundedLastFrame))
					hitsStorage[i] = MMDebug.RayCast (rayOriginPoint,-(Vector2.up),rayLength,PlatformMask & ~OneWayPlatformMask & ~MovingOneWayPlatformMask,Color.blue,Parameters.DrawRaycastsGizmos);	
				else
					hitsStorage[i] = MMDebug.RayCast (rayOriginPoint,-(Vector2.up),rayLength,PlatformMask,Color.blue,Parameters.DrawRaycastsGizmos);					
				
				if ((Mathf.Abs(hitsStorage[smallestDistanceIndex].point.y - verticalRayCastFromLeft.y)) <  _smallValue)
				{
					break;
				}		
				
				if (hitsStorage[i])
				{
					hitConnected=true;
					State.BelowSlopeAngle = Vector2.Angle( hitsStorage[i].normal, Vector2.up )  ;
					if (hitsStorage[i].distance<smallestDistance)
					{
						smallestDistanceIndex=i;
						smallestDistance = hitsStorage[i].distance;
					}
				}								
			}
			if (hitConnected)
			{
				
				StandingOn=hitsStorage[smallestDistanceIndex].collider.gameObject;
			
				// if the character is jumping onto a (1-way) platform but not high enough, we do nothing
				if (
					!State.WasGroundedLastFrame 
					&& (smallestDistance<_rayBoundsRectangle.size.y/2) 
					&& (
						StandingOn.layer==LayerMask.NameToLayer("OneWayPlatforms")  
						||
						StandingOn.layer==LayerMask.NameToLayer("MovingOneWayPlatforms") 
						) 
					)
				{
					State.IsCollidingBelow=false;
					return;
				}
			
				State.IsFalling=false;			
				State.IsCollidingBelow=true;
											

				// if we're applying an external force (jumping, jetpack...) we only apply that
				if (_externalForce.y>0)
				{
					_newPosition.y = _speed.y * Time.deltaTime;
					State.IsCollidingBelow = false;
				}
				// if not, we just adjust the position based on the raycast hit
				else
				{
					_newPosition.y = -Mathf.Abs(hitsStorage[smallestDistanceIndex].point.y - verticalRayCastFromLeft.y) 
					+ _rayBoundsRectangle.height/2 
						+ RayOffset;
				}
				
				if (!State.WasGroundedLastFrame && _speed.y>0)
				{
					_newPosition.y += _speed.y * Time.deltaTime;
				}				
				
				if (Mathf.Abs(_newPosition.y)<_smallValue)
					_newPosition.y = 0;

				// we check if whatever we're standing on applies a friction change
				if (hitsStorage[smallestDistanceIndex].collider.GetComponent<SurfaceModifier>()!=null)
				{
					_friction=hitsStorage[smallestDistanceIndex].collider.GetComponent<SurfaceModifier>().Friction;
				}
				
				// we check if the character is standing on a moving platform
				PathMovement movingPlatform = hitsStorage[smallestDistanceIndex].collider.GetComponent<PathMovement>();

				if (movingPlatform!=null && State.IsGrounded)
				{
					_movingPlatform=movingPlatform;
				}
			}
			else
			{
				State.IsCollidingBelow=false;
				if(State.OnAMovingPlatform)
				{
					DetachFromMovingPlatform();
				}
			}	
			
			
		}

	    /// <summary>
	    /// If we're in the air and moving up, we cast rays above the character's head to check for collisions
	    /// </summary>
	    protected virtual void CastRaysAbove()
		{
			
			if (_newPosition.y<0)
				return;
			
			float rayLength = State.IsGrounded?RayOffset : _newPosition.y*Time.deltaTime;
			rayLength+=_rayBoundsRectangle.height/2;
			
			bool hitConnected=false; 
			
			Vector2 verticalRayCastStart=new Vector2(_rayBoundsRectangle.xMin+_newPosition.x,
			                                         _rayBoundsRectangle.center.y);	
			Vector2 verticalRayCastEnd=new Vector2(	_rayBoundsRectangle.xMax+_newPosition.x,
			                                       _rayBoundsRectangle.center.y);	
			
			RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfVerticalRays];
			float smallestDistance=_largeValue; 
			
			for (int i=0; i<NumberOfVerticalRays;i++)
			{							
				Vector2 rayOriginPoint = Vector2.Lerp(verticalRayCastStart,verticalRayCastEnd,(float)i/(float)(NumberOfVerticalRays-1));
				hitsStorage[i] = MMDebug.RayCast (rayOriginPoint,(Vector2.up),rayLength,PlatformMask & ~OneWayPlatformMask & ~MovingOneWayPlatformMask,Color.green,Parameters.DrawRaycastsGizmos);	

				if (hitsStorage[i])
				{
					hitConnected=true;
					if (hitsStorage[i].distance<smallestDistance)
					{
						smallestDistance = hitsStorage[i].distance;
					}
				}	
				
			}	
			
			if (hitConnected)
			{
				_speed.y=0;
				_newPosition.y = smallestDistance - _rayBoundsRectangle.height/2   ;
				
				if ( (State.IsGrounded) && (_newPosition.y<0) )
				{
					_newPosition.y=0;
				}
							
				State.IsCollidingAbove=true;
				
				if (!State.WasTouchingTheCeilingLastFrame)
				{
					_newPosition.x=0;
					_speed = new Vector2(0, _speed.y);
				}
			}	
		}

	    /// <summary>
	    /// Creates a rectangle with the boxcollider's size for ease of use and draws debug lines along the different raycast origin axis
	    /// </summary>
	    public virtual void SetRaysParameters() 
		{		
			
			_rayBoundsRectangle = new Rect(_boxCollider.bounds.min.x,
			                               _boxCollider.bounds.min.y,
			                               _boxCollider.bounds.size.x,
			                               _boxCollider.bounds.size.y);	


			Debug.DrawLine(new Vector2(_rayBoundsRectangle.center.x,_rayBoundsRectangle.yMin),new Vector2(_rayBoundsRectangle.center.x,_rayBoundsRectangle.yMax),Color.yellow);  
			Debug.DrawLine(new Vector2(_rayBoundsRectangle.xMin,_rayBoundsRectangle.center.y),new Vector2(_rayBoundsRectangle.xMax,_rayBoundsRectangle.center.y),Color.yellow);
			
			
			
		}
		
		
		/// <summary>
		/// Disables the collisions for the specified duration
		/// </summary>
		/// <param name="duration">the duration for which the collisions must be disabled</param>
		public virtual IEnumerator DisableCollisions(float duration)
		{
			// we turn the collisions off
			CollisionsOff();
			// we wait for a few seconds
			yield return new WaitForSeconds (duration);
			// we turn them on again
			CollisionsOn();
		}

		/// <summary>
		/// Disables the collisions with one way platforms for the specified duration
		/// </summary>
		/// <param name="duration">the duration for which the collisions must be disabled</param>
		public virtual IEnumerator DisableCollisionsWithOneWayPlatforms(float duration)
		{
			// we turn the collisions off
			CollisionsOffWithOneWayPlatforms ();
			// we wait for a few seconds
			yield return new WaitForSeconds (duration);
			// we turn them on again
			CollisionsOn();
		}

		/// <summary>
		/// Disables the collisions with moving platforms for the specified duration
		/// </summary>
		/// <param name="duration">the duration for which the collisions must be disabled</param>
		public virtual IEnumerator DisableCollisionsWithMovingPlatforms(float duration)
		{
			// we turn the collisions off
			CollisionsOffWithMovingPlatforms ();
			// we wait for a few seconds
			yield return new WaitForSeconds (duration);
			// we turn them on again
			CollisionsOn();
		}



		/// <summary>
		/// Resets the collision mask with the default settings
		/// </summary>
		public virtual void CollisionsOn()
		{
			PlatformMask=_platformMaskSave;
			PlatformMask |= OneWayPlatformMask;
			PlatformMask |= MovingPlatformMask;
			PlatformMask |= MovingOneWayPlatformMask;
		}

		/// <summary>
		/// Turns all collisions off
		/// </summary>
		public virtual void CollisionsOff()
		{
			PlatformMask=0;
		}

		/// <summary>
		/// Disables collisions only with the one way platform layers
		/// </summary>
		public virtual void CollisionsOffWithOneWayPlatforms()
		{

			PlatformMask -= OneWayPlatformMask;
			PlatformMask -= MovingOneWayPlatformMask;
		}

		/// <summary>
		/// Disables collisions only with moving platform layers
		/// </summary>
		public virtual void CollisionsOffWithMovingPlatforms()
		{
			PlatformMask -= MovingPlatformMask;
			PlatformMask -= MovingOneWayPlatformMask;
		}

		/// <summary>
		/// Resets all overridden parameters.
		/// </summary>
		public virtual void ResetParameters()
		{
			_overrideParameters = DefaultParameters;
		}

		/// <summary>
		/// Slows the character's fall by the specified factor.
		/// </summary>
		/// <param name="factor">Factor.</param>
		public virtual void SlowFall(float factor)
		{
			_fallSlowFactor=factor;
		}

		/// <summary>
	    /// Activates or desactivates the gravity for this character only.
	    /// </summary>
	    /// <param name="state">If set to <c>true</c>, activates the gravity. If set to <c>false</c>, turns it off.</param>	   
		public virtual void GravityActive(bool state)
		{
			if (state)
			{
				_gravityActive = true;
			}
			else
			{
				_gravityActive = false;
			}
		}

		public virtual void ResizeCollider(Vector2 newSize)
		{
			float newYOffset =_originalColliderOffset.y -  (_originalColliderSize.y - newSize.y)/2 ;

			_boxCollider.size = newSize;
			_boxCollider.offset = newYOffset*Vector3.up;
			SetRaysParameters();
		}

		public virtual void ResetColliderSize()
		{
			_boxCollider.size = _originalColliderSize;
			_boxCollider.offset = _originalColliderOffset;
			SetRaysParameters();
		}

		public virtual bool CanGoBackToOriginalSize()
		{
			// if we're already at original size, we return true
			if (_boxCollider.size == _originalColliderSize)
			{
				return true;
			}
			float headCheckDistance = _originalColliderSize.y*transform.localScale.y ;
			bool headCheck = MMDebug.RayCast(_boxCollider.bounds.min+(Vector3.up*_smallValue),Vector2.up,headCheckDistance,PlatformMask,Color.cyan,true);
			return headCheck;
		}

	    // Events


	    /// <summary>
	    /// triggered when the character's raycasts collide with something 
	    /// </summary>
	    protected virtual void OnCorgiColliderHit() 
		{
			foreach (RaycastHit2D hit in _contactList )
			{			
				Rigidbody2D body = hit.collider.attachedRigidbody;
				if (body == null || body.isKinematic)
					return;
							
				Vector3 pushDir = new Vector3(_externalForce.x, 0, 0);
							
				body.velocity = pushDir.normalized * Parameters.Physics2DPushForce;		
			}		
		}

	    /// <summary>
	    /// triggered when the character enters a collider
	    /// </summary>
	    /// <param name="collider">the object we're colliding with.</param>
	    protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			
			CorgiControllerPhysicsVolume2D parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
			if (parameters == null)
				return;
			// if the object we're colliding with has parameters, we apply them to our character.
			_overrideParameters = parameters.ControllerParameters;
		}

	    /// <summary>
	    /// triggered while the character stays inside another collider
	    /// </summary>
	    /// <param name="collider">the object we're colliding with.</param>
	    protected virtual void OnTriggerStay2D( Collider2D collider )
		{
		}

	    /// <summary>
	    /// triggered when the character exits a collider
	    /// </summary>
	    /// <param name="collider">the object we're colliding with.</param>
	    protected virtual void OnTriggerExit2D(Collider2D collider)
		{		
			CorgiControllerPhysicsVolume2D parameters = collider.gameObject.GetComponent<CorgiControllerPhysicsVolume2D>();
			if (parameters == null)
				return;
			
			// if the object we were colliding with had parameters, we reset our character's parameters
			_overrideParameters = null;
		}				
	}
}