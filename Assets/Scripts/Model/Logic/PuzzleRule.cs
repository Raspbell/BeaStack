using UnityEngine;
using Model.Data;

namespace Model.Logic
{
    public class PuzzleRule
    {
        public float TsumConnectDistance;
        public float MinChainCountToClear;

        private GameModel _gameModel;

        public PuzzleRule(GameData gameData, GameModel gameModel)
        {
            TsumConnectDistance = gameData.TsumConnectDistance;
            MinChainCountToClear = gameData.MinChainCountToClear;
            _gameModel = gameModel;
        }

        public bool CanConnectTsums(Vector2 posA, Vector2 posB)
        {
            float distance = Vector2.Distance(posA, posB);
            return distance <= TsumConnectDistance;
        }

        public int GetRandomTsumID(int maxLevelIndex, TsumData tsumData)
        {
            int randomIdx = Random.Range(0, maxLevelIndex + 1);
            if (randomIdx >= tsumData.TsumEntities.Length)
            {
                randomIdx = 0;
            }
            return tsumData.TsumEntities[randomIdx].ID;
        }

        public void AddChainScore(int chainLength, int tsumScore, bool isFever)
        {
            if (chainLength < MinChainCountToClear)
            {
                return;
            }
        }

        public void AddFeverPoints(int points)
        {
            if (_gameModel == null)
            {
                return;
            }
            _gameModel.FeverPoint.Value = Mathf.Min(_gameModel.FeverPoint.Value + points, _gameModel.FeverPoint.Value);
        }

        public void AddScore(int score)
        {
            _gameModel.Score.Value += score;
        }

        public void AddSkillPoints(int score)
        {
            _gameModel.SkillPoint.Value += score;
        }
    }
}