using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;
using MoreMentalMutations.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Presence : MMM_BaseMutation
    {
        public Guid PresenceActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry PresenceActivatedAbility;
        public Guid UnPresenceActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry UnPresenceActivatedAbility;
        public int ChanceToFlee = 10;
        public int PrBaseCooldown = 130;
        public int PrBaseDuration = 20;
        public int PrDurationPerLevel = 4;

        public MMM_Presence()
        {
            DisplayName = "Presence";
            Type = "Mental";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent(this, "CommandPresence");
            Object.RegisterPartEvent(this, "CommandUnPresence");
            Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");

            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            return "Your presence instills fear in hostile and neutral creatures and inspires friendly ones.";
        }

        public override string GetLevelText(int Level)
        {
            return "Hostile and neutral creatures get penalty to hit, DV and Willpower equal to your Ego modifier (multiplied by 2 for DV), -10 to movement speed and hostiles get " + ChanceToFlee.ToString() + "% chance to flee.\n" + "Friendly creatures get bonus to Willpower and Strength equal to your Ego modifier + 2. If no enemies are present nearby, friendly creatures get penalty to Willpower instead.\n" + "Duration: " + (PrBaseDuration + Level * PrDurationPerLevel).ToString() + " turns.\n" + "Cooldown: " + PrBaseCooldown.ToString() + " turns.";
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AIGetOffensiveMutationList")
            {
                int parameter1 = (int)E.GetParameter("Distance");
                List<AICommandList> parameter2 = (List<AICommandList>)E.GetParameter("List");

                if (PresenceActivatedAbility != null && PresenceActivatedAbility.Cooldown <= 0 && parameter1 <= 7)
                {
                    parameter2.Add(new AICommandList("CommandPresence", 1));
                }

                return true;
            }
            if (E.ID == "CommandPresence")
            {
                if (!ParentObject.HasEffect<MMM_EffectPresence>())
                {
                    ParentObject.ApplyEffect(new MMM_EffectPresence(PrBaseDuration + Level * PrDurationPerLevel, ParentObject.Statistics["Ego"].Modifier, ParentObject, ChanceToFlee, ParentObject.AreHostilesNearby()));
                    ParentObject.UseEnergy(1000, "Mental");
                    PresenceActivatedAbility.Cooldown = PrBaseCooldown * 10 + 10;
                }

                return true;
            }
            if (E.ID == "CommandUnPresence")
            {
                if (ParentObject.HasEffect<MMM_EffectPresence>())
                {
                    ParentObject.RemoveEffect<MMM_EffectPresence>();
                    ParentObject.UseEnergy(1000, "Mental");
                }

                return true;
            }
            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilities part = GetActivatedAbilities(GO);

            if (part != null)
            {
                PresenceActivatedAbilityID = part.AddAbility("Presence", "CommandPresence", "Mental Mutation");
                PresenceActivatedAbility = part.AbilityByGuid[PresenceActivatedAbilityID];
                // this.PresenceActivatedAbility.UITileDefault.Tile = "Mutations/presence.png";
                // this.PresenceActivatedAbility.UITileDefault.TileColor = "b";
                // this.PresenceActivatedAbility.UITileDefault.DetailColor = 'B';
                UnPresenceActivatedAbilityID = part.AddAbility("Cease presence", "CommandUnPresence", "Mental Mutation");
                UnPresenceActivatedAbility = part.AbilityByGuid[UnPresenceActivatedAbilityID];
            }

            ChangeLevel(Level);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMutationByGUID(GO, ref PresenceActivatedAbilityID);
            RemoveMutationByGUID(GO, ref UnPresenceActivatedAbilityID);

            return base.Unmutate(GO);
        }
    }
}
