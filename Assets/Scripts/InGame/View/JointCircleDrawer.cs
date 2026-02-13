using System.Collections.Generic;
using UnityEngine;

namespace InGame.View
{
    public class JointCircleDrawer : MonoBehaviour
    {
        [SerializeField] private GameObject _jointPrefab;

        private LineRenderer _lineRenderer;
        private List<GameObject> _joints = new List<GameObject>();

        private void Start()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            DrawJoints();
        }

        private void Update()
        {
            DrawJoints();
        }

        private void DrawJoints()
        {
            _joints.ForEach(joint => Destroy(joint));
            _joints.Clear();

            int count = _lineRenderer.positionCount;
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = _lineRenderer.GetPosition(i);
                GameObject joint = Instantiate(_jointPrefab, pos, Quaternion.identity);
                joint.transform.SetParent(this.transform);

                // float width = _lineRenderer.startWidth;
                // joint.transform.localScale = new Vector3(width, width, width);
                _joints.Add(joint);
            }
        }
    }
}