using UnityEngine;

namespace Model.Interface
{
    public interface ITsum
    {
        int TsumID { get; }
        Vector3 Position { get; }
        GameObject GameObject { get; }
        bool IsDeleting { get; }

        void SetDeleting();
        void PlayDeletedAnimation();
        void PlaySelectedAnimation();
        void DeleteTsum();

        void HighlightTsum(bool isActive);
        void OnSelected();
        void OnUnselected();
    }
}