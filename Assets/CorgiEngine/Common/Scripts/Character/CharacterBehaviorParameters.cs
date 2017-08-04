using System;
using UnityEngine;
using System.Collections;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// The various parameters related to the CharacterBehavior class.
	/// </summary>

	[Serializable]
	public class CharacterBehaviorParameters 
	{
		[Header("Jumps")]
		/// defines how high the character can jump
		public float JumpHeight = 3.025f;
		/// the minimum time in the air allowed when jumping - this is used for pressure controlled jumps
		public float JumpMinimumAirTime = 0.1f;
		/// the maximum number of jumps allowed (0 : no jump, 1 : normal jump, 2 : double jump, etc...)
		public int NumberOfJumps=3;
		public enum JumpBehavior
		{
			CanJumpOnGround,
			CanJumpAnywhere,
			CantJump,
			CanJumpAnywhereAnyNumberOfTimes
		}
		/// basic rules for jumps : where can the player jump ?
		public JumpBehavior JumpRestrictions;
		/// if true, the jump duration/height will be proportional to the duration of the button's press
		public bool JumpIsProportionalToThePressTime=true;

	    [Space(10)]	
		[Header("Speed")]
		/// basic movement speed
		public float MovementSpeed = 8f;
		/// the speed of the character when it's crouching
		public float CrouchSpeed = 4f;
		/// the speed of the character when it's walking
		public float WalkSpeed = 8f;
		/// the speed of the character when it's running
		public float RunSpeed = 16f;
		/// the speed of the character when climbing a ladder
		public float LadderSpeed = 2f;
		
		[Space(10)]	
		[Header("Dash")]
		/// the duration of dash (in seconds)
		public float DashDuration = 0.15f;
		/// the force of the dash
		public float DashForce = 5f;	
		/// the duration of the cooldown between 2 dashes (in seconds)
		public float DashCooldown = 2f;	
		
		[Space(10)]	
		[Header("Walljump")]
		/// the force of a walljump
		public Vector2 WallJumpForce = new Vector2(3,-3);
		/// the slow factor when wall clinging
		public float WallClingingSlowFactor=0.6f;

		[Space(10)]
		[Header("Dangling")]
		/// the origin of the raycast used to detect pits. This is relative to the transform.position of our character
		public Vector3 DanglingRaycastOrigin=new Vector3(0.7f,-0.25f,0f);
		/// the length of the raycast used to detect pits
		public float DanglingRaycastLength=2f;

		[Space(10)]	
		[Header("Crouching")]
		/// the maximum health of the character
		public Vector2 CrouchedBoxColliderSize = new Vector2(1,1);
			
		[Space(10)]	
		[Header("Health")]
		/// the maximum health of the character
		public int MaxHealth = 100;	
		/// the force applied when the character dies
		public Vector2 DeathForce = new Vector2(0,10);

		[Header("Control")]
		/// If set to true, acceleration / deceleration will take place when moving / stopping
		public bool SmoothMovement=true;
		public float VerticalThreshold=0.4f;
		
		[Header("Animation Type")]
		/// Set this to false if you want to implement your own animation system
		public bool UseDefaultMecanim = true;
	}
}