using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{
	public class Grip : MonoBehaviour 
	{

	    protected virtual void OnTriggerEnter2D(Collider2D collider)
		{
			// we check that the object colliding with the grip is actually a corgi controller and a character
			CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
			if (character==null)
				return;		
			CorgiController controller = collider.GetComponent<CorgiController>();
			if (controller==null)
				return;					
			
			character.BehaviorState.Gripping=true;
		}

		protected virtual void OnTriggerStay2D(Collider2D collider)
		{
			// we check that the object colliding with the grip is actually a corgi controller and a character
			CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
			if (character==null)
				return;		
			CorgiController controller = collider.GetComponent<CorgiController>();
			if (controller==null)
				return;	

			if (character.BehaviorState.Gripping)
			{
				controller.transform.position=transform.position;
			}
		}

	    protected virtual void OnTriggerExit2D(Collider2D collider)
		{
			// we check that the object colliding with the grip is actually a corgi controller and a character
			CharacterBehavior character = collider.GetComponent<CharacterBehavior>();
			if (character==null)
				return;		
			CorgiController controller = collider.GetComponent<CorgiController>();
			if (controller==null)
				return;					
			
			character.BehaviorState.Gripping=false;
		}
	}
}