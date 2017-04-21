using System.Collections.Generic;
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
        public List<TransformedBuildPiece> TransformedBuildPieces { get; private set; }
        private char[,] _room;

        public BuildRoomSolution()
        {
            TransformedBuildPieces = new List<TransformedBuildPiece>();  
        }

        
        public void Add(TransformedBuildPiece tBuildPiece)
        {
            TransformedBuildPieces.Add(tBuildPiece);
        }

        public void RemoveLast()
        {
            TransformedBuildPieces.RemoveAt(TransformedBuildPieces.Count - 1);
        }
    }
}