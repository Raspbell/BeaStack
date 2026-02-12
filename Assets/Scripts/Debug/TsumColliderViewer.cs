using UnityEngine;

namespace GameDebug
{
    public class TsumColliderViewer : MonoBehaviour
    {
        [SerializeField]
        private float _radius = 0.38f;

        public void SetRadius(float radius)
        {
            _radius = radius;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;

            int segments = 32;
            float angleStep = 360f / segments;
            Vector3 prevPosition = transform.position + new Vector3(_radius, 0f, 0f);

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 currentPosition = transform.position + new Vector3(Mathf.Cos(angle) * _radius, Mathf.Sin(angle) * _radius, 0f);
                Gizmos.DrawLine(prevPosition, currentPosition);
                prevPosition = currentPosition;
            }
        }
    }
}