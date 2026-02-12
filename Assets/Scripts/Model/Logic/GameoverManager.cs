namespace Model.Logic
{
    public class GameoverManager
    {
        private float _timeFromGameoverZoneEnter = 0f;
        public float CurrentGraceTime => _timeFromGameoverZoneEnter;

        public GameoverManager()
        {
            _timeFromGameoverZoneEnter = 0f;
        }

        public bool IsGameover(float deltaTime, bool isInGameoverZone, float gameoverDelay)
        {
            if (isInGameoverZone)
            {
                _timeFromGameoverZoneEnter += deltaTime;
                if (_timeFromGameoverZoneEnter >= gameoverDelay)
                {
                    return true;
                }
            }
            else
            {
                _timeFromGameoverZoneEnter = 0f;
            }

            return false;
        }

        public float GetGraceProgress(float maxGraceTime)
        {
            if (maxGraceTime <= 0f)
            {
                return 0f;
            }
            return _timeFromGameoverZoneEnter / maxGraceTime;
        }
    }
}