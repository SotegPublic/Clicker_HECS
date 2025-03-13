using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Helpers;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class LevelConfig
{
    [SerializeField] private bool isChunk;
    [SerializeField][ShowIf(nameof(isChunk))] private AssetReference chunkReference;
    
    [SerializeField]
    [EntityContainerDropDown(nameof(EnemyTagComponent))]
    private EntityContainer enemy;
    
    [SerializeField] private int enemyLevelBonus;
    [SerializeField][ValueDropdown(nameof(GetSpecialRewardsContainers))] private EntityContainer[] specialRewards;
    [SerializeField] private UnityFloatModifier[] parametersModifiers;

    public bool IsChunk => isChunk;
    public AssetReference ChunkReference => chunkReference;
    public EntityContainer Enemy => enemy;
    public int EnemyLevelBonus => enemyLevelBonus; 
    public EntityContainer[] SpecialRewards => specialRewards;
    public UnityFloatModifier[] ParametersModifiers => parametersModifiers;

    private IEnumerable<EntityContainer> GetSpecialRewardsContainers()
    {
        var containers = new SOProvider<EntityContainer>().GetCollection().Where(x => x is not PresetContainer && x.IsHaveComponent<SpecialRewardTagComponent>()
           && !x.ContainsComponent(ComponentProvider<IgnoreReferenceContainerTagComponent>.TypeIndex, true));

        return containers;
    }

    public bool TryGetModifiers(out List<UnityFloatModifier> outModifiers)
    {
        if (parametersModifiers != null && parametersModifiers.Length > 0)
        {
            outModifiers = parametersModifiers.ToList();
            return true;
        }

        outModifiers = null;
        return false;
    }

    public bool TryGetSpecialRewards(out List<EntityContainer> outSpecialRewards)
    {
        if (specialRewards != null && specialRewards.Length > 0)
        {
            outSpecialRewards = specialRewards.ToList();
            return true;
        }

        outSpecialRewards = null;
        return false;
    }
}
