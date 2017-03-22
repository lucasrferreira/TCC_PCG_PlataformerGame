using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

namespace Assets.TCC_PCG_PlataformerGame._1_Scripts
{
    [Serializable]
    internal struct Point2D
    {
        public int X;
        public int Y;
        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [Serializable]
    class Room
    {
        [SerializeField]
        private Point2D _size;
        [SerializeField]
        private List<Point2D> _exitPositionList;
        [SerializeField]
        private Point2D _startPosition;

        public Point2D Size
        {
            get { return _size; }
        }

        public List<Point2D> ExitPositionList
        {
            get { return _exitPositionList; }
        }

        public Point2D StartPosition
        {
            get { return _startPosition; }
        }

        public Room(Point2D size, List<Point2D> exitPositionList, Point2D startPosition)
        {
            _size = size;
            _exitPositionList = exitPositionList;
            _startPosition = startPosition;
        }


    }
}
