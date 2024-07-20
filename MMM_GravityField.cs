using System;
using MoreMentalMutations.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_GravityField : MMM_BaseMutation
    {
        public Guid GravityFieldActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry GravityFieldActivatedAbility;
        public int EnemyMissilePenaltyPerLevel = 2;
        public int AVBonusPerLevel = 2;
        public int DVBonusPerLevel = 3;
        public int BaseAVBonus = 2;
        public int BaseDVBonus = 1;
        public int BaseDuration = 20;
        public int BaseCooldown = 103;
        public int DurationPerLevel = 2;
        public int CooldownDecreasePerLevel = 3;

        public MMM_GravityField()
        {
            DisplayName = "Gravity Field";
            Type = "Mental";
        }

        public override void CollectStats(Templates.StatCollector stats)
        {
            stats.CollectCooldownTurns(MyActivatedAbility(GravityFieldActivatedAbilityID), BaseCooldown - Level * CooldownDecreasePerLevel);
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("CommandGravityField");

            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            return "Incoming projectiles are guided away from you, all incoming attacks are softened and are easier to dodge.";
        }

        public override string GetLevelText(int Level)
        {
            return (-Level * EnemyMissilePenaltyPerLevel).ToString()
                + " to enemy missile weapon accuracy, "
                + (Level / AVBonusPerLevel + BaseAVBonus).ToString()
                + " to your AV, "
                + (Level / DVBonusPerLevel + BaseDVBonus).ToString()
                + " to your DV.\n"
                + "Duration: "
                + (BaseDuration + Level * DurationPerLevel).ToString()
                + "\nCooldown: "
                + (BaseCooldown - Level * CooldownDecreasePerLevel).ToString();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandGravityField")
            {
                if (ParentObject.HasEffect<MMM_EffectGravityField>())
                {
                    return true;
                }

                ParentObject.ApplyEffect(new MMM_EffectGravityField(Level, BaseDuration + Level * DurationPerLevel));
                ParentObject.UseEnergy(1000);
                GravityFieldActivatedAbility.Cooldown = BaseCooldown * 10 - CooldownDecreasePerLevel * 10 * Level;

                return true;
            }

            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            DescribeMyActivatedAbility(GravityFieldActivatedAbilityID, new Action<Templates.StatCollector>(CollectStats));

            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilities part = GetActivatedAbilities(GO);

            if (part != null)
            {
                GravityFieldActivatedAbilityID = part.AddAbility("Gravity field", "CommandGravityField", "Mental Mutation");
                GravityFieldActivatedAbility = part.AbilityByGuid[GravityFieldActivatedAbilityID];
            }

            ChangeLevel(Level);
            DescribeMyActivatedAbility(GravityFieldActivatedAbilityID, new Action<Templates.StatCollector>(CollectStats));

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMutationByGUID(GO, ref GravityFieldActivatedAbilityID);

            return base.Unmutate(GO);
        }
    }
}
