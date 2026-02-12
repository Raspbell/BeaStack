using UnityEngine;
using UniRx;
using System;
using UnityEngine.InputSystem;

namespace View
{
    public class InputEventHandler : MonoBehaviour
    {
        private readonly Subject<Unit> _onInputStart = new Subject<Unit>();
        public IObservable<Unit> OnInputStart => _onInputStart;

        private readonly Subject<Unit> _onInputEnd = new Subject<Unit>();
        public IObservable<Unit> OnInputEnd => _onInputEnd;

        private bool _isInputActive = false;
        public bool IsInputActive => _isInputActive;

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                _onInputStart.OnNext(Unit.Default);
                _isInputActive = true;
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                _onInputEnd.OnNext(Unit.Default);
                _isInputActive = false;
            }
        }

        public TsumView SelectTsum()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            TsumView tsum = hit.collider?.GetComponent<TsumView>();
            return tsum;
        }
    }
}