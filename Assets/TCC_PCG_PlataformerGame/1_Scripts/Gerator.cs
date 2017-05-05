using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.TCC_PCG_PlataformerGame.Scripts;
using TCC_PCG_PlataformerGame.Scripts;
using UnityEditor;
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
        private IEnumerator coroutine;

        private static readonly System.Random Rnd = new System.Random();

        private void Awake()
        {
            _room = new char[_roomToGenerate.Size.X, _roomToGenerate.Size.Y];
            for (var i = 0; i < _room.GetLength(0); i++)
            {
                for (var j = 0; j < _room.GetLength(1); j++)
                {

                    _room[i,j] = 'n';
                }
            }

            //foreach (var exits in _roomToGenerate.ExitPositionList)
            //{
            //    _room[exits.X, exits.Y] = 'e';
            //}
            InitiateAlgoritm((x, y) => { PrintRoom(y.GetAtualSolutionRoom(x)); } );
        }

        public void InitiateAlgoritm(Action<char[,], BuildRoomSolution> callbackAction)
        {
            Application.runInBackground = true;
            var room = _room;
            var solution = new BuildRoomSolution();
            var exitPoint = new List<Point2D>(_roomToGenerate.ExitPositionList);
            var exitPointLeft = new List<Point2D>(exitPoint);
            var buildPieces = _buildPieces;
            var startPoints = new List<Point2D> { GetStartPoint(_roomToGenerate) };

            foreach (var ep in exitPoint)
            {
                var bp = new BuildPiece(new char[,]
                {
                    {'e'}
                });
                var tbp = new TransformedBuildPiece(new Point2D(0,0), ep, bp);
                solution.Add(tbp);
            }

            PrintRoom(solution.GetAtualSolutionRoom(room));

            coroutine = Generate(room, solution, exitPoint, exitPointLeft, buildPieces, startPoints, value =>
            {
                StopCoroutine(coroutine);
                callbackAction(room, value);
            });
            StartCoroutine(coroutine);
        }
        


        private IEnumerator Generate(char[,] room, BuildRoomSolution solution, List<Point2D> exitPoint, 
            List<Point2D> exitPointLeft, List<BuildPiece> buildPieces, List<Point2D> startPoints, 
            Action<BuildRoomSolution> callbackAction)
        {
            foreach (var sp in Perm(startPoints))
            {
                foreach (var bp in Perm(buildPieces))
                {
                    foreach (var g in Perm(Transformations(bp, sp)))
                    {
                        if (!SatisfiesConstraints(g, solution, room)) continue;

                        solution.Add(g);
                        PrintRoom(solution.GetAtualSolutionRoom(room));
                        var exits = ContainsExits(g, exitPointLeft) as List<Point2D>;
                        exitPointLeft = exitPointLeft.Except(exits).ToList();
                        Debug.Log("Exits " + exits.Count);
                        Debug.Log("ExitsPointLeft " + exitPointLeft.Count);
                        Debug.Log("ExitsPointLeft " + exitPointLeft.Any());
                        if (!exitPointLeft.Any())
                            callbackAction(solution);
                        if (exits.Any())
                            startPoints = PossibleStartPoints(solution, room) as List<Point2D>;
                        else
                            startPoints = PossibleStartPoints(g, sp) as List<Point2D>;

                        BuildRoomSolution returnedValue = null;
                        var corrotine = Generate(room, solution, exitPoint, exitPointLeft, _buildPieces, startPoints,
                            (value) => { returnedValue = value; }
                        );
                        StartCoroutine(corrotine);
                        while (returnedValue == null)
                        {
                            yield return null;
                        }
                        if (!returnedValue.Empty())
                        {
                            StopCoroutine(corrotine);
                            callbackAction(returnedValue);
                        }

                        solution.RemoveLast();
                        exitPointLeft = exitPointLeft.Union(exits).ToList() as List<Point2D>;
                        PrintRoom(solution.GetAtualSolutionRoom(room));
                        Debug.Log("Fiz o Union " + exitPointLeft.Count);
                    }
                }
            }
            callbackAction(new BuildRoomSolution());
        }

        private static IEnumerable<Point2D> ContainsExits(TransformedBuildPiece tbp, IEnumerable<Point2D> exitPoints)
        {
            return (from ep in exitPoints
                    let c = tbp.ConnectionPoint
                    let p = tbp.StartPoint
                    let infBound = p - c
                    let supBound = infBound + new Point2D(tbp.BuildPiece.Piece.GetLength(0)-1, tbp.BuildPiece.Piece.GetLength(1)-1)
                    let infBoundDif = infBound - ep
                    let supBoundDif = supBound - ep
                    where infBoundDif.Y <= 0 && infBoundDif.X <= 0 && supBoundDif.Y >= 0 && supBoundDif.X >= 0
                    select ep).ToList();
        }

        private static void PrintRoom(char[,] room)
        {
            var rowCount = room.GetLength(0);
            var colCount = room.GetLength(1);
            var sisout = "";
            for (int row = 0; row < rowCount; row++)
            {
                for (int col = 0; col < colCount; col++)
                    sisout += string.Format("{0} ", room[row, col]);
                sisout += "\n";
            }
            print(sisout);
        }

        private IEnumerable<Point2D> PossibleStartPoints(BuildRoomSolution sol, char[,] room)
        {
            var startPoints = new List<Point2D>();

            var atualRoom = sol.GetAtualSolutionRoom(room);
            for (var i = 0; i < atualRoom.GetLength(0); i++)
            {
                for (var j = 0; j < atualRoom.GetLength(1); j++)
                {
                    if (atualRoom[i, j] == 'c')
                        startPoints.Add(new Point2D(i, j));
                }
            }
            return startPoints;
        }
        private IEnumerable<Point2D> PossibleStartPoints(TransformedBuildPiece tbp, Point2D s)
        {
            var startPoints = new List<Point2D>();

            var piece = tbp.BuildPiece.Piece;
            var c = tbp.ConnectionPoint;
            var p = s;
            var diference = p - c;

            for (var i = 0; i < piece.GetLength(0); i++)
            {
                for (var j = 0; j < piece.GetLength(1); j++)
                {
                    if (piece[i, j] == 'c')
                        startPoints.Add(diference + (new Point2D( i, j)) );
                }
            }
            return startPoints;
        }

        private bool SatisfiesConstraints(TransformedBuildPiece tb, BuildRoomSolution sol, char[,] room)
        {
            var roomHeight = room.GetLength(0);
            var roomWidth = room.GetLength(1);
            var pieceHeight = tb.BuildPiece.Piece.GetLength(0);
            var pieceWidth= tb.BuildPiece.Piece.GetLength(1);
            var c = tb.ConnectionPoint;
            var p = tb.StartPoint;
            var diference = p - c;
            if (diference.Y < 0 || diference.X < 0 ||
                diference.Y  + pieceWidth - 1 >= roomWidth || diference.X + pieceHeight - 1 >= roomHeight) //C0 - test piece in room bounds
                return false;

            var atualRoom = sol.GetAtualSolutionRoom(room);
            //if (atualRoom[tb.StartPoint.X, tb.StartPoint.Y] !=
            //    tb.BuildPiece.Piece[tb.ConnectionPoint.X, tb.ConnectionPoint.Y]) // teste if connection cells match! C1
            //    return false;

            var addedPath = false; //will receive true if some connection cell was added to solution path
            for (var i = 0; i < tb.BuildPiece.Piece.GetLength(0); i++)
            {
                var bpPoint = new Point2D(i, 0);
                for (var j = 0; j < tb.BuildPiece.Piece.GetLength(1); j++)
                {
                    bpPoint.Y = j;
                    var newPoint = diference + bpPoint;
                    var roomValue = atualRoom[newPoint.X, newPoint.Y];
                    var bpValue = tb.BuildPiece.Piece[i, j];

                    if (roomValue == 's' && (bpValue == 'c' || bpValue == 'b')) return false; //C1
                    if (bpValue == 's' && (roomValue == 'c' || roomValue == 'b')) return false; //C1
                    if (roomValue == 'e' && bpValue != 'c') return false; //C3
                    if (bpValue == 'c' && (roomValue == 'n' || roomValue == 'b' || roomValue == 'e')) addedPath = true; //added a connection cell to path
                   
                }
            }
            return addedPath; //C2
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
