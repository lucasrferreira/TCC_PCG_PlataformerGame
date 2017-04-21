using System.Collections.Generic;
using System.Linq;
using Assets.TCC_PCG_PlataformerGame.Scripts;
using TCC_PCG_PlataformerGame.Scripts;
using UnityEngine;

namespace Assets.TCC_PCG_PlataformerGame.Scripts
{
    public class Gerator : MonoBehaviour
    {
    
        [SerializeField]
        private readonly List<BuildPiece> _buildPieces = BuildPiecesRepository.BuildPieces;
        [SerializeField]
        private Room _roomToGenerate;
        private char[,] _room;

        private static readonly System.Random Rnd = new System.Random();

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

        public void InitiateAlgoritm()
        {
            var room = _room;
            var solution = new BuildRoomSolution();
            var exitPoint = new List<Point2D>(_roomToGenerate.ExitPositionList);
            var exitPointLeft = new List<Point2D>(exitPoint);
            var buildPieces = _buildPieces;
            var startPoints = new List<Point2D> { GetStartPoint(_roomToGenerate) };
        }

        public void Generate(char[,] room, BuildRoomSolution solution, List<Point2D> exitPoint, 
            List<Point2D> exPointLeft, List<BuildPiece> bp, List<Point2D> startPoints)
        {
            foreach (var startPoint in Perm(startPoints))
            {
                foreach (var buildPiece in Perm(bp))
                {
                    foreach (var transformation in Perm(Transformations(buildPiece, startPoint)))
                    {
                           
                    }
                }
            }
        }

        private bool SatisfiesConstraints(TransformedBuildPiece tb, BuildRoomSolution sol, char[,] room)
        {


            return true;
        }

        private static IEnumerable<TransformedBuildPiece> Transformations(BuildPiece b, Point2D s)
        {
            var cCells = b.GetConnectionCellsPoints();
            var transformations = cCells.Select(point2D => new TransformedBuildPiece(point2D, s, b)).ToList();
            return transformations;
        }
    

        private static List<T> Perm<T>(IEnumerable<T> list)
        {
            var copy = new List<T>(list);
            copy = copy.OrderBy(i => Rnd.Next()).ToList();
            return copy;
        }

        public Point2D GetStartPoint(Room room)
        {
            var count = room.ExitPositionList.Count;
            var rnd = new System.Random();
            var index = rnd.Next(0, count-1);
            return room.ExitPositionList[index];
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
}
