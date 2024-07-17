using System;
using XRL;
using XRL.World;

namespace MoreMentalMutations.Effects
{
    [Serializable]
    public class MMM_EffectStrungOut : Effect
    {
        public int Severity;

        public MMM_EffectStrungOut()
        {
            DisplayName = "&rstrung out";
        }

        public MMM_EffectStrungOut(int _Severity) : this()
        {
            Duration = 1;
            Severity = _Severity;
        }

        public override string GetDetails()
        {
            return "You need something to get high on (" + Severity.ToString() + " penalty to Willpower). Getting confused will end the pain for a time.";
        }

        public override bool Apply(GameObject Object)
        {
            Object.Statistics["Willpower"].Penalty += Severity;
            return true;
        }

        public override void Remove(GameObject Object)
        {
            Object.Statistics["Willpower"].Penalty -= Severity;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");

            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }
    }
}
