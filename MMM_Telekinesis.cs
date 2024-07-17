using System;
using System.Collections.Generic;
using XRL.World.Effects;
using XRL.World.Anatomy;
using XRL.Messages;
using XRL.UI;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Telekinesis : MMM_BaseMutation
    {
        private const int BASE_TELEKINESIS_RANGE = 5;
        private const int BASE_TELEKINESIS_LIFT_WEIGHT = 200;

        private const int TELEKINESIS_RANGE_PER_LEVEL = 2;
        private const int TELEKINESIS_STRENGTH_PER_LEVEL = 2;

        private const int THROW_RANGE_MOD = -2;
        private const int PICKUP_RANGE_MOD = -2;

        public int TelekinesisRange => BASE_TELEKINESIS_RANGE + Level / TELEKINESIS_RANGE_PER_LEVEL;
        public int TelekinesisStrength => Level + TELEKINESIS_STRENGTH_PER_LEVEL;
        public int LiftWeight => BASE_TELEKINESIS_LIFT_WEIGHT * Level;

        public Guid TelekinesisGentlyPickupAndPlaceCreatureActivatedAbilityID = Guid.Empty;
        public Guid TelekinesisPickupActivatedAbilityID = Guid.Empty;
        public Guid TelekinesisThrowActivatedAbilityID = Guid.Empty;
        public Guid TelekinesisLaunchWeaponActivatedAbilityID = Guid.Empty;
        //public Guid TelekinesisSwitchLaunchTypesID = Guid.Empty;
        //public Guid TelekinesisPickUpPreviousWeaponID = Guid.Empty;
        public ActivatedAbilityEntry TelekinesisGentlyPickupAndPlaceCreatureActivatedAbility;
        public ActivatedAbilityEntry TelekinesisPickupActivatedAbility;
        public ActivatedAbilityEntry TelekinesisThrowActivatedAbility;
        public ActivatedAbilityEntry TelekinesisLaunchWeaponActivatedAbility;
        //public ActivatedAbilityEntry TelekinesisSwitchLaunchTypes;
        //public ActivatedAbilityEntry TelekinesisPickUpPreviousWeapon;

        private GameObject lastThrownWeapon;

        public int BasicCooldown = 10;
        public string TelekinesisDamage = "1d6";

        public MMM_Telekinesis()
        {
            DisplayName = "Telekinesis";
            Type = "Mental";
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("CommandTelekinesisCreaturePickup");
            Registrar.Register("CommandTelekinesisPickup");
            Registrar.Register("CommandTelekinesisThrow");
            Registrar.Register("CommandTelekinesisThrowWeapon");

            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            return "You possess telekinetic powers.";
        }

        public override string GetLevelText(int Level)
        {
            return $"Your telekinesis range is {TelekinesisRange}.\nYou may pick up objects from the ground or lift and place creatures.\nYou may lift and throw creatures not heavier than {Level * 200} lbs., damaging them and object they hit for {TelekinesisDamage} with penetration equal to 2 + Level - AV value.\n" + "You may also throw melee weapons - they will not take damage and damage roll will be their base damage instead." + "\nCooldown: " + BasicCooldown.ToString() + ", or " + (BasicCooldown * 4).ToString() + " if you moved a creature.";
        }

        public bool ObjectIsWeapon(GameObject GO)
        {
            MeleeWeapon mw = GO.GetPart<MeleeWeapon>();
            if (mw != null)
            {
                //I can't remember why is iron mace an exception
                return !(mw.BaseDamage == "1d2"
                        && GO.DisplayName != "iron mace"
                        && !GO.HasPart("MissileWeapon"));
            }
            return false;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandTelekinesisThrowWeapon")
            {
                GameObject thrownWeaponObject = null;
                GameObject possibleTarget = null;
                Body body = ParentObject.Body;

                foreach (BodyPart bodyPart in body.GetParts())
                {
                    if (bodyPart.Type != "Hand" || bodyPart.Equipped == null || bodyPart.Equipped != null && bodyPart.Equipped.GetIntProperty("Natural", 0) != 0)
                    {
                        continue;
                    }

                    thrownWeaponObject = bodyPart.Equipped;
                    ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", bodyPart));
                    // thrownWeaponObject.Physics.InInventory.FireEvent(Event.New("CommandDropObject", "Object", thrownWeaponObject));
                    InventoryActionEvent.Check(ParentObject, ParentObject, thrownWeaponObject, "CommandDropObject");

                    break;
                }

                if (thrownWeaponObject == null)
                {
                    if (ParentObject.IsPlayer())
                    {
                        MessageQueue.AddPlayerMessage("You don't have a throwable weapon equipped.");
                    }

                    return true;
                }

                string direction = PickDirectionS();

                if (direction == "." || string.IsNullOrEmpty(direction))
                {
                    return true;
                }

                MeleeWeapon thrownWeapon = thrownWeaponObject.GetPart<MeleeWeapon>();

                lastThrownWeapon = thrownWeaponObject;
                int Damage2 = 0;

                string RealTelekinesisDamage = thrownWeapon.BaseDamage;
                int HitStrength = TelekinesisStrength;
                int i;
                string message = string.Empty;

                if (ParentObject.IsPlayer())
                    message += "You";
                else
                    message += ParentObject.DisplayName;

                message += " throw";

                if (ParentObject.IsPlayer())
                    message += " ";
                else
                    message += "s ";

                message += thrownWeaponObject.the + thrownWeaponObject.DisplayName;

                message += " with ";
                if (ParentObject.IsPlayer())
                    message += "your";
                else
                    message += thrownWeaponObject.its;
                message += " telekinetic power!";

                MessageQueue.AddPlayerMessage(message);

                for (i = 0; i < TelekinesisRange + THROW_RANGE_MOD; i++)
                {
                    List<GameObject> ObjectsSomewhereElse = thrownWeaponObject.Physics.CurrentCell.GetCellFromDirection(direction, true).GetObjectsInCell();

                    foreach (GameObject GO2 in ObjectsSomewhereElse)
                    {
                        if (GO2.Physics.Solid)
                        {
                            possibleTarget = GO2;
                            i = TelekinesisRange;
                            break;
                        }
                    }

                    foreach (GameObject GO3 in ObjectsSomewhereElse)
                    {
                        if (GO3.HasPart("Combat"))
                        {
                            possibleTarget = GO3;
                            i = TelekinesisRange;
                            break;
                        }
                    }

                    thrownWeaponObject.DirectMoveTo(thrownWeaponObject.Physics.CurrentCell.GetCellFromDirection(direction, true));
                }

                if (possibleTarget == null)
                {
                    return true;
                }

                int SolidObjectAV = Stats.GetCombatAV(possibleTarget);

                if (HitStrength > thrownWeapon.MaxStrengthBonus)
                {
                    HitStrength = thrownWeapon.MaxStrengthBonus;
                }

                for (i = 0; i < HitStrength - SolidObjectAV; i++)
                {
                    Damage2 += Stat.Roll(RealTelekinesisDamage);
                }

                if (Damage2 > 0 && possibleTarget.HasPart("Combat"))
                {
                    possibleTarget.BloodsplatterBurst(true, (float)(Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                    if (thrownWeaponObject.GetPart<MeleeWeapon>().Skill == "Cudgel")
                        possibleTarget.ApplyEffect(new Stun(Stat.Random(2, 3), 25 + Level));
                }

                Damage ddamage2 = new Damage(Damage2);
                ddamage2.AddAttribute("Physical");
                message = "";
                Event E2 = Event.New("TakeDamage", 0, 0, 0);
                E2.AddParameter("Damage", ddamage2);
                E2.AddParameter("Owner", ParentObject);
                E2.AddParameter("Attacker", ParentObject);
                if (!possibleTarget.FireEvent(E2) || ddamage2.Amount == 0)
                {
                    if (possibleTarget.IsPlayer())
                        message += "You are ";
                    else
                        message += possibleTarget.the + possibleTarget.DisplayName + possibleTarget.Is;

                    message += " not damaged by ";

                    if (ParentObject.IsPlayer())
                        message += "your";
                    else
                        message += possibleTarget.the + possibleTarget.DisplayName + "'s";

                    message += " telekinesis.";

                    MessageQueue.AddPlayerMessage(message);
                }
                else
                {
                    if (possibleTarget.IsPlayer())
                        message += "You take ";
                    else
                        message += possibleTarget.the + possibleTarget.DisplayName + " takes ";

                    message += ddamage2.Amount.ToString();
                    message += " damage from ";
                    message += thrownWeaponObject.DisplayName;

                    if (thrownWeapon.Skill == "LongBlades" || thrownWeapon.Skill == "ShortBlades" || thrownWeapon.Skill == "Axe")
                        message += " piercing into ";
                    else
                        message += " slamming into ";

                    message += possibleTarget.them + ".";

                    MessageQueue.AddPlayerMessage(message);
                }

                TelekinesisLaunchWeaponActivatedAbility.Cooldown = BasicCooldown * 10 + 10;
                ParentObject.UseEnergy(1000, "Mental");

                return true;
            }
            if (E.ID == "CommandTelekinesisThrow")
            {
                Cell C = PickDestinationCell(TelekinesisRange, AllowVis.OnlyVisible, true);

                if (C == null)
                {
                    return false;
                }

                if (C.DistanceTo(ParentObject) > TelekinesisRange)
                {
                    if (ParentObject.IsPlayer())
                    {
                        Popup.Show("That's too far.");
                    }

                    return false;
                }

                List<GameObject> AllObjects = C.GetObjectsInCell();
                List<GameObject> ProperObjects = new List<GameObject>();

                foreach (GameObject gameObject in AllObjects)
                {
                    if (gameObject.HasPart("Combat") || ObjectIsWeapon(gameObject))
                    {
                        ProperObjects.Add(gameObject);
                    }

                }

                if (ProperObjects.Count == 0)
                {
                    if (ParentObject.IsPlayer())
                    {
                        Popup.Show("There are no targets.");
                    }

                    return false;
                }

                GameObject GO = ProperObjects[ProperObjects.Count - 1];

                if (GO.IsPlayer())
                {
                    Popup.Show("You can't throw yourself. What if you get hurt?");
                    return false;
                }

                if (GO.Physics.Weight > LiftWeight)
                {
                    if (ParentObject.IsPlayer())
                    {
                        Popup.Show(GO.the + GO.DisplayName + " is too heavy for you to lift.");
                    }

                    return false;
                }

                if (GO != null)
                {
                    int i;
                    string direction = PickDirectionS();

                    string message = "";

                    if (ParentObject.IsPlayer())
                        message += "You ";
                    else
                        message += GO.the + GO.DisplayName;

                    message += " throw";

                    if (ParentObject.IsPlayer())
                        message += " ";
                    else
                        message += "s ";

                    if (GO.IsPlayer())
                        message += " you ";
                    else
                        message += GO.the + GO.DisplayName;

                    message += " with ";
                    if (ParentObject.IsPlayer())
                        message += "your";
                    else
                        message += GO.its;
                    message += " telekinetic power!";

                    MessageQueue.AddPlayerMessage(message);

                    if (direction == ".")
                    {
                        int Damage1 = 0;
                        for (i = 0; i < TelekinesisStrength - 5; i++)
                        {
                            Damage1 += Stat.Roll(TelekinesisDamage);
                        }
                        Damage ddamage = new Damage(Damage1);
                        ddamage.AddAttribute("Physical");
                        Event E3 = Event.New("TakeDamage", 0, 0, 0);
                        E3.AddParameter("Damage", ddamage);
                        E3.AddParameter("Owner", ParentObject);
                        E3.AddParameter("Attacker", ParentObject);

                        message = "";

                        if (!GO.FireEvent(E3) || ddamage.Amount == 0)
                        {
                            if (GO.IsPlayer())
                                message += "You are ";
                            else
                                message += GO.the + GO.DisplayName + GO.Is;

                            message += " not damaged by ";

                            if (ParentObject.IsPlayer())
                                message += "your";
                            else
                                message += GO.the + GO.DisplayName + "'s";
                            message += " telekinesis.";
                            if (!ObjectIsWeapon(GO))
                                MessageQueue.AddPlayerMessage(message);
                        }
                        else
                        {
                            if (GO.IsPlayer())
                                message += "You take ";
                            else
                                message += GO.the + GO.DisplayName + " takes ";

                            message += ddamage.Amount.ToString();
                            message += " damage from slamming into floor";
                            MessageQueue.AddPlayerMessage(message + ".");
                        }

                        TelekinesisGentlyPickupAndPlaceCreatureActivatedAbility.Cooldown = BasicCooldown * 10 + 10;
                        TelekinesisThrowActivatedAbility.Cooldown = BasicCooldown * 10 + 10;
                        ParentObject.UseEnergy(1000, "Mental");

                        return true;
                    }

                    //Proceeding to fly
                    GameObject PossibleTarget = null;
                    for (i = 0; i < Level; i++)
                    {
                        List<GameObject> ObjectsSomewhereElse = GO.Physics.CurrentCell.GetCellFromDirection(direction, true).GetObjectsInCell();

                        foreach (GameObject GO2 in ObjectsSomewhereElse)
                        {
                            if ((GO2.Physics.Solid == true || GO2.HasPart("Combat")) && GO2 != GO)
                            {
                                PossibleTarget = GO2;
                                i = Level;
                                break;
                            }
                        }
                        GO.DirectMoveTo(GO.Physics.CurrentCell.GetCellFromDirection(direction, true));
                    }

                    if (PossibleTarget != null)
                    {
                        int TargetAV = Stats.GetCombatAV(GO);
                        int SolidObjectAV = Stats.GetCombatAV(PossibleTarget);
                        int Damage1 = 0;
                        int Damage2 = 0;

                        string RealTelekinesisDamage = TelekinesisDamage;
                        int HitStrength = TelekinesisStrength;

                        if (ObjectIsWeapon(GO))
                        {
                            RealTelekinesisDamage = GO.GetPart<MeleeWeapon>().BaseDamage;

                            if (HitStrength > GO.GetPart<MeleeWeapon>().MaxStrengthBonus)
                                HitStrength = GO.GetPart<MeleeWeapon>().MaxStrengthBonus;

                            //this.ParentObject.ParticleText(RealTelekinesisDamage);
                            //this.ParentObject.ParticleText(HitStrength.ToString());
                        }

                        for (i = 0; i < HitStrength - TargetAV; i++)
                        {
                            Damage1 += Stat.Roll(RealTelekinesisDamage);
                        }

                        if (ObjectIsWeapon(GO))
                        {
                            Damage1 = 0;
                            PossibleTarget.Physics.CurrentCell.AddObject(GO);
                        }

                        for (i = 0; i < HitStrength - SolidObjectAV - TargetAV; i++)
                        {
                            Damage2 += Stat.Roll(RealTelekinesisDamage);
                        }

                        if (Damage1 > 0 && GO.HasPart("Combat"))
                        {
                            GO.BloodsplatterBurst(true, (float)(Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                            if (!ObjectIsWeapon(GO))
                                GO.ApplyEffect(new Stun(Stat.Random(1, 2), 25 + Level));
                        }
                        if (Damage2 > 0 && PossibleTarget.HasPart("Combat"))
                        {
                            PossibleTarget.BloodsplatterBurst(true, (float)(Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                            if (!ObjectIsWeapon(GO))
                                PossibleTarget.ApplyEffect(new Stun(Stat.Random(1, 2), 25 + Level));
                        }

                        Damage ddamage = new Damage(Damage1);
                        Damage ddamage2 = new Damage(Damage2);
                        ddamage.AddAttribute("Physical");
                        ddamage2.AddAttribute("Physical");
                        Event E3 = Event.New("TakeDamage", 0, 0, 0);
                        E3.AddParameter("Damage", ddamage);
                        E3.AddParameter("Owner", ParentObject);
                        E3.AddParameter("Attacker", ParentObject);

                        message = "";

                        if (!GO.FireEvent(E3) || ddamage.Amount == 0)
                        {
                            if (GO.IsPlayer())
                                message += "You are ";
                            else
                                message += GO.the + GO.DisplayName + GO.Is;

                            message += " not damaged by ";

                            if (ParentObject.IsPlayer())
                                message += "your";
                            else
                                message += GO.the + GO.DisplayName + "'s";
                            message += " telekinesis.";
                            if (!ObjectIsWeapon(GO))
                                MessageQueue.AddPlayerMessage(message);
                        }
                        else
                        {
                            if (GO.IsPlayer())
                                message += "You take ";
                            else
                                message += GO.the + GO.DisplayName + " takes ";

                            message += ddamage.Amount.ToString();
                            message += " damage from ";
                            if (GO.GetPart<MeleeWeapon>().Skill == "LongBlades" || GO.GetPart<MeleeWeapon>().Skill == "ShortBlades" || GO.GetPart<MeleeWeapon>().Skill == "Axe")
                                message += "piercing into";
                            else
                                message += "slamming into";
                            message += " ";

                            if (PossibleTarget.IsPlayer())
                                message += "you";
                            else
                                message += PossibleTarget.the + PossibleTarget.DisplayName;
                            MessageQueue.AddPlayerMessage(message + ".");
                        }

                        message = "";
                        Event E2 = Event.New("TakeDamage", 0, 0, 0);
                        E2.AddParameter("Damage", ddamage2);
                        E2.AddParameter("Owner", ParentObject);
                        E2.AddParameter("Attacker", ParentObject);
                        if (!PossibleTarget.FireEvent(E2) || ddamage2.Amount == 0)
                        {
                            if (PossibleTarget.IsPlayer())
                                message += "You are ";
                            else
                                message += PossibleTarget.the + PossibleTarget.DisplayName + PossibleTarget.Is;

                            message += " not damaged by ";

                            if (ParentObject.IsPlayer())
                                message += " your ";
                            else
                                message += PossibleTarget.the + PossibleTarget.DisplayName + "'s";
                            message += " telekinesis.";
                            MessageQueue.AddPlayerMessage(message);
                        }
                        else
                        {
                            if (PossibleTarget.IsPlayer())
                                message += "You take ";
                            else
                                message += PossibleTarget.the + PossibleTarget.DisplayName + " takes ";

                            message += ddamage2.Amount.ToString();
                            message += " damage from ";
                            message += GO.DisplayName;
                            MeleeWeapon mw = GO.GetPart<MeleeWeapon>();
                            if (mw.Skill == "LongBlades" || mw.Skill == "ShortBlades" || mw.Skill == "Axe")
                                message += " piercing into ";
                            else
                                message += " slamming into ";
                            message += PossibleTarget.them + ".";
                            MessageQueue.AddPlayerMessage(message);
                        }
                    }

                    TelekinesisGentlyPickupAndPlaceCreatureActivatedAbility.Cooldown = BasicCooldown * 10 + 10;
                    TelekinesisThrowActivatedAbility.Cooldown = BasicCooldown * 10 + 10;
                    ParentObject.UseEnergy(1000, "Mental");

                    return true;
                }
                return false;
            }
            if (E.ID == "CommandTelekinesisCreaturePickup")
            {
                Cell C = PickDestinationCell(Level, AllowVis.OnlyVisible, true);
                Cell C2;
                GameObject GO = null;
                GameObject Player = null;

                if (C == null)
                {
                    return false;
                }

                List<GameObject> CurrentObjectsInCell = C.GetObjectsWithPart("Combat");

                if (CurrentObjectsInCell.Count == 0)
                {
                    if (ParentObject.IsPlayer())
                        Popup.Show("There are no targets.");
                    return false;
                }
                else
                {
                    GO = CurrentObjectsInCell[0];
                }

                if (ParentObject.IsPlayer())
                {
                    Player = ParentObject;
                }

                if (GO != null && Player != null)
                {
                    XRLCore.Core.Game.Player.Body = GO; //For target picking which can only begin from player object
                    C2 = PickTarget.ShowPicker(PickTarget.PickStyle.Line, TelekinesisRange, TelekinesisRange, GO.Physics.CurrentCell.X, GO.Physics.CurrentCell.Y, false, AllowVis.OnlyVisible);
                    XRLCore.Core.Game.Player.Body = Player;
                    if (C2 != null)
                    {
                        if (C2.DistanceTo(GO) <= TelekinesisRange)
                        {
                            List<Point> pointList = Zone.Line(ParentObject.Physics.CurrentCell.X, ParentObject.Physics.CurrentCell.Y, C2.X, C2.Y);
                            int num = 0;
                            foreach (Point point in pointList)
                            {
                                Cell cell2 = ParentObject.Physics.CurrentCell.ParentZone.GetCell(point.X, point.Y);

                                if (cell2 != ParentObject.Physics.CurrentCell)
                                {
                                    foreach (GameObject gameObject in cell2.GetObjectsWithPart("Physics"))
                                    {
                                        if (num == pointList.Count - 1)
                                        {
                                            if (gameObject.Physics.Solid || gameObject.HasPart("Combat"))
                                            {
                                                Popup.Show("You can only place yourself into an empty space.");

                                                return true;
                                            }
                                        }
                                        else if (gameObject.Physics.Solid && !gameObject.Physics.HasTag("Flyover"))
                                        {
                                            Popup.Show("You can't place yourself over " + gameObject.the + gameObject.ShortDisplayName + ".");
                                            return true;
                                        }
                                    }
                                }

                                ++num;
                            }

                            C2.AddObject(GO);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }

                TelekinesisGentlyPickupAndPlaceCreatureActivatedAbility.Cooldown = BasicCooldown * 40 + 10;
                TelekinesisThrowActivatedAbility.Cooldown = BasicCooldown * 40 + 10;

                ParentObject.UseEnergy(1000, "Mental");

                return true;
            }
            if (E.ID == "CommandTelekinesisPickup")
            {
                Inventory ParentInventory = ParentObject.GetPart<Inventory>();
                XRLCore.Core.RenderBaseToBuffer(Popup._ScreenBuffer);
                Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer, null, false);
                char key = 'a';
                Cell cell = PickTarget.ShowPicker(PickTarget.PickStyle.Line, Level, Level, ParentObject.Physics.CurrentCell.X, ParentObject.Physics.CurrentCell.Y, false, AllowVis.OnlyVisible);

                if (cell == null)
                {
                    return true;
                }

                if (cell.DistanceTo(ParentObject) > TelekinesisRange)
                {
                    MessageQueue.AddPlayerMessage("You can't reach that far.");

                    return true;
                }

                List<GameObject> objectsInCell = cell.GetObjectsInCell();

                //Stopped working after some update, probably not necessary anyway and I can't seem to find any replacement
                //objectsInCell.Sort((IComparer<XRL.World.GameObject>)new SortGORenderLayer());
                string str = string.Empty;
                Dictionary<char, GameObject> objectsToPickUp = new Dictionary<char, XRL.World.GameObject>();

                foreach (GameObject gameObject in objectsInCell)
                {
                    if (gameObject != ParentObject && gameObject.GetPart<Physics>().Takeable == true)
                    {
                        objectsToPickUp.Add(key, gameObject);
                        str = str + key.ToString() + ") " + gameObject.DisplayName + "&y\n";
                        ++key;
                    }
                }

                bool requestInterfaceExit = false;

                if (objectsToPickUp.Count == 0)
                {
                    if (ParentObject.IsPlayer())
                    {
                        Popup.Show("There's nothing to take.");
                    }
                }
                else
                {
                    //Throws null reference errors at unknown lines if you enter item menu but it seems to work anyway
                    PickItem.ShowPicker(new List<GameObject>(objectsToPickUp.Values), ref requestInterfaceExit, null, PickItem.PickItemDialogStyle.GetItemDialog, ParentObject);
                }

                ParentObject.UseEnergy(1000, "Mental");

                return true;
            }
            if (E.ID == "CommandTelekinesisPickupThrownWeapon")
            {
                Popup.Show("You may pick up your weapon.");
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
            ActivatedAbilities part = GetActivatedAbilities(GO);

            if (part != null)
            {
                TelekinesisGentlyPickupAndPlaceCreatureActivatedAbilityID = part.AddAbility("Lift and place creature", "CommandTelekinesisCreaturePickup", "Telekinesis");
                TelekinesisGentlyPickupAndPlaceCreatureActivatedAbility = part.AbilityByGuid[TelekinesisGentlyPickupAndPlaceCreatureActivatedAbilityID];
                TelekinesisPickupActivatedAbilityID = part.AddAbility("Pick up", "CommandTelekinesisPickup", "Telekinesis");
                TelekinesisPickupActivatedAbility = part.AbilityByGuid[TelekinesisPickupActivatedAbilityID];
                TelekinesisThrowActivatedAbilityID = part.AddAbility("Throw", "CommandTelekinesisThrow", "Telekinesis");
                TelekinesisThrowActivatedAbility = part.AbilityByGuid[TelekinesisThrowActivatedAbilityID];
                TelekinesisLaunchWeaponActivatedAbilityID = part.AddAbility("Throw weapon", "CommandTelekinesisThrowWeapon", "Telekinesis");
                TelekinesisLaunchWeaponActivatedAbility = part.AbilityByGuid[TelekinesisLaunchWeaponActivatedAbilityID];
                //this.TelekinesisPickUpPreviousWeaponID = part.AddAbility("Return weapon", "CommandTelekinesisPickupThrownWeapon", "Telekinesis");
                //this.TelekinesisPickUpPreviousWeapon = part.AbilityByGuid[this.TelekinesisPickUpPreviousWeaponID];
            }

            ChangeLevel(Level);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMutationByGUID(GO, ref TelekinesisGentlyPickupAndPlaceCreatureActivatedAbilityID);
            RemoveMutationByGUID(GO, ref TelekinesisPickupActivatedAbilityID);
            RemoveMutationByGUID(GO, ref TelekinesisLaunchWeaponActivatedAbilityID);
            RemoveMutationByGUID(GO, ref TelekinesisThrowActivatedAbilityID);

            return base.Unmutate(GO);
        }
    }
}
