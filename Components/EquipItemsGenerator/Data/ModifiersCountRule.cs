using Helpers;
using System;
using UnityEngine;

[Serializable]
public class ModifiersCountRule
{
    [SerializeField, IdentifierDropDown(nameof(EquipItemQualityIdentifier))] private int qualityID;
    [SerializeField] private int modifiersCount = 1;

    public int QualityID => qualityID;
    public int ModifiersCount => modifiersCount;
}
