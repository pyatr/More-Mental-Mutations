using System;
using System.Collections.Generic;
using System.Threading;
using XRL.Core;
using XRL.Rules;
using XRL.World.AI.GoalHandlers;
using XRL.World.Anatomy;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Dynakineticism : MMM_BaseMutation
    {
        public Guid DynakineticismActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry DynakineticismActivatedAbility;
        public int Cooldown = 25;

        public MMM_Dynakineticism()
        {
            DisplayName = "Dynakineticism";
            Type = "Mental";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent(this, "CommandDynakineticism");
            Object.RegisterPartEvent(this, "AIGetOffensiveMutationList");

            base.Register(Object, Registrar);
        }

        public string GetDamageDice(int Level)
        {
            string s = "d";
            int d1, d2;
            int gradeLevel = 6;
            d1 = Level / gradeLevel + 1;
            d2 = Level % gradeLevel + 5;

            return d1.ToString() + s + d2.ToString();
        }

        public override string GetDescription()
        {
            return "You creature pressure within your target, crushing it from inside";
        }

        public override string GetLevelText(int Level)
        {
            return $"Cooldown: {Cooldown} rounds\nDamage: {GetDamageDice(Level)}";
        }

        public void Attack(Cell C)
        {
            int num = Math.Max(ParentObject.Statistics["Ego"].Modifier, Level - 2);
            if (C != null && C.GetObjectsInCell().Count > 0)
            {
                List<GameObject> CurrentObjectsInCell = C.GetObjectsInCell();
                foreach (GameObject GO in CurrentObjectsInCell)
                {
                    if (GO.HasPart("Combat"))
                    {
                        int Result = Stat.RollDamagePenetrations(
                            GO.Statistics["Toughness"].Modifier,
                            num,
                            num
                        );
                        Result += 2; //Because good toughness is almost as common as good AV
                        int _Amount = 0;
                        for (int index = 0; index < Result; ++index)
                        {
                            _Amount += Stat.Roll(GetDamageDice(Level));
                        }

                        if (_Amount == 0)
                        {
                            if (ParentObject.IsPlayer())
                                XRLCore.Core.Game.Player.Messages.Add(
                                    "&rYou fail to do any damage to " + GO.DisplayName + "."
                                );
                        }
                        else
                        {
                            Damage damage = new Damage(_Amount);
                            damage.AddAttribute("Physical");
                            string resultColor = Stat.GetResultColor(Result);
                            //XRLCore.Core.Game.Player.Messages.Add(GO.DisplayName + " has " + GO.Statistics["Hitpoints"].Value.ToString() + " hp, while he is supposed to be hit for " + damage.Amount.ToString() + " hp.");
                            if (
                                GO.Statistics["Hitpoints"].Value <= damage.Amount
                                && !GO.HasPart("Robot")
                                && GO.GetPhase() == ParentObject.GetPhase()
                            )
                            {
                                Body targetBody = GO.GetPart<Body>();
                                List<BodyPart> AllBodyParts = targetBody.GetParts();
                                List<BodyPart> BodyPartsToLose = new List<BodyPart>();
                                int BodyPartsToLoseAmount = 0;
                                foreach (BodyPart bodyPart in AllBodyParts)
                                {
                                    if (
                                        bodyPart.Type != "Body"
                                        && bodyPart.Type != "Back"
                                        &&
                                            bodyPart.Type != "Floating Nearby"
                                            && bodyPart.Type != "Missile Weapon"

                                        &&
                                            bodyPart.Type != "Thrown Weapon"
                                            && !bodyPart.Type.Contains("Ammo")
                                            && bodyPart.Type != "Hands"

                                    )
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
                                    GameObject severedBodyPart = targetBody.Dismember(BodyPartsToLose[i - 1]);
                                    Physics part = severedBodyPart.Physics;
                                    part.Push(Directions.GetRandomDirection(), 10, 1);
                                }

                                if (GO.HasPart("Corpse"))
                                    GO.GetPart<Corpse>().CorpseChance = 0;

                                int ToughMod = GO.Statistics["Toughness"].Modifier;
                                if (ToughMod < 1)
                                    ToughMod = 2;

                                int randombones = Stat.Random(1, ToughMod);

                                for (i = 0; i < randombones; i++)
                                {
                                    GameObject NewBones = GameObjectFactory.Factory.CreateObject("Bones");
                                    C.AddObject(NewBones);
                                    Physics part = NewBones.Physics;
                                    part.Push(Directions.GetRandomDirection(), 10, 1);
                                }

                                i = Stat.Random(5, 5 + ToughMod * 2);
                                do
                                {
                                    for (int splatters = 0; splatters < 9; splatters++)
                                    {
                                        GO.BloodsplatterBurst(
                                            true,
                                            (float)(
                                                Stat.Random(0, 359)
                                                / 360.0
                                                * 3.14159274101257
                                                * 2.0
                                            ),
                                            45
                                        );
                                    }
                                    i--;
                                } while (i > 0);
                            }

                            Event E = Event.New("TakeDamage", 0, 0, 0);
                            E.AddParameter("Damage", damage);
                            E.AddParameter("Owner", ParentObject);
                            E.AddParameter("Attacker", ParentObject);

                            if (!GO.FireEvent(E) || damage.Amount == 0)
                            {
                                if (GO.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add(
                                        "&rThe "
                                            + ParentObject.DisplayName
                                            + " tries to explode you, but fails."
                                    );
                                else if (ParentObject.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add(
                                        "You fail to do any damage to "
                                            + GO.the
                                            + GO.DisplayName
                                            + "."
                                    );
                            }
                            else
                            {
                                if (GO.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add(
                                        "&rThe "
                                            + ParentObject.DisplayName
                                            + " harms you "
                                            + resultColor
                                            + "(x"
                                            + Result.ToString()
                                            + ")&y for "
                                            + damage.Amount.ToString()
                                            + " damage!"
                                    );
                                else if (ParentObject.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add(
                                        "&gYou apply pressure to "
                                            + GO.DisplayName
                                            + resultColor
                                            + "(x"
                                            + Result.ToString()
                                            + ")&y for "
                                            + damage.Amount.ToString()
                                            + " damage!"
                                    );
                            }
                        }
                    }
                    else if ( /*GO.HasTag("Wall") || GO.HasPart("Door")*/GO.Physics.Solid == true)
                    {
                        int Result = Stat.RollDamagePenetrations(
                            GO.Statistics["AV"].Value,
                            num,
                            num
                        );
                        Result *= 2;
                        int _Amount = 0;
                        for (int index = 0; index < Result; ++index)
                        {
                            _Amount += Stat.Roll(GetDamageDice(Level));
                        }

                        if (_Amount == 0)
                        {
                            if (ParentObject.IsPlayer())
                                XRLCore.Core.Game.Player.Messages.Add(
                                    "&rYou fail to do any damage to " + GO.DisplayName + "."
                                );
                        }
                        else
                        {
                            Damage damage = new Damage(_Amount);
                            damage.AddAttribute("Physical");
                            string resultColor = Stat.GetResultColor(Result);
                            //XRLCore.Core.Game.Player.Messages.Add(GO.DisplayName + " has " + GO.Statistics["Hitpoints"].Value.ToString() + " hp, while he is supposed to be hit for " + damage.Amount.ToString() + " hp.");

                            Event E = Event.New("TakeDamage", 0, 0, 0);
                            E.AddParameter("Damage", damage);
                            E.AddParameter("Owner", ParentObject);
                            E.AddParameter("Attacker", ParentObject);

                            if (!GO.FireEvent(E) || damage.Amount == 0)
                            {
                                if (GO.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add(
                                        "&rThe "
                                            + ParentObject.DisplayName
                                            + " tries to explode you, but fails."
                                    );
                                else if (ParentObject.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add(
                                        "You fail to do any damage to "
                                            + GO.the
                                            + GO.DisplayName
                                            + "."
                                    );
                            }
                            else
                            {
                                if (GO.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add(
                                        "&rThe "
                                            + ParentObject.DisplayName
                                            + " harms you "
                                            + resultColor
                                            + "(x"
                                            + Result.ToString()
                                            + ")&y for "
                                            + damage.Amount.ToString()
                                            + " damage!"
                                    );
                                else if (ParentObject.IsPlayer())
                                    XRLCore.Core.Game.Player.Messages.Add(
                                        "&gYou apply pressure to "
                                            + GO.DisplayName
                                            + resultColor
                                            + "(x"
                                            + Result.ToString()
                                            + ")&y for "
                                            + damage.Amount.ToString()
                                            + " damage!"
                                    );
                            }
                        }
                    }
                }
            }
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "AIGetOffensiveMutationList")
            {
                int distance = (int)E.GetParameter("Distance");
                GameObject target = E.GetParameter("Target") as GameObject;
                List<AICommandList> aiCommandList = (List<AICommandList>)E.GetParameter("List");

                if (DynakineticismActivatedAbility != null &&
                    DynakineticismActivatedAbility.Cooldown <= 0 &&
                    distance <= 80 &&
                    ParentObject.HasLOSTo(target, true) &&
                    ParentObject.Physics.CurrentCell.DistanceTo(target) <= 8)
                {
                    aiCommandList.Add(new AICommandList("CommandDynakineticism", 1));
                }

                return true;
            }
            if (E.ID == "CommandDynakineticism")
            {
                Cell C = PickDestinationCell(80, AllowVis.OnlyVisible, true);
                if (C != null)
                {
                    Attack(C);
                    Thread.Sleep(50);
                    DynakineticismActivatedAbility.Cooldown = Cooldown * 10;
                    UseEnergy(1000, "Mental");
                }
            }
            return true;
        }

        public override bool ChangeLevel(int NewLevel)
        {
            return base.ChangeLevel(NewLevel);
        }

        public override bool Mutate(GameObject GO, int Level)
        {
            ActivatedAbilities part = GetActivatedAbilities(GO);

            if (part != null)
            {
                DynakineticismActivatedAbilityID = part.AddAbility("Dynakineticism", "CommandDynakineticism", "Mental Mutation");
                DynakineticismActivatedAbility = part.AbilityByGuid[DynakineticismActivatedAbilityID];
            }

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMutationByGUID(GO, ref DynakineticismActivatedAbilityID);

            return base.Unmutate(GO);
        }
    }
}
