using System.Collections.Generic;

namespace InGame.Model.Interface
{
    public interface IChainLineHandler
    {
        void UpdateLine(IEnumerable<ITsumView> chainTsums);
        void FixLineAndFadeOut(float duration);
    }
}