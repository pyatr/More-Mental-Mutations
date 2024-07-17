using System;
using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace MoreMentalMutations.Effects
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
            DisplayName = "&OUnder someone's presence";
        }

        public MMM_EffectUnderPresence(GameObject _PresenceEmanator, int _Strength, int _Duration, int _ChanceToFlee, bool _HostilesNearby) : this()
        {
            PresenceEmanator = _PresenceEmanator;
            //this.DisplayName = "&OUnder " + _PresenceEmanator.DisplayName + "'s presence";
            DisplayName = "&OUnder someone's presence";
            Strength = _Strength;
            Duration = _Duration;
            ChanceToFlee = _ChanceToFlee;
            HostilesNearby = _HostilesNearby;
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
            if (!GameObject.Validate(ref PresenceEmanator))
            {

            }

            Feeling = Object.Brain.GetFeeling(PresenceEmanator);
            if (Feeling > 0)
            {
                if (HostilesNearby)
                {
                    Object.Statistics["Willpower"].Bonus += Strength;
                }
                else
                {
                    Object.Statistics["Willpower"].Penalty += Strength;
                }

                Object.Statistics["Strength"].Bonus += Strength;
            }
            else
            {
                Object.Statistics["Willpower"].Penalty += Strength;
                Object.Statistics["MoveSpeed"].Penalty += 10;
                Object.Statistics["DV"].Penalty += Strength * 2;
                Object.ModIntProperty("HitBonus", -Strength);
            }
        }

        public void UnapplyEffect()
        {
            if (Feeling > 0)
            {
                if (HostilesNearby)
                    Object.Statistics["Willpower"].Bonus -= Strength;
                else
                    Object.Statistics["Willpower"].Penalty -= Strength;
                Object.Statistics["Strength"].Bonus -= Strength;
            }
            else
            {
                Object.Statistics["Willpower"].Penalty -= Strength;
                Object.Statistics["MoveSpeed"].Penalty -= 10;
                Object.Statistics["DV"].Penalty -= Strength * 2;
                Object.ModIntProperty("HitBonus", Strength);
            }

            PresenceEmanator = null;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterEffectEvent(this, TriggerEvent);
            Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
            Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");

            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == TriggerEvent)
            {
                if (GameObject.Validate(ref PresenceEmanator) && PresenceEmanator != null)
                {
                    //this.Object.ParticleText("My willpower penalty/bonus are " + this.Object.Statistics["Willpower"].Penalty + "/" + this.Object.Statistics["Willpower"].Bonus);
                    if (Feeling < 0)
                    {
                        if (Stat.Random(1, 100) < ChanceToFlee)
                        {
                            PerformMentalAttack(new Mental.Attack(Terrified.OfAttacker), PresenceEmanator, Object, PresenceEmanator, "Get scared by presence", "1d8", 1, Stat.Roll("3d3"));
                            //Fear.ApplyFearToObject("1d8", Stat.Roll("3d3"), this.Object, this.PresenceEmanator, this.PresenceEmanator);
                            //this.Object.ParticleText("&g*fleeing!*");
                        }
                    }
                    Duration--;
                }
                else
                {
                    Duration = 0;
                }

                return true;
            }

            if (E.ID == "BeforeDeepCopyWithoutEffects")
            {
                UnapplyEffect();
            }

            if (E.ID == "AfterDeepCopyWithoutEffects")
            {
                ApplyEffect();
            }
            return base.FireEvent(E);
        }
    }
}