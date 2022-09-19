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
    public class MMM_EffectIgnoreObject : Effect
    {
        public GameObject ObjectToIgnore;
        public int PreviousFeeling;

        public MMM_EffectIgnoreObject()
        {
            this.DisplayName = "";
        }

        public MMM_EffectIgnoreObject(GameObject Object, int _Duration) : this()
        {
            this.ObjectToIgnore = Object;
            this.Duration = _Duration;
            this.DisplayName = "";// &Cignoring" + ObjectToIgnore.DisplayName + " located at " + ObjectToIgnore.pPhysics.CurrentCell.Pos2D.ToString();
        }

        public override string GetDetails()
        {
            return "";// "Someone's hiding from your attention.";
        }

        public override bool Apply(GameObject Object)
        {
            this.ApplyEffect();
            return true;
        }

        public override void Remove(GameObject Object)
        {
            this.RestoreFeeling();
        }

        public void ApplyEffect()
        {
            GameObject.validate(ref this.ObjectToIgnore);
            if (this.Object.GetPart<Brain>().PartyLeader != this.ObjectToIgnore)
            {
                this.Object.GetPart<Brain>().Target = (GameObject)null;
                if (this.Object.GetPart<Brain>().Goals != null)
                    this.Object.GetPart<Brain>().Goals.Clear();
                this.PreviousFeeling = this.Object.GetPart<Brain>().GetFeeling(this.ObjectToIgnore);
                this.Object.GetPart<Brain>().SetFeeling(this.ObjectToIgnore, 0);
            }
        }

        public void RestoreFeeling()
        {
            GameObject.validate(ref this.ObjectToIgnore);
            if (this.Object.GetPart<Brain>().PartyLeader != this.ObjectToIgnore)
                this.Object.GetPart<Brain>().SetFeeling(this.ObjectToIgnore, this.PreviousFeeling);
            this.ObjectToIgnore = (GameObject)null;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterEffectEvent((Effect)this, "EndTurn");
            Object.RegisterEffectEvent((Effect)this, "BeginTakeAction");
            Object.RegisterEffectEvent((Effect)this, "AfterDeepCopyWithoutEffects");
            Object.RegisterEffectEvent((Effect)this, "BeforeDeepCopyWithoutEffects");
        }

        public override void Unregister(GameObject Object)
        {
            Object.UnregisterEffectEvent((Effect)this, "EndTurn");
            Object.UnregisterEffectEvent((Effect)this, "BeginTakeAction");
            Object.UnregisterEffectEvent((Effect)this, "AfterDeepCopyWithoutEffects");
            Object.UnregisterEffectEvent((Effect)this, "BeforeDeepCopyWithoutEffects");
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeginTakeAction")
            {
                if (!GameObject.validate(ref this.ObjectToIgnore))
                    this.Duration = 0;
                if (this.Duration > 0)
                {
                    if (this.Object.GetPart<Brain>().GetFeeling(this.ObjectToIgnore) != 0)
                    {
                        this.PreviousFeeling = this.Object.GetPart<Brain>().GetFeeling(this.ObjectToIgnore);
                        this.ApplyEffect();
                    }
                    if (this.ObjectToIgnore.HasEffect("MMM_EffectObfuscated"))
                    {
                        if (this.Object.GetPart<Brain>().Target == this.ObjectToIgnore)
                        {
                            this.Object.GetPart<Brain>().Target = (GameObject)null;
                        }
                    }
                }
            }
            if (E.ID == "EndTurn")
            {
                if (this.ObjectToIgnore.HasEffect("MMM_EffectObfuscated") && this.Duration > 0)
                {
                    --this.Duration;
                }
                else
                {
                    this.RestoreFeeling();
                    this.Duration = 0;
                }
                return true;
            }
            if (E.ID == "BeforeDeepCopyWithoutEffects")
                this.RestoreFeeling();
            if (E.ID == "AfterDeepCopyWithoutEffects")
                this.ApplyEffect();
            return base.FireEvent(E);
        }
    }
}