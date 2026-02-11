using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Model.Interface;
using Model.Data;

namespace Model.Logic
{
    public class PuzzleManager
    {
        private GameModel _gameModel;
        private PuzzleRule _puzzleRule;
        private ChainManager _chainManager;
        private GameData _gameData;
        private TsumData _tsumData;

        private ITsumSpawner _tsumSpawner;
        private IChainLineHandler _chainLineHandler;

        private readonly List<TsumEntity> _allTsumEntity = new List<TsumEntity>();
        public List<TsumEntity> AllTsums => _allTsumEntity;

        private readonly Dictionary<ITsumView, TsumEntity> _viewToEntityMap = new Dictionary<ITsumView, TsumEntity>();
        public Dictionary<ITsumView, TsumEntity> ViewToEntityMap => _viewToEntityMap;

        private TsumEntity _lastSelectedTsum;
        public TsumEntity LastSelectedTsum => _lastSelectedTsum;

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
            ITsumSpawner tsumSpawner,       // インターフェースで受け取る
            IChainLineHandler chainLineHandler // インターフェースで受け取る
        )
        {
            _gameModel = gameModel;
            _puzzleRule = puzzleRule;
            _gameData = gameData;
            _tsumData = tsumData;
            _chainManager = chainManager;
            _tsumSpawner = tsumSpawner;
            _chainLineHandler = chainLineHandler;
        }

        public void RegisterTsum(int tsumID, ITsumView tsumView)
        {
            if (tsumView == null)
            {
                return;
            }

            var entity = new TsumEntity(tsumID, tsumView);

            _allTsumEntity.Add(entity);
            _viewToEntityMap[tsumView] = entity;
        }

        public void OnSelectionStart(TsumEntity firstTsum)
        {
            _currentSelectingTsumID = firstTsum.TsumID;
            _isSelectionActive = true;
            _lastSelectedTsum = firstTsum;
            _chainManager.AddTsumToChain(firstTsum);
            firstTsum.OnSelected();
            UpdateLine();
        }

        public void OnSelectionEnd()
        {
            _currentSelectingTsumID = -1;
            _isSelectionActive = false;
            _lastSelectedTsum = null;

            TurnOffAllHighlights();

            var currentChain = _chainManager.CurrentChain.ToList();
            _chainManager.ClearChain();

            if (currentChain.Count >= _puzzleRule.MinChainCountToClear)
            {
                float duration = currentChain.Count * _gameData.ChainClearInterval;
                _chainLineHandler.FixLineAndFadeOut(duration);
                ResolveChain(currentChain).Forget();
            }
            else
            {
                _chainLineHandler.UpdateLine(null);
                foreach (var tsum in currentChain)
                {
                    tsum.OnUnselected();
                }
            }
        }

        public bool CanConnectTsums(TsumEntity tsum)
        {
            if (_currentSelectingTsumID != tsum.TsumID)
            {
                return false;
            }
            if (_lastSelectedTsum == null)
            {
                return false;
            }
            TsumEntity lastTsum = _chainManager.CurrentChain.Last();

            return _puzzleRule.CanConnectTsums(lastTsum.Position, tsum.Position);
        }

        public void TryConnectTsum(ITsumView touchedTsumView)
        {
            if (touchedTsumView == null)
            {
                return;
            }

            if (!_viewToEntityMap.TryGetValue(touchedTsumView, out TsumEntity entity))
            {
                return;
            }

            if (!IsSelectionActive)
            {
                OnSelectionStart(entity);
                UpdateSelectableHighlight();
                return;
            }

            if (entity.IsDeleting) return;
            if (CurrentSelectingTsumID != entity.TsumID) return;

            var chain = _chainManager.CurrentChain;
            if (chain.Count >= 2 && chain[chain.Count - 2] == entity)
            {
                var tip = chain.Last();
                _chainManager.RemoveLastTsumFromChain();
                tip.OnUnselected();
                UpdateLine();
                return;
            }

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

            TsumEntity startNode = _chainManager.CurrentChain.Last();
            HashSet<TsumEntity> visited = new HashSet<TsumEntity>(_chainManager.CurrentChain);
            Queue<TsumEntity> queue = new Queue<TsumEntity>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                TsumEntity current = queue.Dequeue();

                foreach (var neighborEntity in _allTsumEntity)
                {
                    if (visited.Contains(neighborEntity))
                    {
                        continue;
                    }

                    if (neighborEntity.TsumID == _currentSelectingTsumID &&
                        !neighborEntity.IsDeleting &&
                        _puzzleRule.CanConnectTsums(current.Position, neighborEntity.Position))
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

        public List<TsumEntity> FindSelectableTsums(TsumEntity lastSelectedTsum)
        {
            List<TsumEntity> selectableTsums = new List<TsumEntity>();
            foreach (TsumEntity tsumEntity in _allTsumEntity)
            {
                if (tsumEntity == lastSelectedTsum)
                {
                    continue;
                }

                if (_puzzleRule.CanConnectTsums(lastSelectedTsum.Position, tsumEntity.Position))
                {
                    selectableTsums.Add(tsumEntity);
                }
            }
            return selectableTsums;
        }

        public async UniTask ResolveChain(List<TsumEntity> chainToResolve)
        {
            if (chainToResolve.Count == 0)
            {
                return;
            }

            int currentTsumID = chainToResolve[0].TsumID;
            TsumEntity lastTsum = chainToResolve.Last();
            Vector3 evolvePosition = lastTsum.Position;

            foreach (var tsumEntity in chainToResolve)
            {
                if (tsumEntity != null)
                {
                    tsumEntity.SetDeleting();
                }
            }

            for (int i = 0; i < chainToResolve.Count; i++)
            {
                TsumEntity tsumEntity = chainToResolve[i];
                ITsumView tsumView = tsumEntity?.TsumView;
                if (tsumEntity != null)
                {
                    tsumView.PlayDeletedAnimation();
                }
                await UniTask.Delay((int)(_gameData.ChainClearInterval * 1000));
            }

            for (int i = 0; i < chainToResolve.Count; i++)
            {
                TsumEntity tsumEntityToDelete = chainToResolve[i];
                if (tsumEntityToDelete == null)
                {
                    continue;
                }

                if (tsumEntityToDelete.TsumView != null)
                {
                    _viewToEntityMap.Remove(tsumEntityToDelete.TsumView);
                }

                _allTsumEntity.Remove(tsumEntityToDelete);
                tsumEntityToDelete.DeleteTsum();
            }

            int nextTsumID = GetNextLevelTsumID(currentTsumID);

            if (nextTsumID != -1)
            {
                ITsumView newTsumView = _tsumSpawner.SpawnTsum(nextTsumID, evolvePosition);
                TsumEntity newTsumEntity = new TsumEntity(nextTsumID, newTsumView);
                if (newTsumView != null)
                {
                    RegisterTsum(nextTsumID, newTsumView);
                    newTsumView.PlaySelectedAnimation();
                }
            }

            int score = chainToResolve.Count * 100 * (GetTsumLevel(currentTsumID) + 1);
            _gameModel.Score.Value += score;
        }

        private int GetNextLevelTsumID(int currentID)
        {
            for (int i = 0; i < _tsumData.TsumEntities.Length - 1; i++)
            {
                if (_tsumData.TsumEntities[i].TsumID == currentID)
                {
                    return _tsumData.TsumEntities[i + 1].TsumID;
                }
            }
            return -1;
        }

        private int GetTsumLevel(int currentID)
        {
            for (int i = 0; i < _tsumData.TsumEntities.Length; i++)
            {
                if (_tsumData.TsumEntities[i].TsumID == currentID)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}