using UnityEngine;

namespace InGame.Model.Data
{
    [CreateAssetMenu(fileName = "GameData", menuName = "Game/GameData")]
    public class GameData : ScriptableObject
    {
        [Header("最大ツム数")] public int MaxTsumCount = 100;
        [Header("削除エフェクト用パーティクルの最大数")] public int MaxDeletedTsumEffectCount = 100;
        [Header("初期配置するツムの数")] public int InitialTsumCount = 1;
        [Header("スキルポイントの最大値")] public int MaxSkillPoint = 100;
        [Header("フィーバーポイントの最大値")] public int MaxFeverPoint = 30;

        [Header("デッドラインを超えてからゲームオーバーになるまでの猶予（秒）")] public float GameOverGraceTime = 3.0f;
        [Header("最初の落下間隔（秒）")] public float InitialSpawnInterval = 2.0f;
        [Header("最小の落下間隔（秒）")] public float MinSpawnInterval = 0.5f;
        [Header("最小の落下間隔にまるまでの時間（秒）")] public float timeToMaxDifficulity = 120f;
        [Header("落下してくるツムの最大レベル")] public int MaxSpawnTsumLevelIndex = 3;

        [Header("ツム接続可能距離")] public float TsumConnectDistance = 1.5f;
        [Header("消去に必要な最小チェイン数")] public int MinChainCountToClear = 3;
        [Header("チェインの消去間隔")] public float ChainClearInterval = 0.1f;

        [Header("チェイン長によるスコアボーナス倍率")]
        public AnimationCurve ChainLengthBonusCurve = AnimationCurve.Linear(3, 1, 20, 5);
        [Header("ツムID(ランク)ごとのスコアボーナス倍率")]
        public AnimationCurve TsumIdRankBonusCurve = AnimationCurve.Linear(0, 1.0f, 10, 3.0f);
        [Header("ワイルドカード巻き込み時のボーナス倍率")]
        public AnimationCurve WildcardInvolvementBonusCurve = AnimationCurve.Linear(0, 0.5f, 10, 2.0f);
        [Header("ワイルドカードのみのチェイン時に適用する仮想ツムID")]
        public int WildcardOnlyVirtualId = 100;
        [Header("爆発で巻き込まれたツムの基礎スコアに対する倍率")]
        public float ExplosionScoreMultiplier = 2f;
    }
}