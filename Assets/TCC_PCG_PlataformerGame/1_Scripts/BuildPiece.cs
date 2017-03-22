using System;
using UnityEngine;

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

    }
}
