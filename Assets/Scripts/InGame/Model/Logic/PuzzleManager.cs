using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using InGame.Model.Interface;
using InGame.Model.Data;

namespace InGame.Model.Logic
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
        private SkillManager _skillManager;

        private GameData _gameData;
        private TsumData _tsumData;

        private ITsumSpawner _tsumSpawner;
        private IChainLineHandler _chainLineHandler;
        private ISEView _seView;

        private int _lockedChainID = -1;

        private List<Tsum> _allTsumEntity = new List<Tsum>();
        public List<Tsum> AllTsums => _allTsumEntity;

        private Dictionary<ITsumView, Tsum> _viewToEntityMap = new Dictionary<ITsumView, Tsum>();
        public Dictionary<ITsumView, Tsum> ViewToEntityMap => _viewToEntityMap;

        private Tsum _lastSelectedTsum;
        public Tsum LastSelectedTsum => _lastSelectedTsum;

        private int _currentSelectingTsumID = -1;
        public int CurrentSelectingTsumID => _currentSelectingTsumID;

        private bool _isSelectionActive = false;
        public bool IsSelectionActive => _isSelectionActive;

        // public readonly Subject<Unit> OnBombExploded = new Subject<Unit>();

        // UpdateSelectableHighlight用のキャッシュ（GCされないように）
        private HashSet<Tsum> _visitedCache = new HashSet<Tsum>();
        private Queue<Tsum> _queueCache = new Queue<Tsum>();

        public PuzzleManager
        (
            GameModel gameModel,
            PuzzleRule puzzleRule,
            GameData gameData,
            TsumData tsumData,
            ChainManager chainManager,
            TsumPhysicsManager tsumPhysics,
            ITsumSpawner tsumSpawner,
            IChainLineHandler chainLineHandler,
            ISEView seView,
            SkillManager skillManager
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
            _seView = seView;
            _skillManager = skillManager;
        }

        // ツムを生成
        public Tsum CreateTsum(int tsumID, Vector2 spawnPosition)
        {
            int physicsIndex = _tsumPhysicsManager.AllocatePhysicsIndex();
            if (physicsIndex == -1)
            {
                return null;
            }

            // TsumDataからデータを取得 (TsumData更新によりWildcardも取得可能)
            var tsumComponent = _tsumData.GetTsumComponentById(tsumID);
            if (tsumComponent == null)
            {
                Debug.LogError($"ID: {tsumID} のツムデータが見つかりません。");
                _tsumPhysicsManager.ReleasePhysicsIndex(physicsIndex);
                return null;
            }

            float radius = tsumComponent.Radius * _tsumData.BaseScale;
            _tsumPhysicsManager.InitializeTsum(physicsIndex, spawnPosition, radius);

            // GameObjectの生成
            ITsumView tsumView = _tsumSpawner.SpawnTsum(
                tsumID,
                spawnPosition,
                tsumComponent.Radius,
                tsumComponent.Sprite,
                tsumComponent.Color,
                tsumComponent.HighlightColor
            );

            if (tsumView == null)
            {
                _tsumPhysicsManager.ReleasePhysicsIndex(physicsIndex);
                return null;
            }

            var tsum = new Tsum(tsumID, tsumView, physicsIndex);

            if (_tsumData.WildcardTsumEntity != null && tsumID == _tsumData.WildcardTsumEntity.ID)
            {
                tsum.SetType(TsumType.Wildcard);
            }

            _allTsumEntity.Add(tsum);
            _viewToEntityMap[tsumView] = tsum;
            return tsum;
        }

        // 選択開始時
        public void OnSelectionStart(Tsum firstTsum)
        {
            _lockedChainID = -1;
            if (firstTsum.Type == TsumType.Normal)
            {
                _lockedChainID = firstTsum.TsumID;
            }

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

        public bool CanConnectTsums(Tsum tsum)
        {
            bool isWildcardInvolved = tsum.Type == TsumType.Wildcard ||
                                      (_lastSelectedTsum != null && _lastSelectedTsum.Type == TsumType.Wildcard);

            if (!isWildcardInvolved)
            {
                if (_currentSelectingTsumID != tsum.TsumID)
                {
                    return false;
                }
            }

            if (_lastSelectedTsum == null)
            {
                return false;
            }

            Tsum lastTsum = _chainManager.CurrentChain.Last();

            bool isIdMatch = CheckConnectionIdLogic(lastTsum, tsum);
            if (!isIdMatch)
            {
                return false;
            }

            Vector2 lastTsumPosition = _tsumPhysicsManager.GetTsumPosition(lastTsum.PhysicsIndex);
            Vector2 tsumPosition = _tsumPhysicsManager.GetTsumPosition(tsum.PhysicsIndex);

            return _puzzleRule.CanConnectTsums(lastTsumPosition, tsumPosition);
        }

        private bool CheckConnectionIdLogic(Tsum lastTsum, Tsum currentTsum)
        {
            bool isLastWild = lastTsum.Type == TsumType.Wildcard;
            bool isCurrentWild = currentTsum.Type == TsumType.Wildcard;
            int targetId = currentTsum.TsumID;

            if (isLastWild || isCurrentWild)
            {
                if (_lockedChainID == -1)
                {
                    if (!isCurrentWild) _lockedChainID = targetId;
                    return true;
                }
                else
                {
                    return isCurrentWild || (targetId == _lockedChainID);
                }
            }

            if (_lockedChainID == -1)
            {
                if (lastTsum.TsumID == currentTsum.TsumID)
                {
                    _lockedChainID = currentTsum.TsumID;
                    return true;
                }
                return false;
            }
            return targetId == _lockedChainID;
        }

        public void ActivateWildcardSkill(ITsumView targetView)
        {
            if (!_viewToEntityMap.TryGetValue(targetView, out Tsum targetTsum))
            {
                return;
            }

            var wildcardData = _tsumData.WildcardTsumEntity;
            if (wildcardData == null)
            {
                Debug.LogError("WildcardTsumEntity is null in TsumData.");
                return;
            }

            int targetPhysicsIndex = targetTsum.PhysicsIndex;
            Vector2 position = _tsumPhysicsManager.GetTsumPosition(targetPhysicsIndex);

            RemoveTsumInstant(targetTsum);
            Tsum newWildcard = CreateTsum(wildcardData.ID, position);
            newWildcard.TsumView.PlaySelectedAnimation(false);

            _skillManager.CompleteSkillActivation();
            _seView.PlaySkillUsedSound();
        }

        private void RemoveTsumInstant(Tsum tsum)
        {
            if (tsum == null) return;

            if (tsum.TsumView != null)
            {
                _viewToEntityMap.Remove(tsum.TsumView);
            }

            _tsumPhysicsManager.ReleasePhysicsIndex(tsum.PhysicsIndex);
            _allTsumEntity.Remove(tsum);
            tsum.DeleteTsum();
        }

        public void UpdateSelection(ITsumView touchedTsumView)
        {
            if (touchedTsumView == null) return;
            if (!_viewToEntityMap.TryGetValue(touchedTsumView, out Tsum entity)) return;

            if (!IsSelectionActive)
            {
                OnSelectionStart(entity);
                UpdateSelectableHighlight();
                return;
            }

            if (entity.IsDeleting) return;

            bool isWildcardInvolved = entity.Type == TsumType.Wildcard ||
                                      (_lastSelectedTsum != null && _lastSelectedTsum.Type == TsumType.Wildcard);

            if (!isWildcardInvolved && CurrentSelectingTsumID != entity.TsumID)
            {
                return;
            }

            var chain = _chainManager.CurrentChain;
            if (chain.Count >= 2 && chain[chain.Count - 2] == entity)
            {
                Tsum lastTsum = chain.Last();
                _chainManager.RemoveLastTsumFromChain();
                lastTsum.OnUnselected();
                RecalculateLockedID();
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

        private void RecalculateLockedID()
        {
            var chain = _chainManager.CurrentChain;
            int foundId = -1;
            foreach (var tsum in chain)
            {
                if (tsum.Type == TsumType.Normal)
                {
                    if (foundId == -1) foundId = tsum.TsumID;
                    else if (foundId != tsum.TsumID) return;
                }
            }
            _lockedChainID = foundId;
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
            _visitedCache.Clear();
            foreach (var tsum in _chainManager.CurrentChain)
            {
                _visitedCache.Add(tsum);
            }
            _queueCache.Clear();
            _queueCache.Enqueue(startNode);

            while (_queueCache.Count > 0)
            {
                Tsum current = _queueCache.Dequeue();
                Vector2 currentPosition = _tsumPhysicsManager.GetTsumPosition(current.PhysicsIndex);

                foreach (var neighborEntity in _allTsumEntity)
                {
                    if (_visitedCache.Contains(neighborEntity)) continue;

                    Vector2 neighborPosition = _tsumPhysicsManager.GetTsumPosition(neighborEntity.PhysicsIndex);

                    if (!neighborEntity.IsDeleting &&
                        _puzzleRule.CanConnectTsums(currentPosition, neighborPosition))
                    {
                        bool isIdOK = false;
                        if (neighborEntity.Type == TsumType.Wildcard || current.Type == TsumType.Wildcard)
                        {
                            if (_lockedChainID == -1 || neighborEntity.Type == TsumType.Wildcard || neighborEntity.TsumID == _lockedChainID)
                            {
                                isIdOK = true;
                            }
                        }
                        else
                        {
                            if (_lockedChainID == -1) isIdOK = (neighborEntity.TsumID == current.TsumID);
                            else isIdOK = (neighborEntity.TsumID == _lockedChainID);
                        }

                        if (isIdOK)
                        {
                            _visitedCache.Add(neighborEntity);
                            _queueCache.Enqueue(neighborEntity);
                            neighborEntity.SetHighlight(true);
                        }
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

        public async UniTask ResolveChain(List<Tsum> chainToResolve)
        {
            if (chainToResolve.Count == 0) return;

            HashSet<Tsum> bombTargets = new HashSet<Tsum>();
            HashSet<Tsum> chainSet = new HashSet<Tsum>(chainToResolve);
            List<int> nearbyIndices = new List<int>();

            foreach (var tsum in chainToResolve)
            {
                if (tsum.Type == TsumType.Wildcard)
                {
                    Vector2 pos = _tsumPhysicsManager.GetTsumPosition(tsum.PhysicsIndex);
                    _tsumPhysicsManager.GetTsumsInRadius(pos, _tsumData.WildcardTsumEntity.ExplosionRadius, nearbyIndices);

                    foreach (int idx in nearbyIndices)
                    {
                        Tsum neighbor = FindTsumByPhysicsIndex(idx);
                        if (neighbor != null && !neighbor.IsDeleting && !chainSet.Contains(neighbor))
                        {
                            bombTargets.Add(neighbor);
                        }
                    }
                }
            }

            Tsum lastTsum = chainToResolve.Last();
            Vector2 evolvePosition = _tsumPhysicsManager.GetTsumPosition(lastTsum.PhysicsIndex);
            int currentTsumID = chainToResolve[0].TsumID;

            var allTargets = chainSet.Concat(bombTargets);
            foreach (var tsumEntity in allTargets)
            {
                if (tsumEntity != null)
                {
                    tsumEntity.SetDeleting();
                    _tsumPhysicsManager.SetStatic(tsumEntity.PhysicsIndex, true);
                }
            }

            foreach (var tsumEntity in chainToResolve)
            {
                if (tsumEntity != null && tsumEntity.TsumView != null)
                {
                    tsumEntity.TsumView.PlayDeletedAnimation(true);
                    _gameModel.SkillPoint.Value++;
                }
                await UniTask.Delay((int)(_gameData.ChainClearInterval * 1000));
            }

            if (bombTargets.Count > 0)
            {
                _seView.PlayExplosionSound();
                foreach (var tsumEntity in bombTargets)
                {
                    if (tsumEntity != null && tsumEntity.TsumView != null)
                    {
                        tsumEntity.TsumView.PlayDeletedAnimation(false);
                    }
                }
            }

            List<Tsum> finalDeleteList = allTargets.ToList();
            foreach (var tsumEntityToDelete in finalDeleteList)
            {
                if (tsumEntityToDelete == null) continue;

                if (tsumEntityToDelete.TsumView != null)
                {
                    _viewToEntityMap.Remove(tsumEntityToDelete.TsumView);
                }

                _tsumPhysicsManager.ReleasePhysicsIndex(tsumEntityToDelete.PhysicsIndex);
                _allTsumEntity.Remove(tsumEntityToDelete);
                tsumEntityToDelete.DeleteTsum();
            }

            if (chainToResolve[0].Type == TsumType.Normal)
            {
                int nextTsumID = GetNextLevelTsumID(currentTsumID);
                if (nextTsumID != -1)
                {
                    float startRadius = _tsumData.GetTsumComponentById(currentTsumID).Radius * _tsumData.BaseScale;
                    float targetRadius = _tsumData.GetTsumComponentById(nextTsumID).Radius * _tsumData.BaseScale;

                    CreateTsum(nextTsumID, evolvePosition);
                    Tsum newTsum = _allTsumEntity.Last();

                    if (newTsum != null && newTsum.TsumView != null)
                    {
                        int physicsIndex = newTsum.PhysicsIndex;
                        _tsumPhysicsManager.SetRadius(physicsIndex, startRadius);

                        float currentRadius = startRadius;
                        DOTween.To(() => currentRadius,
                        x =>
                        {
                            currentRadius = x;
                            if (newTsum != null && !newTsum.IsDeleting)
                            {
                                _tsumPhysicsManager.SetRadius(newTsum.PhysicsIndex, currentRadius);
                            }
                        },
                        targetRadius, 0.5f).SetEase(Ease.OutQuad);

                        newTsum.TsumView.PlaySelectedAnimation(false);
                    }
                }
            }

            long score = _puzzleRule.CalculateScore(chainToResolve, bombTargets.ToList(), _tsumData);
            _gameModel.Score.Value += score;
        }

        private Tsum FindTsumByPhysicsIndex(int index)
        {
            return _allTsumEntity.FirstOrDefault(t => t.PhysicsIndex == index);
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