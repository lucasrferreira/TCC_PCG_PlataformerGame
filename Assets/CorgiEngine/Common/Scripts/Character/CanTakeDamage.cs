using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Public interface for objects that can take damage
	/// </summary>

	public interface CanTakeDamage
	{

		void TakeDamage(int damage,GameObject instigator);
	}
}