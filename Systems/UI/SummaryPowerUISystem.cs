using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable][Documentation(Doc.UI, Doc.Equipment,  "this system update summary power on main screen")]
    public sealed class SummaryPowerUISystem : BaseSystem, IReactGlobalCommand<SummaryPowerUpdate> , IAfterEntityInit
    {
        [Single]
        public PlayerTagComponent PlayerTagComponent;

        [Required]
        public UIAccessProviderComponent UIAccessProviderComponent;

        public void AfterEntityInit()
        {
            CommandGlobalReact(new SummaryPowerUpdate());
        }

        public void CommandGlobalReact(SummaryPowerUpdate command)
        {
            var summaryPower = PlayerTagComponent.Owner.GetComponent<SummaryItemPowerComponent>().Value;
            UIAccessProviderComponent.Get.GetTextMeshProUGUI(UIAccessIdentifierMap.Text).text = summaryPower.ToString();
        }

        public override void InitSystem()
        {
           
        }
    }
}