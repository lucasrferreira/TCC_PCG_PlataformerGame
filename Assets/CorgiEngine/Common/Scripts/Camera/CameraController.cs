using UnityEngine;
using System.Collections;
using UnityStandardAssets.CinematicEffects;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	[RequireComponent(typeof(Camera))]
	/// <summary>
	/// The Corgi Engine's Camera Controller. Handles camera movement, shakes, player follow.
	/// </summary>
	public class CameraController : MonoBehaviour 
	{
		/// True if the camera should follow the player
		public bool FollowsPlayer{get;set;}

		[Space(10)]	
		[Header("Distances")]
		[Information("The Horizontal Look Distance defines how far ahead from the player the camera should be. The camera offset is an offset applied at all times. The LookAheadTrigger defines the minimal distance you need to move to trigger camera movement. Finally you can define how far up or below the camera will move when looking up or down.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
		/// How far ahead from the Player the camera is supposed to be		
		public float HorizontalLookDistance = 3;
		/// Vertical Camera Offset	
		public Vector3 CameraOffset ;
		/// Minimal distance that triggers look ahead
		public float LookAheadTrigger = 0.1f;
		/// How high (or low) from the Player the camera should move when looking up/down
		public float ManualUpDownLookDistance = 3;
		
		
		[Space(10)]	
		[Header("Movement Speed")]
		[Information("Here you can define how fast the camera goes back to the player, and how fast it moves generally.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
		/// How fast the camera goes back to the Player
		public float ResetSpeed = 0.5f;
		/// How fast the camera moves
		public float CameraSpeed = 0.3f;
		
		[Space(10)]	
		[Header("Camera Zoom")]
		[Information("Determine here the min and max zoom, and the zoom speed. By default the engine will zoom out when your character is going at full speed, and zoom in when you slow down (or stop).",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
		[Range (1, 20)]
		public float MinimumZoom=5f;
		[Range (1, 20)]
		public float MaximumZoom=10f;	
		public float ZoomSpeed=0.4f;
		
		[Space(10)]	
		[Header("Camera Effects")]
		[Information("If EnableEffectsOnMobile is set to false, all Cinematic Effects on the camera will be removed at start on mobile targets.",MoreMountains.Tools.InformationAttribute.InformationType.Info,false)]
		/// If set to false, all Cinematic Effects on the camera will be removed at start on mobile targets
		public bool EnableEffectsOnMobile=false;
		
		// Private variables
		
		protected Transform _target;
	    protected CorgiController _targetController;
		protected Bounds _levelBounds;

	    protected float _xMin;
	    protected float _xMax;
	    protected float _yMin;
	    protected float _yMax;	 
		
		protected float _offsetZ;
		protected Vector3 _lastTargetPosition;
	    protected Vector3 _currentVelocity;
	    protected Vector3 _lookAheadPos;

	    protected float _shakeIntensity;
	    protected float _shakeDecay;
	    protected float _shakeDuration;
		
		protected float _currentZoom;	
		protected Camera _camera;

	    protected Vector3 _lookDirectionModifier = new Vector3(0,0,0);
		
		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Start ()
		{		
			// we get the camera component
			_camera=GetComponent<Camera>();

			#if UNITY_ANDROID || UNITY_IPHONE
				RemoveEffectsOnMobile();
			#endif

			// We make the camera follow the player
			FollowsPlayer=true;
			_currentZoom=MinimumZoom;
			
			// player and level bounds initialization
			_target = GameManager.Instance.Player.transform;
			if (_target.GetComponent<CorgiController>()==null)
				return;
			_targetController=_target.GetComponent<CorgiController>();

			if (LevelManager.Instance!=null)
			{
				_levelBounds = LevelManager.Instance.LevelBounds;
			}
			
			// we store the target's last position
			_lastTargetPosition = _target.position;
			_offsetZ = (transform.position - _target.position).z;
			transform.parent = null;
			
			//_lookDirectionModifier=new Vector3(0,0,0);
			
			Zoom();
		}


	    /// <summary>
	    /// Every frame, we move the camera if needed
	    /// </summary>
	    protected virtual void Update () 
		{
			// if the camera is not supposed to follow the player, we do nothing
			if (!FollowsPlayer)
				return;
				
			Zoom();
				
			// if the player has moved since last update
			float xMoveDelta = (_target.position - _lastTargetPosition).x;
			
			bool updateLookAheadTarget = Mathf.Abs(xMoveDelta) > LookAheadTrigger;
			
			if (updateLookAheadTarget) 
			{
				_lookAheadPos = HorizontalLookDistance * Vector3.right * Mathf.Sign(xMoveDelta);
			} 
			else 
			{
				_lookAheadPos = Vector3.MoveTowards(_lookAheadPos, Vector3.zero, Time.deltaTime * ResetSpeed);	
			}
			
			Vector3 aheadTargetPos = _target.position + _lookAheadPos + Vector3.forward * _offsetZ + _lookDirectionModifier + CameraOffset;
					
			Vector3 newCameraPosition = Vector3.SmoothDamp(transform.position, aheadTargetPos, ref _currentVelocity, CameraSpeed);
					
			Vector3 shakeFactorPosition = new Vector3(0,0,0);
			
			// If shakeDuration is still running.
			if (_shakeDuration>0)
			{
				shakeFactorPosition= Random.insideUnitSphere * _shakeIntensity * _shakeDuration;
				_shakeDuration-=_shakeDecay*Time.deltaTime ;
			}		
			newCameraPosition = newCameraPosition+shakeFactorPosition;		


			if (_camera.orthographic==true)
			{
				float posX,posY,posZ=0f;
				// Clamp to level boundaries
				if (_levelBounds.size != Vector3.zero)
				{
					posX = Mathf.Clamp(newCameraPosition.x, _xMin, _xMax);
					posY = Mathf.Clamp(newCameraPosition.y, _yMin, _yMax);
				}
				else
				{
					posX = newCameraPosition.x;
					posY = newCameraPosition.y;
				}
				posZ = newCameraPosition.z;
				// We move the actual transform
				transform.position=new Vector3(posX, posY, posZ);
			}
			else
			{
				transform.position=newCameraPosition;
			}		

			
			_lastTargetPosition = _target.position;		
		}
		
		/// <summary>
		/// Handles the zoom of the camera based on the main character's speed
		/// </summary>
		protected virtual void Zoom()
		{
		
			float characterSpeed=Mathf.Abs(_targetController.Speed.x);
			float currentVelocity=0f;
			
			_currentZoom=Mathf.SmoothDamp(_currentZoom,(characterSpeed/10)*(MaximumZoom-MinimumZoom)+MinimumZoom,ref currentVelocity,ZoomSpeed);
				
			_camera.orthographicSize=_currentZoom;
			GetLevelBounds();
		}

		protected virtual void RemoveEffectsOnMobile()
		{
			if (!EnableEffectsOnMobile)
			{
				if (GetComponent<TonemappingColorGrading>()!=null)
				{
					Destroy(GetComponent<TonemappingColorGrading>());
				}					
				if (GetComponent<Bloom>()!=null)
				{
					Destroy(GetComponent<Bloom>());
				}					
				if (GetComponent<AmbientOcclusion>()!=null)
				{
					Destroy(GetComponent<AmbientOcclusion>());
				}					
				if (GetComponent<DepthOfField>()!=null)
				{
					Destroy(GetComponent<DepthOfField>());
				}					
				if (GetComponent<LensAberrations>()!=null)
				{
					Destroy(GetComponent<LensAberrations>());
				}					
			}
		}

	    /// <summary>
	    /// Gets the levelbounds coordinates to lock the camera into the level
	    /// </summary>
	    protected virtual void GetLevelBounds()
		{
			if (_levelBounds.size==Vector3.zero)
			{
				return;
			}

			// camera size calculation (orthographicSize is half the height of what the camera sees.
			float cameraHeight = Camera.main.orthographicSize * 2f;		
			float cameraWidth = cameraHeight * Camera.main.aspect;
			
			_xMin = _levelBounds.min.x+(cameraWidth/2);
			_xMax = _levelBounds.max.x-(cameraWidth/2); 
			_yMin = _levelBounds.min.y+(cameraHeight/2); 
			_yMax = _levelBounds.max.y-(cameraHeight/2);	
		}
		
		/// <summary>
		/// Use this method to shake the camera, passing in a Vector3 for intensity, duration and decay
		/// </summary>
		/// <param name="shakeParameters">Shake parameters : intensity, duration and decay.</param>
		public virtual void Shake(Vector3 shakeParameters)
		{
			_shakeIntensity = shakeParameters.x;
			_shakeDuration=shakeParameters.y;
			_shakeDecay=shakeParameters.z;
		}

	    /// <summary>
	    /// Moves the camera up
	    /// </summary>
	    public virtual void LookUp()
		{
			_lookDirectionModifier = new Vector3(0,ManualUpDownLookDistance,0);
		}
	    /// <summary>
	    /// Moves the camera down
	    /// </summary>
	    public virtual void LookDown()
		{
			_lookDirectionModifier = new Vector3(0,-ManualUpDownLookDistance,0);
		}
	    /// <summary>
	    /// Resets the look direction modifier
	    /// </summary>
	    public virtual void ResetLookUpDown()
		{	
			_lookDirectionModifier = new Vector3(0,0,0);
		}		
	}
}