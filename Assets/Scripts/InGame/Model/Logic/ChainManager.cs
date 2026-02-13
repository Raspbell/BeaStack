using UnityEngine;
using UniRx;
using System.Collections.Generic;
using System.Linq;
using InGame.Model.Interface;

namespace InGame.Model.Logic
{
    public class ChainManager
    {
        private PuzzleRule _puzzleRule;
        private TsumPhysicsManager _tsumPhysicsManager;

        private readonly List<Tsum> _currentChain = new List<Tsum>();
        public List<Tsum> CurrentChain => _currentChain;

        public ChainManager(PuzzleRule puzzleRule, TsumPhysicsManager tsumPhysicsManager)
        {
            _puzzleRule = puzzleRule;
            _tsumPhysicsManager = tsumPhysicsManager;
        }

        public bool AddTsumToChain(Tsum addedTsum)
        {
            if (_currentChain.Count == 0)
            {
                _currentChain.Add(addedTsum);
                return true;
            }

            Tsum lastTsum = _currentChain.Last();
            Vector2 lastTsumPosition = _tsumPhysicsManager.GetTsumPosition(lastTsum.PhysicsIndex);
            Vector2 addedTsumPosition = _tsumPhysicsManager.GetTsumPosition(addedTsum.PhysicsIndex);

            if (_puzzleRule.CanConnectTsums(lastTsumPosition, addedTsumPosition))
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

        public IEnumerable<ITsumView> GetCurrentChainViews()
        {
            return _currentChain.Select(tsumEntity => tsumEntity.TsumView);
        }

        public void ClearChain()
        {
            _currentChain.Clear();
        }
    }
}