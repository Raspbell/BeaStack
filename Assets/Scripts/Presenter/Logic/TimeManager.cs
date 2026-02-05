using UnityEngine;

public class TimeManager
{
    private GameModel _gameModel;

    public TimeManager(GameModel gameModel)
    {
        _gameModel = gameModel;
    }

    public void TimerTick(float deltaTime)
    {
        if (_gameModel == null || _gameModel.CurrentGameState.Value != GameModel.GameState.Playing)
        {
            return;
        }

        _gameModel.TimeRemaining.Value = Mathf.Max(0, _gameModel.TimeRemaining.Value - deltaTime);
        if (_gameModel.TimeRemaining.Value <= 0f)
        {
            _gameModel.TimeRemaining.Value = 0f;
            OnTimeUp();
        }
    }

    public void AddTime(float timeToAdd)
    {
        if (_gameModel == null)
        {
            return;
        }
        _gameModel.TimeRemaining.Value += timeToAdd;
    }

    private void OnTimeUp()
    {
        _gameModel.CurrentGameState.Value = GameModel.GameState.GameOver;
    }
}