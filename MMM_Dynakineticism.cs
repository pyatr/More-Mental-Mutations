using System;
using System.Collections.Generic;
using System.Threading;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI.GoalHandlers;
using XRL.World;
using XRL.World.Parts;
using XRL.Liquids;
using XRL.World.Parts.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Dynakineticism : BaseMutation
    {
        public Guid DynakineticismActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry DynakineticismActivatedAbility;
        public int Cooldown = 25;

        public MMM_Dynakineticism()
        {
            this.DisplayName = "Dynakineticism";
            this.Type = "Mental";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "CommandDynakineticism");
            Object.RegisterPartEvent((IPart)this, "AIGetOffensiveMutationList");
        }

        public string GetDamageDice(int Level)
        {
            //return ((Level + 1) / 2).ToString() + "d4";
            string s = "d";
            int d1, d2;
            int gradeLevel = 6;
            d1 = Level / gradeLevel + 1;
            d2 = Level % gradeLevel + 5;
            return d1.ToString() + s + d2.ToString();
        }

        public override string GetDescription()
        {
            //return "You apply the pressure to a 3x3 area, crushing objects inside it.";
            return "You creature pressure within your target, crushing it from inside";
        }

        public override string GetLevelText(int Level)
        {
            return "Cooldown: " + this.Cooldown + " rounds\n"/* + "Range: 8\n"*/ + "Damage: " + this.GetDamageDice(Level);
        }

        public void Attack(Cell C)
        {
            int num = Math.Max(this.ParentObject.Statistics["Ego"].Modifier, this.Level - 2);
            if (C != null && C.GetObjectsInCell().Count > 0)
            {
                //bool bSomeoneDied = false;
                List<GameObject> CurrentObjectsInCell = C.GetObjectsInCell();
                foreach (GameObject GO in CurrentObjectsInCell)
                {
                    if (GO.HasPart("Combat"))
                    {
                        int Result = Stat.RollDamagePenetrations(GO.Statistics["Toughness"].Modifier, num, num);
                        Result += 2;//Because good toughness is almost as common as good AV
                        int _Amount = 0;
                        for (int index = 0; index < Result; ++index)
                        {
                            _Amount += Stat.Roll(this.GetDamageDice(this.Level));
                        }

                        if (_Amount == 0)
                        {
                            if (this.ParentObject.IsPlayer())
                                XRLCore.Core.Game.Player.Messages.Add("&rYou fail to do any damage to " + GO.DisplayName + ".");
                        }
                        else
                        {
                            Damage damage = new Damage(_Amount);
                            damage.AddAttribute("Physical");
                            string resultColor = Stat.GetResultColor(Result);
                            //XRLCore.Core.Game.Player.Messages.Add(GO.DisplayName + " has " + GO.Statistics["Hitpoints"].Value.ToString() + " hp, while he is supposed to be hit for " + damage.Amount.ToString() + " hp.");
                            if (GO.Statistics["Hitpoints"].Value <= damage.Amount && !GO.HasPart("Robot"))
                            {
                                Body TargetBody = GO.GetPart<Body>();
                                List<BodyPart> AllBodyParts = (GO.GetPart("Body") as Body).GetParts();
                                List<BodyPart> BodyPartsToLose = new List<BodyPart>();
                                int BodyPartsToLoseAmount = 0;
                                foreach (BodyPart bodyPart in AllBodyParts)
                                {
                                    if (bodyPart.Type != "Body" && bodyPart.Type != "Back" && (bodyPart.Type != "Floating Nearby" && bodyPart.Type != "Missile Weapon") && (bodyPart.Type != "Thrown Weapon" && !bodyPart.Type.Contains("Ammo") && bodyPart.Type != "Hands"))
                                    {
                                        if (bodyPart.GetParentPart().Type == "Body")
                                        {
                                            BodyPartsToLoseAmount++;
                                            BodyPartsToLose.Add(bodyPart);
                                        }
                                    }
                                }
                                int i = 0;
                                for (i = BodyPartsToLoseAmount; i > 0; i--)
                                {
                                    GameObject severedBodyPart = TargetBody.Dismember(BodyPartsToLose[i - 1]);
                                    Physics part = severedBodyPart.GetPart("Physics") as Physics;
                                    part.Push(Directions.GetRandomDirection(), 10, 1);
                                }

                                if (GO.HasPart("Corpse"))
                                    GO.GetPart<Corpse>().CorpseChance = 0;

                                int ToughMod = GO.Statistics["Toughness"].Modifier;
                                if (ToughMod < 1) ToughMod = 2;

                                int randombones = Stat.Random(1, ToughMod);

                                for (i = 0; i < randombones; i++)
                                {
                                    GameObject NewBones = GameObjectFactory.Factory.CreateObject("Bones");
                                    C.AddObject(NewBones);
                                    Physics part = NewBones.GetPart("Physics") as Physics;
                                    part.Push(Directions.GetRandomDirection(), 10, 1);
                                }

                                i = Stat.Random(5, 5 + ToughMod * 2);
                                do
                                {
                                    Physics part0 = GO.GetPart("Physics") as Physics;
                                    GO.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                                    GO.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                                    GO.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                                    GO.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                                    GO.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                                    GO.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                                    GO.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                                    GO.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                                    i--;
                                } while (i > 0);
                                //C.AddObject(GO);
                                //bSomeoneDied = true;
                            }

                            Event E = Event.New("TakeDamage", 0, 0, 0);
                            E.AddParameter("Damage", (object)damage);
                            E.AddParameter("Owner", (object)this.ParentObject);
                            E.AddParameter("Attacker", (object)this.ParentObject);

                            if (!GO.FireEvent(E) || damage.Amount == 0)
                            {
                                if (GO.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add("&rThe " + this.ParentObject.DisplayName + " tries to explode you, but fails.");
                                else if (this.ParentObject.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add("You fail to do any damage to " + GO.the + GO.DisplayName + ".");
                            }
                            else
                            {
                                if (GO.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add("&rThe " + this.ParentObject.DisplayName + " harms you " + resultColor + "(x" + Result.ToString() + ")&y for " + damage.Amount.ToString() + " damage!");
                                else if (this.ParentObject.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add("&gYou apply pressure to " + GO.DisplayName + resultColor + "(x" + Result.ToString() + ")&y for " + damage.Amount.ToString() + " damage!");
                            }
                        }
                    }
                    else if (/*GO.HasTag("Wall") || GO.HasPart("Door")*/GO.pPhysics.Solid == true)
                    {
                        int Result = Stat.RollDamagePenetrations(GO.Statistics["AV"].Value, num, num);
                        Result *= 2;
                        int _Amount = 0;
                        for (int index = 0; index < Result; ++index)
                        {
                            _Amount += Stat.Roll(this.GetDamageDice(this.Level));
                        }

                        if (_Amount == 0)
                        {
                            if (this.ParentObject.IsPlayer())
                                XRLCore.Core.Game.Player.Messages.Add("&rYou fail to do any damage to " + GO.DisplayName + ".");
                        }
                        else
                        {
                            Damage damage = new Damage(_Amount);
                            damage.AddAttribute("Physical");
                            string resultColor = Stat.GetResultColor(Result);
                            //XRLCore.Core.Game.Player.Messages.Add(GO.DisplayName + " has " + GO.Statistics["Hitpoints"].Value.ToString() + " hp, while he is supposed to be hit for " + damage.Amount.ToString() + " hp.");

                            Event E = Event.New("TakeDamage", 0, 0, 0);
                            E.AddParameter("Damage", (object)damage);
                            E.AddParameter("Owner", (object)this.ParentObject);
                            E.AddParameter("Attacker", (object)this.ParentObject);

                            if (!GO.FireEvent(E) || damage.Amount == 0)
                            {
                                if (GO.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add("&rThe " + this.ParentObject.DisplayName + " tries to explode you, but fails.");
                                else if (this.ParentObject.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add("You fail to do any damage to " + GO.the + GO.DisplayName + ".");
                            }
                            else
                            {
                                if (GO.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add("&rThe " + this.ParentObject.DisplayName + " harms you " + resultColor + "(x" + Result.ToString() + ")&y for " + damage.Amount.ToString() + " damage!");
                                else if (this.ParentObject.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add("&gYou apply pressure to " + GO.DisplayName + resultColor + "(x" + Result.ToString() + ")&y for " + damage.Amount.ToString() + " damage!");
                            }
                        }
                    }
                }

                //What was that?
                /*
                if (bSomeoneDied)
                {
                    foreach (GameObject GO2 in C.GetObjectsInCell())
                    {
                        if (!GO2.HasPart("Brain"))
                        {
                            //Somehow throws everything instead of just the object
                            Physics part = GO2.GetPart("Physics") as Physics;
                            part.Push(Directions.GetRandomDirection(), 10, 1);
                        }
                    }
                }
                */
            }
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AIGetOffensiveMutationList")
            {
                int parameter1 = (int)E.GetParameter("Distance");
                GameObject parameter2 = E.GetParameter("Target") as GameObject;
                List<AICommandList> parameter3 = (List<AICommandList>)E.GetParameter("List");
                if (this.DynakineticismActivatedAbility != null && this.DynakineticismActivatedAbility.Cooldown <= 0 && (parameter1 <= 80 && this.ParentObject.HasLOSTo(parameter2, true) && this.ParentObject.pPhysics.CurrentCell.DistanceTo(parameter2) <= 8))
                    parameter3.Add(new AICommandList("CommandDynakineticism", 1));
                return true;
            }
            if (E.ID == "CommandDynakineticism")
            {
                Cell C = this.PickDestinationCell(80, AllowVis.OnlyVisible, true);
                this.Attack(C);
                /*List<Cell> Cells = this.PickBurst(1, 8, false, AllowVis.OnlyVisible);
                foreach (Cell cell in Cells)
                {
                    if (cell.DistanceTo(this.ParentObject) > 9)
                    {
                        if (this.ParentObject.IsPlayer())
                            Popup.Show("Target is out of range! (8 cells)", true);
                        return true;
                    }
                }
                foreach (Cell cell in Cells)
                {
                    this.Attack(cell);
                }*/
                Thread.Sleep(50);
                this.DynakineticismActivatedAbility.Cooldown = Cooldown * 10;
                this.UseEnergy(1000, "Mental");
            }
            return true;
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            this.Unmutate(GO);
            ActivatedAbilities part = GO.GetPart("ActivatedAbilities") as ActivatedAbilities;
            this.DynakineticismActivatedAbilityID = part.AddAbility("Dynakineticism", "CommandDynakineticism", "Mental Mutation");
            this.DynakineticismActivatedAbility = part.AbilityByGuid[this.DynakineticismActivatedAbilityID];
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if (this.DynakineticismActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.DynakineticismActivatedAbilityID);
                this.DynakineticismActivatedAbilityID = Guid.Empty;
            }
            return base.Unmutate(GO);
        }
    }
}