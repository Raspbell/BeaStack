using Unity.Mathematics;
using UnityEngine;
using InGame.Model.Data;

namespace InGame.Model.Logic
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
            float startRate = 1f / _gameData.InitialSpawnInterval;
            float maxDifficultyRate = 1f / _gameData.MinSpawnInterval;

            float currentRate = Mathf.LerpUnclamped(startRate, maxDifficultyRate, _totalTimeElapsed / _gameData.timeToMaxDifficulity);

            _currentSpawnInterval = 1f / currentRate;
        }
    }
}