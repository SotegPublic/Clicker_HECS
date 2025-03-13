using Helpers;
using System;
using UnityEngine;

[Serializable]
public class QualityBonusForValueRule
{
    [SerializeField] private EquipItemQualityIdentifier qualityID;
    [SerializeField, Range(0f, 1)] private float qualityBonusPercent = 0f;

    public EquipItemQualityIdentifier QualityID => qualityID;
    public float QualityBonusPercent => qualityBonusPercent;
}