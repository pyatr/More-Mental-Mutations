using System;
using System.Collections.Generic;
using XRL.World.Parts.Mutation;
using XRL.World.Parts;
using XRL.World;
using XRL;

namespace MoreMentalMutations.Effects
{
    [Serializable]
    public class EffectObfuscated : Effect
    {
        public GameObject HiddenObject;
        public new string DisplayName = "&cobfuscated";

        public EffectObfuscated(int _Duration, GameObject _HiddenObject)
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
            GameObject.Validate(ref HiddenObject);

            if (!HiddenObject.IsPlayer())
            {
                HiddenObject.Render.Visible = false;
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
                if (GO.HasEffect("EffectIgnoreObject"))
                {
                    GO.RemoveEffect<MMM_EffectIgnoreObject>();
                }
            }

            HiddenObject = null;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterEffectEvent(this, "EndTurn");
            Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
            Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");
            //Object.RegisterEffectEvent((Effect)this, "BeginAttack");
            Object.RegisterEffectEvent(this, "BeginConversation");
            Object.RegisterEffectEvent(this, "BeginTakeAction");
            Object.RegisterEffectEvent(this, "MeleeAttackWithWeapon");
            Object.RegisterEffectEvent(this, "FiredMissileWeapon");
            Object.RegisterEffectEvent(this, "CommandThrowWeapon");

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
            /* if (E.ID == "MeleeAttackWithWeapon")//not working, whatever
             {
                 if (this.Duration <= 0)
                     return true;
                 GameObject Defender = E.GetParameter("Defender") as GameObject;
                 GameObject Weapon = E.GetParameter("Weapon") as GameObject;
                 if (Defender.HasEffect("EffectIgnoreObject"))
                 {
                     //this.Object.ParticleText("Defend yourself, " + Defender.DisplayName);
                     Event E1 = Event.New("MeleeAttackWithWeapon", 0, 0, 0);
                     E1.AddParameter("Attacker", (object)this.HiddenObject);
                     E1.AddParameter("Defender", (object)Defender);
                     E1.AddParameter("Weapon", (object)Weapon);
                     E1.AddParameter("Properties", "Autohit,Critical");

                     this.Duration = 0;
                     this.HiddenObject.FireEvent(E1);
                 }
                 return true;
             }*/

            if (E.ID == "BeginConversation")
            {
                Duration = 0;
                Object.UseEnergy(1000);

                return false;
            }

            if (E.ID == "BeginTakeAction")
            {
                if (!GameObject.Validate(ref HiddenObject))
                {
                    Duration = 0;
                }

                if (Duration > 0)
                {
                    Physics hiddenObjectPhysics = HiddenObject.Physics;
                    List<GameObject> Creatures = new List<GameObject>(10);

                    if (hiddenObjectPhysics != null && hiddenObjectPhysics.CurrentCell != null)
                    {
                        Creatures = hiddenObjectPhysics.CurrentCell.ParentZone.FastSquareSearch(hiddenObjectPhysics.CurrentCell.X, hiddenObjectPhysics.CurrentCell.Y, 80, "Combat");
                    }

                    foreach (GameObject c in Creatures)
                    {
                        if (IsTargetViable(c))
                        {
                            if (c != HiddenObject && !c.HasEffect("EffectIgnoreObject"))
                            {
                                c.ApplyEffect(new MMM_EffectIgnoreObject(HiddenObject, Duration));
                            }
                        }
                    }
                }

                return true;
            }

            if (E.ID == "MeleeAttackWithWeapon" || E.ID == "FiredMissileWeapon" || E.ID == "CommandThrowWeapon")
            {
                Duration = 0;
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