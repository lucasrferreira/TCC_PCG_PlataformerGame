using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	[RequireComponent(typeof(BoxCollider2D))]
	[RequireComponent(typeof(Health))]

	/// <summary>
	/// Add this class to an enemy (or whatever you want), to be able to stomp on it
	/// </summary>
	public class Stompable : MonoBehaviour 
	{
		[Information("Add this component to an object (an enemy for example) you want the player to be able to stomp by jumping on it. You can define how many rays will be used to detect the collision (you can see them in debug mode), try and have a space between each ray smaller than your player's width), the force that will be applied to the stomper on impact, the mask used to detect the player, and how much damage each stomp should cause.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
		/// The number of vertical rays cast to detect stomping
		public int NumberOfRays=5;
		/// The force the hit will apply to the stomper
	    public float KnockbackForce = 15f;
		/// The layer the player is on
	    public LayerMask PlayerMask;
		/// The amount of damage each stomp causes to the stomped enemy
	    public int DamagePerStomp;

		// private stuff
	    protected BoxCollider2D _boxCollider;
	    protected Health _health;

	    /// <summary>
	    /// On start, we get the various components
	    /// </summary>
	    protected virtual void Start ()
	    {
	        _boxCollider = (BoxCollider2D)GetComponent<BoxCollider2D>();
	        _health = (Health)GetComponent<Health>();	
		}

	    /// <summary>
	    /// Each update, we cast rays above
	    /// </summary>
	    protected virtual void Update () 
		{
	        CastRaysAbove();
		}

		/// <summary>
		/// Casts the rays above to detect stomping
		/// </summary>
	    protected virtual void CastRaysAbove()
	    {
	        float rayLength = 0.5f;

	        bool hitConnected = false;
	        int hitConnectedIndex = 0;

	        Vector2 verticalRayCastStart = new Vector2(_boxCollider.bounds.min.x,
	                                                    _boxCollider.bounds.max.y);
	        Vector2 verticalRayCastEnd = new Vector2(_boxCollider.bounds.max.x,
	                                                   _boxCollider.bounds.max.y);

	        RaycastHit2D[] hitsStorage = new RaycastHit2D[NumberOfRays];

	        for (int i = 0; i < NumberOfRays; i++)
	        {
	            Vector2 rayOriginPoint = Vector2.Lerp(verticalRayCastStart, verticalRayCastEnd, (float)i / (float)(NumberOfRays - 1));
				hitsStorage[i] = MMDebug.RayCast(rayOriginPoint, Vector2.up, rayLength, PlayerMask, Color.black, true);

	            if (hitsStorage[i])
	            {
	                hitConnected = true;
	                hitConnectedIndex = i;
	                break;
	            }
	        }

	        if (hitConnected)
	        {
	        	// if the player is not hitting this enemy from above, we do nothing
				if (_boxCollider.bounds.max.y > hitsStorage[hitConnectedIndex].collider.bounds.min.y)
				{
					return;
				}
	            CorgiController corgiController = hitsStorage[hitConnectedIndex].collider.GetComponent<CorgiController>();
				if (corgiController!=null)
	            {
	            	// if the player is not going down, we do nothing and exit
					if (corgiController.Speed.y >= 0)
					{
						return;
					}
					corgiController.AddVerticalForce(KnockbackForce);
	                _health.TakeDamage(DamagePerStomp, gameObject);
	            }
	        }
	    }
	}
}