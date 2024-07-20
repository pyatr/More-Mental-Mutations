using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;
using MoreMentalMutations.Effects;
using UnityEngine;
using XRL.Messages;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Obfuscate : MMM_BaseMutation
    {
        public Guid ObfuscateActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry ObfuscateActivatedAbility;
        public Guid UnObfuscateActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry UnObfuscateActivatedAbility;

        public int Cooldown = 110;
        public int BaseDuration = 20;
        public int DurationPerLevel = 3;

        public MMM_Obfuscate()
        {
            DisplayName = "Obfuscate";
            Type = "Mental";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");
            Registrar.Register("CommandObfuscate");
            Registrar.Register("CommandUnObfuscate");
            Registrar.Register("AIGetDefensiveMutationList");

            base.Register(Object, Registrar);
        }

        public override void CollectStats(Templates.StatCollector stats)
        {
            stats.CollectCooldownTurns(MyActivatedAbility(ObfuscateActivatedAbilityID), Cooldown);
        }

        public override string GetDescription()
        {
            return "Creatures with mind ignore your presence.";
        }

        public override string GetLevelText(int Level)
        {
            return $"Duration: {BaseDuration + Level * DurationPerLevel} rounds" + $"\nCooldown: {Cooldown} rounds.";
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AIGetDefensiveMutationList" && ObfuscateActivatedAbility.Cooldown <= 0)
            {
                E.GetParameter<List<AICommandList>>("List").Add(new AICommandList("CommandObfuscate", 1));
            }
            if (E.ID == "EndTurn")
            {
                if (!ParentObject.HasEffect<MMM_EffectObfuscated>())
                {
                    UnObfuscateActivatedAbility.Enabled = false;
                }
            }
            if (E.ID == "CommandObfuscate")
            {
                if (!ParentObject.HasEffect<MMM_EffectObfuscated>())
                {
                    ParentObject.ApplyEffect(new MMM_EffectObfuscated(20 + Level * 3, ParentObject));
                    ParentObject.UseEnergy(1000, "Mental");
                    ObfuscateActivatedAbility.Cooldown = 1110;
                    UnObfuscateActivatedAbility.Enabled = true;
                }

                return true;
            }
            if (E.ID == "CommandUnObfuscate")
            {
                if (ParentObject.HasEffect<MMM_EffectObfuscated>())
                {
                    ParentObject.RemoveEffect(typeof(MMM_EffectObfuscated));
                    ParentObject.UseEnergy(1000, "Mental");
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
                UnObfuscateActivatedAbility.Enabled = false;
            }

            ChangeLevel(Level);
            DescribeMyActivatedAbility(ObfuscateActivatedAbilityID, new Action<Templates.StatCollector>(CollectStats));

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
