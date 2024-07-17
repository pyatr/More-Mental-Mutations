using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;
using MoreMentalMutations.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Obfuscate : MMM_BaseMutation
    {
        public Guid ObfuscateActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry ObfuscateActivatedAbility;
        public Guid UnObfuscateActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry UnObfuscateActivatedAbility;

        public MMM_Obfuscate()
        {
            DisplayName = "Obfuscate";
            Type = "Mental";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent(this, "CommandObfuscate");
            Object.RegisterPartEvent(this, "CommandUnObfuscate");
            Object.RegisterPartEvent(this, "AIGetDefensiveMutationList");

            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            return "Creatures with mind ignore your presence.";
        }

        public override string GetLevelText(int Level)
        {
            return "Duration: " + (20 + Level * 3) + " rounds" + "\nCooldown: " + 110 + " rounds\n";
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AIGetDefensiveMutationList" && ObfuscateActivatedAbility.Cooldown <= 0)
            {
                E.GetParameter<List<AICommandList>>("List").Add(new AICommandList("CommandObfuscate", 1));
            }
            if (E.ID == "CommandObfuscate")
            {
                if (!ParentObject.HasEffect("EffectObfuscated"))
                {
                    ParentObject.ApplyEffect(new EffectObfuscated(20 + Level * 3, ParentObject));
                    ParentObject.UseEnergy(1000, "Mental");
                    ObfuscateActivatedAbility.Cooldown = 1110;
                    UnObfuscateActivatedAbility.Enabled = true;
                }
                return true;
            }
            if (E.ID == "CommandUnObfuscate")
            {
                if (ParentObject.HasEffect("EffectObfuscated"))
                {
                    ParentObject.RemoveEffect(typeof(EffectObfuscated));
                    UnObfuscateActivatedAbility.Enabled = false;
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
            ActivatedAbilities part = GO.GetPart<ActivatedAbilities>();

            if (part != null)
            {
                ObfuscateActivatedAbilityID = part.AddAbility("Obfuscate", "CommandObfuscate", "Mental Mutation");
                ObfuscateActivatedAbility = part.AbilityByGuid[ObfuscateActivatedAbilityID];
                UnObfuscateActivatedAbilityID = part.AddAbility("Disable obfuscation", "CommandUnObfuscate", "Mental Mutation");
                UnObfuscateActivatedAbility = part.AbilityByGuid[UnObfuscateActivatedAbilityID];
                UnObfuscateActivatedAbility.Enabled = true;
            }

            ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMutationByGUID(GO, ref ObfuscateActivatedAbilityID);
            RemoveMutationByGUID(GO, ref UnObfuscateActivatedAbilityID);

            return base.Unmutate(GO);
        }
    }
}
