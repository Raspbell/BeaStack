using UnityEngine;

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

    private void AddScore(int score)
    {
        _gameModel.Score.Value += score;
    }
}