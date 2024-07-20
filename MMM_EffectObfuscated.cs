using System;
using System.Collections.Generic;
using XRL.World.Parts.Mutation;
using XRL.World.Parts;
using XRL.World;
using XRL;
using XRL.Messages;

namespace MoreMentalMutations.Effects
{
    [Serializable]
    public class MMM_EffectObfuscated : Effect
    {
        public GameObject HiddenObject;

        [NonSerialized]
        public List<string> obfuscationBreakEvents = new() { "BeforeMeleeAttack", "FiredMissileWeapon", "BeginAttack" };

        public MMM_EffectObfuscated()
        {
            DisplayName = "&cobfuscated";
        }

        public MMM_EffectObfuscated(int _Duration, GameObject _HiddenObject) : this()
        {
            Duration = _Duration;
            HiddenObject = _HiddenObject;
        }

        public override string GetDetails()
        {
            return "Creatures with mind ignore you. (" + Duration.ToString() + " turns left).";
        }

        public bool IsTargetViable(GameObject c)
        {
            return c.HasPart<Brain>() && c.HasPart<Combat>() && !c.HasPart<MentalShield>() && !c.HasPart<Clairvoyance>();
        }

        public override bool Apply(GameObject Object)
        {
            Obfuscate();

            return true;
        }

        public override void Remove(GameObject Object)
        {
            Unobfuscate();
        }

        public void Obfuscate()
        {
            if (!GameObject.Validate(ref HiddenObject))
            {
                return;
            }

            if (!HiddenObject.IsPlayer() && !IsTargetViable(The.Player))
            {
                HiddenObject.Render.Visible = false;
            }

            Physics hiddenObjectPhysics = HiddenObject.Physics;
            List<GameObject> Creatures = new List<GameObject>(10);

            if (hiddenObjectPhysics != null && hiddenObjectPhysics.CurrentCell != null)
            {
                Creatures = hiddenObjectPhysics.CurrentCell.ParentZone.FastSquareSearch(hiddenObjectPhysics.CurrentCell.X, hiddenObjectPhysics.CurrentCell.Y, 80, "Combat");
            }

            foreach (GameObject creature in Creatures)
            {
                if (!IsTargetViable(creature))
                {
                    continue;
                }

                MMM_EffectIgnoreObject ignoreOnObject = creature.GetEffect<MMM_EffectIgnoreObject>();

                //If creature is not object itself, if it has no ignore effect or its ignore effect is from other creature
                if (creature != HiddenObject &&
                   (ignoreOnObject == null ||
                   (ignoreOnObject != null && ignoreOnObject.ObjectToIgnore != HiddenObject)))
                {
                    creature.ApplyEffect(new MMM_EffectIgnoreObject(HiddenObject, Duration));
                }
            }
        }

        public void Unobfuscate()
        {
            GameObject.Validate(ref HiddenObject);
            List<GameObject> Creatures = new List<GameObject>(10);
            Physics hiddenObjectPhysics = HiddenObject.Physics;

            if (hiddenObjectPhysics != null && hiddenObjectPhysics.CurrentCell != null)
            {
                Creatures = hiddenObjectPhysics.CurrentCell.ParentZone.FastSquareSearch(hiddenObjectPhysics.CurrentCell.X, hiddenObjectPhysics.CurrentCell.Y, 80, "Combat");
            }

            if (!HiddenObject.IsPlayer())
            {
                HiddenObject.Render.Visible = true;
            }

            foreach (GameObject GO in Creatures)
            {
                MMM_EffectIgnoreObject ignore = GO.GetEffect<MMM_EffectIgnoreObject>();

                if (ignore != null && ignore.ObjectToIgnore == HiddenObject)
                {
                    GO.RemoveEffect(ignore);
                }
            }

            HiddenObject = null;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");
            Registrar.Register("AfterDeepCopyWithoutEffects");
            Registrar.Register("BeforeDeepCopyWithoutEffects");
            Registrar.Register("BeginConversation");
            Registrar.Register("BeginTakeAction");

            foreach (string breakEvent in obfuscationBreakEvents)
            {
                Registrar.Register(breakEvent);
            }

            base.Register(Object, Registrar);
        }

        public override bool Render(RenderEvent E)
        {
            if (Duration <= 0)
            {
                return true;
            }

            E.ColorString = "&c";

            return false;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeginConversation")
            {
                Duration = 0;

                if (GameObject.Validate(ref HiddenObject))
                {
                    HiddenObject.UseEnergy(1000);
                }

                return false;
            }

            if (E.ID == "BeginTakeAction")
            {
                if (!GameObject.Validate(ref HiddenObject))
                {
                    Duration = 0;

                    return true;
                }

                Obfuscate();

                return true;
            }

            if (obfuscationBreakEvents.Contains(E.ID) && Duration > 0)
            {
                Duration = 0;
                MessageQueue.AddPlayerMessage("Obfuscation broken!");

                return true;
            }

            if (E.ID == "EndTurn")
            {
                if (Duration > 0)
                {
                    --Duration;
                }

                return true;
            }

            if (E.ID == "BeforeDeepCopyWithoutEffects")
            {
                Unobfuscate();
            }

            if (E.ID == "AfterDeepCopyWithoutEffects")
            {
                Obfuscate();
            }

            return base.FireEvent(E);
        }
    }
}