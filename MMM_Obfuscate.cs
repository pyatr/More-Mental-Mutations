using System;
using System.Collections.Generic;
using XRL.World.Parts.Effects;
using XRL.Messages;
using XRL.UI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Obfuscate : BaseMutation
    {
        public Guid ObfuscateActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry ObfuscateActivatedAbility;
        public Guid UnObfuscateActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry UnObfuscateActivatedAbility;

        public MMM_Obfuscate()
        {
            this.DisplayName = "Obfuscate";
            this.Type = "Mental";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "CommandObfuscate");
            Object.RegisterPartEvent((IPart)this, "CommandUnObfuscate");
            Object.RegisterPartEvent((IPart)this, "AIGetDefensiveMutationList");
        }

        public override string GetDescription()
        {
            return "You make creatures with mind ignore your presence.";
        }

        public override string GetLevelText(int Level)
        {
            return "Duration: " + (object)(20 + Level * 3) + " rounds" + "\nCooldown: " + (object)(110) + " rounds\n";
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AIGetDefensiveMutationList" && this.ObfuscateActivatedAbility.Cooldown <= 0)
                E.GetParameter<List<AICommandList>>("List").Add(new AICommandList("CommandObfuscate", 1));
            if (E.ID == "CommandObfuscate")
            {
                if (!this.ParentObject.HasEffect("MMM_EffectObfuscated"))
                {
                    this.ParentObject.ApplyEffect((Effect)new MMM_EffectObfuscated(20 + this.Level * 3, this.ParentObject));
                    this.ParentObject.UseEnergy(1000, "Mental");
                    this.ObfuscateActivatedAbility.Cooldown = 1110;
                }
                return true;
            }
            if (E.ID == "CommandUnObfuscate")
            {
                if (this.ParentObject.HasEffect("MMM_EffectObfuscated"))                
                    this.ParentObject.RemoveEffect("MMM_EffectObfuscated");
                //this.ParentObject.UseEnergy(1000, "Mental");
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
                this.ObfuscateActivatedAbilityID = part.AddAbility("Obfuscate", "CommandObfuscate", "Mental Mutation");
                this.ObfuscateActivatedAbility = part.AbilityByGuid[this.ObfuscateActivatedAbilityID];
                this.UnObfuscateActivatedAbilityID = part.AddAbility("Disable obfuscation", "CommandUnObfuscate", "Mental Mutation");
                this.UnObfuscateActivatedAbility = part.AbilityByGuid[this.UnObfuscateActivatedAbilityID];
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if (this.ObfuscateActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.ObfuscateActivatedAbilityID);
                this.ObfuscateActivatedAbilityID = Guid.Empty;
            }
            if (this.UnObfuscateActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.UnObfuscateActivatedAbilityID);
                this.UnObfuscateActivatedAbilityID = Guid.Empty;
            }
            return base.Unmutate(GO);
        }
    }
}
