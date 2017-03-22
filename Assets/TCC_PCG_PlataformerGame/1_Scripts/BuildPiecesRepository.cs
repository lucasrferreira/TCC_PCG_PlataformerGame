using System;
using System.Collections.Generic;
using TCC_PCG_PlataformerGame.Scripts;

namespace Assets.TCC_PCG_PlataformerGame.Scripts
{
    [Serializable]
    static class BuildPiecesRepository
    {
        public static readonly List<BuildPiece> BuildPieces = new List<BuildPiece>()
        {
            new BuildPiece(new char[,]
            {
                {'c','c'},
                {'s','s'}
            }),
            new BuildPiece(new char[,]
            {
                {'b','c'},
                {'b','s'},
                {'c','n'},
                {'s','n'}
            }),
        };
    }
}
