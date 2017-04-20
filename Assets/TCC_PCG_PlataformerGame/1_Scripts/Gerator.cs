using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Assets.TCC_PCG_PlataformerGame.Scripts;
using Assets.TCC_PCG_PlataformerGame._1_Scripts;
using TCC_PCG_PlataformerGame.Scripts;
using UnityEngine;

public class Gerator : MonoBehaviour
{

    [SerializeField]
    private List<BuildPiece> _buildPieces = BuildPiecesRepository.BuildPieces;
    [SerializeField]
    private Room _roomToGenerate;
    [SerializeField]
    private char[,] _room;

    private void Awake()
    {
        _room = new char[_roomToGenerate.Size.X, _roomToGenerate.Size.Y];
        for (var i = 0; i < _room.Length/2; i++)
        {
            for (var j = 0; j < _room.Length/2; j++)
            {

                _room[i,j] = 'n';
            }
        }

        foreach (var exits in _roomToGenerate.ExitPositionList)
        {
            _room[exits.X, exits.X] = 'e';
        }
    }

    public void Generate()
    {
        _room[_roomToGenerate.StartPosition.X, _roomToGenerate.StartPosition.Y] = 'c';

        var rnd = new System.Random();
        foreach (var startPoint in GetStartPositions(_room).OrderBy(item => rnd.Next()))
        {
            foreach (var buildPiece in _buildPieces.OrderBy(item => rnd.Next()))
            {

            }
        }
    }

    private static IEnumerable<Point2D> GetStartPositions(char[,] room)
    {
        var sList = new List<Point2D>();
        for (var i = 0; i < room.Length/2; i++)
        {
            for (var j = 0; j < room.Length/2; j++)
            {
                if(room[i,j] == 'c')
                    sList.Add(new Point2D(i,j));
            }
        }
        return sList;
    }


   
}
