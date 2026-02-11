using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Model.Interface;
using Model.Data;

namespace Model.Logic
{
    /// <summary>
    /// パズルロジック管理
    /// </summary>
    public class PuzzleManager
    {
        private GameModel _gameModel;
        private PuzzleRule _puzzleRule;
        private ChainManager _chainManager;
        private TsumPhysicsManager _tsumPhysicsManager;

        private GameData _gameData;
        private TsumData _tsumData;

        private ITsumSpawner _tsumSpawner;
        private IChainLineHandler _chainLineHandler;

        private readonly List<Tsum> _allTsumEntity = new List<Tsum>();
        public List<Tsum> AllTsums => _allTsumEntity;

        private readonly Dictionary<ITsumView, Tsum> _viewToEntityMap = new Dictionary<ITsumView, Tsum>();
        public Dictionary<ITsumView, Tsum> ViewToEntityMap => _viewToEntityMap;

        private Tsum _lastSelectedTsum;
        public Tsum LastSelectedTsum => _lastSelectedTsum;

        private int _currentSelectingTsumID = -1;
        public int CurrentSelectingTsumID
        {
            get { return _currentSelectingTsumID; }
            set { _currentSelectingTsumID = value; }
        }

        private bool _isSelectionActive = false;
        public bool IsSelectionActive
        {
            get { return _isSelectionActive; }
            set { _isSelectionActive = value; }
        }


        public PuzzleManager
        (
            GameModel gameModel,
            PuzzleRule puzzleRule,
            GameData gameData,
            TsumData tsumData,
            ChainManager chainManager,
            TsumPhysicsManager tsumPhysics,
            ITsumSpawner tsumSpawner,
            IChainLineHandler chainLineHandler
        )
        {
            _gameModel = gameModel;
            _puzzleRule = puzzleRule;
            _gameData = gameData;
            _tsumData = tsumData;
            _chainManager = chainManager;
            _tsumPhysicsManager = tsumPhysics;
            _tsumSpawner = tsumSpawner;
            _chainLineHandler = chainLineHandler;
        }

        // ツムを生成
        public void CreateTsum(int tsumID, Vector2 spawnPosition)
        {
            int physicsIndex = _tsumPhysicsManager.AllocatePhysicsIndex();
            if (physicsIndex == -1)
            {
                return;
            }

            float radius = _tsumData.GetTsumComponentById(tsumID).Radius;
            _tsumPhysicsManager.InitializeTsum(physicsIndex, spawnPosition, radius);

            // GameObjectの生成
            ITsumView tsumView = _tsumSpawner.SpawnTsum(tsumID, spawnPosition);

            if (tsumView == null)
            {
                _tsumPhysicsManager.ReleasePhysicsIndex(physicsIndex);
                return;
            }

            var tsum = new Tsum(tsumID, tsumView, physicsIndex);

            _allTsumEntity.Add(tsum);
            _viewToEntityMap[tsumView] = tsum;
        }

        // 選択開始時
        public void OnSelectionStart(Tsum firstTsum)
        {
            _currentSelectingTsumID = firstTsum.TsumID;
            _isSelectionActive = true;
            _lastSelectedTsum = firstTsum;
            _chainManager.AddTsumToChain(firstTsum);
            firstTsum.OnSelected();
            UpdateLine();
        }

        // 選択終了時
        public void OnSelectionEnd()
        {
            _currentSelectingTsumID = -1;
            _isSelectionActive = false;
            _lastSelectedTsum = null;

            TurnOffAllHighlights();

            // チェインを値参照してコピー
            var currentChain = _chainManager.CurrentChain.ToList();
            _chainManager.ClearChain();

            // チェインの長さが十分なら解決処理を非同期で投げる
            if (currentChain.Count >= _puzzleRule.MinChainCountToClear)
            {
                float duration = currentChain.Count * _gameData.ChainClearInterval;
                _chainLineHandler.FixLineAndFadeOut(duration);
                ResolveChain(currentChain).Forget();
            }
            // そうでなければ選択解除
            else
            {
                _chainLineHandler.UpdateLine(null);
                foreach (var tsum in currentChain)
                {
                    tsum.OnUnselected();
                }
            }
        }

