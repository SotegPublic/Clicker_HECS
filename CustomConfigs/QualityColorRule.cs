using Helpers;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(QualityColorRule), menuName = "CustomConfigs/QualityColorRule", order = 8)]
public class QualityColorRule : ScriptableObject
{
    [SerializeField, IdentifierDropDown(nameof(EquipItemQualityIdentifier))] private int qualityID;
    [SerializeField] private Color qualityColor;

    public int QualityID => qualityID;
    public Color QualityColor => qualityColor;
}
