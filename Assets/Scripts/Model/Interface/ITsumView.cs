using UnityEngine;

namespace Model.Interface
{
    public interface ITsumView
    {
        // 線を引くために使う位置情報 (物理計算には使わない)
        Vector3 Position { get; }

        void SetDeleting();
        void PlayDeletedAnimation();
        void PlaySelectedAnimation();
        void DeleteTsum();
        void SetHighlight(bool isActive);
        void UpdateTransform(Vector2 position, float rotation);

        void OnSelected();
        void OnUnselected();
    }
}