using System;
using System.Collections.Generic;
using XRL.Messages;
using MoreMentalMutations.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    internal class MMM_DefectChemicalAddiction : MMM_BaseMutation
    {
        public int StrungOutSeverity = 1;
        public int GeneralTurns = 1000;
        public int TurnsToNextConsumption = 0;
        public bool GotConfusedFromRightThing = false;
        //public int intoxication = 0;

        private string[] AcceptableLiquids = { "wine", "cider" };

        public MMM_DefectChemicalAddiction()
        {
            //this.Name = nameof(DefectChemicalAddiction);
            DisplayName = "Chemical addiction (&rD&y)";
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EnteredCell");
            Registrar.Register("EndTurn");
            Registrar.Register("Eating");
            Registrar.Register("Drank");

            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            return "You have the unstoppable desire to consume mind-altering items at least every " + GeneralTurns.ToString() + " turns. Withdrawal gets worse every time you do not satisfy your need to get high, giving -1 or more penalty to your willpower.";
        }

        public override string GetLevelText(int Level)
        {
            return string.Empty;
        }

        public void GiveWithdrawal()
        {
            if (ParentObject.HasEffect<MMM_EffectStrungOut>())
            {
                ParentObject.RemoveEffect<MMM_EffectStrungOut>();

                if (ParentObject.IsPlayer())
                {
                    MessageQueue.AddPlayerMessage("&rYour withdrawal gets worse.&y");
                }
            }
            else if (ParentObject.IsPlayer())
            {
                MessageQueue.AddPlayerMessage("&rWithdrawal kicks in.");
            }

            ParentObject.ApplyEffect(new MMM_EffectStrungOut(StrungOutSeverity));
        }

        public void Relieve()
        {
            if (ParentObject.HasEffect<MMM_EffectStrungOut>())
            {
                ParentObject.RemoveEffect<MMM_EffectStrungOut>();

                if (ParentObject.IsPlayer())
                {
                    MessageQueue.AddPlayerMessage("&gYou are relieved of withdrawal.");
                }
            }

            StrungOutSeverity--;
        }

        public void FindSomethingToGetHighOn()
        {
            List<GameObject> inventory = ParentObject.GetPart<Inventory>().Objects;
            List<GameObject> ConfusingItems = new List<GameObject>();

            foreach (GameObject GO in inventory)
            {
                LiquidVolume liquid = GO.GetPart<LiquidVolume>();

                if (liquid != null)
                {

                    if (liquid.Volume > 0 && AcceptableLiquids.Contains(liquid.Primary))
                    {
                        ConfusingItems.Add(GO);
                    }
                }
                else if (GO.HasPart("ConfuseOnEat"))
                {
                    ConfusingItems.Add(GO);
                }
            }

            if (ConfusingItems.Count > 0)
            {
                GameObject GO2 = ConfusingItems[0];

                if (GO2.HasPart("LiquidVolume"))
                {
                    GO2.FireEvent(Event.New("InvCommandDrinkObject", "Owner", ParentObject));
                }
                else
                {
                    GO2.FireEvent(Event.New("InvCommandEatObject", "Owner", ParentObject));
                }
            }
        }

        public void CheckTurns()
        {
            if (TurnsToNextConsumption > 0)
            {
                //MessageQueue.AddPlayerMessage(this.TurnsToNextConsumption.ToString() + " turns until you have to drink again.");
                return;
            }
            else if (!ParentObject.AreHostilesNearby() && !ParentObject.OnWorldMap())//As not to get confused in inappropriate time
            {
                int i = 0;
                int stopbothering = 20;
                MessageQueue.AddPlayerMessage("&rYou look for something to drink or get high on in your pockets.");

                while (!ParentObject.HasEffect("Confused") && i < stopbothering)
                {
                    FindSomethingToGetHighOn();
                    i++;
                }
            }
            if (ParentObject.HasEffect("Confused"))
            {
                Relieve();
            }
            else
            {
                GiveWithdrawal();
                StrungOutSeverity++;
            }

            TurnsToNextConsumption = GeneralTurns;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EnteredCell" && ParentObject.OnWorldMap())
            {
                TurnsToNextConsumption -= 90;
                if (ParentObject.HasEffect("Confused"))
                {
                    if (GotConfusedFromRightThing)
                    {
                        Relieve();
                        GotConfusedFromRightThing = false;
                    }
                }
                else
                {
                    CheckTurns();
                }

                return true;
            }
            if (E.ID == "EndTurn" && !ParentObject.OnWorldMap())
            {
                //if (this.intoxication > 0) this.intoxication--;
                TurnsToNextConsumption--;
                if (ParentObject.HasEffect("Confused"))
                {
                    if (GotConfusedFromRightThing)
                    {
                        Relieve();
                        GotConfusedFromRightThing = false;
                    }
                }
                else
                {
                    CheckTurns();
                }

                return true;
            }

            if (E.ID == "Eating")
            {
                GameObject Food = E.GetParameter("Food") as GameObject;
                GotConfusedFromRightThing = Food.HasPart("ConfuseOnEat");

                return true;
            }

            if (E.ID == "Drank")
            {
                GameObject Drink = E.GetParameter("Object") as GameObject;
                GotConfusedFromRightThing = AcceptableLiquids.Contains(Drink.GetPart<LiquidVolume>().Primary);

                return true;
            }

            return base.FireEvent(E);
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            TurnsToNextConsumption = GeneralTurns;
            ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            return base.Unmutate(GO);
        }
    }
}