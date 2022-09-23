using System;
using System.Text;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.Messages;
using XRL.Rules;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Levitation : BaseMutation, IFlightSource
    {
        private int BASE_FLIGHT_DURATION = 20;
        private int ADDITIONAL_TURNS_PER_LEVEL = 3;

        public int Duration => this.Level * ADDITIONAL_TURNS_PER_LEVEL + BASE_FLIGHT_DURATION;
        public int FlightLevel => this.Level;
        public int FlightBaseFallChance => this.BaseFallChance;

        public bool FlightRequiresOngoingEffort => true;
        public string FlightEvent => "CommandFlight";
        public string FlightActivatedAbilityClass => "Mental Mutation";
        public string FlightSourceDescription => string.Empty;

        public bool FlightFlying
        {
            set
            {
                this._FlightFlying = value;
            }
            get
            {
                return this._FlightFlying;
            }
        }

        public Guid FlightActivatedAbilityID
        {
            set
            {
                this._FlightActivatedAbilityID = value;
            }
            get
            {
                return this._FlightActivatedAbilityID;
            }
        }

        public int BaseFallChance = 0;
        public Guid _FlightActivatedAbilityID = Guid.Empty;
        public bool _FlightFlying;
        public int Cooldown = 40;

        public override IPart DeepCopy(GameObject Parent)
        {
            MMM_Levitation MMM_Levitation = base.DeepCopy(Parent) as MMM_Levitation;
            return (IPart)MMM_Levitation;
        }

        public MMM_Levitation()
        {
            this.DisplayName = "Levitation";
        }

        public override bool GeneratesEquipment()
        {
            return true;
        }

        public override bool WantEvent(int ID, int cascade)
        {
            if (!base.WantEvent(ID, cascade))
                return ID == GetItemElementsEvent.ID;
            return true;
        }

        public override bool HandleEvent(GetItemElementsEvent E)
        {
            E.Add("travel", 1);
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "AIGetOffensiveMutationList");
            Object.RegisterPartEvent((IPart)this, "AIGetPassiveMutationList");
            Object.RegisterPartEvent((IPart)this, "BeginTakeAction");
            Object.RegisterPartEvent((IPart)this, "EndTurn");
            Object.RegisterPartEvent((IPart)this, "EnteredCell");
            Object.RegisterPartEvent((IPart)this, "CommandFlight");
            base.Register(Object);
        }

        public override string GetDescription()
        {
            return "You can levitate.";
        }

        public override string GetLevelText(int Level)
        {
            StringBuilder stringBuilder = Event.NewStringBuilder((string)null);
            stringBuilder.Append($"You may levitate outside and underground for {Duration} turns (you cannot be hit in melee by grounded creatures while levitating). You may not levitate while travelling.\n Cooldown: {Cooldown}\n");
            return stringBuilder.ToString();
        }

        bool StartFlyingAnywhere(GameObject Source, GameObject Object, IFlightSource FS)
        {
            if (FS.FlightFlying)
                return false;
            if (!Object.CanChangeMovementMode("Flying", true, false, false))
                return false;
            if (Object.GetEffectCount("Flying") == 0)
            {
                if (Object.IsPlayer())
                    MessageQueue.AddPlayerMessage("You begin levitating!", 'g');
                else if (Object.IsVisible())
                    MessageQueue.AddPlayerMessage(Object.The + Object.ShortDisplayName + Object.GetVerb("begin", true, false) + " levitating.");
                Object.ApplyEffect((Effect)new MMM_EffectLevitation(Duration, this.ParentObject));
                Object.MovementModeChanged("Flying", false);
            }
            else if (Object.IsPlayer())
                MessageQueue.AddPlayerMessage("You cannot levitate and fly at the same time.", 'g');

            FS.FlightFlying = true;
            Object.ApplyEffect((Effect)new Flying(FS.FlightLevel, Source), (GameObject)null);
            Object.RemoveEffect("Prone", false);
            Object.ToggleActivatedAbility(FS.FlightActivatedAbilityID);
            Object.FireEvent("FlightStarted");
            ObjectStartedFlyingEvent.SendFor(Object);
            return true;
        }

        public void StopFlying()
        {
            if (this.ParentObject.HasEffect("MMM_EffectLevitation"))
                this.ParentObject.RemoveEffect("MMM_EffectLevitation");
            Flight.StopFlying(this.ParentObject, this.ParentObject, (IFlightSource)this, false, false);
        }

        public override bool FireEvent(Event E)
        {
            //if (E.ID == "EndTurn")
            //    Flight.MaintainFlight(this.ParentObject, this.ParentObject, (IFlightSource)this);
            if (E.ID == "BeginTakeAction")
            {
                if (this.ParentObject.OnWorldMap())
                    Flight.StopFlying(this.ParentObject, this.ParentObject, (IFlightSource)this, true, false);
            }
            else if (E.ID == "EnteredCell")
            {
                Flight.CheckFlight(this.ParentObject, this.ParentObject, (IFlightSource)this);
            }
            else if (E.ID == "CommandFlight")
            {
                if (!this.ParentObject.OnWorldMap())
                {
                    if (this.IsMyActivatedAbilityToggledOn(this.FlightActivatedAbilityID, (GameObject)null))
                    {
                        if (this.ParentObject.IsPlayer() && this.currentCell != null && this.ParentObject.GetEffectCount("MMM_EffectLevitation") <= 1)
                        {
                            int index = 0;
                            for (int count = this.currentCell.Objects.Count; index < count; ++index)
                            {
                                GameObject gameObject = this.currentCell.Objects[index];
                                StairsDown part = gameObject.GetPart("StairsDown") as StairsDown;
                                if (part != null && part.IsLongFall() && Popup.ShowYesNo("It looks like a long way down " + gameObject.the + gameObject.ShortDisplayName + " you're above. Are you sure you want to stop levitating?", true, DialogResult.Yes) != DialogResult.Yes)
                                    return false;
                            }
                        }
                        this.StopFlying();
                    }
                    else
                    {
                        if (!this.ParentObject.HasEffect("MMM_EffectLevitation"))
                        {
                            this.ParentObject.CooldownActivatedAbility(((IFlightSource)this).FlightActivatedAbilityID, Cooldown, (string)null);
                            this.ParentObject.UseEnergy(1000, "Mental");
                            this.StartFlyingAnywhere(this.ParentObject, this.ParentObject, (IFlightSource)this);
                        }
                        else
                        {
                            if (this.ParentObject.IsPlayer())
                                Popup.Show("You already are levitating", false);
                        }
                    }
                }
                else
                {
                    if (this.ParentObject.IsPlayer())
                        Popup.Show("You can't fly on world map", false);
                }
            }
            else if (E.ID == "AIGetOffensiveMutationList" || E.ID == "AIGetPassiveMutationList")
            {
                if (!this.FlightFlying/* && Flight.EnvironmentAllowsFlight(this.ParentObject)*/ && Flight.IsAbilityAIUsable((IFlightSource)this, this.ParentObject))
                    E.AddAICommand(this.FlightEvent, 1, (GameObject)null, false);
            }
            else if ((E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged") && this.FlightFlying)
            {
                if (E.HasFlag("Involuntary"))
                    Flight.FailFlying(this.ParentObject, this.ParentObject, (IFlightSource)this);
                else
                    Flight.StopFlying(this.ParentObject, this.ParentObject, (IFlightSource)this, false, false);
            }
            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            this.ChangeLevel(Level);
            Flight.AbilitySetup(GO, GO, (IFlightSource)this);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            Flight.AbilityTeardown(GO, GO, (IFlightSource)this);
            return base.Unmutate(GO);
        }
    }
}