using System.Collections.Generic;
using System.Linq;
using Assets.TCC_PCG_PlataformerGame.Scripts;
using TCC_PCG_PlataformerGame.Scripts;

namespace Assets.TCC_PCG_PlataformerGame.Scripts
{
    public struct TransformedBuildPiece
    {
        public Point2D ConnectionPoint { get; private set; }
        public Point2D StartPoint { get; private set; }
        public BuildPiece BuildPiece { get; private set; }

        public TransformedBuildPiece(Point2D connectionPoint, Point2D startPoint, BuildPiece buildPiece) : this()
        {
            ConnectionPoint = connectionPoint;
            StartPoint = startPoint;
            BuildPiece = buildPiece;
        }
    }

    public class BuildRoomSolution
    {
        private readonly List<TransformedBuildPiece> _transformedBuildPieces;
        private char[,] _room;
        private bool _alreadyCalculated= false;

        public BuildRoomSolution()
        {
            _transformedBuildPieces = new List<TransformedBuildPiece>();  
        }

        public char[,] GetAtualSolutionRoom(char[,] room)
        {
            if(_alreadyCalculated) return _room;
            _room = (char[,]) room.Clone();

            foreach (var tbp in _transformedBuildPieces)
            {
                var dif = tbp.StartPoint - tbp.ConnectionPoint;
                for (var i = 0; i < tbp.BuildPiece.Piece.GetLength(0); i++)
                {
                    var cPoint = new Point2D(i,0);
                    for (var j = 0; j < tbp.BuildPiece.Piece.GetLength(1); j++)
                    {
                        cPoint.Y = j;
                        var newPoint = dif + cPoint;
                        _room[newPoint.X, newPoint.Y] = tbp.BuildPiece.Piece[i, j];
                    }
                }
            }
            _alreadyCalculated = true;
            return _room;
        }

        public void Add(TransformedBuildPiece tBuildPiece)
        {
            _transformedBuildPieces.Add(tBuildPiece);
            _alreadyCalculated = false;
        }

        public void RemoveLast()
        {
            _transformedBuildPieces.RemoveAt(_transformedBuildPieces.Count - 1);
            _alreadyCalculated = false;
        }

        public bool Empty()
        {
            return !_transformedBuildPieces.Any();
        }
    }
}