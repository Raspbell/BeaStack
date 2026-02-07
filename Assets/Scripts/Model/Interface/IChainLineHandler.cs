using System.Collections.Generic;

namespace Model.Interface
{
    public interface IChainLineHandler
    {
        void UpdateLine(IEnumerable<ITsumView> chainTsums);
        void FixLineAndFadeOut(float duration);
    }
}