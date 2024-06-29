using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.World.Parts;
using XRL.Rules;

namespace XRL.World.Parts.Effects
{
    [Serializable]
    public class MMM_EffectPresence : Effect
    {
        public GameObject PresenceEmanator;
        public int Strength = 1;
        public int ChanceToFlee = 1;
        public bool HostilesNearby = false;
        public int Radius = 7;

        public MMM_EffectPresence()
        {
            this.DisplayName = "&oPresence";
        }

        public MMM_EffectPresence(int _Duration, int _Strength, GameObject _PresenceEmanator, int _ChanceToFlee, bool _HostilesNearby) : this()
        {
            this.DisplayName = "&oPresence";
            this.Duration = _Duration;
            this.Strength = _Strength;
            this.PresenceEmanator = _PresenceEmanator;
            this.ChanceToFlee = _ChanceToFlee;
            this.HostilesNearby = _HostilesNearby;
        }

        public override string GetDetails()
        {
            return (-this.Strength).ToString() + " to hostile/neutral to-hit/DV, " + (-this.Strength).ToString() + " to their willpower and willpower of friendly creatures if no hostiles are nearby. Friendly creatures get " + this.Strength.ToString() + " to their willpower/strength values" + " (" + this.Duration.ToString() + " turns left).";
        }

        public override bool Apply(GameObject Object)
        {
            GameObject.validate(ref this.PresenceEmanator);
            return true;
        }

        public override void Remove(GameObject Object)
        {
            GameObject.validate(ref this.PresenceEmanator);
            PresenceEmanator = (GameObject)null;
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

        public override bool Render(RenderEvent E)
        {
            if (this.Duration <= 0)
                return true;
            E.ColorString = "&O";
            return false;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn")
            {
                if (GameObject.validate(ref this.PresenceEmanator) && this.PresenceEmanator != null)
                {
                    List<GameObject> Creatures = new List<GameObject>();
                    Physics part = this.PresenceEmanator.GetPart("Physics") as Physics;
                    if (part != null && part.CurrentCell != null)
                    {
                        Creatures = part.CurrentCell.ParentZone.FastSquareSearch(part.CurrentCell.X, part.CurrentCell.Y, Radius, "Combat");
                    }

                    foreach (GameObject GO in Creatures)
                    {
                        if (GO.HasPart("Brain") && GO.HasPart("Combat") && !GO.HasPart("MentalShield"))
                        {
                            if (GO != this.PresenceEmanator && !GO.HasEffect("MMM_EffectUnderPresence"))
                            {
                                GO.ApplyEffect((Effect)new MMM_EffectUnderPresence(this.PresenceEmanator, this.Strength, 2, this.ChanceToFlee, this.HostilesNearby));
                            }
                        }
                    }
                    --this.Duration;
                }
                else
                    this.Duration = 0;
            }
            if (E.ID == "BeforeDeepCopyWithoutEffects")
                GameObject.validate(ref this.PresenceEmanator);
            if (E.ID == "AfterDeepCopyWithoutEffects")
                GameObject.validate(ref this.PresenceEmanator);
            return base.FireEvent(E);
        }
    }
}
