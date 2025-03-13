using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = nameof(FXWarmupConfig), menuName = "CustomConfigs/FXWarmupConfig", order = 1)]
public class FXWarmupConfig : ScriptableObject
{
    [SerializeField] private AssetReference fxReference;
    [SerializeField] private int warmupCount;

    public AssetReference FxReference => fxReference;
    public int WarmupCount => warmupCount;
}
