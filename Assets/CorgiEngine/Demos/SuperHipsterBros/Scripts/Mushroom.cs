using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Add this class to an object and it'll double the size of a character behavior if it touches one
	/// </summary>
	public class Mushroom : MonoBehaviour, IPlayerRespawnListener
	{
		/// The effect to instantiate when the mushroom is hit
		public GameObject Effect;

		/// <summary>
		/// Triggered when something collides with the coin
		/// </summary>
		/// <param name="collider">Other.</param>
		public virtual void OnTriggerEnter2D (Collider2D collider) 
		{
			// if what's colliding with the coin ain't a characterBehavior, we do nothing and exit
			if (collider.GetComponent<SuperHipsterBrosCharacter>() == null)
				return;
	        
			// adds an instance of the effect at the coin's position
			Instantiate(Effect,transform.position,transform.rotation);

	        // double the size of the character behavior
	        collider.GetComponent<SuperHipsterBrosCharacter>().Grow(2f);

	        // we desactivate the gameobject
	        gameObject.SetActive(false);
		}
		/// <summary>
		/// When the player respawns, we reinstate the object
		/// </summary>
		/// <param name="checkpoint">Checkpoint.</param>
		/// <param name="player">Player.</param>
		public virtual void onPlayerRespawnInThisCheckpoint(CheckPoint checkpoint, CharacterBehavior player)
		{
			gameObject.SetActive(true);
		}
	}
}