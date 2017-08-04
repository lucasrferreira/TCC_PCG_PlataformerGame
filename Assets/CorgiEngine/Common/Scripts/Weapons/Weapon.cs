using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{
	/// <summary>
	/// Weapon parameters
	/// </summary>
	public class Weapon : MonoBehaviour 
	{
		/// the projectile type the weapon shoots
		public Projectile Projectile;
		/// the firing frequency
		public float FireRate;
		/// center of rotation of the gun
		public GameObject GunRotationCenter;
		/// the particle system to instantiate every time the weapon shoots
		public ParticleSystem GunFlames;
		/// the shells the weapon emits
		public ParticleSystem GunShells;	
		/// the initial projectile firing position
		public Transform ProjectileFireLocation;
		// the sound to play when the player shoots
		public AudioClip GunShootFx;

		protected virtual void Start()
		{
			SetGunFlamesEmission (false);
			SetGunShellsEmission (false);
		}
		
		public virtual void SetGunFlamesEmission(bool state)
		{
			ParticleSystem.EmissionModule emissionModule = GunFlames.emission;
			emissionModule.enabled=state;
		}
		
		public virtual void SetGunShellsEmission(bool state)
		{
			ParticleSystem.EmissionModule emissionModule = GunShells.emission;
			emissionModule.enabled=state;
		}
	}
}