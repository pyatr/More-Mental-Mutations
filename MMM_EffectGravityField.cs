using System;
using XRL;
using XRL.World;

namespace MoreMentalMutations.Effects
{
    [Serializable]
    public class MMM_EffectGravityField : Effect
    {
        public int Level = 1;

        //Required for deseralization!
        public MMM_EffectGravityField()
        {
            DisplayName = "&ggravity field";
        }

        public MMM_EffectGravityField(int MutationLevel, int _Duration) : this()
        {
            Duration = _Duration;
            Level = MutationLevel;
        }

        public override string GetDetails()
        {
            return (-Level * 2).ToString() + " to enemy accuracy, " + (Level / 2 + 2).ToString() + " to your AV, " + (Level / 3 + 1).ToString() + " to your DV (" + Duration.ToString() + " turns left).";
        }

        public override bool Apply(GameObject Object)
        {
            Object.ModIntProperty("IncomingAimModifier", Level * 2);
            Object.Statistics["AV"].Bonus += Level / 2 + 2;
            Object.Statistics["DV"].Bonus += Level / 3 + 1;

            return true;
        }

        public override void Remove(GameObject Object)
        {
            Object.ModIntProperty("IncomingAimModifier", -Level * 2);
            Object.Statistics["AV"].Bonus -= Level / 2 + 2;
            Object.Statistics["DV"].Bonus -= Level / 3 + 1;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");

            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn")
            {
                --Duration;
            }

            return true;
        }
    }
}