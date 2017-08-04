using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a CorgiController2D and it will walk, turn back when it hits a wall, and try and avoid holes if you ask it to.
	/// </summary>
	public class AISimpleWalk : MonoBehaviour,IPlayerRespawnListener
	{
		[Header("Behaviour")]
		[Information("Add this component to a CorgiController2D and it will walk, turn back when it hits a wall, and try and avoid holes if you ask it to. Here you can set the speed at which it walks, if it's supposed to go right initially, if it should try and avoid falling when meeting a hole in the ground or the end of a platform, and how far from the center of its transform the raycast origin should be positioned, and how long it should be. You can see that raycast in debug mode (it's the yellow one).",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]

		/// The speed of the agent
		public float Speed;
		/// The initial direction
		public bool GoesRightInitially=true;
	    /// If set to true, the agent will try and avoid falling
	    public bool AvoidFalling = false;
	    /// The offset the hole detection should take into account
	    public Vector3 HoleDetectionOffset = new Vector3(0, 0, 0);
	    public float HoleDetectionRaycastLength=1f;


	    // private stuff
	    protected CorgiController _controller;
	    protected Vector2 _direction;
	    protected Vector2 _startPosition;
	    protected Vector2 _initialDirection;
	    protected Vector3 _initialScale;
	    protected Animator _animator;

	    /// <summary>
	    /// Initialization
	    /// </summary>
	    protected virtual void Awake()
	    {
			// we get the CorgiController2D component
			_controller = GetComponent<CorgiController>();
			// initialize the start position
			_startPosition = transform.position;
			// initialize the direction
	        _direction = GoesRightInitially ? Vector2.right : -Vector2.right;
			_initialDirection = _direction;
	        _initialScale = transform.localScale;
	        if (GetComponent<Animator>()!=null)
	        {
	        	_animator=GetComponent<Animator>();
	        }
	    }
		
		/// <summary>
		/// Every frame, moves the agent and checks if it can shoot at the player.
		/// </summary>
		protected virtual void Update () 
		{

	        // moves the agent in its current direction
			_controller.SetHorizontalForce(_direction.x * Speed);
	        CheckForWalls();
	        if (AvoidFalling)
	        {
	            CheckForHoles();
	        }
			if (_animator!=null)
			{
		        UpdateAnimator();
			}
	    }

	    protected virtual void UpdateAnimator()
	    {
			if (_animator!= null)
	        { 
			    MMAnimator.UpdateAnimatorFloat(_animator,"Speed",Mathf.Abs(_controller.Speed.x));
		    }
	    }

	    /// <summary>
	    /// Checks for a wall and changes direction if it meets one
	    /// </summary>
	    protected virtual void CheckForWalls()
	    {
	        // if the agent is colliding with something, make it turn around
	        if ((_direction.x < 0 && _controller.State.IsCollidingLeft) || (_direction.x > 0 && _controller.State.IsCollidingRight))
	        {
	            ChangeDirection();
	        }
	    }

	    /// <summary>
	    /// Checks for holes 
	    /// </summary>
	    protected virtual void CheckForHoles()
	    {
	        // we send a raycast at the extremity of the character in the direction it's facing, and modified by the offset you can set in the inspector.
	        Vector2 raycastOrigin = new Vector2(transform.position.x+_direction.x*(HoleDetectionOffset.x+Mathf.Abs(GetComponent<BoxCollider2D>().bounds.size.x)/2), transform.position.y+ HoleDetectionOffset.y - (transform.localScale.y / 2));
	        RaycastHit2D raycast = MMDebug.RayCast(raycastOrigin, Vector2.down, HoleDetectionRaycastLength, 1 << LayerMask.NameToLayer("Platforms"),  Color.yellow,true);
	        // if the raycast doesn't hit anything
	        if (!raycast)
	        {
	            // we change direction
	            ChangeDirection();
	        }
	    }

	    /// <summary>
	    /// Changes the agent's direction and flips its transform
	    /// </summary>
	    protected virtual void ChangeDirection()
	    {
	        _direction = -_direction;
	        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	    }

	    /// <summary>
	    /// When the player respawns, we reinstate this agent.
	    /// </summary>
	    /// <param name="checkpoint">Checkpoint.</param>
	    /// <param name="player">Player.</param>
	    public virtual void onPlayerRespawnInThisCheckpoint (CheckPoint checkpoint, CharacterBehavior player)
		{
			_direction = _initialDirection;
			transform.localScale= _initialScale;
			transform.position=_startPosition;
			gameObject.SetActive(true);
		}
	}
}