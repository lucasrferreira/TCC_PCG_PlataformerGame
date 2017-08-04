using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Add this class to a character so it can shoot projectiles
	/// </summary>
	public class CharacterShoot : MonoBehaviour 
	{
		/// the initial weapon owned by the character
		public Weapon InitialWeapon;
		
		/// the position the weapon will be attached to
		public Transform WeaponAttachment;
		/// true if the character is allowed to shoot in more than 1 direction
		public bool EightDirectionShooting=true;
		/// true if the character can only shoot in the "strict" 8 directions (top, top right, right, bottom right, etc...)
		public bool StrictEightDirectionShooting=true;


	    protected Weapon _weapon;
	    protected float _fireTimer;

	    protected float _horizontalMove;
	    protected float _verticalMove;

	    protected CharacterBehavior _characterBehavior;
	    protected CorgiController _controller;

	    // Initialization
	    protected virtual void Start () 
		{
			// initialize the private vars
			_characterBehavior = GetComponent<CharacterBehavior>();
			_controller = GetComponent<CorgiController>();
			
			// filler if the WeaponAttachment has not been set
			if (WeaponAttachment==null)
				WeaponAttachment=transform;

			// we set the initial weapon
			ChangeWeapon(InitialWeapon);			
		}

		
		/// <summary>
		/// Makes the character shoot once.
		/// </summary>
		public virtual void ShootOnce()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing. If the player is dead we do nothing.
			if (!_characterBehavior.Permissions.ShootEnabled || _characterBehavior.BehaviorState.IsDead)
				return;
			// if the character can't fire, we do nothing		
			if (!_characterBehavior.BehaviorState.CanShoot)
			{			
				// we just reset the firing direction (this happens when the character gets on a ladder for example.
				_characterBehavior.BehaviorState.FiringDirection=3;
				return;		
			}
			
			// if the character is not in a position where it can move freely, we do nothing.
			if (!_characterBehavior.BehaviorState.CanMoveFreely)
				return;
			
			// we fire a projectile and reset the fire timer
			FireProjectile();	
			_fireTimer = 0;	
		}
		
		/// <summary>
		/// Causes the character to start shooting
		/// </summary>
		public virtual void ShootStart()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing.  If the player is dead we do nothing.
			if (!_characterBehavior.Permissions.ShootEnabled || _characterBehavior.BehaviorState.IsDead)
				return;
			// if the character can't fire, we do nothing		
			if (!_characterBehavior.BehaviorState.CanShoot)
			{			
				// we just reset the firing direction (this happens when the character gets on a ladder for example.
				_characterBehavior.BehaviorState.FiringDirection=3;
				return;		
			}
			
			// if the character is not in a position where it can move freely, we do nothing.
			if (!_characterBehavior.BehaviorState.CanMoveFreely)
				return;			
			
			// firing state reset								
			_characterBehavior.BehaviorState.FiringStop = false;			
			_characterBehavior.BehaviorState.Firing = true;

			_weapon.SetGunFlamesEmission(true);
			_weapon.SetGunShellsEmission (true);
			_fireTimer += Time.deltaTime;
			if(_fireTimer > _weapon.FireRate)
			{
				FireProjectile();
				_fireTimer = 0; // reset timer for fire rate
			}				
		}
		
		/// <summary>
		/// Causes the character to stop shooting
		/// </summary>
		public virtual void ShootStop()
		{
			// if the Shoot action is enabled in the permissions, we continue, if not we do nothing
			if (!_characterBehavior.Permissions.ShootEnabled)
				return;
			// if the character can't fire, we do nothing		
			if (!_characterBehavior.BehaviorState.CanShoot)
			{			
				// we just reset the firing direction (this happens when the character gets on a ladder for example.
				_characterBehavior.BehaviorState.FiringDirection=3;
				return;		
			}
			_characterBehavior.BehaviorState.FiringStop = true;		
			_characterBehavior.BehaviorState.Firing = false;
			// reset the firing direction
			_characterBehavior.BehaviorState.FiringDirection=3;
			ParticleSystem.EmissionModule gunEmissionModule = _weapon.GunFlames.emission;
			gunEmissionModule.enabled=false;
			ParticleSystem.EmissionModule shellsEmissionModule = _weapon.GunShells.emission;
			shellsEmissionModule.enabled=false;
		}
		
		/// <summary>
		/// Changes the character's current weapon to the one passed as a parameter
		/// </summary>
		/// <param name="newWeapon">The new weapon.</param>
		public virtual void ChangeWeapon(Weapon newWeapon)
		{
			// if the character already has a weapon, we make it stop
			if(_weapon!=null)
			{
				ShootStop();
			}
			// weapon instanciation
			_weapon=(Weapon)Instantiate(newWeapon,WeaponAttachment.transform.position,WeaponAttachment.transform.rotation);	
			_weapon.transform.parent = transform;
			// we turn off the gun's emitters.
			_weapon.SetGunFlamesEmission (false);
			_weapon.SetGunShellsEmission (false);
		}

	    /// <summary>
	    /// Fires one of the current weapon's projectiles
	    /// </summary>
	    protected virtual void FireProjectile () 
		{
			// we get the direction the player is inputing.		
			float HorizontalShoot = _horizontalMove;
			float VerticalShoot = _verticalMove;
			
			// if the projectile fire location has not been set, we do nothing and exit
			if (_weapon.ProjectileFireLocation==null)
				return;
			
			// if we don't want 8 direction shooting at all, we set these two floats to zero.
			if (!EightDirectionShooting)
			{
				HorizontalShoot=0;
				VerticalShoot=0;
			}
			
			// if we want a strict 8 direction shot, we round the direction values.		
			if (StrictEightDirectionShooting)
			{
				HorizontalShoot = Mathf.Round(HorizontalShoot);
				VerticalShoot = Mathf.Round(VerticalShoot);
			}
			
			// we calculate the angle based on the buttons the player is pressing to determine the direction of the shoot.									
			float angle = Mathf.Atan2(HorizontalShoot, VerticalShoot) * Mathf.Rad2Deg;
			
			Vector2 direction = Vector2.up;
			
			// if the user is not pressing any direction button, we set the shoot direction based on the direction it's facing.
			if (HorizontalShoot>-0.1f && HorizontalShoot<0.1f && VerticalShoot>-0.1f && VerticalShoot<0.1f )
			{
				bool _isFacingRight = transform.localScale.x > 0;
				angle=_isFacingRight?90f : -90f;
				
			}
			
			// We set the animation depending on where the character is shooting
			
			// if shooting up
			if ( Mathf.Abs(HorizontalShoot)<0.1f && VerticalShoot>0.1f )
				_characterBehavior.BehaviorState.FiringDirection=1;
			// if shooting diagonal up
			if ( Mathf.Abs(HorizontalShoot)>0.1f && VerticalShoot>0.1f )
				_characterBehavior.BehaviorState.FiringDirection=2;
			// if shooting diagonal down
			if ( Mathf.Abs(HorizontalShoot)>0.1f && VerticalShoot<-0.1f )
				_characterBehavior.BehaviorState.FiringDirection=4;
			// if shooting down
			if ( Mathf.Abs(HorizontalShoot)<0.1f && VerticalShoot<-0.1f )
				_characterBehavior.BehaviorState.FiringDirection=5;
			if (Mathf.Abs(VerticalShoot)<0.1f)
				_characterBehavior.BehaviorState.FiringDirection=3;
			
			// we move the ProjectileFireLocation according to the angle
			bool _facingRight = transform.localScale.x > 0;
			float horizontalDirection=_facingRight?1f:-1f;
									
			// we apply the angle rotation to the direction.
			direction = Quaternion.Euler(0,0,-angle) * direction;
			
			// we rotate the gun towards the direction
			_weapon.GunRotationCenter.transform.rotation=Quaternion.Euler (new Vector3(0, 0, -angle*horizontalDirection+90f));
			
			// we instantiate the projectile at the projectileFireLocation's position.				
			var projectile = (Projectile)Instantiate(_weapon.Projectile,_weapon.ProjectileFireLocation.position,_weapon.ProjectileFireLocation.rotation);
			projectile.Initialize(gameObject,direction,_controller.Speed);	
			
			// we play the shooting sound
			FXPlayGunSound();
		}

		/// <summary>
		/// Sets the horizontal move value.
		/// </summary>
		/// <param name="value">Horizontal move value, between -1 and 1 - positive : will move to the right, negative : will move left </param>
		public virtual void SetHorizontalMove(float value)
		{
			_horizontalMove=value;
		}
		
		/// <summary>
		/// Sets the vertical move value.
		/// </summary>
		/// <param name="value">Vertical move value, between -1 and 1
		public virtual void SetVerticalMove(float value)
		{
			_verticalMove=value;
		}
		
		public virtual void Flip()
		{
			if (_weapon.GunShells != null)
				_weapon.GunShells.transform.eulerAngles = new Vector3 (_weapon.GunShells.transform.eulerAngles.x, _weapon.GunShells.transform.eulerAngles.y + 180, _weapon.GunShells.transform.eulerAngles.z);
			if (_weapon.GunFlames != null)
				_weapon.GunFlames.transform.eulerAngles = new Vector3 (_weapon.GunFlames.transform.eulerAngles.x, _weapon.GunFlames.transform.eulerAngles.y + 180, _weapon.GunFlames.transform.eulerAngles.z);
		}

		protected virtual void FXPlayGunSound()
		{
			if (_weapon.GunShootFx!=null)
			{
				SoundManager.Instance.PlaySound(_weapon.GunShootFx,transform.position);		
			}
		}
	}
}