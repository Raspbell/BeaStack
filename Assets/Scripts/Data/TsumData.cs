using UnityEngine;
using System.Collections.Generic; // Dictionaryのために必要

[CreateAssetMenu(fileName = "TsumData", menuName = "Game/TsumData")]
public class TsumData : ScriptableObject
{
    public TsumEntity[] TsumEntities;

    [System.NonSerialized]
    private Dictionary<int, TsumEntity> _idToEntityMap;

    [System.Serializable]
    public class TsumEntity
    {
        public int TsumID;
        public string TsumName;
        public int TsumScore;
        public Sprite TsumSprite;
        public Color TsumColor;
        public Color HighlightColor;
    }

    public TsumEntity GetTsumEntityById(int tsumId)
    {
        if (_idToEntityMap == null)
        {
            InitializeLookupDict();
        }

        if (_idToEntityMap.TryGetValue(tsumId, out var entity))
        {
            return entity;
        }

        Debug.LogWarning($"TsumData: ID {tsumId} が見つかりませんでした。");
        return null;
    }

    private void InitializeLookupDict()
    {
        _idToEntityMap = new Dictionary<int, TsumEntity>();

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