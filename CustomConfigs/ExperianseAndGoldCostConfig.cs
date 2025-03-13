using UnityEngine;

[CreateAssetMenu(fileName = nameof(ExperianseAndGoldCostConfig), menuName = "CustomConfigs/ExperianseAndGoldCostConfig", order = 8)]
public class ExperianseAndGoldCostConfig : ScriptableObject
{
    [SerializeField] private float expCostModifierPerLevel;
    [SerializeField] private float goldCostModifierPerLevel;

    public float ExpCostModifierPerLevel => expCostModifierPerLevel;
    public float GoldCostModifierPerLevel => goldCostModifierPerLevel;
}
