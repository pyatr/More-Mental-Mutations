using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Effects
{
    [Serializable]
    public class MMM_EffectUnderPresence : Effect
    {
        public GameObject PresenceEmanator;
        private string TriggerEvent = "EndTurn";
        public int Strength;
        public int Feeling;
        public int ChanceToFlee;
        public bool HostilesNearby;

        public MMM_EffectUnderPresence()
        {
            this.DisplayName = "&OUnder someone's presence";
        }

        public MMM_EffectUnderPresence(GameObject _PresenceEmanator, int _Strength, int _Duration, int _ChanceToFlee, bool _HostilesNearby) : this()
        {
            this.PresenceEmanator = _PresenceEmanator;
            //this.DisplayName = "&OUnder " + _PresenceEmanator.DisplayName + "'s presence";
            this.DisplayName = "&OUnder someone's presence";
            this.Strength = _Strength;
            this.Duration = _Duration;
            this.ChanceToFlee = _ChanceToFlee;
            this.HostilesNearby = _HostilesNearby;
        }

        public override string GetDetails()
        {
            return "";
        }

        public override bool Apply(GameObject Object)
        {
            ApplyEffect();
            return true;
        }

        public override void Remove(GameObject Object)
        {
            UnapplyEffect();
        }

        public void ApplyEffect()
        {
            if (GameObject.validate(ref this.PresenceEmanator))
            {
                this.Feeling = Object.GetPart<Brain>().GetFeeling(this.PresenceEmanator);
                if (this.Feeling > 0)
                {
                    if (HostilesNearby)
                        Object.Statistics["Willpower"].Bonus += this.Strength;
                    else
                        Object.Statistics["Willpower"].Penalty += this.Strength;
                    Object.Statistics["Strength"].Bonus += this.Strength;
                }
                else
                {
                    Object.Statistics["Willpower"].Penalty += this.Strength;
                    Object.Statistics["MoveSpeed"].Penalty += 10;
                    Object.Statistics["DV"].Penalty += this.Strength * 2;
                    Object.ModIntProperty("HitBonus", -this.Strength);
                }
            }
        }

        public void UnapplyEffect()
        {
            if (this.Feeling > 0)
            {
                if (HostilesNearby)
                    Object.Statistics["Willpower"].Bonus -= this.Strength;
                else
                    Object.Statistics["Willpower"].Penalty -= this.Strength;
                Object.Statistics["Strength"].Bonus -= this.Strength;
            }
            else
            {
                Object.Statistics["Willpower"].Penalty -= this.Strength;
                Object.Statistics["MoveSpeed"].Penalty -= 10;
                Object.Statistics["DV"].Penalty -= this.Strength * 2;
                Object.ModIntProperty("HitBonus", this.Strength);
            }
            this.PresenceEmanator = (GameObject)null;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterEffectEvent((Effect)this, TriggerEvent);
            Object.RegisterEffectEvent((Effect)this, "AfterDeepCopyWithoutEffects");
            Object.RegisterEffectEvent((Effect)this, "BeforeDeepCopyWithoutEffects");
        }

        public override void Unregister(GameObject Object)
        {
            Object.UnregisterEffectEvent((Effect)this, TriggerEvent);
            Object.UnregisterEffectEvent((Effect)this, "AfterDeepCopyWithoutEffects");
            Object.UnregisterEffectEvent((Effect)this, "BeforeDeepCopyWithoutEffects");
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == TriggerEvent)
            {
                if (GameObject.validate(ref this.PresenceEmanator) && this.PresenceEmanator != null)
                {
                    //this.Object.ParticleText("My willpower penalty/bonus are " + this.Object.Statistics["Willpower"].Penalty + "/" + this.Object.Statistics["Willpower"].Bonus);
                    if (this.Feeling < 0)
                    {
                        if (Stat.Random(1, 100) < this.ChanceToFlee)
                        {
                            this.PerformMentalAttack(new Mental.Attack(Terrified.OfAttacker), this.PresenceEmanator, this.Object, this.PresenceEmanator, "Get scared by presence", "1d8", 1, Stat.Roll("3d3"));
                            //Fear.ApplyFearToObject("1d8", Stat.Roll("3d3"), this.Object, this.PresenceEmanator, this.PresenceEmanator);
                            //this.Object.ParticleText("&g*fleeing!*");
                        }
                    }
                    this.Duration--;
                }
                else
                    this.Duration = 0;
                return true;
            }
            if (E.ID == "BeforeDeepCopyWithoutEffects")
                this.UnapplyEffect();
            if (E.ID == "AfterDeepCopyWithoutEffects")
                this.ApplyEffect();
            return base.FireEvent(E);
        }
    }
}