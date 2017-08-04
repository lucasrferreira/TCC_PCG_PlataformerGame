﻿using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this script to a platform and it'll fall down when walked upon by a playable character
	/// </summary>
	public class FallingPlatform : MonoBehaviour 
	{
		/// the time (in seconds) before the fall of the platform
		public float TimeBeforeFall = 2f;
		/// 
		public float FallSpeed = 2f;
		
		public float ShakeIntensity = 2f;

	    // private stuff
	    protected Animator _animator;
	    protected bool _shaking=false;
	    protected Vector2 _newPosition;
	    protected Bounds _bounds;

	    /// <summary>
	    /// Initialization
	    /// </summary>
	    protected virtual void Start()
		{
			// we get the animator
			_animator = GetComponent<Animator>();
			_bounds=LevelManager.Instance.LevelBounds;
		}
		
		/// <summary>
		/// This is called every frame.
		/// </summary>
		protected virtual void FixedUpdate()
		{		
			// we send our various states to the animator.		
			UpdateAnimator ();		
			
			if (TimeBeforeFall<0)
			{
				_newPosition = new Vector2(0,
				                           -FallSpeed*Time.deltaTime);
				                           
				transform.Translate(_newPosition,Space.World);
				
				if (transform.position.y < _bounds.min.y)
				{
					Destroy(gameObject);
				}
			}
		}

	    protected virtual void UpdateAnimator()
		{				
			if (_animator!=null)
			{
				MMAnimator.UpdateAnimatorBool(_animator,"Shaking",_shaking);	
			}
		}
		
		/// <summary>
		/// Triggered when a CorgiController touches the platform
		/// </summary>
		/// <param name="controller">The corgi controller that collides with the platform.</param>
		
		public virtual void OnTriggerStay2D(Collider2D collider)
		{
			CorgiController controller=collider.GetComponent<CorgiController>();
			if (controller==null)
				return;
			
			if (TimeBeforeFall>0)
			{
				if (collider.transform.position.y>transform.position.y)
				{
					TimeBeforeFall -= Time.deltaTime;
					_shaking=true;
				}
			}	
			else
			{
				_shaking=false;
			}
		}
	    /// <summary>
	    /// Triggered when a CorgiController exits the platform
	    /// </summary>
	    /// <param name="controller">The corgi controller that collides with the platform.</param>

	    protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			CorgiController controller=collider.GetComponent<CorgiController>();
			if (controller==null)
				return;
			
			_shaking=false;
		}
	}
}