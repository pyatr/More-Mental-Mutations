using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Chimerstry : BaseMutation
    {
        public Guid DummyActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry DummyActivatedAbility;

        public MMM_Chimerstry()
        {
            this.DisplayName = "Chimerstry";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "CommandDummy");
        }

        public override string GetDescription()
        {
            return "";
        }

        public override string GetLevelText(int Level)
        {
            return "";
        }

        public override bool FireEvent(Event E)
        {
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
                this.DummyActivatedAbilityID = part.AddAbility("huh", "CommandDummy", "Mental Mutation");
                this.DummyActivatedAbility = part.AbilityByGuid[this.DummyActivatedAbilityID];
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if (this.DummyActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.DummyActivatedAbilityID);
                this.DummyActivatedAbilityID = Guid.Empty;
            }
            return base.Unmutate(GO);
        }
    }
}
