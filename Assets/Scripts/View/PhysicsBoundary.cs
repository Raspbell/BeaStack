using System;
using UnityEngine;

namespace View
{
    public class PhysicsBoundary : MonoBehaviour
    {
        [SerializeField] private float _leftX = -3f;
        [SerializeField] private float _rightX = 3f;
        [SerializeField] private float _bottomY = -5f;
        [SerializeField] private float _topY = 5f;
        [SerializeField] private float _deadLineY = 0f;

        public float LeftX => _leftX;
        public float RightX => _rightX;
        public float BottomY => _bottomY;
        public float TopY => _topY;
        public float DeadLineY => _deadLineY;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Vector3 topLeft = new Vector3(_leftX, _topY, 0f);
            Vector3 topRight = new Vector3(_rightX, _topY, 0f);
            Vector3 bottomLeft = new Vector3(_leftX, _bottomY, 0f);
            Vector3 bottomRight = new Vector3(_rightX, _bottomY, 0f);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            Gizmos.color = Color.red;
            Vector3 gameOverLeft = new Vector3(_leftX, _deadLineY, 0f);
            Vector3 gameOverRight = new Vector3(_rightX, _deadLineY, 0f);
            Gizmos.DrawLine(gameOverLeft, gameOverRight);
        }
#endif
    }
}