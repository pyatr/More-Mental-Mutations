using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;
using XRL.World.Parts.Effects;
using XRL.Messages;
using XRL.UI;
using XRL.Rules;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Presence : BaseMutation
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
            this.DisplayName = "Presence";
            this.Type = "Mental";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "CommandPresence");
            Object.RegisterPartEvent((IPart)this, "CommandUnPresence");
            Object.RegisterPartEvent((IPart)this, "AIGetOffensiveMutationList");
        }

        public override string GetDescription()
        {
            return "Your presence instills fear in hostile and neutral creatures and inspires friendly ones.";
        }

        public override string GetLevelText(int Level)
        {
            return "Hostile and neutral creatures get penalty to hit, DV and Willpower equal to your Ego modifier (multiplied by 2 for DV), -10 to movement speed and hostiles get " + this.ChanceToFlee.ToString() + "% chance to flee.\n" + "Friendly creatures get bonus to Willpower and Strength equal to your Ego modifier + 2. If no enemies are present nearby, friendly creatures get penalty to Willpower instead.\n" + "Duration: " + (this.PrBaseDuration + Level * this.PrDurationPerLevel).ToString() + " turns.\n" + "Cooldown: " + this.PrBaseCooldown.ToString() + " turns.";
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AIGetOffensiveMutationList")
            {
                int parameter1 = (int)E.GetParameter("Distance");
                List<AICommandList> parameter2 = (List<AICommandList>)E.GetParameter("List");
                if (this.PresenceActivatedAbility != null && this.PresenceActivatedAbility.Cooldown <= 0 && parameter1 <= 7)
                {
                    parameter2.Add(new AICommandList("CommandPresence", 1));
                }
                return true;
            }
            if (E.ID == "CommandPresence")
            {
                if (!this.ParentObject.HasEffect("MMM_EffectPresence"))
                {
                    this.ParentObject.ApplyEffect((Effect)new MMM_EffectPresence(this.PrBaseDuration + this.Level * this.PrDurationPerLevel, this.ParentObject.Statistics["Ego"].Modifier, this.ParentObject, this.ChanceToFlee, this.ParentObject.AreHostilesNearby()));
                    this.ParentObject.UseEnergy(1000, "Mental");
                    this.PresenceActivatedAbility.Cooldown = this.PrBaseCooldown * 10 + 10;
                }
                return true;
            }
            if (E.ID == "CommandUnPresence")
            {
                if (this.ParentObject.HasEffect("MMM_EffectPresence"))
                {
                    this.ParentObject.RemoveEffect("MMM_EffectPresence");
                    this.ParentObject.UseEnergy(1000, "Mental");
                }
                return true;
            }
            return true;
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilities part = GO.GetPart("ActivatedAbilities") as ActivatedAbilities;
            if (part != null)
            {
                this.PresenceActivatedAbilityID = part.AddAbility("Presence", "CommandPresence", "Mental Mutation");
                this.PresenceActivatedAbility = part.AbilityByGuid[this.PresenceActivatedAbilityID];
                this.UnPresenceActivatedAbilityID = part.AddAbility("Cease presence", "CommandUnPresence", "Mental Mutation");
                this.UnPresenceActivatedAbility = part.AbilityByGuid[this.UnPresenceActivatedAbilityID];
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if (this.PresenceActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.PresenceActivatedAbilityID);
                this.PresenceActivatedAbilityID = Guid.Empty;
            }
            if (this.UnPresenceActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.UnPresenceActivatedAbilityID);
                this.UnPresenceActivatedAbilityID = Guid.Empty;
            }
            return base.Unmutate(GO);
        }
    }
}
