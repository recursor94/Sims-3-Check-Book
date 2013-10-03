using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Sims3.Gameplay.Objects;
using Sims3.Gameplay.Objects.Miscellaneous;
using Sims3.Gameplay.Objects.Electronics;
using Sims3.Gameplay.Objects.CookingObjects;
using Sims3.Gameplay.Interactions;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ObjectComponents;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.ActorSystems.Children;
using Sims3.Gameplay.Autonomy;
using Sims3.Gameplay.Core;
using Sims3.SimIFace;
using Sims3.SimIFace.Enums;
using Sims3.SimIFace.CustomContent;
using Sims3.UI;
using Sims3.Gameplay.Interfaces;
namespace Sims3.Gameplay.Objects.Miscellaneous.Recursor94
{

    class CheckBook : GameObject, ICarryable, IUseCarrySitTransitions, ISuctionable, IBaseCarryable, IHasRouteRadius, IGameObject, IScriptObject, IScriptLogic, IHasScriptProxy, IObjectUI, IExportableContent
    {

        /* Area for interface Implenentations */
        public string CarryModelName
        {
            get
            {
                return "homework"; //!!! Change later when you have an actual model to use
            }
        }

        public float CarryRouteToObjectRadius //Perhaps for how close to hold an object while carrying?
        {
            get
            {
                return 1f;
            }
        }

        public bool OnSuctioned(Sim actor, bool isOutOfControl) //Not sure what this does. Maybe for repoman/robber suction? In that case it's unecessary
        {

            return true;
        }
        /***/


        public sealed class WriteCheckInteraction : Interaction<Sim, CheckBook>
        {
            public static readonly InteractionDefinition Singleton = new Definition();
            //private Slot mChecBookSlot;
            //private ISurface mCheckBookSurface;

            private CAS.SimDescription getSimSelection()
            {
                List<PhoneSimPicker.SimPickerInfo> choices = Phone.Call.GetAllValidCallees(base.Actor, 0f, false, true, false, false);  //use the in game phone book for possible check recipients

                List<object> chosen = Sims3.UI.PhoneSimPicker.Show(true, ModalDialog.PauseMode.PauseSimulator, choices, "Choose Recepient", "Send Check", "Cancel");
                if (chosen == null)
                {
                    return null;
                }
                CAS.SimDescription s = null;
                foreach (object o in chosen)
                {
                    s = (CAS.SimDescription)o;
                }

                return s;
            }

            private void transferFunds(CAS.SimDescription giver, CAS.SimDescription receiver, int amount)
            {
                // Pay sims money by modiyfing each's funds.
                if (giver.FamilyFunds >= amount)
                { //test case to make sure recepient doesn't earn more than the giver actually has in funds

                    giver.ModifyFunds(-amount);
                    receiver.ModifyFunds(amount);
                }
                else
                {
                    //Give all of what the sim does have, and send a message to the player.
                    giver.CreatedSim.ShowTNSIfSelectable("I don't  have enough money to pay this in full. I can only give what I have.", StyledNotification.NotificationStyle.kSimTalking);
                    giver.ModifyFunds(-giver.FamilyFunds);
                    receiver.ModifyFunds(giver.FamilyFunds);

                }

            }



            private bool checkAnimation()
            {
                base.EnterStateMachine("2BTech_HomeworkSolo", "Enter", "x");
                base.SetActor("homework1", base.Target);
                base.SetActor("homework2", base.Target);
                base.SetParameter("inInventory", false);
                base.SetParameter("isBeingHelped", false);
                base.SetParameter("startFromInventory", false);
               /* if (this.Target.Parent != null)
                {
                    base.SetParameter("surfaceHeight", SurfaceHeight.Table);
                    base.SetActor("table", base.Target.Parent);
                }
                else
                {*/
                    base.SetParameter("surfaceHeight", SurfaceHeight.Floor);
                    this.mCurrentStateMachine.AddSynchronousOneShotScriptEventHandler(101u, new SacsEventHandler(delegate(StateMachineClient sender, IEvent e)
                    {
                        this.Target.UnParent();
                        if (!GlobalFunctions.PlaceAtGoodLocation(base.Target, new World.FindGoodLocationParams(base.Target.Position), false))
                        {
                            this.Target.SetPosition(base.Target.Position);
                        }
                    }));
                
                base.BeginCommodityUpdates();
                base.StandardEntry(true);
                this.Actor.LookAtManager.DisableLookAts();
                base.AnimateSim("DoHomeworkLoop");
                bool succeeded = base.DoTimedLoop(10f);
                base.Actor.LookAtManager.EnableLookAts();
                base.EndCommodityUpdates(succeeded);
                base.AnimateSim("Exit");
                base.StandardExit();
                return succeeded;


            }

            private ISurface placeOnSurface() {
                //find a valid nearby surface and place the checkbook on it.
                List<ISurface> surfaces = new List<ISurface>(base.Actor.LotCurrent.GetObjects<ISurface>());
                ISurface surface = GlobalFunctions.FindNearestSurfaceOfType(base.Actor, SurfaceType.Homework, this.Target, surfaces, true);
                CarrySystem.PutDownOnNearestSurface(base.Actor, surfaces, SurfaceType.Homework, true, true, true);
                return surface;
            }





