using UnityEngine;
using InGame.Model.Data;
using System.Collections.Generic;

namespace InGame.Model.Logic
{
    public class PuzzleRule
    {
        public float TsumConnectDistance;
        public float MinChainCountToClear;

        private GameModel _gameModel;
        private GameData _gameData;

        public PuzzleRule(GameData gameData, GameModel gameModel)
        {
            _gameData = gameData;
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

        public long CalculateScore(List<Tsum> chain, List<Tsum> bombTargets, TsumData tsumData)
        {
            if (chain == null || chain.Count == 0)
            {
                return 0;
            }

            int mainTsumId = -1;
            int wildcardCount = 0;
            float baseChainScore = 0f;

            foreach (var tsum in chain)
            {
                var tsumComponent = tsumData.GetTsumComponentById(tsum.TsumID);
                if (tsumComponent != null)
                {
                    baseChainScore += tsumComponent.Score;
                }

                if (tsum.Type == TsumType.Wildcard)
                {
                    wildcardCount++;
                }
                else
                {
                    mainTsumId = tsum.TsumID;
                }
            }

            if (mainTsumId == -1)
            {
                mainTsumId = _gameData.WildcardOnlyVirtualId;
            }

            // カーブの定義域端でClampして外挿を防ぐ
            float clampedChainCount = Mathf.Clamp(chain.Count, 3f, 20f);
            float clampedMainTsumId = Mathf.Clamp(mainTsumId, 0f, 10f);

            float chainLengthMultiplier = _gameData.ChainLengthBonusCurve.Evaluate(clampedChainCount);
            float rankMultiplier = _gameData.TsumIdRankBonusCurve.Evaluate(clampedMainTsumId);

            float wildcardBonusMultiplier = 1.0f;
            if (wildcardCount > 0)
            {
                wildcardBonusMultiplier = 1.0f + (wildcardCount * _gameData.WildcardInvolvementBonusCurve.Evaluate(clampedMainTsumId));
            }

            float totalChainScore = baseChainScore * chainLengthMultiplier * rankMultiplier * wildcardBonusMultiplier;

            float totalBombScore = 0f;
            if (bombTargets != null && bombTargets.Count > 0)
            {
                float baseBombScore = 0f;
                foreach (var tsum in bombTargets)
                {
                    var tsumComponent = tsumData.GetTsumComponentById(tsum.TsumID);
                    if (tsumComponent != null)
                    {
                        baseBombScore += tsumComponent.Score;
                    }
                }
                totalBombScore = baseBombScore * _gameData.ExplosionScoreMultiplier;
            }

            return (long)Mathf.Round(totalChainScore + totalBombScore);
        }
    }
}