using System;
using System.Collections.Generic;
using System.Linq;
using MoreMentalMutations.Opinions;
using XRL;
using XRL.Messages;
using XRL.World;
using XRL.World.AI;

namespace MoreMentalMutations.Effects
{
    [Serializable]
    public class MMM_EffectIgnoreObject : Effect
    {
        public GameObject ObjectToIgnore;
        [NonSerialized]
        public Dictionary<IOpinion, float> CachedOpinions = new();
        public int DVPenalty = 4;

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

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            Writer.Write(CachedOpinions);

            base.Write(Basis, Writer);
        }

        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            CachedOpinions = Reader.ReadDictionary<IOpinion, float>();

            base.Read(Basis, Reader);
        }

        private void PrintCachedOpinions()
        {
            foreach (KeyValuePair<IOpinion, float> opinion in CachedOpinions)
            {
                MessageQueue.AddPlayerMessage(opinion.Key.GetType().ToString() + "/" + opinion.Value);
            }
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
                ClearPlayerTarget();
                Object.Brain.AddOpinion<OpinionObfuscate>(ObjectToIgnore);
                UpdateOpinions();
            }
        }

        public void UpdateOpinions()
        {
            MMM_EffectObfuscated obfuscated = ObjectToIgnore.GetEffect<MMM_EffectObfuscated>();

            if (obfuscated != null && obfuscated.HiddenObject == ObjectToIgnore)
            {
                Object.Brain.TryGetOpinions(ObjectToIgnore, out OpinionList opinionList);

                foreach (IOpinion opinionOnHiddenObject in opinionList)
                {
                    if (opinionOnHiddenObject.GetType() == typeof(OpinionObfuscate))
                    {
                        continue;
                    }

                    if (!CachedOpinions.ContainsKey(opinionOnHiddenObject))
                    {
                        CachedOpinions.Add(opinionOnHiddenObject, opinionOnHiddenObject.Magnitude);
                    }

                    opinionOnHiddenObject.Magnitude = 0;
                }

                opinionList.RefreshTotal();
                ClearPlayerTarget();
            }
        }

        public void RestoreFeeling()
        {
            if (!GameObject.Validate(ref ObjectToIgnore) || Object.Brain == null)
            {
                return;
            }

            if (Object.Brain.PartyLeader != ObjectToIgnore)
            {
                Object.Brain.RemoveOpinion<OpinionObfuscate>(ObjectToIgnore);
                //For some reason opinion removal can cause creature to become hostile if feeling restoration happened after save was loaded
                //So we have to clear expired opinions
                Object.Brain.TryGetOpinions(ObjectToIgnore, out OpinionList opinionList);

                foreach (IOpinion opinionOnHiddenObject in opinionList)
                {
                    // opinionOnHiddenObject.Magnitude = CachedOpinions[opinionOnHiddenObject];
                    bool hasKey = false;// CachedOpinions.TryGetValue(opinionOnHiddenObject, out float cachedMagnitude);
                    float cachedMagnitude = 0;

                    //If we're getting value after loading a game it gives 0 instead of 1 but always gives 1
                    //Let's just leave it that way
                    foreach (KeyValuePair<IOpinion, float> opinion in CachedOpinions)
                    {
                        if (opinion.Key.GetType() == opinionOnHiddenObject.GetType())
                        {
                            cachedMagnitude = opinion.Value;
                            hasKey = true;
                            break;
                        }
                    }

                    if (hasKey && cachedMagnitude > 0)
                    {
                        opinionOnHiddenObject.Magnitude = cachedMagnitude;
                        // MessageQueue.AddPlayerMessage("Restored magniutude on " + opinionOnHiddenObject.GetType().ToString() + " so that means " + opinionOnHiddenObject.Value);
                    }
                }

                opinionList.RefreshTotal();
                Object.Brain.Opinions.ClearExpired();
            }

            ObjectToIgnore = null;
        }

        public void ClearPlayerTarget()
        {
            if (Object.Brain.Target == ObjectToIgnore)
            {
                Object.Brain.Target = null;
            }
        }

        //It's kinda buggy with save/load but there's no need for it anyway since this effect doesn't last too long
        public void ClearOldOpinions()
        {
            for (int i = 0; i < CachedOpinions.Count; i++)
            {
                IOpinion opinion = CachedOpinions.Keys.ToArray()[i];
                Object.Brain.TryGetOpinions(ObjectToIgnore, out OpinionList opinionList);

                if (!opinionList.Contains(opinion))
                {
                    // MessageQueue.AddPlayerMessage("Removed opinion on " + opinion.GetType().ToString() + " with mag " + opinion.Magnitude);
                    CachedOpinions.Remove(opinion);
                }
            }
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");
            Registrar.Register("BeginTakeAction");
            Registrar.Register("GetDefenderDV");
            Registrar.Register("AfterDeepCopyWithoutEffects");
            Registrar.Register("BeforeDeepCopyWithoutEffects");

            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "GetDefenderDV")
            {
                GameObject attacker = E.GetParameter<GameObject>("Attacker");

                if (attacker == ObjectToIgnore)
                {
                    E.SetParameter("DV", E.GetIntParameter("DV") - DVPenalty);
                }

                return true;
            }

            if (E.ID == "BeginTakeAction" || E.ID == "EndTurn")
            {
                if (!GameObject.Validate(ref ObjectToIgnore))
                {
                    Duration = 0;
                }

                if (Duration <= 0)
                {
                    return true;
                }

                UpdateOpinions();

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