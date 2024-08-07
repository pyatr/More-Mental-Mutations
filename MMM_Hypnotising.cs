using System;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Hypnotising : MMM_BaseMutation
    {
        public Guid DummyActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry DummyActivatedAbility;

        public MMM_Hypnotising()
        {
            DisplayName = "Hypnotising";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("CommandDummy");

            base.Register(Object, Registrar);
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
            ActivatedAbilities part = GetActivatedAbilities(GO);

            if (part != null)
            {
                DummyActivatedAbilityID = part.AddAbility("huh", "CommandDummy", "Mental Mutation");
                DummyActivatedAbility = part.AbilityByGuid[DummyActivatedAbilityID];
            }

            ChangeLevel(Level);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMutationByGUID(GO, ref DummyActivatedAbilityID);

            return base.Unmutate(GO);
        }
    }
}
