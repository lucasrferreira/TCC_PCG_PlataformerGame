using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to an object that should give damage to the player when colliding with it
	/// </summary>
	public class GiveDamageToPlayer : MonoBehaviour 
	{
		/// The amount of health to remove from the player's health
		public int DamageToGive = 10;

	    // storage		
	    protected Vector2
			_lastPosition,
			_velocity;
		
		/// <summary>
		/// During last update, we store the position and velocity of the object
		/// </summary>
		public virtual void Update () 
		{
			_velocity = (_lastPosition - (Vector2)transform.position) /Time.deltaTime;
			_lastPosition = transform.position;
		}
		
		/// <summary>
		/// When a collision with the player is triggered, we give damage to the player and knock it back
		/// </summary>
		/// <param name="collider">what's colliding with the object.</param>
		public virtual void OnTriggerEnter2D(Collider2D collider)
		{
			var player=collider.GetComponent<CharacterBehavior>();
			if (player==null)
				return;
					
			if (collider.tag!="Player")
				return;


			var controller=player.GetComponent<CorgiController>();
			var totalVelocity=controller.Speed + _velocity;
	        
	        controller.SetForce(new Vector2(
				-1*Mathf.Sign(totalVelocity.x) * Mathf.Clamp(Mathf.Abs(totalVelocity.x) * 5,10,20),
				-1*Mathf.Sign(totalVelocity.y) * Mathf.Clamp(Mathf.Abs(totalVelocity.y) * 2,0,15)			
				));
	        player.TakeDamage(DamageToGive, gameObject);
	    }
	}
}