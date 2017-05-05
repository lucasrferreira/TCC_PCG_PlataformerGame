using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using UnityEngine;

namespace Assets.TCC_PCG_PlataformerGame.Scripts
{
    [Serializable]
    public struct Point2D
    {
        public int X;
        public int Y;
        public Point2D(int x, int y)
        {
            X = x;
            Y = y;
        }
        public static Point2D operator +(Point2D c1, Point2D c2)
        {
            return new Point2D(c1.X + c2.X, c1.Y + c2.Y);
        }
        public static Point2D operator -(Point2D c1, Point2D c2)
        {
            return new Point2D(c1.X - c2.X, c1.Y - c2.Y);
        }
        
        public bool Equals(Point2D other)
        {
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }
    }

    [Serializable]
    public class Room
    {
        [SerializeField]
        private Point2D _size;
        [SerializeField]
        private List<Point2D> _exitPositionList;
        [SerializeField]
        //private Point2D _startPosition;

        public Point2D Size
        {
            get { return _size; }
        }

        public List<Point2D> ExitPositionList
        {
            get { return _exitPositionList; }
        }

        //public Point2D StartPosition
        //{
        //    get { return _startPosition; }
        //}

        public Room(Point2D size, List<Point2D> exitPositionList, Point2D startPosition)
        {
            _size = size;
            _exitPositionList = exitPositionList;
            //_startPosition = startPosition;
        }


    }
}
