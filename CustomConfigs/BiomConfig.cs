using HECSFramework.Unity;
using Helpers;
using Strategies;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = nameof(BiomConfig), menuName = "CustomConfigs/BiomConfig", order = 0)]
public class BiomConfig : ScriptableObject
{
    [SerializeField][IdentifierDropDown(nameof(BiomIdentifier))] private int biomIdentifier;
    [SerializeField][IdentifierDropDown(nameof(BiomMoveTypeIdentifier))] private int moveTypeIdentifier;
    [SerializeField] private AssetReference biomReference;
    [SerializeField] private AssetReference[] standartChunkReferences;
    [SerializeField] private AssetReference[] largeChunkReferences;
    [SerializeField] private EntityContainer[] biomEnemies;

    [Space]
    [Header("Scenario")]
    [SerializeField] private LevelConfig[] levelsConfigs;

    public int BiomID => biomIdentifier;
    public int MoveTypeID => moveTypeIdentifier;
    public AssetReference BiomReference => biomReference;
    public AssetReference[] StandartChunkReferences => standartChunkReferences;
    public AssetReference[] LargeChunkReferences => largeChunkReferences;
    public EntityContainer[] BiomEnemies => biomEnemies;
    public LevelConfig[] Levels => levelsConfigs;
}
