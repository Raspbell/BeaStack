using UnityEngine;
using System.Collections.Generic; // Dictionaryのために必要

[CreateAssetMenu(fileName = "TsumData", menuName = "Game/TsumData")]
public class TsumData : ScriptableObject
{
    public TsumComponent[] TsumEntities;

    [System.NonSerialized]
    private Dictionary<int, TsumComponent> _idToEntityMap;

    [System.Serializable]
    public class TsumComponent
    {
        public int TsumID;
        public string TsumName;
        public int TsumScore;
        public Sprite TsumSprite;
        public Color TsumColor;
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

        Debug.LogWarning($"TsumData: ID {tsumId} が見つかりませんでした。");
        return null;
    }

    private void InitializeLookupDict()
    {
        _idToEntityMap = new Dictionary<int, TsumComponent>();

        foreach (var entity in TsumEntities)
        {
            if (_idToEntityMap.ContainsKey(entity.TsumID))
            {
                Debug.LogError($"TsumData: ID {entity.TsumID} が重複");
                continue;
            }
            _idToEntityMap.Add(entity.TsumID, entity);
        }
    }

    private void OnValidate()
    {
        _idToEntityMap = null;
    }
}