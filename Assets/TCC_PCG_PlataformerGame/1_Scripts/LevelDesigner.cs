using System;
using System.Collections.Generic;
using Assets.TCC_PCG_PlataformerGame._1_Scripts;
using UnityEngine;

namespace Assets.TCC_PCG_PlataformerGame.Scripts
{
    public class LevelDesigner: MonoBehaviour
    {
        public LevelDraw LevelDraw;
        [SerializeField] private Generator _generatorPrefab;
        [SerializeField]
        private int _simutaniousTry;
        [SerializeField] private List<Generator> _generator;
        
        private Room _roomToGenerate;
        [SerializeField]
        private GameObject _blockPrefab;
        [SerializeField]
        private float _spriteSize;

        [SerializeField] private char[,] _generatedLevel;
        [SerializeField] private BuildRoomSolution _solution;
        public bool Finished { get; private set; }

        public GameObject ExitPrefab;
        public Action DesignFinish;

        public void InitializeDesigner(Room roomToGenerate)
        {
            _roomToGenerate = roomToGenerate;
            _generator = new List<Generator>(_simutaniousTry);
            for (int i = 0; i < _simutaniousTry; i++)
            {
                var generator = Instantiate(_generatorPrefab);
                generator.InitializeGenerator(_roomToGenerate);
                _generator.Add(generator);
            }
            Finished = false;
        }

        public void Generate()
        {
            foreach (var generator in _generator)
            {
                generator.InitiateAlgoritm(FinishToGenerate);
            }
        }

        private void FinishToGenerate(char[,] generatedLevel, BuildRoomSolution solution)
        {
            _solution = solution;
            _generatedLevel = generatedLevel;
            foreach (var generator in _generator)
            {
                generator.StopGeneration();
                Destroy(generator.gameObject);
            }
            DesignFinish();
            Generator.PrintRoom(solution.GetAtualSolutionRoom(generatedLevel));
            Finished = true;
        }

        public void DrawLevel()
        {
            Debug.Log("Desenhar");
            _generatedLevel = _solution.GetAtualSolutionRoom(_generatedLevel);
            var augmentedSolution = new char[_generatedLevel.GetLength(0)+4, _generatedLevel.GetLength(1)+4];

            var jSize = augmentedSolution.GetLength(1);
            var iSize = augmentedSolution.GetLength(0);
            for (int i = 0; i < iSize; i++)
            {
                for (int j = 0; j < jSize; j++)
                {
                    if (i == 0 || j == 0 || i == iSize - 1 || j == jSize - 1)
                    {
                        augmentedSolution[i, j] = 'b';
                        continue;
                    }
                    if (i == 1 || j == 1 || i == iSize - 2 || j == jSize - 2)
                    {
                        augmentedSolution[i, j] = 'n';
                        continue;
                    }
                    augmentedSolution[i, j] = _generatedLevel[i-2,j-2];

                }
            }

            for (var i = 1; i < iSize -1 ; i++)
            {
                for (var j = 1; j < jSize -1; j++)
                {
                    if (augmentedSolution[i, j] != 's' && augmentedSolution[i, j] != 'n') continue;

                    var round = augmentedSolution.Slice(i - 1, i - 1 + 2, j - 1, j - 1 + 2);
                    var sprite = LevelDraw.GetSprite(round);
                    var x = (j -2 - _roomToGenerate.Size.Y / 2) * _spriteSize;
                    var y = (-i +2 + _roomToGenerate.Size.X / 2) * _spriteSize;
                    var position = new Vector3(x, y);
                    var block = Instantiate(_blockPrefab, Vector3.zero, Quaternion.identity, transform);
                    block.transform.localPosition = position;
                    block.GetComponent<SpriteRenderer>().sprite = sprite;

                    
                }
            }

            foreach (var point2D in _roomToGenerate.ExitPositionList)
            {
                Debug.Log(point2D);
                var block = Instantiate(ExitPrefab, Vector3.zero, Quaternion.identity, transform);
                var x = (point2D.Y - _roomToGenerate.Size.Y / 2) * _spriteSize;
                var y = (-point2D.X + _roomToGenerate.Size.X / 2) * _spriteSize;
                var position = new Vector3(x, y);
                Debug.Log(position);
                block.transform.localPosition = position;
            }
        }

    }
}