using System;
using MoreMentalMutations.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_GravityField : MMM_BaseMutation
    {
        public Guid GravityFieldActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry GravityFieldActivatedAbility;

        public MMM_GravityField()
        {
            DisplayName = "Gravity Field";
            Type = "Mental";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent(this, "CommandGravityField");

            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            return "Incoming projectiles are guided away from you, all incoming attacks are softened and are easier to dodge.";
        }

        public override string GetLevelText(int Level)
        {
            return (-Level * 2).ToString()
                + " to enemy missile weapon accuracy, "
                + (Level / 2 + 2).ToString()
                + " to your AV, "
                + (this.Level / 3 + 1).ToString()
                + " to your DV.\n"
                + "Duration: "
                + (20 + Level * 2).ToString()
                + "\nCooldown: "
                + (123 - Level * 3).ToString();
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandGravityField")
            {
                if (!ParentObject.HasEffect("EffectGravityField"))
                {
                    ParentObject.ApplyEffect(new MMM_EffectGravityField(Level, 20 + Level * 2));
                    ParentObject.UseEnergy(1000);
                    GravityFieldActivatedAbility.Cooldown = 1240 - 30 * Level;
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
                GravityFieldActivatedAbilityID = part.AddAbility("Gravity field", "CommandGravityField", "Mental Mutation");
                GravityFieldActivatedAbility = part.AbilityByGuid[GravityFieldActivatedAbilityID];
            }

            ChangeLevel(Level);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMutationByGUID(GO, ref GravityFieldActivatedAbilityID);

            return base.Unmutate(GO);
        }
    }
}
