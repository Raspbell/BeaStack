using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Model.Interface;

namespace Model.Logic
{
    public class PuzzleManager
    {
        private GameModel _gameModel;
        private PuzzleRule _puzzleRule;
        private ChainManager _chainManager;
        private GameData _gameData;
        private TsumData _tsumData;

        // インターフェースに変更
        private ITsumSpawner _tsumSpawner;
        private IChainLineHandler _chainLineHandler;

        // リストの中身を ITsum に変更
        private readonly List<ITsum> _allTsums = new List<ITsum>();
        public List<ITsum> AllTsums => _allTsums;

        private ITsum _lastSelectedTsum;
        public ITsum LastSelectedTsum => _lastSelectedTsum;

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

        public void OnSelectionStart(ITsum firstTsum)
        {
            _currentSelectingTsumID = firstTsum.TsumID;
            _isSelectionActive = true;
            _lastSelectedTsum = firstTsum;
        }

        public void OnSelectionEnd()
        {
            _currentSelectingTsumID = -1;
            _isSelectionActive = false;
            _lastSelectedTsum = null;

            TurnOffAllHighlights();

            // ITsum のリストとして取得
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

        public void RegisterTsum(ITsum tsum)
        {
            _allTsums.Add(tsum);
        }

        public bool CanConnectTsums(ITsum tsum)
        {
            if (_currentSelectingTsumID != tsum.TsumID)
            {
                return false;
            }
            if (_lastSelectedTsum == null)
            {
                return false;
            }
            ITsum lastTsum = _chainManager.CurrentChain.Last();

            return _puzzleRule.CanConnectTsums(lastTsum.Position, tsum.Position);
        }

        public void UpdateSelectableHighlight()
        {
            if (!_isSelectionActive || _chainManager.CurrentChain.Count == 0)
            {
                TurnOffAllHighlights();
                return;
            }

            TurnOffAllHighlights();

            ITsum startNode = _chainManager.CurrentChain.Last();
            HashSet<ITsum> visited = new HashSet<ITsum>(_chainManager.CurrentChain);
            Queue<ITsum> queue = new Queue<ITsum>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                ITsum current = queue.Dequeue();

                foreach (var neighbor in _allTsums)
                {
                    if (visited.Contains(neighbor))
                    {
                        continue;
                    }

                    if (neighbor.TsumID == _currentSelectingTsumID &&
                        !neighbor.IsDeleting &&
                        _puzzleRule.CanConnectTsums(current.Position, neighbor.Position))
                    {
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                        neighbor.HighlightTsum(true);
                    }
                }
            }
        }

        public void TurnOffAllHighlights()
        {
            foreach (var tsum in _allTsums)
            {
                if (tsum != null)
                {
                    tsum.HighlightTsum(false);
                }
            }
        }

        public List<ITsum> FindSelectableTsums(ITsum lastSelectedTsum)
        {
            List<ITsum> selectableTsums = new List<ITsum>();
            foreach (ITsum tsum in _allTsums)
            {
                if (tsum == lastSelectedTsum)
                {
                    continue;
                }

                if (_puzzleRule.CanConnectTsums(lastSelectedTsum.Position, tsum.Position))
                {
                    selectableTsums.Add(tsum);
                }
            }
            return selectableTsums;
        }

        public async UniTask ResolveChain(List<ITsum> chainToResolve)
        {
            if (chainToResolve.Count == 0)
            {
                return;
            }

            int currentTsumID = chainToResolve[0].TsumID;
            ITsum lastTsum = chainToResolve.Last();
            Vector3 evolvePosition = lastTsum.Position;

            foreach (var tsum in chainToResolve)
            {
                if (tsum != null)
                {
                    tsum.SetDeleting();
                }
            }

            for (int i = 0; i < chainToResolve.Count; i++)
            {
                ITsum tsum = chainToResolve[i];
                if (tsum != null && tsum.GameObject != null)
                {
                    tsum.PlayDeletedAnimation();
                }
                await UniTask.Delay((int)(_gameData.ChainClearInterval * 1000));
            }

            for (int i = 0; i < chainToResolve.Count; i++)
            {
                ITsum tsumToDelete = chainToResolve[i];
                if (tsumToDelete == null)
                {
                    continue;
                }

                _allTsums.Remove(tsumToDelete);
                tsumToDelete.DeleteTsum();
            }

            // 進化処理
            int nextTsumID = GetNextLevelTsumID(currentTsumID);

            if (nextTsumID != -1)
            {
                ITsum newTsum = _tsumSpawner.SpawnTsumAt(nextTsumID, evolvePosition);
                if (newTsum != null)
                {
                    RegisterTsum(newTsum);
                    newTsum.PlaySelectedAnimation();
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