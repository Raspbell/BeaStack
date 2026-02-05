using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ChainLineHandler : MonoBehaviour
{
    [SerializeField] private LineRenderer _linePrefab;

    private LineRenderer _currentLine;
    private IReadOnlyList<Tsum> _trackingTsums;

    private void Awake()
    {
        CreateNewLine();
    }

    private void LateUpdate()
    {
        if (_currentLine == null || _trackingTsums == null || _trackingTsums.Count == 0)
        {
            return;
        }

        if (_currentLine.positionCount != _trackingTsums.Count)
        {
            _currentLine.positionCount = _trackingTsums.Count;
        }

        for (int i = 0; i < _trackingTsums.Count; i++)
        {
            if (_trackingTsums[i] != null)
            {
                Vector3 pos = _trackingTsums[i].transform.position;
                pos.z = -1f;
                _currentLine.SetPosition(i, pos);
            }
        }
    }

    public void UpdateLine(List<Tsum> chainTsums)
    {
        if (_currentLine == null)
        {
            CreateNewLine();
        }

        _trackingTsums = chainTsums;

        if (chainTsums == null || chainTsums.Count == 0)
        {
            _currentLine.positionCount = 0;
            return;
        }

        _currentLine.positionCount = chainTsums.Count;
        for (int i = 0; i < chainTsums.Count; i++)
        {
            Vector3 pos = chainTsums[i].transform.position;
            pos.z = -1f;
            _currentLine.SetPosition(i, pos);
        }
    }

    public void ClearLine()
    {
        _trackingTsums = null;

        if (_currentLine != null)
        {
            _currentLine.positionCount = 0;
        }
    }

    public void FixLineAndFadeOut(float duration)
    {
        if (_currentLine == null)
        {
            return;
        }
        if (_currentLine.positionCount == 0)
        {
            return;
        }

        LineRenderer oldLine = _currentLine;
        _trackingTsums = null;

        CreateNewLine();
        TweenColor(oldLine, duration);
    }

    private void CreateNewLine()
    {
        _currentLine = Instantiate(_linePrefab, transform);
        _currentLine.positionCount = 0;
        _currentLine.transform.localPosition = Vector3.zero;

        _trackingTsums = null;
    }

    private void TweenColor(LineRenderer line, float duration)
    {
        Color startColor = line.startColor;
        Color endColor = line.endColor;

        DOVirtual.Float(1f, 0f, duration, value =>
        {
            if (line == null)
            {
                return;
            }
            startColor.a = value;
            endColor.a = value;
            line.startColor = startColor;
            line.endColor = endColor;
        })
        .OnComplete(() =>
        {
            if (line != null)
            {
                Destroy(line.gameObject);
            }
        });
    }
}