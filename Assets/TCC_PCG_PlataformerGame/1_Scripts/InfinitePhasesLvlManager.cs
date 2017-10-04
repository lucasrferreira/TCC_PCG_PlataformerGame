using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Acrocatic;
using Assets.TCC_PCG_PlataformerGame.Scripts;
using UnityEngine;
//using MoreMountains.CorgiEngine;

public class InfinitePhasesLvlManager : MonoBehaviour
{

    public CameraFollow Camera;
    private InfinitePhasesManager _ipManager;
    [SerializeField] private Player _playerPrefab;
    private Player _playerInstance;
    
    private Exit _atualStartPoint;
    private List<Exit> _atualExits;
    private bool _isFirstPhase = true;

    private bool _creationFinished = false;
    private bool _lvlFinished = false;

    private void Awake ()
	{
        _ipManager = FindObjectOfType<InfinitePhasesManager>();
	    _ipManager.CreationFinish += CreationFinish;
    }

    private void OnDestroy()
    {
        _ipManager.CreationFinish -= CreationFinish;
    }

    private void Start()
    {
        _ipManager.CreateNewLevel();
    }

    private void CreationFinish()
    {
        if (_isFirstPhase)
        {
            _isFirstPhase = false;
            InitiatePhase();
            return;
        }
        _creationFinished = true;
        if(!_lvlFinished) return;
        InitiatePhase();
    }

    private void LvlFinish()
    {
        if (!_creationFinished) return;
        InitiatePhase();
    }
    private void InitiatePhase()
    {
        if (_playerInstance == null) { 
            _playerInstance = Instantiate(_playerPrefab);
            Camera.player = _playerInstance.transform;
        }
        _ipManager.DrawNewGame();
        _atualExits = FindObjectsOfType<Exit>().ToList();
        _atualStartPoint = _atualExits[Random.Range(0, _atualExits.Count)];
        foreach (var exit in _atualExits)
        {
            if(exit == _atualStartPoint) continue;
            exit.InfinitePhasesLvlManager = this;
        }
        _playerInstance.transform.position = _atualStartPoint.transform.position;
        _ipManager.CreateNewLevel();
    }
}
