using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// This class handles what happens when the player reaches the level bounds.
	/// </summary>
	public class PlayerBounds : MonoBehaviour 
	{
		public enum BoundsBehavior 
		{
			Nothing,
			Constrain,
			Kill
		}
		/// what to do to the player when it reaches the top level bound
		public BoundsBehavior Above;
		/// what to do to the player when it reaches the bottom level bound
		public BoundsBehavior Below;
		/// what to do to the player when it reaches the left level bound
		public BoundsBehavior Left;
		/// what to do to the player when it reaches the right level bound
		public BoundsBehavior Right;

	    protected Bounds _bounds;
	    protected CharacterBehavior _player;
	    protected BoxCollider2D _boxCollider;
		
		/// <summary>
		/// Initialization
		/// </summary>
		public virtual void Start () 
		{
			_player=GetComponent<CharacterBehavior>();
			_boxCollider=GetComponent<BoxCollider2D>();
			if (LevelManager.Instance != null)
			{
				_bounds=LevelManager.Instance.LevelBounds;
			}
		}
		
		/// <summary>
		/// Every frame, we check if the player is colliding with a level bound
		/// </summary>
		public virtual void Update () 
		{
			// if the player is dead, we do nothing
			if (_player.BehaviorState.IsDead)
				return;			
			
			// we calculate the player's boxcollider size	
			var colliderSize=new Vector2(
				_boxCollider.size.x * Mathf.Abs (transform.localScale.x),
				_boxCollider.size.y * Mathf.Abs (transform.localScale.y))/2;

			if (_bounds.size != Vector3.zero)
			{		
				// when the player reaches a bound, we apply the specified bound behavior
				if (Above != BoundsBehavior.Nothing && transform.position.y + colliderSize.y > _bounds.max.y)
					ApplyBoundsBehavior(Above, new Vector2(transform.position.x,_bounds.max.y - colliderSize.y));
				
				if (Below != BoundsBehavior.Nothing && transform.position.y - colliderSize.y < _bounds.min.y)
					ApplyBoundsBehavior(Below, new Vector2(transform.position.x, _bounds.min.y + colliderSize.y));
				
				if (Right != BoundsBehavior.Nothing && transform.position.x + colliderSize.x > _bounds.max.x)
					ApplyBoundsBehavior(Right, new Vector2(_bounds.max.x - colliderSize.x,transform.position.y));		
				
				if (Left != BoundsBehavior.Nothing && transform.position.x - colliderSize.x < _bounds.min.x)
					ApplyBoundsBehavior(Left, new Vector2(_bounds.min.x + colliderSize.x,transform.position.y));
			}
			
		}

	    /// <summary>
	    /// Applies the specified bound behavior to the player
	    /// </summary>
	    /// <param name="behavior">Behavior.</param>
	    /// <param name="constrainedPosition">Constrained position.</param>
	    protected virtual void ApplyBoundsBehavior(BoundsBehavior behavior, Vector2 constrainedPosition)
		{
			if (behavior== BoundsBehavior.Kill)
			{
				LevelManager.Instance.KillPlayer ();
			}	
			transform.position = constrainedPosition;	
		}
	}
}