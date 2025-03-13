using UnityEngine;

[CreateAssetMenu(fileName = nameof(ModifiersCountRules), menuName = "CustomConfigs/ModifiersCountRules", order = 8)]
public class ModifiersCountRules : ScriptableObject
{
    [SerializeField] private ModifiersCountRule[] modifiersCountRules;

    public int GetModifiersCount(int qualityID)
    {
        for(int i = 0; i < modifiersCountRules.Length; i++)
        {
            if (modifiersCountRules[i].QualityID == qualityID)
            {
                return modifiersCountRules[i].ModifiersCount;
            }
        }

        return 1;
    }
}