        /// <summary>
        /// ツム同士が接続可能かどうか
        /// </summary>
        public bool CanConnectTsums(Tsum tsum)
        {
            if (_currentSelectingTsumID != tsum.TsumID)
            {
                return false;
            }
            if (_lastSelectedTsum == null)
            {
                return false;
            }
            Tsum lastTsum = _chainManager.CurrentChain.Last();
            Vector2 lastTsumPosition = _tsumPhysicsManager.GetTsumPosition(lastTsum.PhysicsIndex);
            Vector2 tsumPosition = _tsumPhysicsManager.GetTsumPosition(tsum.PhysicsIndex);

            return _puzzleRule.CanConnectTsums(lastTsumPosition, tsumPosition);
        }

        /// <summary>
        /// 現在つなげ途中のチェインを更新
        /// </summary>
        public void UpdateSelection(ITsumView touchedTsumView)
        {
            if (touchedTsumView == null)
            {
                return;
            }

            if (!_viewToEntityMap.TryGetValue(touchedTsumView, out Tsum entity))
            {
                return;
            }

            if (!IsSelectionActive)
            {
                OnSelectionStart(entity);
                UpdateSelectableHighlight();
                return;
            }

            if (entity.IsDeleting)
            {
                return;
            }

            if (CurrentSelectingTsumID != entity.TsumID)
            {
                return;
            }

            // 一つ前のツムをタップした場合はチェインから外す (戻す処理)
            var chain = _chainManager.CurrentChain;
            if (chain.Count >= 2 && chain[chain.Count - 2] == entity)
            {
                Tsum lastTsum = chain.Last();
                _chainManager.RemoveLastTsumFromChain();
                lastTsum.OnUnselected();
                UpdateLine();
                return;
            }

            // ここまでたどり着けたらチェインに追加
            if (!chain.Contains(entity) && CanConnectTsums(entity))
            {
                _chainManager.AddTsumToChain(entity);
                entity.OnSelected();
                UpdateLine();
            }
        }

        private void UpdateLine()
        {
            List<ITsumView> viewList = new List<ITsumView>();
            foreach (var entity in _chainManager.CurrentChain)
            {
                viewList.Add(entity.TsumView);
            }
            _chainLineHandler.UpdateLine(viewList);
        }

        public void UpdateSelectableHighlight()
        {
            if (!_isSelectionActive || _chainManager.CurrentChain.Count == 0)
            {
                TurnOffAllHighlights();
                return;
            }

            TurnOffAllHighlights();

            Tsum startNode = _chainManager.CurrentChain.Last();
            HashSet<Tsum> visited = new HashSet<Tsum>(_chainManager.CurrentChain);
            Queue<Tsum> queue = new Queue<Tsum>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                Tsum current = queue.Dequeue();
                Vector2 currentPosition = _tsumPhysicsManager.GetTsumPosition(current.PhysicsIndex);

                foreach (var neighborEntity in _allTsumEntity)
                {
                    if (visited.Contains(neighborEntity))
                    {
                        continue;
                    }

                    Vector2 neighborPosition = _tsumPhysicsManager.GetTsumPosition(neighborEntity.PhysicsIndex);

                    if (neighborEntity.TsumID == _currentSelectingTsumID &&
                        !neighborEntity.IsDeleting &&
                        _puzzleRule.CanConnectTsums(currentPosition, neighborPosition))
                    {
                        visited.Add(neighborEntity);
                        queue.Enqueue(neighborEntity);
                        neighborEntity.SetHighlight(true);
                    }
                }
            }
        }

