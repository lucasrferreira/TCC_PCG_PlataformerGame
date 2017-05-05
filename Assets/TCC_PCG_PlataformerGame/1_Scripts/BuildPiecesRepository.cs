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
                {'c','s'},
                {'s','n'}
            }),
            new BuildPiece(new char[,]
            {
                {'c','b'},
                {'s','c'},
                {'n','s'}
            }),
            new BuildPiece(new char[,]
            {
                {'b','c'},
                {'b','s'},
                {'c','n'},
                {'s','n'}
            }),
            new BuildPiece(new char[,]
            {
                {'c','b'},
                {'s','b'},
                {'n','c'},
                {'n','s'}
            }),
            new BuildPiece(new char[,]
            {
                {'n','b','c'},
                {'c','b','s'},
                {'s','n','n'},
            }),
            new BuildPiece(new char[,]
            {
                {'c','b','n'},
                {'s','b','c'},
                {'n','n','s'},
            }),
            new BuildPiece(new char[,]
            {
                {'c','b','c'},
                {'s','n','s'},
            }),
        };
    }
}
