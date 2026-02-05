using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

// ツムを消したりパズルの進行を管理するクラス
public class PuzzleManager
{
    private GameModel _gameModel;
    private PuzzleRule _puzzleRule;
    private ChainManager _chainManager;
    private GameData _gameData;
    private TsumData _tsumData;
    private GameUIView _gameUIView;
    private TsumSpawner _tsumSpawner;
    private ChainLineHandler _chainLineHandler;

    private readonly List<Tsum> _allTsums = new List<Tsum>();
    public List<Tsum> AllTsums => _allTsums;

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
        GameUIView gameUIView,
        ChainManager chainManager,
        TsumSpawner tsumSpawner,
        ChainLineHandler chainLineHandler
    )
    {
        _gameModel = gameModel;
        _puzzleRule = puzzleRule;
        _gameData = gameData;
        _tsumData = tsumData;
        _gameUIView = gameUIView;
        _chainManager = chainManager;
        _tsumSpawner = tsumSpawner;
        _chainLineHandler = chainLineHandler;
    }

    public void OnSelectionStart(Tsum firstTsum)
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

    public void RegisterTsum(Tsum tsum)
    {
        _allTsums.Add(tsum);
    }

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

        return _puzzleRule.CanConnectTsums(lastTsum.transform.position, tsum.transform.position);
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

            foreach (var neighbor in _allTsums)
            {
                if (visited.Contains(neighbor))
                {
                    continue;
                }

                if (neighbor.TsumID == _currentSelectingTsumID &&
                    !neighbor.IsDeleting &&
                    _puzzleRule.CanConnectTsums(current.transform.position, neighbor.transform.position))
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
            if (tsum != null) tsum.HighlightTsum(false);
        }
    }

    public List<Tsum> FindSelectableTsums(Tsum lastSelectedTsum)
    {
        List<Tsum> selectableTsums = new List<Tsum>();
        foreach (Tsum tsum in _allTsums)
        {
            if (tsum == lastSelectedTsum)
                continue;

            if (_puzzleRule.CanConnectTsums(lastSelectedTsum.transform.position, tsum.transform.position))
            {
                selectableTsums.Add(tsum);
            }
        }
        return selectableTsums;
    }

    public async UniTask ResolveChain(List<Tsum> chainToResolve)
    {
        foreach (var tsum in chainToResolve)
        {
            if (tsum != null)
            {
                tsum.SetDeleting();
            }
        }

        for (int i = 0; i < chainToResolve.Count; i++)
        {
            Tsum tsum = chainToResolve[i];
            if (tsum != null && tsum.gameObject != null)
            {
                tsum.PlayDeletedAnimation();
            }
            await UniTask.Delay((int)(_gameData.ChainClearInterval * 1000));
        }

        for (int i = 0; i < chainToResolve.Count; i++)
        {
            Tsum tsumToDelete = chainToResolve[i];
            if (tsumToDelete == null || tsumToDelete.gameObject == null)
            {
                continue;
            }

            _allTsums.Remove(tsumToDelete);
            tsumToDelete.DeleteTsum();

            int randomIdx = UnityEngine.Random.Range(0, _tsumData.TsumEntities.Length);
            int tsumId = _tsumData.TsumEntities[randomIdx].TsumID;
            Tsum newTsum = _tsumSpawner.SpawnTsum(tsumId);
            RegisterTsum(newTsum);
        }

        int score = chainToResolve.Count * 100;
        _gameModel.Score.Value += score;
    }
}