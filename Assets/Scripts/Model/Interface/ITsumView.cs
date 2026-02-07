using UnityEngine;

namespace Model.Interface
{
    public interface ITsumView
    {
        public Vector3 Position { get; }

        void SetDeleting();
        void PlayDeletedAnimation();
        void PlaySelectedAnimation();
        void DeleteTsum();

        void SetHighlight(bool isActive);
        void OnSelected();
        void OnUnselected();
    }
}