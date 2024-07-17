using System;
using System.Text;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.Messages;
using XRL.World.Effects;
using MoreMentalMutations.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Levitation : MMM_BaseMutation, IFlightSource
    {
        private int BASE_FLIGHT_DURATION = 20;
        private int ADDITIONAL_TURNS_PER_LEVEL = 3;

        public int Duration => Level * ADDITIONAL_TURNS_PER_LEVEL + BASE_FLIGHT_DURATION;
        public int FlightLevel => Level;
        public int FlightBaseFallChance => BaseFallChance;

        public bool FlightRequiresOngoingEffort => true;
        public string FlightEvent => "CommandFlight";
        public string FlightActivatedAbilityClass => "Mental Mutation";
        public string FlightSourceDescription => string.Empty;

        public bool FlightFlying
        {
            set
            {
                _FlightFlying = value;
            }
            get
            {
                return _FlightFlying;
            }
        }

        public Guid FlightActivatedAbilityID
        {
            set
            {
                _FlightActivatedAbilityID = value;
            }
            get
            {
                return _FlightActivatedAbilityID;
            }
        }

        public int BaseFallChance = 0;
        public Guid _FlightActivatedAbilityID = Guid.Empty;
        public bool _FlightFlying;
        public int Cooldown = 40;
        public int BaseDuration = 20;
        public int DurationPerLevel = 3;

        public override IPart DeepCopy(GameObject Parent)
        {
            return base.DeepCopy(Parent) as MMM_Levitation;
        }

        public MMM_Levitation()
        {
            DisplayName = "Levitation";
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

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("AIGetOffensiveMutationList");
            Registrar.Register("AIGetPassiveMutationList");
            Registrar.Register("BeginTakeAction");
            Registrar.Register("EndTurn");
            Registrar.Register("EnteredCell");
            Registrar.Register("CommandFlight");

            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            return "You can levitate.";
        }

        public override string GetLevelText(int Level)
        {
            StringBuilder stringBuilder = Event.NewStringBuilder();
            stringBuilder.Append($"You may levitate outside and underground for {GetDuration()} turns (you cannot be hit in melee by grounded creatures while levitating). You may not levitate while travelling.\n Cooldown: {Cooldown}\n");

            return stringBuilder.ToString();
        }

        public int GetDuration()
        {
            return Level * DurationPerLevel + BaseDuration;
        }

        bool StartFlyingAnywhere(GameObject Source, GameObject Object, IFlightSource FS)
        {
            if (FS.FlightFlying)
                return false;
            if (!Object.CanChangeMovementMode("Flying", true, false, false))
                return false;
            if (Object.GetEffectCount(typeof(Flying)) == 0)
            {
                if (Object.IsPlayer())
                    MessageQueue.AddPlayerMessage("You begin levitating!", 'g');
                else if (Object.IsVisible())
                    MessageQueue.AddPlayerMessage(Object.The + Object.ShortDisplayName + Object.GetVerb("begin", true, false) + " levitating.");
                Object.ApplyEffect(new MMM_EffectLevitation(GetDuration(), ParentObject));
                Object.MovementModeChanged("Flying", false);
            }
            else if (Object.IsPlayer())
            {
                MessageQueue.AddPlayerMessage("You cannot levitate and fly at the same time.", 'g');
            }

            FS.FlightFlying = true;
            Object.ApplyEffect(new Flying(FS.FlightLevel, Source), null);
            Object.RemoveEffect<Prone>();
            Object.ToggleActivatedAbility(FS.FlightActivatedAbilityID);
            Object.FireEvent("FlightStarted");
            ObjectStartedFlyingEvent.SendFor(Object);
            return true;
        }

        public void StopFlying()
        {
            if (ParentObject.HasEffect("EffectLevitation"))
            {
                ParentObject.RemoveEffect(typeof(MMM_EffectLevitation));
            }

            Flight.StopFlying(ParentObject, ParentObject, this, false, false);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeginTakeAction")
            {
                if (ParentObject.OnWorldMap())
                    Flight.StopFlying(ParentObject, ParentObject, this, true, false);
            }
            else if (E.ID == "EnteredCell")
            {
                Flight.CheckFlight(ParentObject, ParentObject, this);
            }
            else if (E.ID == "CommandFlight")
            {
                if (!ParentObject.OnWorldMap())
                {
                    if (IsMyActivatedAbilityToggledOn(FlightActivatedAbilityID, null))
                    {
                        if (ParentObject.IsPlayer() && currentCell != null && ParentObject.GetEffectCount(typeof(MMM_EffectLevitation)) <= 1)
                        {
                            int index = 0;
                            for (int count = currentCell.Objects.Count; index < count; ++index)
                            {
                                GameObject gameObject = currentCell.Objects[index];
                                StairsDown part = gameObject.GetPart<StairsDown>();

                                if (part != null && part.IsLongFall() && Popup.ShowYesNo("It looks like a long way down " + gameObject.the + gameObject.ShortDisplayName + " you're above. Are you sure you want to stop levitating?", true, DialogResult.Yes) != DialogResult.Yes)
                                {
                                    return false;
                                }
                            }
                        }
                        StopFlying();
                    }
                    else
                    {
                        if (!ParentObject.HasEffect("EffectLevitation"))
                        {
                            ParentObject.CooldownActivatedAbility(((IFlightSource)this).FlightActivatedAbilityID, Cooldown, null);
                            ParentObject.UseEnergy(1000, "Mental");
                            StartFlyingAnywhere(ParentObject, ParentObject, this);
                        }
                        else if (ParentObject.IsPlayer())
                        {
                            Popup.Show("You already are levitating");
                        }
                    }
                }
                else if (ParentObject.IsPlayer())
                {
                    Popup.Show("You can't fly on world map");
                }
            }
            else if (E.ID == "AIGetOffensiveMutationList" || E.ID == "AIGetPassiveMutationList")
            {
                if (!FlightFlying/* && Flight.EnvironmentAllowsFlight(this.ParentObject)*/ && Flight.IsAbilityAIUsable(this, ParentObject))
                {
                    E.AddAICommand(FlightEvent, 1, null, false);
                }
            }
            else if ((E.ID == "MovementModeChanged" || E.ID == "BodyPositionChanged") && FlightFlying)
            {
                if (E.HasFlag("Involuntary"))
                {
                    Flight.FailFlying(ParentObject, ParentObject, this);
                }
                else
                {
                    Flight.StopFlying(ParentObject, ParentObject, this, false, false);
                }
            }
            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ChangeLevel(Level);
            Flight.AbilitySetup(GO, GO, this);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            Flight.AbilityTeardown(GO, GO, this);
            return base.Unmutate(GO);
        }
    }
}