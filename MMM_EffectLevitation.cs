using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts.Effects
{
    [Serializable]
    public class MMM_EffectLevitation : Effect
    {
        public GameObject Levitator;

        public MMM_EffectLevitation()
        {
            this.DisplayName = "&clevitation";
        }

        public MMM_EffectLevitation(int _Duration, GameObject parent) : this()
        {
            this.DisplayName = "&clevitation";
            this.Duration = _Duration;
            this.Levitator = parent;
        }

        public override string GetDetails()
        {
            return "You are levitating (" + this.Duration.ToString() + " turns left).";
        }

        public override bool Apply(GameObject Object)
        {
            GameObject.validate(ref this.Levitator);
            return true;
        }

        public override void Remove(GameObject Object)
        {
            GameObject.validate(ref this.Levitator);
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterEffectEvent((Effect)this, "EndTurn");
            Object.RegisterEffectEvent((Effect)this, "AfterDeepCopyWithoutEffects");
            Object.RegisterEffectEvent((Effect)this, "BeforeDeepCopyWithoutEffects");
        }

        public override void Unregister(GameObject Object)
        {
            Object.UnregisterEffectEvent((Effect)this, "EndTurn");
            Object.UnregisterEffectEvent((Effect)this, "AfterDeepCopyWithoutEffects");
            Object.UnregisterEffectEvent((Effect)this, "BeforeDeepCopyWithoutEffects");
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn")
            {
                --this.Duration;
                if (this.Duration == 0 && GameObject.validate(ref this.Levitator))
                {
                    MMM_Levitation l = this.Levitator.GetPart("MMM_Levitation") as MMM_Levitation;
                    if (l != null)
                        l.StopFlying();
                }
            }
            if (E.ID == "BeforeDeepCopyWithoutEffects")
                GameObject.validate(ref this.Levitator);
            if (E.ID == "AfterDeepCopyWithoutEffects")
                GameObject.validate(ref this.Levitator);
            return true;
        }
    }
}