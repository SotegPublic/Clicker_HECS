using UnityEngine;

[CreateAssetMenu(fileName = nameof(QualitiesByItemLevelRule), menuName = "CustomConfigs/QualitiesByItemLevelRule", order = 8)]
public class QualitiesByItemLevelRule : ScriptableObject
{
    [SerializeField] private int maxItemLevelValue;
    [SerializeField] private EquipItemQualityIdentifier[] qualitiesSet;

    public int MaxItemLevelValue => maxItemLevelValue;
    public EquipItemQualityIdentifier[] QualitiesSet => qualitiesSet;
}
