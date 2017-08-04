using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Adds this class to an object so it can have health (and lose it)
	/// </summary>
	public class Health : MonoBehaviour, CanTakeDamage
	{
	    /// the initial amount of health of the object
	    public int InitialHealth;
		/// the current amount of health of the object
		public int CurrentHealth { get; protected set; }
		/// the points the player gets when the object's health reaches zero
		public int PointsWhenDestroyed;
		/// the effect to instantiate when the object takes damage
		public GameObject HurtEffect;
		/// the effect to instantiate when the object gets destroyed
		public GameObject DestroyEffect;

		protected Color _initialColor;

	    protected void Start()
	    {
			CurrentHealth = InitialHealth;
			if (GetComponent<Renderer>()!=null)
			{
				if (GetComponent<Renderer>().material.HasProperty("_Color"))
				{
					_initialColor = GetComponent<Renderer>().material.color;
				}
			}
	    }

	    protected void OnEnable()
	    {
	        CurrentHealth = InitialHealth;
	    }

		/// <summary>
		/// What happens when the object takes damage
		/// </summary>
		/// <param name="damage">Damage.</param>
		/// <param name="instigator">Instigator.</param>
		public virtual void TakeDamage(int damage,GameObject instigator)
		{	
			// when the object takes damage, we instantiate its hurt effect
			Instantiate(HurtEffect,instigator.transform.position,transform.rotation);
			// and remove the specified amount of health	
			CurrentHealth -= damage;
			// if the object doesn't have health anymore, we destroy it
			if (CurrentHealth<=0)
			{
				DestroyObject();
	            return;
			}

	        // We make the character's sprite flicker
	        Color flickerColor = new Color32(255, 20, 20, 255);
			StartCoroutine(Flicker(_initialColor, flickerColor, 0.02f));	
		}

	    /// <summary>
	    /// Coroutine used to make the character's sprite flicker (when hurt for example).
	    /// </summary>
	    protected virtual IEnumerator Flicker(Color initialColor, Color flickerColor, float flickerSpeed)
	    {
			if (GetComponent<Renderer>()!=null)
			{
			  	if (!GetComponent<Renderer>().material.HasProperty("_Color"))
			  	{
			  		yield return null;
			  	}
		        for (var n = 0; n < 10; n++)
		        {
		            GetComponent<Renderer>().material.color = initialColor;
		            yield return new WaitForSeconds(flickerSpeed);
		            GetComponent<Renderer>().material.color = flickerColor;
		            yield return new WaitForSeconds(flickerSpeed);
		        }
		        GetComponent<Renderer>().material.color = initialColor;
			}
	        // makes the character colliding again with layer 12 (Projectiles) and 13 (Enemies)
	        Physics2D.IgnoreLayerCollision(9, 12, false);
	        Physics2D.IgnoreLayerCollision(9, 13, false);
	    }


	    /// <summary>
	    /// Destroys the object
	    /// </summary>
	    protected virtual void DestroyObject()
		{
			// instantiates the destroy effect
			if (DestroyEffect!=null)
			{
				var instantiatedEffect=(GameObject)Instantiate(DestroyEffect,transform.position,transform.rotation);
	            instantiatedEffect.transform.localScale = transform.localScale;
			}
			// Adds points if needed.
			if(PointsWhenDestroyed != 0)
			{
				GameManager.Instance.AddPoints(PointsWhenDestroyed);
			}
			// destroys the object
			gameObject.SetActive(false);
		}
	}
}