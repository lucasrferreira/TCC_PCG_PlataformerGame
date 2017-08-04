using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Manages the health bar
	/// </summary>
	public class HealthBar : MonoBehaviour 
	{
		/// the healthbar's foreground sprite
		public Transform ForegroundSprite;
		/// the color when at max health
		public Color MaxHealthColor = new Color(255/255f, 63/255f, 63/255f);
		/// the color for min health
		public Color MinHealthColor = new Color(64/255f, 137/255f, 255/255f);

	    protected CharacterBehavior _character;

	    /// <summary>
	    /// Initialization, gets the player
	    /// </summary>
	    protected virtual void Start()
		{
			_character = GameManager.Instance.Player;
		}

	    /// <summary>
	    /// Every frame, sets the foreground sprite's width to match the character's health.
	    /// </summary>
	    protected virtual void Update()
		{
			if (_character==null)
				return;
			var healthPercent = _character.Health / (float) _character.BehaviorParameters.MaxHealth;
			ForegroundSprite.localScale = new Vector3(healthPercent,1,1);
			
		}		
	}
}