using System;
using System.Collections.Generic;
using System.Text;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Autonomy;
using Sims3.SimIFace;
using Sims3.UI;
namespace Sims3.Gameplay.Objects.Miscellaneous.Recursor94
{
    class HellDucky : RubberDucky
    {
        private sealed class Burn : ImmediateInteraction<Sim, HellDucky>
        {
            [DoesntRequireTuning]
            private sealed class Definition : ImmediateInteractionDefinition<Sim, HellDucky, HellDucky.Burn>
            {
                protected override string GetInteractionName(Sim a, HellDucky target, InteractionObjectPair interaction)
                {
                    return "Play With";
                }
                protected override bool Test(Sim a, HellDucky target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return !isAutonomous;
                }
            }
            public static readonly InteractionDefinition Singleton = new Definition();
            protected override bool Run()
            {
                base.Actor.ShowTNSIfSelectable("Hello!", StyledNotification.NotificationStyle.kSimTalking);
                return true;
            }
        }

        public override void OnStartup() {
            base.OnStartup();
            base.AddInteraction(Burn.Singleton);
        }
    }
}
