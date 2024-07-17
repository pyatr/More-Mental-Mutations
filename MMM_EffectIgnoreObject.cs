using System;
using MoreMentalMutations.Opinions;
using XRL;
using XRL.World;

namespace MoreMentalMutations.Effects
{
    [Serializable]
    public class MMM_EffectIgnoreObject : Effect
    {
        public GameObject ObjectToIgnore;

        public MMM_EffectIgnoreObject()
        {
            DisplayName = "";
        }

        public MMM_EffectIgnoreObject(GameObject Object, int _Duration) : this()
        {
            ObjectToIgnore = Object;
            Duration = _Duration;
            // DisplayName = "&Cignoring" + ObjectToIgnore.DisplayName + " located at " + ObjectToIgnore.Physics.CurrentCell.Pos2D.ToString();
        }

        public override string GetDetails()
        {
            return "";
            // return "Someone's hiding from your attention.";
        }

        public override bool Apply(GameObject Object)
        {
            ApplyEffect();

            return true;
        }

        public override void Remove(GameObject Object)
        {
            RestoreFeeling();
        }

        public void ApplyEffect()
        {
            GameObject.Validate(ref ObjectToIgnore);

            if (Object.Brain.PartyLeader != ObjectToIgnore)
            {
                Object.Brain.Target = null;
                Object.Brain.AddOpinion<OpinionObfuscate>(ObjectToIgnore, 1);
                Object.Brain.Forgive(ObjectToIgnore);
            }
        }

        public void RestoreFeeling()
        {
            GameObject.Validate(ref ObjectToIgnore);

            if (Object.Brain.PartyLeader != ObjectToIgnore)
            {
                Object.Brain.RemoveOpinion<OpinionObfuscate>(ObjectToIgnore);
            }

            ObjectToIgnore = null;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterEffectEvent(this, "EndTurn");
            Object.RegisterEffectEvent(this, "BeginTakeAction");
            Object.RegisterEffectEvent(this, "AfterDeepCopyWithoutEffects");
            Object.RegisterEffectEvent(this, "BeforeDeepCopyWithoutEffects");

            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "BeginTakeAction")
            {
                if (!GameObject.Validate(ref ObjectToIgnore))
                {
                    Duration = 0;
                }

                if (Duration > 0)
                {
                    if (Object.Brain.GetFeeling(ObjectToIgnore) != 0)
                    {
                        ApplyEffect();
                    }

                    if (ObjectToIgnore.HasEffect<MMM_EffectObfuscated>())
                    {
                        if (Object.Brain.Target == ObjectToIgnore)
                        {
                            Object.Brain.Target = null;
                        }
                    }
                }
            }

            if (E.ID == "EndTurn")
            {
                if (ObjectToIgnore.HasEffect<MMM_EffectObfuscated>() && Duration > 0)
                {
                    --Duration;
                }
                else
                {
                    RestoreFeeling();
                    Duration = 0;
                }

                return true;
            }

            if (E.ID == "BeforeDeepCopyWithoutEffects")
            {
                RestoreFeeling();
            }

            if (E.ID == "AfterDeepCopyWithoutEffects")
            {
                ApplyEffect();
            }

            return base.FireEvent(E);
        }
    }
}