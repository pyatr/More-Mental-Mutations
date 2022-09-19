using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.World.Parts;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts.Effects
{
    [Serializable]
    public class MMM_EffectObfuscated : Effect
    {
        public GameObject HiddenObject;

        public MMM_EffectObfuscated()
        {
            this.DisplayName = "&cobfuscated";
        }

        public MMM_EffectObfuscated(int _Duration, GameObject _HiddenObject) : this()
        {            
            this.Duration = _Duration;
            this.HiddenObject = _HiddenObject;
        }

        public override string GetDetails()
        {
            return "Creatures with mind ignore you. (" + this.Duration.ToString() + " turns left).";
        }

        public override bool Apply(GameObject Object)
        {
            this.Obfuscate();
            return true;
        }

        public override void Remove(GameObject Object)
        {
            this.Unobfuscate();
        }

        public void Obfuscate()
        {
            GameObject.validate(ref this.HiddenObject);
            if (!this.HiddenObject.IsPlayer())
            {
                this.HiddenObject.pRender.Visible = false;
            }
        }

        public void Unobfuscate()
        {
            GameObject.validate(ref this.HiddenObject);
            List<GameObject> Creatures = new List<GameObject>(10);
            Physics part = this.HiddenObject.GetPart("Physics") as Physics;
            if (part != null && part.CurrentCell != null)
            {
                Creatures = part.CurrentCell.ParentZone.FastSquareSearch(part.CurrentCell.X, part.CurrentCell.Y, 40, "Combat");
            }

            if (!this.HiddenObject.IsPlayer())
            {
                this.HiddenObject.pRender.Visible = true;
            }

            foreach (GameObject GO in Creatures)
            {
                if (GO.HasEffect("MMM_EffectIgnoreObject"))
                {
                    GO.RemoveEffect("MMM_EffectIgnoreObject");
                }
            }
            this.HiddenObject = (GameObject)null;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterEffectEvent((Effect)this, "EndTurn");
            Object.RegisterEffectEvent((Effect)this, "AfterDeepCopyWithoutEffects");
            Object.RegisterEffectEvent((Effect)this, "BeforeDeepCopyWithoutEffects");
            //Object.RegisterEffectEvent((Effect)this, "BeginAttack");
            Object.RegisterEffectEvent((Effect)this, "PlayerBeginConversation");
            Object.RegisterEffectEvent((Effect)this, "BeginTakeAction");
            Object.RegisterEffectEvent((Effect)this, "MeleeAttackWithWeapon");
            Object.RegisterEffectEvent((Effect)this, "FiredMissileWeapon");
            Object.RegisterEffectEvent((Effect)this, "CommandThrowWeapon");
        }

        public override void Unregister(GameObject Object)
        {
            Object.UnregisterEffectEvent((Effect)this, "EndTurn");
            Object.UnregisterEffectEvent((Effect)this, "AfterDeepCopyWithoutEffects");
            Object.UnregisterEffectEvent((Effect)this, "BeforeDeepCopyWithoutEffects");
            //Object.UnregisterEffectEvent((Effect)this, "BeginAttack");
            Object.UnregisterEffectEvent((Effect)this, "PlayerBeginConversation");
            Object.UnregisterEffectEvent((Effect)this, "BeginTakeAction");
            Object.UnregisterEffectEvent((Effect)this, "MeleeAttackWithWeapon");
            Object.UnregisterEffectEvent((Effect)this, "FiredMissileWeapon");
            Object.UnregisterEffectEvent((Effect)this, "CommandThrowWeapon");
        }

        public override bool Render(RenderEvent E)
        {
            if (this.Duration <= 0)
                return true;
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
                 if (Defender.HasEffect("MMM_EffectIgnoreObject"))
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
            if (E.ID == "PlayerBeginConversation")
            {
                this.Duration = 0;
                this.Object.UseEnergy(1000);
                return false;
            }
            if (E.ID == "BeginTakeAction")
            {
                if (!GameObject.validate(ref this.HiddenObject))
                    this.Duration = 0;
                if (this.Duration > 0)
                {
                    Physics part = this.HiddenObject.GetPart("Physics") as Physics;
                    List<GameObject> Creatures = new List<GameObject>(10);
                    if (part != null && part.CurrentCell != null)
                    {
                        Creatures = part.CurrentCell.ParentZone.FastSquareSearch(part.CurrentCell.X, part.CurrentCell.Y, 40, "Combat");
                    }
                    foreach (GameObject c in Creatures)
                    {
                        if (c.HasPart("Brain") && c.HasPart("Combat") && !c.HasPart("MentalShield") && !c.HasPart("Clairvoyance"))
                        {
                            if (c != this.HiddenObject && !c.HasEffect("MMM_EffectIgnoreObject"))
                            {
                                c.ApplyEffect((Effect)new MMM_EffectIgnoreObject(this.HiddenObject, this.Duration));
                            }
                        }
                    }
                }
                return true;
            }
            if (E.ID == "MeleeAttackWithWeapon" || E.ID == "FiredMissileWeapon" || E.ID == "CommandThrowWeapon")
            {
                this.Duration = 0;
                return true;
            }
            if (E.ID == "EndTurn")
            {
                if (this.Duration > 0)                                   
                    --this.Duration;
                return true;
            }
            if (E.ID == "BeforeDeepCopyWithoutEffects")
                this.Unobfuscate();
            if (E.ID == "AfterDeepCopyWithoutEffects")
                this.Obfuscate();
            return base.FireEvent(E);
        }
    }
}