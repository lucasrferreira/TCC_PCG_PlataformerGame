using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	public class SuperHipsterBrosCharacter : CharacterBehavior
	{
	    /// <summary>
	    /// Doubles the size of the character
	    /// </summary>
	    public virtual void Grow(float growthFactor)
	    {
	        transform.localScale *= growthFactor;
	    }

	    /// <summary>
	    /// Shrinks the size of the character
	    /// </summary>
	    public virtual void Shrink(float shrinkFactor)
	    {
	        transform.localScale = transform.localScale / shrinkFactor;
	    }

	    /// <summary>
	    /// Resets the size of the character
	    /// </summary>
	    public virtual void ResetScale(float growthFactor)
	    {
	        transform.localScale = _initialScale;
	    }

	    /// <summary>
		/// Called when the player takes damage
		/// </summary>
		/// <param name="damage">The damage applied.</param>
		/// <param name="instigator">The damage instigator.</param>
		public override void TakeDamage(int damage, GameObject instigator)
	    {
	        // When the character takes damage, we create an auto destroy hurt particle system
	        if (HurtEffect != null)
	        {
	            Instantiate(HurtEffect, transform.position, transform.rotation);
	        }
	        if (transform.localScale.y==_initialScale.y)
	        {
	            LevelManager.Instance.KillPlayer();
	        }
	        else
	        {
	            // we prevent the character from colliding with layer 12 (Projectiles) and 13 (Enemies)        
	            Physics2D.IgnoreLayerCollision(9, 12, true);
	            Physics2D.IgnoreLayerCollision(9, 13, true);
	            StartCoroutine(ResetLayerCollision(0.5f));
	            Shrink(2f); 
	            // We make the character's sprite flicker
	            if (GetComponent<Renderer>() != null)
	            {
	                Color flickerColor = new Color32(255, 20, 20, 255);
					StartCoroutine(MMImage.Flicker(GetComponent<Renderer>(), flickerColor, 0.05f,0.5f));
	            }
	        }
	    }

	    /// <summary>
		/// Kills the character, sending it in the air
		/// </summary>
		public override void Kill()
	    {
	        _controller.SetForce(new Vector2(0, 0));
	        // we make it ignore the collisions from now on
	        _controller.CollisionsOff();
	        GetComponent<Collider2D>().enabled = false;
	        // we set its dead state to true
	        BehaviorState.IsDead = true;
	        // we set its health to zero (useful for the healthbar)
	        Health = 0;
	        // we send it in the air
	        _controller.ResetParameters();
	        ResetParameters();
	        _controller.SetForce(new Vector2(0, 20));
	    }
	}
}