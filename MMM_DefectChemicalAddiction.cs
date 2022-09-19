using System;
using System.Text;
using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;
using XRL.Liquids;
using XRL.Rules;
using XRL.Messages;
using XRL.UI;
using XRL.World.Parts.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    internal class MMM_DefectChemicalAddiction : BaseMutation
    {
        public int StrungOutSeverity = 1;
        public int GeneralTurns = 1000;
        public int TurnsToNextConsumption = 0;
        public bool GotConfusedFromRightThing = false;
        //public int intoxication = 0;

        public MMM_DefectChemicalAddiction()
        {
            this.Name = nameof(MMM_DefectChemicalAddiction);
            this.DisplayName = "Chemical addiction (&rD&y)";
        }

        public override bool CanLevel()
        {
            return false;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "EnteredCell");
            Object.RegisterPartEvent((IPart)this, "EndTurn");
            Object.RegisterPartEvent((IPart)this, "Eating");
            Object.RegisterPartEvent((IPart)this, "Drank");
        }

        public override string GetDescription()
        {
            return "You have the unstoppable desire to consume mind-altering items at least every " + this.GeneralTurns.ToString() + " turns. Withdrawal gets worse every time you do not satisfy your need to get high, giving -1 or more penalty to your willpower.";
        }

        public override string GetLevelText(int Level)
        {
            return string.Empty;
        }

        public void GiveWithdrawal()
        {
            if (this.ParentObject.HasEffect("MMM_EffectStrungOut"))
            {
                this.ParentObject.RemoveEffect("MMM_EffectStrungOut");
                if (this.ParentObject.IsPlayer())
                {
                    MessageQueue.AddPlayerMessage("&rYour withdrawal gets worse.&y");
                }
            }
            else
            {
                if (this.ParentObject.IsPlayer())
                {
                    MessageQueue.AddPlayerMessage("&rWithdrawal kicks in.");
                }
            }
            this.ParentObject.ApplyEffect((Effect)new MMM_EffectStrungOut(this.StrungOutSeverity));
        }

        public void Relieve()
        {
            if (this.ParentObject.HasEffect("MMM_EffectStrungOut"))
            {
                this.ParentObject.RemoveEffect("MMM_EffectStrungOut");
                if (this.ParentObject.IsPlayer())
                {
                    MessageQueue.AddPlayerMessage("&gYou are relieved of withdrawal.");
                }
            }
            this.StrungOutSeverity--;
        }

        public void FindSomethingToGetHighOn()
        {
            List<GameObject> inventory = this.ParentObject.GetPart<Inventory>().Objects;
            List<GameObject> ConfusingItems = new List<GameObject>();
            foreach (GameObject GO in inventory)
            {
                if (GO.HasPart("LiquidVolume"))
                {
                    if (GO.GetPart<LiquidVolume>().Volume > 0 && (GO.GetPart<LiquidVolume>().Primary == "wine" || GO.GetPart<LiquidVolume>().Primary == "cider"))
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
                    GO2.FireEvent(Event.New("InvCommandDrinkObject", "Owner", (object)this.ParentObject));                
                else                
                    GO2.FireEvent(Event.New("InvCommandEatObject", "Owner", (object)this.ParentObject));              
            }
        }

        public void CheckTurns()
        {
            if (this.TurnsToNextConsumption > 0)
            {
                //MessageQueue.AddPlayerMessage(this.TurnsToNextConsumption.ToString() + " turns until you have to drink again.");
                return;
            }
            else if (!this.ParentObject.AreHostilesNearby() && !this.ParentObject.OnWorldMap())//As not to get confused in inappropriate time
            {
                int i = 0;
                int stopbothering = 20;

                MessageQueue.AddPlayerMessage("&rYou look for something to drink or get high on in your pockets.");
                while (!this.ParentObject.HasEffect("Confused") && i < stopbothering)
                {
                    this.FindSomethingToGetHighOn();
                    i++;
                }
            }
            if (this.ParentObject.HasEffect("Confused"))
            {
                this.Relieve();
            }
            else
            {
                this.GiveWithdrawal();
                this.StrungOutSeverity++;
            }
            this.TurnsToNextConsumption = this.GeneralTurns;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EnteredCell" && this.ParentObject.OnWorldMap())
            {
                this.TurnsToNextConsumption -= 90;
                if (this.ParentObject.HasEffect("Confused"))
                {
                    if (this.GotConfusedFromRightThing)
                    {
                        this.Relieve();
                        this.GotConfusedFromRightThing = false;
                    }
                }
                else
                    this.CheckTurns();
                return true;
            }
            if (E.ID == "EndTurn" && !this.ParentObject.OnWorldMap())
            {
                //if (this.intoxication > 0) this.intoxication--;
                this.TurnsToNextConsumption--;
                if (this.ParentObject.HasEffect("Confused"))
                {
                    if (this.GotConfusedFromRightThing)
                    {
                        this.Relieve();
                        this.GotConfusedFromRightThing = false;
                    }
                }
                else
                    this.CheckTurns();
                return true;
            }
            if (E.ID == "Eating")
            {
                GameObject Food = E.GetParameter("Food") as GameObject;
                if(Food.HasPart("ConfuseOnEat"))
                    this.GotConfusedFromRightThing = true;
                else
                    this.GotConfusedFromRightThing = false;
                return true;
            }
            if (E.ID == "Drank")
            {
                GameObject Drink = E.GetParameter("Object") as GameObject;
                if (Drink.GetPart<LiquidVolume>().Primary == "wine" || Drink.GetPart<LiquidVolume>().Primary == "cider")
                    this.GotConfusedFromRightThing = true;
                else
                    this.GotConfusedFromRightThing = false;

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
            this.TurnsToNextConsumption = this.GeneralTurns;
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            return base.Unmutate(GO);
        }
    }
}