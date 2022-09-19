using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.World.Parts;

namespace XRL.World.Parts.Effects
{
    [Serializable]
    public class MMM_EffectStrungOut : Effect
    {
        private int Severity;

        public MMM_EffectStrungOut()
        {
            this.DisplayName = "&rStrung out";
            this.Severity = 0;
        }
        
        public MMM_EffectStrungOut(int _Severity)
          : this()
        {
            this.DisplayName = "&rStrung out";
            this.Duration = 1;
            this.Severity = _Severity;
        }

        public override string GetDetails()
        {
            return "You need something to get high on (" + this.Severity.ToString() + " penalty to Willpower). Getting confused will end the pain for a time.";
        }

        public override bool Apply(GameObject Object)
        {
            Object.Statistics["Willpower"].Penalty += this.Severity;
            return true;
        }

        public override void Remove(GameObject Object)
        {
            Object.Statistics["Willpower"].Penalty -= this.Severity;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterEffectEvent((Effect)this, "EndTurn");
        }

        public override void Unregister(GameObject Object)
        {
            Object.UnregisterEffectEvent((Effect)this, "EndTurn");
        }

        public override bool FireEvent(Event E)
        {
            return base.FireEvent(E);
        }
    }
}
