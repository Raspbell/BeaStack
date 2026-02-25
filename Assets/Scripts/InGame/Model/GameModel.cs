using UniRx;
using InGame.Model.Data;

namespace InGame.Model
{
    public class GameModel
    {
        public ReactiveProperty<GameState> CurrentGameState = new ReactiveProperty<GameState>(GameState.None);
        public FloatReactiveProperty TimeRemaining = new FloatReactiveProperty(0f);
        public LongReactiveProperty Score = new LongReactiveProperty(0L);
        public IntReactiveProperty SkillPoint = new IntReactiveProperty(0);
        public IntReactiveProperty FeverPoint = new IntReactiveProperty(0);
        public IntReactiveProperty CurrentTsumNum = new IntReactiveProperty(0);
        public IntReactiveProperty CurrentCombo = new IntReactiveProperty(0);
        public BoolReactiveProperty IsInGame = new BoolReactiveProperty(false);
        public BoolReactiveProperty IsFever = new BoolReactiveProperty(false);
        public BoolReactiveProperty IsSkillActivationReady = new BoolReactiveProperty(false);

        private int _maxSkillPoint = 100;
        private int _maxFeverPoint = 100;

        public enum GameState
        {
            None,
            Ready,
            Playing,
            GameOver
        }

        public GameModel(GameData gameData)
        {
            _maxSkillPoint = gameData.MaxSkillPoint;
            _maxFeverPoint = gameData.MaxFeverPoint;
        }
    }
}