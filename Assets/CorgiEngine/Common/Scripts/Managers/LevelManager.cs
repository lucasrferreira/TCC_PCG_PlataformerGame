using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.Tools;

namespace MoreMountains.CorgiEngine
{	
	/// <summary>
	/// Spawns the player, and 
	/// </summary>
	public class LevelManager : Singleton<LevelManager>
	{	
		/// the prefab you want for your player
		[Header("Prefabs")]
		public CharacterBehavior PlayerPrefab ;
		/// Debug spawn	
		public CheckPoint DebugSpawn;
		/// the elapsed time since the start of the level
		public TimeSpan RunningTime { get { return DateTime.UtcNow - _started ;}}
		
		[Space(10)]
		[Header("Intro and Outro durations")]
		/// duration of the initial fade in
		public float IntroFadeDuration=1f;
		/// duration of the fade to black at the end of the level
		public float OutroFadeDuration=1f;

		[Space(10)]
		[Header("Level Bounds")]
		/// the level limits, camera and player won't go beyond this point.
		public Bounds LevelBounds = new Bounds(Vector3.zero,Vector3.one*10);

	    // private stuff
	    protected CharacterBehavior _player;
	    protected List<CheckPoint> _checkpoints;
	    protected int _currentCheckPointIndex;
	    protected DateTime _started;
	    protected int _savedPoints;
	    protected CameraController _cameraController ;
		
		/// <summary>
		/// On awake, instantiates the player
		/// </summary>
		protected override void Awake()
		{
			base.Awake();
	        if (PlayerPrefab!= null)
	        { 
	    		_player = (CharacterBehavior)Instantiate(PlayerPrefab, new Vector3(0, 0, 0), Quaternion.identity);
		    	GameManager.Instance.Player=_player;
	        }
	    }
		
		/// <summary>
		/// Initialization
		/// </summary>
		public virtual void Start()
		{
			// storage
			_savedPoints=GameManager.Instance.Points;
			_checkpoints = FindObjectsOfType<CheckPoint>().OrderBy(t => t.transform.position.x).ToList();
			_currentCheckPointIndex = _checkpoints.Count > 0 ? 0 : -1;
			_started = DateTime.UtcNow;

			// we get the camera				
			_cameraController = FindObjectOfType<CameraController>();

			// we get the list of spawn points
			var listeners = FindObjectsOfType<MonoBehaviour>().OfType<IPlayerRespawnListener>();
			foreach(var listener in listeners)
			{
				for (var i = _checkpoints.Count - 1; i>=0; i--)
				{
					var distance = ((MonoBehaviour) listener).transform.position.x - _checkpoints[i].transform.position.x;
					if (distance<0)
						continue;
					
					_checkpoints[i].AssignObjectToCheckPoint(listener);
					break;
				}
			}
			
	        if (GUIManager.Instance!= null)
	        { 

			    // set the level name in the GUI
			    GUIManager.Instance.SetLevelName(SceneManager.GetActiveScene().name);
			
			    // fade in
			    GUIManager.Instance.FaderOn(false,IntroFadeDuration);
	        }

	        // in debug mode we spawn the player on the debug spawn point
	        #if UNITY_EDITOR 
	        if (DebugSpawn!= null)
			{
				DebugSpawn.SpawnPlayer(_player);
			}
			else if (_currentCheckPointIndex != -1)
			{
				_checkpoints[_currentCheckPointIndex].SpawnPlayer(_player);
			}
			#else
			if (_currentCheckPointIndex != -1)
			{			
				_checkpoints[_currentCheckPointIndex].SpawnPlayer(_player);
			}
			#endif		
		}

		/// <summary>
		/// Every frame we check for checkpoint reach
		/// </summary>
		public virtual void Update()
		{
			var isAtLastCheckPoint = _currentCheckPointIndex + 1 >= _checkpoints.Count;
			if (isAtLastCheckPoint)
				return;
			
			var distanceToNextCheckPoint = _checkpoints[_currentCheckPointIndex+1].transform.position.x - _player.transform.position.x;
			if (distanceToNextCheckPoint>=0)
				return;
			
			_checkpoints[_currentCheckPointIndex].PlayerLeftCheckPoint();
			
			_currentCheckPointIndex++;
			_checkpoints[_currentCheckPointIndex].PlayerHitCheckPoint();
			
			_savedPoints = GameManager.Instance.Points;
			_started = DateTime.UtcNow;
		}

		/// <summary>
		/// Gets the player to the specified level
		/// </summary>
		/// <param name="levelName">Level name.</param>
		public virtual void GotoLevel(string levelName)
		{		
	        if (GUIManager.Instance!= null)
	        { 
	    		GUIManager.Instance.FaderOn(true,OutroFadeDuration);
	        }
	        StartCoroutine(GotoLevelCo(levelName));
	    }

	    /// <summary>
	    /// Waits for a short time and then loads the specified level
	    /// </summary>
	    /// <returns>The level co.</returns>
	    /// <param name="levelName">Level name.</param>
	    protected virtual IEnumerator GotoLevelCo(string levelName)
		{
	        if (_player!= null)
	        { 
	    		_player.Disable();
	        }


	        if (Time.timeScale > 0.0f)
	        { 
	            yield return new WaitForSeconds(OutroFadeDuration);
	        }
	        GameManager.Instance.UnPause();

	        if (string.IsNullOrEmpty(levelName))
	        {
				LoadingSceneManager.LoadScene("StartScreen");
			}
			else
			{
				LoadingSceneManager.LoadScene(levelName);
			}
			
		}

		/// <summary>
		/// Kills the player.
		/// </summary>
		public virtual void KillPlayer()
		{
			StartCoroutine(KillPlayerCo());
		}

	    /// <summary>
	    /// Coroutine that kills the player, stops the camera, resets the points.
	    /// </summary>
	    /// <returns>The player co.</returns>
	    protected virtual IEnumerator KillPlayerCo()
		{
			_player.Kill();
			_cameraController.FollowsPlayer=false;
			yield return new WaitForSeconds(2f);
			
			_cameraController.FollowsPlayer=true;
			if (_currentCheckPointIndex!=-1)
				_checkpoints[_currentCheckPointIndex].SpawnPlayer(_player);
			
			_started = DateTime.UtcNow;
			GameManager.Instance.SetPoints(_savedPoints);
		}
	}
}