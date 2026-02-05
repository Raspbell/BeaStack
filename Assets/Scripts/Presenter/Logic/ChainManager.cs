using UnityEngine;
using UniRx;
using System.Collections.Generic;
using System.Linq;

public class ChainManager
{
    private PuzzleRule _puzzleRule;

    private readonly List<Tsum> _currentChain = new List<Tsum>();
    public List<Tsum> CurrentChain => _currentChain;

    public ChainManager(PuzzleRule puzzleRule)
    {
        _puzzleRule = puzzleRule;
    }

    public bool AddTsumToChain(Tsum addedTsum)
    {
        if (_currentChain.Count == 0)
        {
            _currentChain.Add(addedTsum);
            return true;
        }

        Tsum lastTsum = _currentChain.Last();

        if (_puzzleRule.CanConnectTsums(lastTsum.transform.position, addedTsum.transform.position))
        {
            _currentChain.Add(addedTsum);
            return true;
        }
        return false;
    }

    public void RemoveLastTsumFromChain()
    {
        if (_currentChain.Count == 0)
        {
            return;
        }
        _currentChain.RemoveAt(_currentChain.Count - 1);
    }

    public void ClearChain()
    {
        _currentChain.Clear();
    }
}