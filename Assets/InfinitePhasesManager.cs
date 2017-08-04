using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.TCC_PCG_PlataformerGame.Scripts
{
    public class InfinitePhasesManager : MonoBehaviour
    {
        private static readonly System.Random Rnd = new System.Random();
        [SerializeField] private LevelDesigner _levelDesignerPrefab;
        private LevelDesigner _levelDesigner;
        private LevelDesigner _oldLevelDesigner;
        
        public Point2D levelsSize;

        public Action CreationFinish;

        
        public void CreateNewLevel()
        {
            var exits = new List<Point2D>(3)
            {
                new Point2D(0, Rnd.Next(2, levelsSize.Y - 2)),
                new Point2D(Rnd.Next(2, levelsSize.X - 2), 0),
                new Point2D(Rnd.Next(2, levelsSize.X - 2), levelsSize.Y - 1)
            };
            //for (int i = 0; i < 3; i++)
            //{
            //}

            var roomToGenerate = new Room(levelsSize,exits,exits[Rnd.Next(0,exits.Count-1)]);
            _levelDesigner = Instantiate(_levelDesignerPrefab);
            _levelDesigner.DesignFinish += DesignFinished;
            _levelDesigner.InitializeDesigner(roomToGenerate);
            _levelDesigner.Generate();
        }

        private void DesignFinished()
        {
            if(CreationFinish != null)
                CreationFinish();
        }

        
        public void DrawNewGame()
        {
            if(_oldLevelDesigner!=null)
                UnloadOldLevel();
            _levelDesigner.DrawLevel();
            _oldLevelDesigner = _levelDesigner;
            CreateNewLevel();

        }

        public void UnloadOldLevel()
        {
            Destroy(_oldLevelDesigner.gameObject);
        }
    }
}
