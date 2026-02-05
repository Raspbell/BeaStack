using UnityEngine;
using UniRx;
using TMPro;
using System;

public class ReadyAnimationEvent : MonoBehaviour
{
    private readonly Subject<Unit> _onReady = new Subject<Unit>();
    public IObservable<Unit> OnReady => _onReady;

    public void UpdateReadyToGo(string text)
    {
        var textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = text;
            _onReady.OnNext(Unit.Default);
        }
    }
}
