using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this component to a CorgiController2D and it will try to move towards your player on sight.
	/// </summary>
	public class AIMoveOnSight : MonoBehaviour 
	{

		[Header("Behaviour")]
		[Information("Add this component to a CorgiController2D and it will try to move towards your player on sight. Here you can specify the speed at which the agent moves, how far in front of it it can 'see' you, if your agent is facing right initially, the horizontal distance at which the agent will stop walking if too close from the player, and the offset from the agent's transform center at which the raycast should be set.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]

		/// The speed of the agent
		public float Speed=3f;
		/// The maximum distance at which the AI can see the player
		public float ViewDistance = 10f;
	    /// The character is facing right by default
	    public bool CharacterFacingRight = true;
	    /// the horizontal distance from the player at which the agent will stop moving. Between that distance and the walk distance, the agent will slow down progressively
		public float StopDistance = 1f;
		/// The offset to apply to the raycast origin point (by default the position of the object)
		public Vector2 RaycastOriginOffset = new Vector2(0,0);

	    protected float _canFireIn;
	    protected Vector2 _direction;
	    protected float _distance;
	    protected CorgiController _controller;
	    protected Animator _animator;
	    protected int _facingModifier;

		/// initialization
		protected virtual void Start () 
		{
			// we get the CorgiController2D component
			_controller = GetComponent<CorgiController>();
			// we get the character's animator
			_animator = GetComponent<Animator>();
			_direction=Vector2.right;
	        if (CharacterFacingRight)
	            _facingModifier = -1;
	        else
	            _facingModifier = 1;
		}

	    /// Every frame, check for the player and try and kill it
	    protected virtual void Update () 
		{
			bool hit=false;
	        _distance = 0;
			// we cast a ray to the left of the agent to check for a Player
			Vector2 raycastOrigin = new Vector2(transform.position.x+RaycastOriginOffset.x,transform.position.y+RaycastOriginOffset.y);	

			// we cast it to the left	
			RaycastHit2D raycast = MMDebug.RayCast(raycastOrigin,-Vector2.right,ViewDistance,1<<LayerMask.NameToLayer("Player"),Color.gray,true);
			// if we see a player
			if (raycast)
			{
				hit=true;
				// we change direction
	            _direction = -Vector2.right;
	            _distance= raycast.distance;
			}
			
			// we cast a ray to the right of the agent to check for a Player	
			raycast = MMDebug.RayCast(raycastOrigin,Vector2.right,ViewDistance,1<<LayerMask.NameToLayer("Player"),Color.gray,true);
			if (raycast)
			{
				hit=true;
	            _direction = Vector2.right;
	            _distance = raycast.distance;
			}
			

			// if the ray has hit the player, we move the agent in that direction
	        if ((hit) &&  (_distance > StopDistance))
	            _controller.SetHorizontalForce(_direction.x * Speed);
	        else
	            _controller.SetHorizontalForce(0);


	        if (_direction == Vector2.right)
	            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x) * _facingModifier, transform.localScale.y, transform.localScale.z);
	        else
	            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * _facingModifier, transform.localScale.y, transform.localScale.z);

	        // updates the animator if it's not null
	        if (_animator != null)
	            _animator.SetFloat("Speed", Mathf.Abs(_controller.Speed.x));				
		}		
	}
}