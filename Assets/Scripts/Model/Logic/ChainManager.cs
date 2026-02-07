using UniRx;
using System.Collections.Generic;
using System.Linq;
using Model.Interface;

namespace Model.Logic
{
    public class ChainManager
    {
        private PuzzleRule _puzzleRule;

        private readonly List<TsumEntity> _currentChain = new List<TsumEntity>();
        public List<TsumEntity> CurrentChain => _currentChain;

        public ChainManager(PuzzleRule puzzleRule)
        {
            _puzzleRule = puzzleRule;
        }

        public bool AddTsumToChain(TsumEntity addedTsum)
        {
            if (_currentChain.Count == 0)
            {
                _currentChain.Add(addedTsum);
                return true;
            }

            TsumEntity lastTsum = _currentChain.Last();

            if (_puzzleRule.CanConnectTsums(lastTsum.Position, addedTsum.Position))
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