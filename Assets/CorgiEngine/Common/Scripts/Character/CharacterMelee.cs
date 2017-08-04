using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a character so it can do melee attacks
	/// </summary>
	public class CharacterMelee : MonoBehaviour 
	{
		/// a melee collider2d (box, circle...), preferably attached to the character.
		public GameObject MeleeCollider;
		/// the duration of the attack, in seconds
		public float MeleeAttackDuration=0.3f;
	    // private stuff
	    protected CharacterBehavior _characterBehavior;

	    // initialization
	    protected virtual void Start () 
		{
			// initialize the private vars
			_characterBehavior = GetComponent<CharacterBehavior>();
			
			if (MeleeCollider!=null)
			{
				MeleeCollider.SetActive(false);
			}
		}

		/// <summary>
		/// Causes the player to attack using its melee attack
		/// </summary>
		public virtual void Melee()
		{	
			// if the Melee attack action is enabled in the permissions, we continue, if not we do nothing
			if (!_characterBehavior.Permissions.MeleeAttackEnabled)
				return;
			// if the character is dead we do nothing
			if (_characterBehavior.BehaviorState.IsDead)
				return;
			// if the character is not in a position where it can move freely, we do nothing.
			if (!_characterBehavior.BehaviorState.CanMoveFreely)
				return;
			
			// if the user can melee (for example, not jetpacking)
			if (_characterBehavior.BehaviorState.CanMelee)
			{	
				// we set the meleeAttacking state to true, which will trigger the melee animation, enabling the character's MeleeArea circle collider
				_characterBehavior.BehaviorState.MeleeAttacking=true;
				// we turn the melee collider on			
				MeleeCollider.SetActive(true);
				// we start the coroutine that will end the melee state in 0.3 seconds (tweak that depending on your animation)
				StartCoroutine(MeleeEnd());			
			}
		}

	    /// <summary>
	    /// Coroutine used to stop the melee attack after a delay
	    /// </summary>
	    protected virtual IEnumerator MeleeEnd()
		{
			// after 0.3 seconds, we end the melee state
			yield return new WaitForSeconds(MeleeAttackDuration);
			// reset state
			MeleeCollider.SetActive(false);
			_characterBehavior.BehaviorState.MeleeAttacking=false;
		}
	}
}