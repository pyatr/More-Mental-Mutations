using System;
using System.Collections.Generic;
using XRL.Messages;
using XRL.UI;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_GravityField : BaseMutation
    {
        public Guid GravityFieldActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry GravityFieldActivatedAbility;

        public MMM_GravityField()
        {
            this.DisplayName = "Gravity Field";
            this.Type = "Mental";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "CommandGravityField");
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
                if (!this.ParentObject.HasEffect("MMM_EffectGravityField"))
                {
                    this.ParentObject.ApplyEffect(
                        (Effect)new MMM_EffectGravityField(this.Level, 20 + this.Level * 2)
                    );
                    this.ParentObject.UseEnergy(1000);
                    this.GravityFieldActivatedAbility.Cooldown = 1240 - 30 * this.Level;
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
            ActivatedAbilities part = GO.GetPart("ActivatedAbilities") as ActivatedAbilities;
            if (part != null)
            {
                this.GravityFieldActivatedAbilityID = part.AddAbility(
                    "Gravity field",
                    "CommandGravityField",
                    "Mental Mutation"
                );
                this.GravityFieldActivatedAbility = part.AbilityByGuid[
                    this.GravityFieldActivatedAbilityID
                ];
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if (this.GravityFieldActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(
                    this.GravityFieldActivatedAbilityID
                );
                this.GravityFieldActivatedAbilityID = Guid.Empty;
            }
            return base.Unmutate(GO);
        }
    }
}
