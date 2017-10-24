using System;
using UnityEngine;

namespace Assets.TCC_PCG_PlataformerGame._1_Scripts
{
    [Serializable]
    public class LevelDraw
    {
        private const int TileAroundOrder = 3;
        public LevelSprite _LevelSprite;
        private TileAroundMap _tileMap = new TileAroundMap();

        public GameObject GetSprite(char[,] tileAround)
        {
            if(tileAround.GetLength(0)!=TileAroundOrder || tileAround.GetLength(1)!=TileAroundOrder)
                throw new ArgumentException("tileAround should be char["+TileAroundOrder+","+TileAroundOrder+"]");

            _tileMap.Center = tileAround[1, 1] == 's' || tileAround[1, 1] == 'n';
            _tileMap.Top = tileAround[0, 1] == 's' || tileAround[0, 1] == 'n';
            _tileMap.Bottom = tileAround[2, 1] == 's' || tileAround[2, 1] == 'n';
            _tileMap.Right = tileAround[1, 2] == 's' || tileAround[1, 2] == 'n';
            _tileMap.Left = tileAround[1, 0] == 's' || tileAround[1, 0] == 'n';
            _tileMap.Compute();

            if (_tileMap.Alone)
                return _LevelSprite.Alone;
            if (_tileMap.LeftCliff)
                return _LevelSprite.LeftCliff;
            if (_tileMap.RightCliff)
                return _LevelSprite.RightCliff;
            if (_tileMap.Grass)
            {
                if (_tileMap.UpperLeft && _tileMap.UpperRight && _tileMap.BottomLeft && _tileMap.BottomRight)
                    return _LevelSprite.Grass;

                if (!_tileMap.UpperLeft && !_tileMap.UpperRight)
                    return _LevelSprite.Grass;

                if (_tileMap.UpperLeft)
                    return _LevelSprite.UpperBottomLeftSquared;
                if (_tileMap.UpperRight)
                    return _LevelSprite.UpperBottomRightSquared;
                if (_tileMap.BottomLeft)
                    return _LevelSprite.BottomLeft;
                if (_tileMap.BottomRight)
                    return _LevelSprite.BottomRight;
            }
            return _LevelSprite.NoGrass;
        }

        public struct TileAroundMap
        {
            public bool Top;
            public bool Left;
            public bool Right;
            public bool Bottom;
            public bool Center;

            public bool UpperLeft;
            public bool UpperRight;
            public bool BottomLeft;
            public bool BottomRight;
            public bool LeftCliff;
            public bool RightCliff;
            public bool Grass;
            public bool Alone;
            public bool NoGrass;

            public void Compute()
            {
                UpperLeft = Top || Left;
                UpperRight = Top || Right;
                BottomLeft = Bottom || Left;
                BottomRight = Bottom || Right;

                Grass = !Top;
                NoGrass = !Grass;

                LeftCliff = !BottomLeft && !UpperLeft;
                RightCliff = !BottomRight && !UpperRight;

                Alone = LeftCliff && RightCliff;
                
            }
        }
        [Serializable]
        public struct LevelSprite
        {
            public GameObject UpperBottomLeftSquared;
            public GameObject UpperBottomRightSquared;
            public GameObject BottomLeft;
            public GameObject BottomRight;
            public GameObject LeftCliff;
            public GameObject RightCliff;
            public GameObject Grass;
            public GameObject Alone;
            public GameObject NoGrass;
        }
    }
}