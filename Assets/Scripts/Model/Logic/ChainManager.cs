using UniRx;
using System.Collections.Generic;
using System.Linq;
using Model.Interface;

namespace Model.Logic
{
    public class ChainManager
    {
        private PuzzleRule _puzzleRule;

        private readonly List<ITsum> _currentChain = new List<ITsum>();
        public List<ITsum> CurrentChain => _currentChain;

        public ChainManager(PuzzleRule puzzleRule)
        {
            _puzzleRule = puzzleRule;
        }

        public bool AddTsumToChain(ITsum addedTsum)
        {
            if (_currentChain.Count == 0)
            {
                _currentChain.Add(addedTsum);
                return true;
            }

            ITsum lastTsum = _currentChain.Last();

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

        public void ClearChain()
        {
            _currentChain.Clear();
        }
    }
}