using System;
using System.Linq;
using Commands;
using Components;
using HECSFramework.Core;
using Helpers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems
{
    [Serializable][Documentation(Doc.UI, Doc.Equipment, Doc.Chest, "this system operates ui on main screen, equipment and chests")]
    public sealed class EquipAndChestUISystem : BaseSystem, IReactGlobalCommand<UpdateInventoryUISlotCommand>, IAfterEntityInit, IReactCommand<InputStartedCommand>
    {
        [Required]
        public UIAccessProviderComponent UIAccessProviderComponent;

        private CameraTagComponent cameraTagComponent;

        private RaycastHit[] hits = new RaycastHit[8];
        private int layerMask;

        private InputAction touchAction;

        public override void InitSystem()
        {
            Owner.World.GetSingleComponent<InputActionsComponent>()
                .TryGetInputAction(InputIdentifierMap.ClickTouchPosition, out  touchAction);

            Owner.GetOrAddComponent<InputListenerTagComponent>();
            layerMask = LayerMask.NameToLayer("UI");
            using var components = Owner.World.GetActiveComponents<CameraTagComponent>();
            cameraTagComponent = components.FirstOrDefault(x => x.CameraIdentifier == CameraIdentifierMap.CanvasCameraIdentifier);
        }

        public void AfterEntityInit()
        {
            using var equipment = UIAccessProviderComponent.Get.GetRectTransforms(UIAccessIdentifierMap.Equipment);
            var buttons = UIAccessProviderComponent.Get.Buttons;

            for (int i = 0; i < equipment.Count; i++)
            {
                var currentEquip = equipment.Items[i];
                var neededButton = buttons.FirstOrDefault(x => x.Value.gameObject == currentEquip.gameObject);

                if (neededButton.Value != null)
                    neededButton.Value.onClick.AddListener(() => ShowEquipWindow(neededButton.UIAccessIdentifier));
            }

            DrawCurrentEquipment();
        }

        private void DrawCurrentEquipment()
        {
            var inventory = Owner.World.GetEntityBySingleComponent<PlayerTagComponent>().GetComponent<EquipItemsHolderComponent>();

            foreach(var slot in inventory.EquipItems)
            {
                var sprite = slot.Value.GetComponent<EquipItemParametersComponent>().Icon;
                ChangeIcon(slot.Key, sprite);
            }

        }

        public void ShowEquipWindow(int indexSlot)
        {

        }

        public void ChangeIcon(int slotIndex, Sprite sprite)
        {
            UIAccessProviderComponent.Get.GetImage(slotIndex).sprite = sprite;
        }

        public override void BeforeDispose()
        {
            foreach (var b in UIAccessProviderComponent.Get.Buttons)
            {
                b.Value.onClick.RemoveAllListeners();
            }
        }

        public void CommandGlobalReact(UpdateInventoryUISlotCommand command)
        {
            ChangeIcon(command.SlotID, command.Sprite);
        }

        public void CommandReact(InputStartedCommand command)
        {
            if (command.Index != InputIdentifierMap.Fire)
                return;

            var pos = touchAction.ReadValue<Vector2>();
            var ray = cameraTagComponent.Camera.ScreenPointToRay(pos);

            var collisions = Physics.RaycastNonAlloc(ray, hits, 2000f, -1);

            if (collisions == 0)
                return;

            for (int i = 0; i < collisions; i++)
            {
                if (hits[i].collider.TryGetActorFromCollision(out var actor))
                {
                    if (actor.Entity.ContainsMask<ChestTagComponent>())
                        break;
                }
            } 

            var uiCounter = Owner.World.GetFilter<CounterUITagComponent>().
            FirstOrDefault(x => x.GetComponent<CounterUITagComponent>().CounterIdentifierContainer == CounterIdentifierContainerMap.Chests).
            GetComponent<CountersHolderComponent>().GetOrAddIntCounter(CounterIdentifierContainerMap.Chests);
            var chestEntity = Owner.World.GetEntityBySingleComponent<ChestTagComponent>();

            if (uiCounter.Value < 1 || chestEntity.TryGetComponent<RollAndComparingEquipItemTagComponent>(out var component))
            {
                return;
            }
            else
            {
                chestEntity.AddComponent<RollAndComparingEquipItemTagComponent>();

                var player = new AliveEntity(Owner.World.GetEntityBySingleComponent<PlayerTagComponent>());

                Owner.World.Command(new GlobalSpendResourceCommand
                {
                    Amount = 1,
                    CounterID = CounterIdentifierContainerMap.Chests,
                    DrawRule = DrawRuleIdentifierMap.SpendChest,
                    From = player
                });

                Owner.World.Command(new RollNewItemCommand { PlayerLevel = player.Entity.GetComponent<LevelCounterComponent>().Value });
            }
        }
    }
}