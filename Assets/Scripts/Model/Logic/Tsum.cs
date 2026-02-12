using Model.Interface;
using UnityEngine;

namespace Model.Logic
{
    public class Tsum
    {
        private int _tsumID;
        public int TsumID => _tsumID;

        private TsumType _tsumType = TsumType.Normal;
        public TsumType Type => _tsumType;

        private int _physicsIndex;
        public int PhysicsIndex => _physicsIndex;

        private bool _isConnected;
        public bool IsConnected => _isConnected;

        private bool _isDeleting;
        public bool IsDeleting => _isDeleting;

        private ITsumView _tsumView;
        public ITsumView TsumView => _tsumView;

        public Tsum(int tsumID, ITsumView tsumView, int physicsIndex)
        {
            _tsumID = tsumID;
            _tsumView = tsumView;
            _physicsIndex = physicsIndex;
            _isConnected = false;
            _isDeleting = false;
        }

        public void SetHighlight(bool isActive)
        {
            _tsumView.SetHighlight(isActive);
        }

        public void SetDeleting()
        {
            _isDeleting = true;
            _tsumView.SetDeleting();
        }

        public void SetType(TsumType type)
        {
            _tsumType = type;
        }

        public void OnSelected()
        {
            _isConnected = true;
            _tsumView.OnSelected();
        }

        public void OnUnselected()
        {
            _isConnected = false;
            _tsumView.OnUnselected();
        }

        public void DeleteTsum()
        {
            _tsumView.DeleteTsum();
        }
    }
}