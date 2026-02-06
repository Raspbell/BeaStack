using System.Collections.Generic;

namespace Model.Interface
{
    public interface IChainLineHandler
    {
        void UpdateLine(IEnumerable<ITsum> chainTsums);
        void FixLineAndFadeOut(float duration);
    }
}