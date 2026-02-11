using UnityEngine;
using System.Collections.Generic; // Dictionaryのために必要

[CreateAssetMenu(fileName = "TsumData", menuName = "Game/TsumData")]
public class TsumData : ScriptableObject
{
    public float BaseScale = 1.7f;
    public TsumComponent[] TsumEntities;

    [System.NonSerialized]
    private Dictionary<int, TsumComponent> _idToEntityMap;

    [System.Serializable]
    public class TsumComponent
    {
        public int ID;
        public string Name;
        public float Radius;
        public int Score;
        public Sprite Sprite;
        public Color Color;
        public Color HighlightColor;
    }

    public TsumComponent GetTsumComponentById(int tsumId)
    {
        if (_idToEntityMap == null)
        {
            InitializeLookupDict();
        }

        if (_idToEntityMap.TryGetValue(tsumId, out var component))
        {
            return component;
        }

        return null;
    }

    private void InitializeLookupDict()
    {
        _idToEntityMap = new Dictionary<int, TsumComponent>();

        foreach (var entity in TsumEntities)
        {
            if (_idToEntityMap.ContainsKey(entity.ID))
            {
                Debug.LogError($"TsumData: ID {entity.ID} が重複");
                continue;
            }
            _idToEntityMap.Add(entity.ID, entity);
        }
    }

    private void OnValidate()
    {
        _idToEntityMap = null;
    }
}