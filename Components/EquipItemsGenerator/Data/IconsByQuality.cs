using Helpers;
using System;
using UnityEngine;

[Serializable]
public class IconsByQuality
{
    [SerializeField, IdentifierDropDown(nameof(EquipItemQualityIdentifier))] private int qualityID;
    [SerializeField] private Sprite[] sprites;

    public int QualityID => qualityID;
    public Sprite[] Sprites => sprites;
}
