using Unity.Mathematics;
using UnityEngine;
using Model.Data;

namespace Model.Logic
{
    public class TimeTsumSpawnManager
    {
        private readonly GameData _gameData;

        private float _spawnTimer;
        private float _totalTimeElapsed;
        private float _currentSpawnInterval;

        public TimeTsumSpawnManager(GameData gameData)
        {
            _gameData = gameData;
            Initialize();
        }

        public void Initialize()
        {
            _spawnTimer = 0f;
            _totalTimeElapsed = 0f;
            _currentSpawnInterval = _gameData.InitialSpawnInterval;
        }

        public bool Tick(float deltaTime)
        {
            _spawnTimer += deltaTime;
            _totalTimeElapsed += deltaTime;

            UpdateDifficulty();

            if (_spawnTimer >= _currentSpawnInterval)
            {
                _spawnTimer = 0f;
                return true;
            }

            return false;
        }

        private void UpdateDifficulty()
        {
            float targetInterval = _gameData.InitialSpawnInterval - _gameData.SpawnIntervalDecrement * _totalTimeElapsed;
            _currentSpawnInterval = Mathf.Max(_gameData.MinSpawnInterval, targetInterval);
        }
    }
}