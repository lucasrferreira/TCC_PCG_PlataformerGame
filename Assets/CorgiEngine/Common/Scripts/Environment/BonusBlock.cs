using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a block and it'll behave like these Super Mario blocks that spawn something when hit from below
	/// </summary>
	public class BonusBlock : MonoBehaviour 
	{
		/// the time (in seconds) before the fall of the platform
		public GameObject SpawnedObject;
		public int NumberOfAllowedHits=3;
		
		public float ShakeIntensity = 2f;
	    public float SpawnSpeed = 0.2f;
	    public Vector2 SpawnDestination;

	    // private stuff
	    protected Animator _animator;
	    protected bool _hit=false;
	    protected Vector2 _newPosition;
	    protected int _numberOfHitsLeft;
		
		/// <summary>
		/// Initialization
		/// </summary>
		public virtual void Start()
		{
			// we get the animator
			_animator = GetComponent<Animator>();
			_numberOfHitsLeft=NumberOfAllowedHits;
			if (_numberOfHitsLeft>0)	
				MMAnimator.UpdateAnimatorBool(_animator,"Off",false);
			else			
				MMAnimator.UpdateAnimatorBool(_animator,"Off",true);
		}
		
		/// <summary>
		/// This is called every frame.
		/// </summary>
		protected virtual void Update()
		{		
			// we send our various states to the animator.		
			UpdateAnimator ();	
			_hit=false;
			
		}

	    protected virtual void UpdateAnimator()
		{				
			MMAnimator.UpdateAnimatorBool(_animator,"Hit",_hit);	
		}
		
		/// <summary>
		/// Triggered when a CorgiController touches the platform
		/// </summary>
		/// <param name="controller">The corgi controller that collides with the platform.</param>
		
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			CorgiController controller=collider.GetComponent<CorgiController>();
			if (controller==null)
				return;
			
			// if the block has spent all its hits, we do nothing
			if (_numberOfHitsLeft==0)
				return;
			
			if (collider.transform.position.y<transform.position.y)
			{
				// if the collider's y position is less than the block's y position, we're hitting it from below, we trigger the event
				_hit=true;
				_numberOfHitsLeft--;
				
				GameObject spawned = (GameObject)Instantiate(SpawnedObject);
				spawned.transform.position=transform.position;
				spawned.transform.rotation=Quaternion.identity;
				StartCoroutine(MMMovement.MoveFromTo(spawned,transform.position, new Vector2(transform.position.x+ SpawnDestination.x, transform.position.y+GetComponent<BoxCollider2D>().size.y+SpawnDestination.y),SpawnSpeed,0.05f));
						
			}
			
			if (_numberOfHitsLeft==0)
			{			
				MMAnimator.UpdateAnimatorBool(_animator,"Off",true);
			}
		}
		
		
		
		/// <summary>
		/// Triggered when a CorgiController exits the platform
		/// </summary>
		/// <param name="controller">The corgi controller that collides with the platform.</param>
		
		public virtual void OnTriggerExit2D(Collider2D collider)
		{
			CorgiController controller=collider.GetComponent<CorgiController>();
			if (controller==null)
				return;			
		}
	}
}