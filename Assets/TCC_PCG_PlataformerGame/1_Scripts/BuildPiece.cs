using System;
using System.Collections.Generic;
using Assets.TCC_PCG_PlataformerGame.Scripts;
using UnityEngine;
using UnityEngine.Networking;

namespace TCC_PCG_PlataformerGame.Scripts
{
    [Serializable]
    public class BuildPiece
    {
        [SerializeField]
        private char[,] _piece;

        public char[,] Piece
        {
            get { return _piece; }
        }

        public BuildPiece(char[,] piece)
        {
            _piece = piece;
        }

        public List<Point2D> GetConnectionCellsPoints()
        {
            var ccp = new List<Point2D>();
            var num_of_rows = _piece.GetLength(0);
            var num_of_columns = _piece.GetLength(1);
            for (var i = 0; i < num_of_rows; i++)
            {
                for (var j = 0; j < num_of_columns; j++)
                {
                    if (_piece[i,j].Equals('c'))
                    {
                           ccp.Add(new Point2D(i,j));
                    }
                }
            }
            return ccp;
        }
    }
}
