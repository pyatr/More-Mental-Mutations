using System;
using System.Diagnostics;
using MoreMentalMutations.Opinions;
// using UnityEngine;
using XRL;
using XRL.Messages;
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
            if (!GameObject.Validate(ref ObjectToIgnore) || Object.Brain == null)
            {
                return;
            }

            if (Object.Brain.PartyLeader != ObjectToIgnore)
            {
                if (Object.Brain.Target == ObjectToIgnore)
                {
                    Object.Brain.Target = null;
                }

                Object.Brain.AddOpinion<OpinionObfuscate>(ObjectToIgnore);
            }
        }

        public void RestoreFeeling()
        {
            UnityEngine.Debug.Log(new StackTrace());

            if (!GameObject.Validate(ref ObjectToIgnore) || Object.Brain == null)
            {
                return;
            }

            if (Object.Brain.PartyLeader != ObjectToIgnore)
            {
                bool opinionRemoved = Object.Brain.RemoveOpinion<OpinionObfuscate>(ObjectToIgnore);
                MessageQueue.AddPlayerMessage(Object.Brain.GetFeeling(ObjectToIgnore).ToString() + "/" + opinionRemoved.ToString());
                //For some reason opinion removal can cause creature to become hostile if feeling restoration happened after save was loaded
            }

            ObjectToIgnore = null;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");
            Registrar.Register("BeginTakeAction");
            Registrar.Register("AfterDeepCopyWithoutEffects");
            Registrar.Register("BeforeDeepCopyWithoutEffects");

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
                    MMM_EffectObfuscated obfuscated = ObjectToIgnore.GetEffect<MMM_EffectObfuscated>();

                    if (obfuscated != null && obfuscated.HiddenObject == ObjectToIgnore && Object.Brain.Target == ObjectToIgnore)
                    {
                        Object.Brain.Target = null;

                        if (Object.Brain.GetFeeling(ObjectToIgnore) != 0)
                        {
                            // ApplyEffect();
                        }
                    }
                }

                return true;
            }

            if (E.ID == "EndTurn")
            {
                if (ObjectToIgnore.HasEffect<MMM_EffectObfuscated>() && Duration > 0)
                {
                    --Duration;
                }
                else
                {
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