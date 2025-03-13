using HECSFramework.Unity;
using Helpers;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(SlotModifiersRule), menuName = "CustomConfigs/SlotModifiersRule", order = 8)]
public class SlotModifiersRule: ScriptableObject
{
    [SerializeField, IdentifierDropDown(nameof(EquipItemSlotIdentifier))] private int slotID;
    [SerializeField, IdentifierDropDown(nameof(ModifierIdentifier))] private int mainSlotModifier;
    [SerializeField] private ModifierIdentifier[] secondaryModifiers;

    public int SlotID => slotID;
    public int MainSlotModifier => mainSlotModifier;
    public ModifierIdentifier[] SecondaryModifiers => secondaryModifiers;
}
