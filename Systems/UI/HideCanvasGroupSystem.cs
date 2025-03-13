using HECSFramework.Core;
using Components;
using Commands;

namespace Systems
{
    [Documentation(Doc.UI, "This system hide ui on hide command")]
    public sealed class HideCanvasGroupSystem : BaseSystem, IReactCommand<HideUICommand>
    {
        [Required]
        private UIAccessProviderComponent accessProviderComponent;
        public override void InitSystem()
        { }

        public void CommandReact(HideUICommand command)
        {
            var canvasGroup = accessProviderComponent.Get.GetCanvasGroup(UIAccessIdentifierMap.CanvasGroup);
            canvasGroup.HideCanvasGroup();
            Owner.Command(new AfterCommand<HideUICommand>());
        }
    }
}