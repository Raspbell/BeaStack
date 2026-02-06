using UnityEngine;
using UniRx;
using System;

namespace View.Logic
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
            if (Input.GetMouseButtonDown(0))
            {
                _onInputStart.OnNext(Unit.Default);
                _isInputActive = true;
            }

            if (Input.GetMouseButtonUp(0))
            {
                _onInputEnd.OnNext(Unit.Default);
                _isInputActive = false;
            }
        }

        public Tsum SelectTsum()
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            Tsum tsum = hit.collider?.GetComponent<Tsum>();
            return tsum;
        }
    }
}