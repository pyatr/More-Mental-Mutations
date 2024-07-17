using System;
using System.Collections.Generic;
using XRL;
using XRL.Messages;
using XRL.World;
using XRL.World.Parts;

namespace MoreMentalMutations.Effects
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
            DisplayName = "&opresence";
        }

        public MMM_EffectPresence(int _Duration, int _Strength, GameObject _PresenceEmanator, int _ChanceToFlee, bool _HostilesNearby) : this()
        {
            Duration = _Duration;
            Strength = _Strength;
            PresenceEmanator = _PresenceEmanator;
            ChanceToFlee = _ChanceToFlee;
            HostilesNearby = _HostilesNearby;
        }

        public override string GetDetails()
        {
            return (-Strength).ToString() + " to hostile/neutral to-hit/DV, " + (-Strength).ToString() + " to their willpower and willpower of friendly creatures if no hostiles are nearby. Friendly creatures get " + Strength.ToString() + " to their willpower/strength values" + " (" + Duration.ToString() + " turns left).";
        }

        public override bool Apply(GameObject Object)
        {
            GameObject.Validate(ref PresenceEmanator);

            return true;
        }

        public override void Remove(GameObject Object)
        {
            GameObject.Validate(ref PresenceEmanator);
            PresenceEmanator = null;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");
            Registrar.Register("AfterDeepCopyWithoutEffects");
            Registrar.Register("BeforeDeepCopyWithoutEffects");

            base.Register(Object, Registrar);
        }

        public override bool Render(RenderEvent E)
        {
            if (Duration <= 0)
            {
                return true;
            }

            E.ColorString = "&O";

            return false;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn")
            {
                bool validated = GameObject.Validate(ref PresenceEmanator);
                bool hasPresenceEmanator = PresenceEmanator != null;

                if (validated && hasPresenceEmanator)
                {
                    List<GameObject> Creatures = new List<GameObject>();
                    Physics part = PresenceEmanator.GetPart<Physics>();

                    if (part != null && part.CurrentCell != null)
                    {
                        Creatures = part.CurrentCell.ParentZone.FastSquareSearch(part.CurrentCell.X, part.CurrentCell.Y, Radius, "Combat");
                    }

                    foreach (GameObject GO in Creatures)
                    {
                        MMM_EffectUnderPresence underPresence = GO.GetEffect<MMM_EffectUnderPresence>();

                        if (!GO.HasPart<Brain>() ||
                            !GO.HasPart<Combat>() ||
                            GO.HasPart<MentalShield>() ||
                            GO == PresenceEmanator ||
                            (underPresence != null && underPresence.PresenceEmanator == PresenceEmanator))
                        {
                            continue;
                        }

                        GO.ApplyEffect(new MMM_EffectUnderPresence(PresenceEmanator, Strength, 2, ChanceToFlee, HostilesNearby));
                    }

                    --Duration;
                }
                else
                {
                    Duration = 0;
                }
            }

            if (E.ID == "BeforeDeepCopyWithoutEffects")
            {
                GameObject.Validate(ref PresenceEmanator);
            }

            if (E.ID == "AfterDeepCopyWithoutEffects")
            {
                GameObject.Validate(ref PresenceEmanator);
            }

            return base.FireEvent(E);
        }
    }
}
