using System;
using XRL;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace MoreMentalMutations.Effects
{
    [Serializable]
    public class MMM_EffectLevitation : Effect
    {
        public GameObject Levitator;

        public MMM_EffectLevitation()
        {
            DisplayName = "&clevitation";
        }

        public MMM_EffectLevitation(int _Duration, GameObject parent) : this()
        {
            Duration = _Duration;
            Levitator = parent;
        }

        public override string GetDetails()
        {
            return "You are levitating (" + Duration.ToString() + " turns left).";
        }

        public override bool Apply(GameObject Object)
        {
            GameObject.Validate(ref Levitator);
            return true;
        }

        public override void Remove(GameObject Object)
        {
            GameObject.Validate(ref Levitator);
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");
            Registrar.Register("AfterDeepCopyWithoutEffects");
            Registrar.Register("BeforeDeepCopyWithoutEffects");

            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn")
            {
                --Duration;

                if (Duration == 0 && GameObject.Validate(ref Levitator))
                {
                    Levitator.GetPart<MMM_Levitation>()?.StopFlying();
                }
            }

            if (E.ID == "BeforeDeepCopyWithoutEffects")
            {
                GameObject.Validate(ref Levitator);
            }

            if (E.ID == "AfterDeepCopyWithoutEffects")
            {
                GameObject.Validate(ref Levitator);
            }
            return true;
        }
    }
}