            protected override bool Run()
            {

                if (base.Actor.Inventory.Contains(base.Target))
                {
                    if (!CarrySystem.PickUpFromSimInventory(base.Actor, this.Target))
                    {
                        return false;
                    }
                }
                else if (!base.Actor.RouteToObjectRadius(base.Target, 0.3f))
                {
                    if (!CarrySystem.PickUp(base.Actor, base.Target))
                    {
                        return false;
                    } //pick up object and then bring it to a surface.

                    return false;
                } //move in close to the object and if not possible quit the interaction.
                //alternate path depending on whether the checkbook is already in the inventory.
                
                //place on surface and then enter the chair
                
                 placeOnSurface();

                /*
                if (this.mCheckBookSurface != null) {
                    SurfaceSlot surfaceSlotFramContainedObject = this.mCheckBookSurface.Surface.GetSurfaceSlotFromContainedObject(this.Target);
                    this.mChecBookSlot = surfaceSlotFramContainedObject.ContainmentSlot;
                    Slot chairslot = surfaceSlotFramContainedObject.ChairSlots[0];
                    ISittable Chair = (ISittable) mCheckBookSurface.GetContainedObject(chairslot);
                    InteractionInstance sitOnChair = Chair.RouteToForSitting(base.Actor, chairslot, true,;
                    //base.Actor.RouteToSlot(this.Target, mChecBookSlot);
                    ChildUtils.SetPosturePrecondition(this, CommodityKind.Sitting, new CommodityKind[]
                    {
                        CommodityKind.InFrontOfSurfaceForTarget,
                        CommodityKind.ChairScootedIntoSurface 
                    });
                 * 
                }
   
                */
            
                CAS.SimDescription receiver = getSimSelection();

                if (receiver != null)
                {
                    String samount = StringInputDialog.Show("Amount:", "(Must be round number) §", "100");
                    //must parse the string into an integer
                    int amount = 0;
                    if (int.TryParse(samount, out amount) && receiver != null)
                    {
                        transferFunds(base.Actor.SimDescription, receiver, amount);

                    }

                    else
                    {
                        base.Actor.ShowTNSIfSelectable("Woops! Messed up the check...I can't send this one!", StyledNotification.NotificationStyle.kSimTalking);
                        return false;  //exit interaction when user enters invalid value
                    }
                }
                else
                {
                    return false;
                }
                checkAnimation(); //end with animation
                base.Actor.ShowTNSIfSelectable("Ended animation.", StyledNotification.NotificationStyle.kSimTalking);
                //CarrySystem.AnimateIntoSimInventory(base.Actor);  animation errors out in null. Probably because of table
                base.Actor.ShowTNSIfSelectable("Ended animate into.", StyledNotification.NotificationStyle.kSimTalking);
                base.Actor.Inventory.TryToAdd(this.Target);
                base.Actor.ShowTNSIfSelectable("Ended method", StyledNotification.NotificationStyle.kSimTalking);
                return true;
                //CarrySystem.PutDown(base.Actor, SurfaceType.Normal, true);


            }

            protected override bool RunFromInventory()
            {
                return this.Run();
            }
            [DoesntRequireTuning]
            private sealed class Definition : InteractionDefinition<Sim, CheckBook, CheckBook.WriteCheckInteraction>
            {
                protected override string GetInteractionName(Sim a, CheckBook target, InteractionObjectPair interaction)
                {
                    return "Write Check";
                }

                protected override bool Test(Sim a, CheckBook target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutToolTipCallBack)
                {
                    return !isAutonomous && a.SimDescription.TeenOrAbove; // Children can't write checks! 
                }

            }

        }

        public sealed class PlaceInInventory : ImmediateInteraction<Sim, CheckBook>
        {
            public static readonly InteractionDefinition Singleton = new Definition();
            protected override bool Run()
            {
                
                bool added = base.Actor.Inventory.TryToAdd(this.Target);
                //base.SetParameter("inInventory", true);
                //base.Actor.ShowTNSIfSelectable("Should have been added to inventory " + added, StyledNotification.NotificationStyle.kSystemMessage);
                return true;
            }

            public sealed class Definition : ImmediateInteractionDefinition<Sim, CheckBook, CheckBook.PlaceInInventory>
            {
                protected override string GetInteractionName(Sim a, CheckBook target, InteractionObjectPair interaction)
                {
                    return "Put in inventory";
                }

                protected override bool Test(Sim a, CheckBook target, bool isAutonomous, ref GreyedOutTooltipCallback greyedOutTooltipCallback)
                {
                    return !isAutonomous && a.SimDescription.TeenOrAbove && !target.InInventory; //Can't place in inventory what is already there!
                }
            }
        }

        public bool CanAddToInventory(Inventory inventory)
        {
            return true;
        }


        public override void OnStartup()
        {
            base.OnStartup();
            base.AddComponent<ItemComponent>(new object[]
            {
                ItemComponent.SimInventoryItem
            });
            this.ItemComp.CanAddToInventoryDelegate = new ItemComponent.CanAddToInventoryCallback(this.CanAddToInventory);
            base.AddInteraction(WriteCheckInteraction.Singleton);
            base.AddInteraction(PlaceInInventory.Singleton);
            base.AddInventoryInteraction(CheckBook.WriteCheckInteraction.Singleton);
        }

    }

}
