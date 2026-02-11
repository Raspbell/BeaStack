using UnityEngine;

namespace Model.Data
{
    [CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData")]
    public class GameData : ScriptableObject
    {
        [Header("最大ツム数")] public int MaxTsumCount = 100;
        [Header("初期配置するツムの数")] public int InitialTsumCount = 1;
        [Header("スキルポイントの最大値")] public int MaxSkillPoint = 100;
        [Header("フィーバーポイントの最大値")] public int MaxFeverPoint = 30;

        [Header("デッドラインを超えてからゲームオーバーになるまでの猶予（秒）")] public float GameOverGraceTime = 3.0f;
        [Header("最初の落下間隔（秒）")] public float InitialSpawnInterval = 2.0f;
        [Header("最小の落下間隔（秒）")] public float MinSpawnInterval = 0.5f;
        [Header("1秒あたりの間隔短縮量")] public float SpawnIntervalDecrement = 0.1f;
        [Header("落下してくるツムの最大レベル")] public int MaxSpawnTsumLevelIndex = 3;

        [Header("ツム接続可能距離")] public float TsumConnectDistance = 1.5f;
        [Header("消去に必要な最小チェイン数")] public int MinChainCountToClear = 3;
        [Header("チェインの消去間隔")] public float ChainClearInterval = 0.1f;
        [Header("フィーバーゲージ獲得量（1ツムあたり）")] public int FeverGainPerTsum = 1;
        [Header("フィーバーゲージの時間減少量（1秒あたり）")] public float FeverDecreasePerSecond = 10f;
        [Header("フィーバー時の時間加算量")] public float TimeGainWhenFever = 5f;
    }
}