        public void TurnOffAllHighlights()
        {
            foreach (var tsumEntity in _allTsumEntity)
            {
                if (tsumEntity != null)
                {
                    tsumEntity.SetHighlight(false);
                }
            }
        }

        public List<Tsum> FindSelectableTsums(Tsum lastSelectedTsum)
        {
            Vector2 lastSelectedTsumPosition = _tsumPhysicsManager.GetTsumPosition(lastSelectedTsum.PhysicsIndex);
            List<Tsum> selectableTsums = new List<Tsum>();

            foreach (Tsum tsumEntity in _allTsumEntity)
            {
                if (tsumEntity == lastSelectedTsum)
                {
                    continue;
                }

                Vector2 tsumEntityPosition = _tsumPhysicsManager.GetTsumPosition(tsumEntity.PhysicsIndex);
                if (_puzzleRule.CanConnectTsums(lastSelectedTsumPosition, tsumEntityPosition))
                {
                    selectableTsums.Add(tsumEntity);
                }
            }
            return selectableTsums;
        }

        /// <summary>
        /// チェインを解決
        /// </summary>
        public async UniTask ResolveChain(List<Tsum> chainToResolve)
        {
            if (chainToResolve.Count == 0)
            {
                return;
            }

            int currentTsumID = chainToResolve[0].TsumID;
            Tsum lastTsum = chainToResolve.Last();
            Vector2 evolvePosition = _tsumPhysicsManager.GetTsumPosition(lastTsum.PhysicsIndex);

            // 削除対象としてマーク
            foreach (var tsumEntity in chainToResolve)
            {
                if (tsumEntity != null)
                {
                    tsumEntity.SetDeleting();
                    _tsumPhysicsManager.SetStatic(tsumEntity.PhysicsIndex, true);
                }
            }

            // 間を空けて消去アニメーション再生
            for (int i = 0; i < chainToResolve.Count; i++)
            {
                Tsum tsumEntity = chainToResolve[i];
                ITsumView tsumView = tsumEntity?.TsumView;
                if (tsumEntity != null)
                {
                    tsumView.PlayDeletedAnimation();
                }
                await UniTask.Delay((int)(_gameData.ChainClearInterval * 1000));
            }

            // ツムを削除
            for (int i = 0; i < chainToResolve.Count; i++)
            {
                Tsum tsumEntityToDelete = chainToResolve[i];
                if (tsumEntityToDelete == null)
                {
                    continue;
                }

                if (tsumEntityToDelete.TsumView != null)
                {
                    _viewToEntityMap.Remove(tsumEntityToDelete.TsumView);
                }

                _tsumPhysicsManager.ReleasePhysicsIndex(tsumEntityToDelete.PhysicsIndex);
                _allTsumEntity.Remove(tsumEntityToDelete);
                tsumEntityToDelete.DeleteTsum();
            }

            // 進化後のツムを生成
            int nextTsumID = GetNextLevelTsumID(currentTsumID);
            if (nextTsumID != -1)
            {
                CreateTsum(nextTsumID, evolvePosition);
                Tsum newTsum = _allTsumEntity.Last();
                if (newTsum != null && newTsum.TsumView != null)
                {
                    newTsum.TsumView.PlaySelectedAnimation();
                }
            }

            int score = chainToResolve.Count * 100 * (GetTsumLevel(currentTsumID) + 1);
            _gameModel.Score.Value += score;
        }

        private int GetNextLevelTsumID(int currentID)
        {
            for (int i = 0; i < _tsumData.TsumEntities.Length - 1; i++)
            {
                if (_tsumData.TsumEntities[i].ID == currentID)
                {
                    return _tsumData.TsumEntities[i + 1].ID;
                }
            }
            return -1;
        }

        private int GetTsumLevel(int currentID)
        {
            for (int i = 0; i < _tsumData.TsumEntities.Length; i++)
            {
                if (_tsumData.TsumEntities[i].ID == currentID)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}