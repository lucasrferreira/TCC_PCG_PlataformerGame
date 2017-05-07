using UnityEngine;

namespace Assets.TCC_PCG_PlataformerGame.Scripts
{
    public class LevelDesigner: MonoBehaviour
    {
        [SerializeField]
        private Generator _generator;
        private Room _roomToGenerate;
        [SerializeField]
        private GameObject _blockPrefab;
        [SerializeField]
        private float _spriteSize;
        private void Start()
        {
            _generator.InitiateAlgoritm(DrawLevel);
            _roomToGenerate = _generator._roomToGenerate;
        }

        private void DrawLevel(char[,] c, BuildRoomSolution buildRoomSolution)
        {
            c = buildRoomSolution.GetAtualSolutionRoom(c);
            for (var i = 0; i < c.GetLength(0); i++)
            {
                for (var j = 0; j < c.GetLength(1); j++)
                {
                    if (c[i, j] != 's') continue;
                    var x = (j - _roomToGenerate.Size.Y / 2) * _spriteSize;
                    var y = (-i + _roomToGenerate.Size.X / 2) * _spriteSize;
                    var position = new Vector3(x,y);
                    var block = Instantiate(_blockPrefab, position, Quaternion.identity, transform);
                }
            }
        }

    }
}