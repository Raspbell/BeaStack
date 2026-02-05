using UniRx;

public class GameModel
{
    public ReactiveProperty<GameState> CurrentGameState = new ReactiveProperty<GameState>(GameState.None);
    public FloatReactiveProperty TimeRemaining = new FloatReactiveProperty(0f);
    public IntReactiveProperty Score = new IntReactiveProperty(0);
    public IntReactiveProperty SkillPoint = new IntReactiveProperty(0);
    public IntReactiveProperty FeverPoint = new IntReactiveProperty(0);
    public IntReactiveProperty CurrentTsumNum = new IntReactiveProperty(0);
    public IntReactiveProperty CurrentCombo = new IntReactiveProperty(0);
    public BoolReactiveProperty IsInGame = new BoolReactiveProperty(false);
    public BoolReactiveProperty IsFever = new BoolReactiveProperty(false);

    private int _maxTsumNum = 45;
    private int _maxSkillPoint = 100;
    private int _maxFeverPoint = 100;
    private float _maxTime = 60f;

    private int _feverGainPerTsum = 5;
    private float _timeGainWhenFever = 5f;


    public enum GameState
    {
        None,
        Ready,
        Playing,
        GameOver
    }

    public GameModel(GameData gameData)
    {
        _maxTsumNum = gameData.MaxTsumNum;
        _maxSkillPoint = gameData.MaxSkillPoint;
        _maxFeverPoint = gameData.MaxFeverPoint;
        _maxTime = gameData.MaxTime;

        TimeRemaining.Value = _maxTime;
    }
}