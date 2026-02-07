using Model.Interface;
using UnityEngine;

namespace Model.Logic
{
    public class TsumEntity
    {
        private int _tsumID;
        public int TsumID => _tsumID;

        private bool _isConnected;
        public bool IsConnected => _isConnected;

        private bool _isDeleting;
        public bool IsDeleting => _isDeleting;

        private ITsumView _tsumView;
        public ITsumView TsumView => _tsumView;

        public Vector3 Position => _tsumView.Position;

        public TsumEntity(int tsumID, ITsumView tsumView)
        {
            _tsumID = tsumID;
            _tsumView = tsumView;
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