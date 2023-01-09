using ConsoleLib.Console;
using System;
using System.Collections.Generic;
using XRL.World;
using XRL.Messages;
using XRL.UI;
using XRL.Core;
using XRL.World.Parts;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Telekinesis : BaseMutation
    {
        private const int BASE_TELEKINESIS_RANGE = 5;
        private const int BASE_TELEKINESIS_LIFT_WEIGHT = 200;

        private const int TELEKINESIS_RANGE_PER_LEVEL = 2;
        private const int TELEKINESIS_STRENGTH_PER_LEVEL = 2;

        private const int THROW_RANGE_MOD = -2;
        private const int PICKUP_RANGE_MOD = -2;

        public int TelekinesisRange => (BASE_TELEKINESIS_RANGE + Level / TELEKINESIS_RANGE_PER_LEVEL);
        public int TelekinesisStrength => this.Level + TELEKINESIS_STRENGTH_PER_LEVEL;
        public int LiftWeight => BASE_TELEKINESIS_LIFT_WEIGHT * this.Level;

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
            this.DisplayName = "Telekinesis";
            this.Type = "Mental";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "CommandTelekinesisCreaturePickup");
            Object.RegisterPartEvent((IPart)this, "CommandTelekinesisPickup");
            Object.RegisterPartEvent((IPart)this, "CommandTelekinesisThrow");
            Object.RegisterPartEvent((IPart)this, "CommandTelekinesisThrowWeapon");
            //Object.RegisterPartEvent((IPart)this, "CommandTelekinesisPickupThrownWeapon");
        }

        public override string GetDescription()
        {
            return "You possess telekinetic powers.";
        }

        public override string GetLevelText(int Level)
        {
            return $"Your telekinesis range is {TelekinesisRange}.\nYou may pick up objects from the ground or lift and place creatures.\nYou may lift and throw creatures not heavier than {(Level * 200)} lbs., damaging them and object they hit for {TelekinesisDamage} with penetration equal to 2 + Level - AV value.\n" + "You may also throw melee weapons - they will not take damage and damage roll will be their base damage instead." + "\nCooldown: " + this.BasicCooldown.ToString() + ", or " + (this.BasicCooldown * 4).ToString() + " if you moved a creature.";
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
                string direction = ".";
                direction = this.PickDirectionS();

                if (direction != "." && !string.IsNullOrEmpty(direction))
                {
                    GameObject thrownWeapon = (GameObject)null;
                    Body body = this.ParentObject.GetPart("Body") as Body;

                    foreach (BodyPart bodyPart in body.GetParts())
                    {
                        if (bodyPart.Type == "Hand")
                        {
                            if (bodyPart.Equipped != null && bodyPart.Equipped.GetIntProperty("Natural", 0) == 0)
                            {
                                thrownWeapon = bodyPart.Equipped;
                                this.ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", (object)bodyPart));
                                thrownWeapon.pPhysics.InInventory.FireEvent(Event.New("CommandDropObject", "Object", (object)thrownWeapon));
                                break;
                            }
                        }
                    }

                    if (thrownWeapon != null)
                    {
                        lastThrownWeapon = thrownWeapon;
                        int Damage2 = 0;

                        string RealTelekinesisDamage = TelekinesisDamage;
                        int HitStrength = TelekinesisStrength;
                        int i;
                        string message = string.Empty;

                        if (this.ParentObject.IsPlayer())
                            message += "You ";
                        else
                            message += this.ParentObject.DisplayName;

                        message += " throw";

                        if (this.ParentObject.IsPlayer())
                            message += " ";
                        else
                            message += "s ";

                        message += thrownWeapon.the + thrownWeapon.DisplayName;

                        message += " with ";
                        if (this.ParentObject.IsPlayer())
                            message += "your";
                        else
                            message += thrownWeapon.its;
                        message += " telekinetic power!";

                        MessageQueue.AddPlayerMessage(message);

                        GameObject PossibleTarget = (GameObject)null;
                        for (i = 0; i < TelekinesisRange + THROW_RANGE_MOD; i++)
                        {
                            List<GameObject> ObjectsSomewhereElse = thrownWeapon.pPhysics.CurrentCell.GetCellFromDirection(direction, true).GetObjectsInCell();

                            foreach (GameObject GO2 in ObjectsSomewhereElse)
                            {
                                if (GO2.pPhysics.Solid == true)
                                {
                                    PossibleTarget = GO2;
                                    i = TelekinesisRange;
                                    break;
                                }
                            }
                            foreach (GameObject GO3 in ObjectsSomewhereElse)
                            {
                                if (GO3.HasPart("Combat"))
                                {
                                    PossibleTarget = GO3;
                                    i = TelekinesisRange;
                                    break;
                                }
                            }

                            thrownWeapon.DirectMoveTo(thrownWeapon.pPhysics.CurrentCell.GetCellFromDirection(direction, true));
                        }

                        if (PossibleTarget != null)
                        {
                            int SolidObjectAV = Stats.GetCombatAV(PossibleTarget);
                            RealTelekinesisDamage = thrownWeapon.GetPart<MeleeWeapon>().BaseDamage;
                            if (HitStrength > thrownWeapon.GetPart<MeleeWeapon>().MaxStrengthBonus)
                                HitStrength = thrownWeapon.GetPart<MeleeWeapon>().MaxStrengthBonus;
                            //PossibleTarget.pPhysics.CurrentCell.AddObject(thrownWeapon);

                            for (i = 0; i < HitStrength - SolidObjectAV; i++)
                            {
                                Damage2 += Stat.Roll(RealTelekinesisDamage);
                            }

                            if (Damage2 > 0 && PossibleTarget.HasPart("Combat"))
                            {
                                PossibleTarget.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                                if (thrownWeapon.GetPart<MeleeWeapon>().Skill == "Cudgel")
                                    PossibleTarget.ApplyEffect((Effect)new Stun(Stat.Random(2, 3), 25 + this.Level));
                            }
                            Damage ddamage2 = new Damage(Damage2);
                            ddamage2.AddAttribute("Physical");
                            message = "";
                            Event E2 = Event.New("TakeDamage", 0, 0, 0);
                            E2.AddParameter("Damage", (object)ddamage2);
                            E2.AddParameter("Owner", (object)this.ParentObject);
                            E2.AddParameter("Attacker", (object)this.ParentObject);
                            if (!PossibleTarget.FireEvent(E2) || ddamage2.Amount == 0)
                            {
                                if (PossibleTarget.IsPlayer())
                                    message += "You are ";
                                else
                                    message += PossibleTarget.the + PossibleTarget.DisplayName + PossibleTarget.Is;

                                message += " not damaged by ";

                                if (this.ParentObject.IsPlayer())
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
                                message += thrownWeapon.DisplayName;
                                if (thrownWeapon.GetPart<MeleeWeapon>().Skill == "LongBlades" || thrownWeapon.GetPart<MeleeWeapon>().Skill == "ShortBlades" || thrownWeapon.GetPart<MeleeWeapon>().Skill == "Axe")
                                    message += " piercing into ";
                                else
                                    message += " slamming into ";
                                message += PossibleTarget.them + ".";
                                MessageQueue.AddPlayerMessage(message);
                            }
                        }
                        this.TelekinesisLaunchWeaponActivatedAbility.Cooldown = this.BasicCooldown * 10 + 10;
                    }
                    this.ParentObject.UseEnergy(1000, "Mental");
                }
                return true;
            }
            if (E.ID == "CommandTelekinesisThrow")
            {
                Cell C = this.PickDestinationCell(TelekinesisRange, AllowVis.OnlyVisible, true);
                if (C == null)
                    return false;

                if (C.DistanceTo(this.ParentObject) > TelekinesisRange)
                {
                    if (this.ParentObject.IsPlayer())
                        Popup.Show("That's too far.", true);
                    return false;
                }

                GameObject GO = (GameObject)null;
                List<GameObject> AllObjects = C.GetObjectsInCell();
                List<GameObject> ProperObjects = new List<GameObject>();
                foreach (GameObject kok in AllObjects)
                {
                    if (kok.HasPart("Combat") || ObjectIsWeapon(kok))
                        ProperObjects.Add(kok);
                }
                if (ProperObjects.Count == 0)
                {
                    if (this.ParentObject.IsPlayer())
                        Popup.Show("There are no targets.", true);
                    return false;
                }
                GO = ProperObjects[ProperObjects.Count - 1];

                if (GO.IsPlayer())
                {
                    Popup.Show("You can't throw yourself. What if you get hurt?", true);
                    return false;
                }

                if (GO.pPhysics.Weight > this.LiftWeight)
                {
                    if (this.ParentObject.IsPlayer())
                        Popup.Show(GO.the + GO.DisplayName + " is too heavy for you to lift.", true);
                    return false;
                }

                if (GO != null)
                {
                    int i;
                    string direction = this.PickDirectionS();

                    string message = "";

                    if (this.ParentObject.IsPlayer())
                        message += "You ";
                    else
                        message += GO.the + GO.DisplayName;

                    message += " throw";

                    if (this.ParentObject.IsPlayer())
                        message += " ";
                    else
                        message += "s ";

                    if (GO.IsPlayer())
                        message += " you ";
                    else
                        message += GO.the + GO.DisplayName;

                    message += " with ";
                    if (this.ParentObject.IsPlayer())
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
                        E3.AddParameter("Damage", (object)ddamage);
                        E3.AddParameter("Owner", (object)this.ParentObject);
                        E3.AddParameter("Attacker", (object)this.ParentObject);

                        message = "";

                        if (!GO.FireEvent(E3) || ddamage.Amount == 0)
                        {
                            if (GO.IsPlayer())
                                message += "You are ";
                            else
                                message += GO.the + GO.DisplayName + GO.Is;

                            message += " not damaged by ";

                            if (this.ParentObject.IsPlayer())
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
                        this.TelekinesisGentlyPickupAndPlaceCreatureActivatedAbility.Cooldown = this.BasicCooldown * 10 + 10;
                        this.TelekinesisThrowActivatedAbility.Cooldown = this.BasicCooldown * 10 + 10;
                        this.ParentObject.UseEnergy(1000, "Mental");
                        return true;
                    }

                    //Proceeding to fly
                    GameObject PossibleTarget = (GameObject)null;
                    for (i = 0; i < this.Level; i++)
                    {
                        List<GameObject> ObjectsSomewhereElse = GO.pPhysics.CurrentCell.GetCellFromDirection(direction, true).GetObjectsInCell();

                        foreach (GameObject GO2 in ObjectsSomewhereElse)
                        {
                            if ((GO2.pPhysics.Solid == true || GO2.HasPart("Combat")) && GO2 != GO)
                            {
                                PossibleTarget = GO2;
                                i = this.Level;
                                break;
                            }
                        }
                        GO.DirectMoveTo(GO.pPhysics.CurrentCell.GetCellFromDirection(direction, true));
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
                            PossibleTarget.pPhysics.CurrentCell.AddObject(GO);
                        }

                        for (i = 0; i < HitStrength - SolidObjectAV - TargetAV; i++)
                        {
                            Damage2 += Stat.Roll(RealTelekinesisDamage);
                        }

                        if (Damage1 > 0 && GO.HasPart("Combat"))
                        {
                            GO.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                            if (!ObjectIsWeapon(GO))
                                GO.ApplyEffect((Effect)new Stun(Stat.Random(1, 2), 25 + this.Level));
                        }
                        if (Damage2 > 0 && PossibleTarget.HasPart("Combat"))
                        {
                            PossibleTarget.BloodsplatterBurst(true, (float)((double)Stat.Random(0, 359) / 360.0 * 3.14159274101257 * 2.0), 45);
                            if (!ObjectIsWeapon(GO))
                                PossibleTarget.ApplyEffect((Effect)new Stun(Stat.Random(1, 2), 25 + this.Level));
                        }

                        Damage ddamage = new Damage(Damage1);
                        Damage ddamage2 = new Damage(Damage2);
                        ddamage.AddAttribute("Physical");
                        ddamage2.AddAttribute("Physical");
                        Event E3 = Event.New("TakeDamage", 0, 0, 0);
                        E3.AddParameter("Damage", (object)ddamage);
                        E3.AddParameter("Owner", (object)this.ParentObject);
                        E3.AddParameter("Attacker", (object)this.ParentObject);

                        message = "";

                        if (!GO.FireEvent(E3) || ddamage.Amount == 0)
                        {
                            if (GO.IsPlayer())
                                message += "You are ";
                            else
                                message += GO.the + GO.DisplayName + GO.Is;

                            message += " not damaged by ";

                            if (this.ParentObject.IsPlayer())
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
                        E2.AddParameter("Damage", (object)ddamage2);
                        E2.AddParameter("Owner", (object)this.ParentObject);
                        E2.AddParameter("Attacker", (object)this.ParentObject);
                        if (!PossibleTarget.FireEvent(E2) || ddamage2.Amount == 0)
                        {
                            if (PossibleTarget.IsPlayer())
                                message += "You are ";
                            else
                                message += PossibleTarget.the + PossibleTarget.DisplayName + PossibleTarget.Is;

                            message += " not damaged by ";

                            if (this.ParentObject.IsPlayer())
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
                    this.TelekinesisGentlyPickupAndPlaceCreatureActivatedAbility.Cooldown = this.BasicCooldown * 10 + 10;
                    this.TelekinesisThrowActivatedAbility.Cooldown = this.BasicCooldown * 10 + 10;
                    this.ParentObject.UseEnergy(1000, "Mental");
                    return true;
                }
                return false;
            }
            if (E.ID == "CommandTelekinesisCreaturePickup")
            {
                Cell C = this.PickDestinationCell(this.Level, AllowVis.OnlyVisible, true);
                Cell C2;
                if (C == null)
                    return false;

                List<GameObject> CurrentObjectsInCell = C.GetObjectsWithPart("Combat");
                GameObject GO = (GameObject)null;
                if (CurrentObjectsInCell.Count == 0)
                {
                    if (this.ParentObject.IsPlayer())
                        Popup.Show("There are no targets.", true);
                    return false;
                }
                else
                    GO = CurrentObjectsInCell[0];

                GameObject Player = (GameObject)null;
                if (this.ParentObject.IsPlayer())
                    Player = this.ParentObject;

                if (GO != null && Player != null)
                {
                    XRLCore.Core.Game.Player.Body = GO; //For target picking which can only begin from player object
                    C2 = PickTarget.ShowPicker(PickTarget.PickStyle.Line, TelekinesisRange, TelekinesisRange, GO.pPhysics.CurrentCell.X, GO.pPhysics.CurrentCell.Y, false, AllowVis.OnlyVisible);
                    XRLCore.Core.Game.Player.Body = Player;
                    if (C2 != null)
                    {
                        if (C2.DistanceTo(GO) <= TelekinesisRange)
                        {
                            List<Point> pointList = Zone.Line(this.ParentObject.pPhysics.CurrentCell.X, this.ParentObject.pPhysics.CurrentCell.Y, C2.X, C2.Y);
                            int num = 0;
                            foreach (Point point in pointList)
                            {
                                Cell cell2 = this.ParentObject.pPhysics.CurrentCell.ParentZone.GetCell(point.X, point.Y);
                                if (cell2 != this.ParentObject.pPhysics.CurrentCell)
                                {
                                    foreach (GameObject gameObject in cell2.GetObjectsWithPart("Physics"))
                                    {
                                        if (num == pointList.Count - 1)
                                        {
                                            if (gameObject.pPhysics.Solid || gameObject.HasPart("Combat"))
                                            {
                                                Popup.Show("You can only place yourself into an empty space.", true);
                                                return true;
                                            }
                                        }
                                        else if (gameObject.pPhysics.Solid && !gameObject.pPhysics.HasTag("Flyover"))
                                        {
                                            Popup.Show("You can't place yourself over " + gameObject.the + gameObject.ShortDisplayName + ".", true);
                                            return true;
                                        }
                                    }
                                }
                                ++num;
                            }
                            C2.AddObject(GO);
                        }
                        else
                            return false;
                    }
                }
                else
                    return false;

                this.TelekinesisGentlyPickupAndPlaceCreatureActivatedAbility.Cooldown = this.BasicCooldown * 40 + 10;
                this.TelekinesisThrowActivatedAbility.Cooldown = this.BasicCooldown * 40 + 10;
                this.ParentObject.UseEnergy(1000, "Mental");
                return true;
            }
            if (E.ID == "CommandTelekinesisPickup")
            {
                Inventory ParentInventory = this.ParentObject.GetPart<Inventory>();
                XRLCore.Core.RenderBaseToBuffer(Popup._ScreenBuffer);
                Popup._TextConsole.DrawBuffer(Popup._ScreenBuffer, (IScreenBufferExtra)null, false);
                char key = 'a';
                Cell cell;
                cell = PickTarget.ShowPicker(PickTarget.PickStyle.Line, this.Level, this.Level, this.ParentObject.pPhysics.CurrentCell.X, this.ParentObject.pPhysics.CurrentCell.Y, false, AllowVis.OnlyVisible);
                if (cell.DistanceTo(this.ParentObject) > TelekinesisRange)
                {
                    MessageQueue.AddPlayerMessage("You can't reach that far.");
                    return true;
                }
                if (cell == null)
                    return true;
                List<GameObject> objectsInCell = new List<GameObject>();
                objectsInCell = cell.GetObjectsInCell();
                objectsInCell.Sort((IComparer<XRL.World.GameObject>)new SortGORenderLayer());
                string str = string.Empty;
                Dictionary<char, XRL.World.GameObject> objectsToPickUp = new Dictionary<char, XRL.World.GameObject>();

                foreach (GameObject gameObject in objectsInCell)
                {
                    if (gameObject != this.ParentObject && gameObject.GetPart<Physics>().Takeable == true)
                    {
                        objectsToPickUp.Add(key, gameObject);
                        str = str + key.ToString() + ") " + gameObject.DisplayName + "&y\n";
                        ++key;
                    }
                }

                bool requestInterfaceExit = false;
                if (objectsToPickUp.Count == 0)
                    if (this.ParentObject.IsPlayer())
                        Popup.Show("There's nothing to take.", true);
                    else if (objectsToPickUp.Count > 0)
                        PickItem.ShowPicker(new List<XRL.World.GameObject>((IEnumerable<XRL.World.GameObject>)objectsToPickUp.Values), ref requestInterfaceExit, (string)null, PickItem.PickItemDialogStyle.GetItemDialog, this.ParentObject);

                this.ParentObject.UseEnergy(1000, "Mental");
                return true;
            }
            if (E.ID == "CommandTelekinesisPickupThrownWeapon")
            {
                Popup.Show("You may pick up your weapon.", true);
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
            ActivatedAbilities part = GO.GetPart("ActivatedAbilities") as ActivatedAbilities;
            if (part != null)
            {
                this.TelekinesisGentlyPickupAndPlaceCreatureActivatedAbilityID = part.AddAbility("Lift and place creature", "CommandTelekinesisCreaturePickup", "Telekinesis");
                this.TelekinesisGentlyPickupAndPlaceCreatureActivatedAbility = part.AbilityByGuid[this.TelekinesisGentlyPickupAndPlaceCreatureActivatedAbilityID];
                this.TelekinesisPickupActivatedAbilityID = part.AddAbility("Pick up", "CommandTelekinesisPickup", "Telekinesis");
                this.TelekinesisPickupActivatedAbility = part.AbilityByGuid[this.TelekinesisPickupActivatedAbilityID];
                this.TelekinesisThrowActivatedAbilityID = part.AddAbility("Throw", "CommandTelekinesisThrow", "Telekinesis");
                this.TelekinesisThrowActivatedAbility = part.AbilityByGuid[this.TelekinesisThrowActivatedAbilityID];
                this.TelekinesisLaunchWeaponActivatedAbilityID = part.AddAbility("Throw weapon", "CommandTelekinesisThrowWeapon", "Telekinesis");
                this.TelekinesisLaunchWeaponActivatedAbility = part.AbilityByGuid[this.TelekinesisLaunchWeaponActivatedAbilityID];
                //this.TelekinesisPickUpPreviousWeaponID = part.AddAbility("Return weapon", "CommandTelekinesisPickupThrownWeapon", "Telekinesis");
                //this.TelekinesisPickUpPreviousWeapon = part.AbilityByGuid[this.TelekinesisPickUpPreviousWeaponID];
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if (this.TelekinesisGentlyPickupAndPlaceCreatureActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.TelekinesisGentlyPickupAndPlaceCreatureActivatedAbilityID);
                this.TelekinesisGentlyPickupAndPlaceCreatureActivatedAbilityID = Guid.Empty;
            }
            if (this.TelekinesisPickupActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.TelekinesisPickupActivatedAbilityID);
                this.TelekinesisPickupActivatedAbilityID = Guid.Empty;
            }
            if (this.TelekinesisLaunchWeaponActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.TelekinesisLaunchWeaponActivatedAbilityID);
                this.TelekinesisLaunchWeaponActivatedAbilityID = Guid.Empty;
            }
            if (this.TelekinesisThrowActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.TelekinesisThrowActivatedAbilityID);
                this.TelekinesisThrowActivatedAbilityID = Guid.Empty;
            }
            /*
            if (this.TelekinesisPickUpPreviousWeaponID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.TelekinesisThrowActivatedAbilityID);
                this.TelekinesisPickUpPreviousWeaponID = Guid.Empty;
            }
            */
            return base.Unmutate(GO);
        }
    }
}


/*=== More mental mutations Errors ===
\MMM_Telekinesis.cs(560,233): error CS1503: Argument 9: cannot convert from 'XRL.World.GameObject' to 'System.Predicate<XRL.World.GameObject>'
\MMM_Telekinesis.cs(613,239): error CS1503: Argument 9: cannot convert from 'XRL.World.GameObject' to 'System.Predicate<XRL.World.GameObject>'
== Warnings ==
\MMM_Obtenebration.cs(85,35): warning CS0219: The variable 'allCells' is assigned but its value is never used*/