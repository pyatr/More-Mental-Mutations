using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.World.Parts;

namespace XRL.World.Parts.Effects
{
    [Serializable]
    public class MMM_EffectGravityField : Effect
    {
        public int Level = 1;

        public MMM_EffectGravityField()
        {
            this.DisplayName = "&gGravity field";
        }

        public MMM_EffectGravityField(int MutationLevel, int _Duration)
          : this()
        {
            this.DisplayName = "&gGravity field";
            this.Duration = _Duration;
            this.Level = MutationLevel;
        }

        public override string GetDetails()
        {
            return (-this.Level * 2).ToString() + " to enemy accuracy, " + (this.Level / 2 + 2).ToString() + " to your AV, " + (this.Level / 3 + 1).ToString() + " to your DV (" + this.Duration.ToString() + " turns left).";
        }

        public override bool Apply(GameObject Object)
        {
            Object.ModIntProperty("IncomingAimModifier", this.Level * 2);
            Object.Statistics["AV"].Bonus += (this.Level / 2 + 2);
            Object.Statistics["DV"].Bonus += (this.Level / 3 + 1);
            return true;
        }

        public override void Remove(GameObject Object)
        {
            Object.ModIntProperty("IncomingAimModifier", -this.Level * 2);
            Object.Statistics["AV"].Bonus -= (this.Level / 2 + 2);
            Object.Statistics["DV"].Bonus -= (this.Level / 3 + 1);
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
            if (E.ID == "EndTurn")
            {
                --this.Duration;
            }
            return true;
        }
    }
}