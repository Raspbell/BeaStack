using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData")]
public class GameData : ScriptableObject
{
    [Header("画面上の最大ツム数")] public int MaxTsumNum = 45;
    [Header("スキルポイントの最大値")] public int MaxSkillPoint = 100;
    [Header("フィーバーポイントの最大値")] public int MaxFeverPoint = 30;
    [Header("ゲームの最大時間")] public float MaxTime = 60f;

    [Header("ツム接続可能距離")] public float TsumConnectDistance = 1.5f;
    [Header("消去に必要な最小チェイン数")] public int MinChainCountToClear = 3;
    [Header("チェインの消去間隔")] public float ChainClearInterval = 0.1f;
    [Header("フィーバーゲージ獲得量（1ツムあたり）")] public int FeverGainPerTsum = 1;
    [Header("フィーバーゲージの時間減少量（1秒あたり）")] public float FeverDecreasePerSecond = 10f;
    [Header("フィーバー時の時間加算量")] public float TimeGainWhenFever = 5f;
}