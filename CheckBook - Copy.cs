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
         protected Slot checkSlot;
         protected ISurface checkSurface;
         protected Vector3 checkPos;
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
             private void EventCallbackParentToSurface(StateMachineClient sender, IEvent evt)
             {
                 this.Target.ParentToSlot(Target.checkSurface, Target.checkSlot);
                 if (evt.EventId == 100u)
                 {
                     this.Target.SetGeometryState("geoState_open");
                 }
                 if (evt.EventId == 101u)
                 {
                     this.Target.SetGeometryState("default");
                 }
             }

             private void EventCallbackParentToFloor(StateMachineClient sender, IEvent evt)
             {
                 this.Target.UnParent();
                 if (!GlobalFunctions.PlaceAtGoodLocation(this.Target, new World.FindGoodLocationParams(Target.checkPos), false))
                 {
                     this.Target.SetPosition(Target.checkPos);
                 }
             }

             private CAS.SimDescription getSimSelection()
             {
                 List<PhoneSimPicker.SimPickerInfo> choices = Phone.Call.GetAllValidCallees(base.Actor, 0f, false, true, false, false);  //use the in game phone book for possible check recipients

                 List<object> chosen = Sims3.UI.PhoneSimPicker.Show(true, ModalDialog.PauseMode.PauseSimulator, choices, "Choose Recepient", "Send Check", "Cancel");
                 if (chosen == null) {
                     return null;
                 }
                 CAS.SimDescription s = null;
                 foreach (object o in chosen)
                 {
                     s = (CAS.SimDescription) o;
                 }

                 return s;
             }

             private void transferFunds(CAS.SimDescription giver, CAS.SimDescription receiver, int amount) {
                 // Pay sims money by modiyfing each's funds.
                 if (giver.FamilyFunds >= amount) { //test case to make sure recepient doesn't earn more than the giver actually has in funds

                     giver.ModifyFunds(-amount);
                     receiver.ModifyFunds(amount);
                 }
                 else {
                     //Give all of what the sim does have, and send a message to the player.
                     giver.CreatedSim.ShowTNSIfSelectable("I don't  have enough money to pay this in full. I can only give what I have.", StyledNotification.NotificationStyle.kSimTalking);
                     giver.ModifyFunds(-giver.FamilyFunds);
                     receiver.ModifyFunds(giver.FamilyFunds);

                 }

             }
             internal void writeCheckSMCSetup (bool inInventory, bool startFromInventory) {

                 EnterStateMachine("2BTech_HomeworkSolo", "Enter", "x");
                 SetActor("homework1", this.Target);
                 SetActor("homework2", this.Target);
                 SetParameter("inInventory", inInventory);
                 SetParameter("isBeingHelped", false);
                 SetParameter("startFromInventory", startFromInventory);
	            if (Target.Parent != null)
	            {
		            SetParameter("surfaceHeight", SurfaceHeight.Table);
		            SetActor("chair", this.InstanceActor.Posture.Container);
		            SetActor("table", this.Target.Parent);
		            return;
	            }
	            SetParameter("surfaceHeight", SurfaceHeight.Floor);


             }

             private  bool onWriteStarted()
             {
                 return true;
             }


             private bool checkAnimation()
             {
                 //base.EnterStateMachine("2BTech_HomeworkSolo", "Enter", "x");
                 //base.SetActor("homework1", base.Target);
                 //base.SetActor("homework2", base.Target);
                 //base.SetParameter("inInventory", false);
                 //base.SetParameter("isBeingHelped", false);
                 //base.SetParameter("startFromInventory", false);
                 //base.SetParameter("surfaceHeight", SurfaceHeight.Floor);
                 //this.mCurrentStateMachine.AddSynchronousOneShotScriptEventHandler(101u, new SacsEventHandler(delegate(StateMachineClient sender, IEvent e)
                 //{
                 //    this.Target.UnParent();
                 //    if (!GlobalFunctions.PlaceAtGoodLocation(base.Target, new World.FindGoodLocationParams(base.Target.Position), false))
                 //    {
                 //        this.Target.SetPosition(base.Target.Position);
                 //    }
                 //}));
                 //base.BeginCommodityUpdates();
                 //base.StandardEntry(true);
                 //this.Actor.LookAtManager.DisableLookAts();
                 //base.AnimateSim("DoHomeworkLoop");
                 //bool succeeded = base.DoTimedLoop(10f);
                 //base.Actor.LookAtManager.EnableLookAts();
                 //base.EndCommodityUpdates(succeeded);
                 //base.AnimateSim("Exit");
                 //base.StandardExit();
                 //return succeeded;
                 if (this.Target.Parent != null && !(this.Actor.Posture is SittingPosture))
                 {
                     return false;
                 }
                 base.StandardEntry(false);
                 Target.checkPos = this.Target.Position;
                 this.writeCheckSMCSetup(false, false);
                 Target.checkSurface = (this.Target.Parent as ISurface);
                 if (Target.checkSurface!= null)
                 {
                     SurfaceSlot surfaceSlotFromContainedObject = Target.checkSurface.Surface.GetSurfaceSlotFromContainedObject(this.Target);
                     Target.checkSlot = surfaceSlotFromContainedObject.ContainmentSlot;
                     this.mCurrentStateMachine.AddSynchronousOneShotScriptEventHandler(100u, new SacsEventHandler(this.EventCallbackParentToSurface));
                     this.mCurrentStateMachine.AddSynchronousOneShotScriptEventHandler(101u, new SacsEventHandler(this.EventCallbackParentToSurface));
                 }
                 else
                 {
                     this.mCurrentStateMachine.AddSynchronousOneShotScriptEventHandler(101u, new SacsEventHandler(this.EventCallbackParentToFloor));
                 }
                 base.BeginCommodityUpdates();
                 this.Actor.LookAtManager.DisableLookAts();
                 base.AnimateSim("DoHomeworkLoop");
                 bool succeded = false;
                 succeded = base.DoTimedLoop(10f);
                 this.Actor.LookAtManager.EnableLookAts();
                 base.EndCommodityUpdates(succeded);
                 base.SetParameter("inInventory", true);
                 base.AnimateSim("Exit");
                 this.Actor.Inventory.TryToAdd(this.Target);
                 this.Target.UnParent();
                 base.StandardExit(true);
                 return succeded;

             }
         
             
             public static readonly InteractionDefinition Singleton = new Definition();


             protected override bool Run()
             {
                 if (!base.Actor.RouteToObjectRadius(base.Target, 0.3f)) {
                     return false;
                 } //move in close to the object and if not possible quit the interaction.
                 
                 if (!CarrySystem.PickUp(base.Actor, base.Target)) {  
                     return false;
                 } //pick up object and then bring it to a surface.
                
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
                 else {
                     return false;
                 }
                 checkAnimation(); //end with animation

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

        public override void OnStartup()
        {
            base.OnStartup();
            base.AddInteraction(WriteCheckInteraction.Singleton);
        }

    }
}
