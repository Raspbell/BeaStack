using UnityEngine;

namespace InGame.Model.Interface
{
    public interface ITsumView
    {
        // 線を引くために使う位置情報 (物理計算には使わない)
        Vector3 Position { get; }

        void SetDeleting();
        void PlayDeletedAnimation(bool playSound);
        void PlaySelectedAnimation(bool playSound);
        void DeleteTsum();
        void SetHighlight(bool isActive);
        void UpdateTransform(Vector2 position, float rotation);
        void ChangeVisual(Sprite sprite, Color color, Color highlightColor);

        void OnSelected();
        void OnUnselected();
    }
}