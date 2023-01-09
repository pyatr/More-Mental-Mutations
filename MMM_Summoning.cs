using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Effects;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Summoning : BaseMutation
    {
        public Guid SummonWeaponActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry SummonWeaponActivatedAbility;
        public Guid SummonCreatureActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry SummonCreatureActivatedAbility;
        public Guid WeaponMenuActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry WeaponMenuActivatedAbility;
        public Guid CreatureMenuActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry CreatureMenuActivatedAbility;
        public Guid SpawnTwohandedActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry SpawnTwohandedActivatedAbility;
        public Guid SpawnDualWeaponsActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry SpawnDualWeaponsActivatedAbility;

        private List<string> WeaponBlueprints = new List<string>();
        private List<string> CreatureBlueprints = new List<string>();
        private string ChosenWeaponBlueprint = "long sword";
        private string ChosenCreatureBlueprint = "goat supreme";

        private List<GameObject> SpawnedWeapons = new List<GameObject>();

        public int SummonDuration = 50;
        
        public MMM_Summoning()
        {
            this.DisplayName = "Summoning";
            this.Type = "Mental";
            WeaponBlueprints.Add("long sword");
            WeaponBlueprints.Add("short sword");
            WeaponBlueprints.Add("dagger");
            WeaponBlueprints.Add("axe");
            WeaponBlueprints.Add("hammer");
            WeaponBlueprints.Add("mace");
            WeaponBlueprints.Add("staff");
            CreatureBlueprints.Add("goat supreme");
            CreatureBlueprints.Add("strider");
            CreatureBlueprints.Add("hell hound");
            CreatureBlueprints.Add("chimera");
            CreatureBlueprints.Add("wasps");
            CreatureBlueprints.Add("pseudo-flesh");
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "EndTurn");
            Object.RegisterPartEvent((IPart)this, "CommandSummonWeapon");
            Object.RegisterPartEvent((IPart)this, "CommandSummonCreature");
            Object.RegisterPartEvent((IPart)this, "CommandWeaponMenu");
            Object.RegisterPartEvent((IPart)this, "CommandCreatureMenu");
            Object.RegisterPartEvent((IPart)this, "CommandWeaponSpawnTwoHanded");
            Object.RegisterPartEvent((IPart)this, "CommandWeaponSpawnDual");
        }

        public override string GetDescription()
        {
            return "You may summon extradimensional creatures or weapons for " + this.SummonDuration.ToString() + " turns.";
        }

        public override string GetLevelText(int Level)
        {
            string stri = "";
            if (Level != this.Level)
            {
                stri += "Summon better weapons and creatures.";
            }
            else
            {

            }
            return stri;
        }

        public static string Menu(string Title, List<string> Entries)
        {
            if (Entries.Count > 0)
            {
                ScreenBuffer scrapBuffer1 = ScreenBuffer.GetScrapBuffer1(false);
                int MenuPosition = 0;
                string line = "";
                string emptyline = "";
                int i, max = 1;
                bool FinishedChoosing = false;
                Keys keys = Keys.None;
                for (i = 0; i < Entries.Count; i++)
                    if (Entries[i].Length > max)
                        max = Entries[i].Length;
                if (Title.Length > max)
                    max = Title.Length;

                for (i = 0; i <= max + 2; i++)
                    emptyline += " ";

                scrapBuffer1.SingleBox(40 - (2 + max / 2) - 1, 12 - (1 + Entries.Count / 2) - 1, 40 + (2 + max / 2) + 1, 12 + (0 + Entries.Count / 2 + Entries.Count % 2) + 1, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));

                scrapBuffer1.Goto(40 - (2 + max / 2), 12 - (1 + Entries.Count / 2));
                scrapBuffer1.Write(emptyline);
                scrapBuffer1.Goto(40 - (2 + max / 2), 12 + (1 + Entries.Count / 2) + 2);
                scrapBuffer1.Write(emptyline);

                string actualtitle = "[";
                actualtitle += Title += "]";
                scrapBuffer1.Goto(40 - (2 + max / 2) + 1, 12 - (1 + Entries.Count / 2) - 1);
                scrapBuffer1.Write(actualtitle);

                while (!FinishedChoosing)
                {
                    for (i = 0; i < Entries.Count; i++)
                    {
                        line = "";
                        if (i == MenuPosition)
                            line += ">";
                        else
                            line += " ";
                        line += " ";
                        line += Entries[i];
                        scrapBuffer1.Goto(40 - (2 + max / 2), 12 - (1 + Entries.Count / 2) + i + 1);
                        scrapBuffer1.Write(emptyline);
                        scrapBuffer1.Goto(40 - (2 + max / 2) + 1, 12 - (1 + Entries.Count / 2) + i + 1);
                        scrapBuffer1.Write(line);
                        Popup._TextConsole.DrawBuffer(scrapBuffer1, (IScreenBufferExtra)null, false);
                    }
                    keys = Keyboard.getvk(Options.GetOption("OptionMapDirectionsToKeypad", string.Empty) == "Yes", true);
                    if (keys == Keys.Enter || keys == Keys.Space)
                        FinishedChoosing = true;
                    if (keys == Keys.NumPad2)
                    {
                        if (MenuPosition >= Entries.Count - 1)
                            MenuPosition = 0;
                        else
                            MenuPosition++;
                    }
                    if (keys == Keys.NumPad8)
                    {
                        if (MenuPosition <= 0)
                            MenuPosition = Entries.Count - 1;
                        else
                            MenuPosition--;
                    }
                    keys = Keys.None;
                }
                return Entries[MenuPosition];
            }
            else return "";
        }

        public GameObject SpawnWeapon()
        {
            GameObject GO = (GameObject)null;
            int Dices = 1, Sides = 1, Bonus = 0;
            GO = GameObjectFactory.Factory.CreateObject("MMM_SummonedMeleeWeapon", 0, 0, null);//Making sure it doesn't spawn with mods
            GO.pPhysics.Weight = 12;
            if (this.ChosenWeaponBlueprint == "long sword")
            {
                GO.GetPart<MeleeWeapon>().Skill = "LongBlades";
                GO.GetPart<Render>().Tile = "items/sw_sword.bmp";
                Dices = this.Level / 6 + 1;
                Sides = 1 + (this.Level) % 6;
                if (this.SpawnTwohandedActivatedAbility.ToggleState == true) { GO.pPhysics.UsesTwoSlots = true; Dices++; Sides += 2; }
            }
            if (this.ChosenWeaponBlueprint == "short sword")
            {
                GO.GetPart<MeleeWeapon>().Skill = "ShortBlades";
                GO.GetPart<Render>().Tile = "items/sw_dagger1.bmp";
                GO.pPhysics.Weight /=2;
                Dices = 1;
                if (this.Level <= 9)
                {
                    Sides = 3 + this.Level;
                }
                else
                {
                    Sides = 12;
                    Bonus = this.Level - 9;
                }
            }
            if (this.ChosenWeaponBlueprint == "axe")
            {
                GO.GetPart<MeleeWeapon>().Skill = "Axe";
                GO.GetPart<Render>().Tile = "Assets_Content_Textures_Items_sw_axe.bmp";
                if (this.Level <= 6)
                    Sides += this.Level + 1;
                else
                {
                    Sides = 8;
                    Bonus += this.Level - 6;
                }
                if (this.SpawnTwohandedActivatedAbility.ToggleState == true)
                {
                    GO.pPhysics.UsesTwoSlots = true;
                    Bonus += 2;
                }
            }
            if (this.ChosenWeaponBlueprint == "dagger")
            {
                GO.GetPart<MeleeWeapon>().Skill = "ShortBlades";
                GO.GetPart<Render>().Tile = "items/sw_dagger1.bmp";
                GO.pPhysics.Weight /= 2;
                Dices = 1;
                if (this.Level <= 9) { Sides = 3 + this.Level; }
                else { Sides = 12; Bonus = this.Level - 9; }
            }
            if (this.ChosenWeaponBlueprint == "hammer")
            {
                GO.GetPart<MeleeWeapon>().Skill = "Cudgel";
                GO.GetPart<Render>().RenderString = "/";
                Dices = 2 + this.Level / 10;
                Sides = 1 + this.Level % 12;
                if (this.SpawnTwohandedActivatedAbility.ToggleState == true) { GO.pPhysics.UsesTwoSlots = true; Bonus = 2; }
            }
            if (this.ChosenWeaponBlueprint == "mace")
            {
                GO.GetPart<MeleeWeapon>().Skill = "Cudgel";
                GO.GetPart<Render>().RenderString = "/";
                Dices = 2 + this.Level / 6;
                Sides = 1 + this.Level % 3;
                if (this.SpawnTwohandedActivatedAbility.ToggleState == true) { GO.pPhysics.UsesTwoSlots = true; Bonus = 3; }
            }
            if (this.ChosenWeaponBlueprint == "staff")
            {
                GO.GetPart<MeleeWeapon>().Skill = "Cudgel";
                GO.GetPart<Render>().RenderString = "/";
                Dices = 2;
                Sides = 1 + this.Level / 2;
                if (this.SpawnTwohandedActivatedAbility.ToggleState == true)
                {
                    GO.pPhysics.UsesTwoSlots = true; Dices++;
                }
            }

            GO.GetPart<Description>().Short = "";//Adds to the mystery
            GO.GetPart<MeleeWeapon>().MaxStrengthBonus = this.Level;
            GO.GetPart<MeleeWeapon>().BaseDamage = Dices.ToString() + "d" + Sides.ToString();
            if (Bonus > 0) GO.GetPart<MeleeWeapon>().BaseDamage += "+" + Bonus.ToString();
            if (GO.pPhysics.UsesTwoSlots == true)
            {
                GO.DisplayName = "&oextradimensional &btwo-handed " + this.ChosenWeaponBlueprint;
                GO.pPhysics.Weight *= 10;
                GO.pPhysics.Weight /= 7;
            }
            else
                GO.DisplayName = "&oextradimensional &b" + this.ChosenWeaponBlueprint;
            GO.pRender.ColorString = "&b";
            GO.AddPart<Temporary>(new Temporary(this.SummonDuration), true);
            this.ParentObject.ApplyEffect((Effect)new MMM_EffectSummonedSomething(this.SummonDuration, GO.DisplayName));
            return GO;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandSummonWeapon")
            {
                if (SpawnedWeapons.Count == 0)
                {
                    List<BodyPart> ReqHands = new List<BodyPart>();

                    SpawnedWeapons.Add(SpawnWeapon());
                    if (SpawnedWeapons[0].pPhysics.UsesTwoSlots == false && this.SpawnDualWeaponsActivatedAbility.ToggleState)
                        SpawnedWeapons.Add(SpawnWeapon());

                    Body part1 = this.ParentObject.GetPart("Body") as Body;

                    foreach (BodyPart bodyPart in part1.GetParts())
                    {
                        if (bodyPart.Type == "Hand")
                        {
                            if (ReqHands.Count < 1 || (this.SpawnTwohandedActivatedAbility.ToggleState || this.SpawnDualWeaponsActivatedAbility.ToggleState) && ReqHands.Count < 2)
                                ReqHands.Add(bodyPart);
                        }
                    }

                    if (SpawnedWeapons[0].pPhysics.UsesTwoSlots != true && this.SpawnDualWeaponsActivatedAbility.ToggleState)//Spawning dual weapons
                    {
                        int HandNumber = 0;
                        //MessageQueue.AddPlayerMessage(ReqHands.Count.ToString());
                        foreach (BodyPart bodyPart in ReqHands)
                        {
                            if (bodyPart.Equipped != null && bodyPart.Equipped.GetIntProperty("Natural", 0) == 0/* && (bodyPart.Equipped.GetBlueprint().InheritsFrom("MeleeWeapon"))*/)
                            {
                                GameObject equipped = bodyPart.Equipped;
                                this.ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", (object)bodyPart));
                            }

                            Event E2 = Event.New("CommandForceEquipObject", 0, 0, 0);
                            E2.AddParameter("Object", (object)SpawnedWeapons[HandNumber]);
                            E2.AddParameter("BodyPart", (object)bodyPart);
                            this.ParentObject.FireEvent(E2);
                            HandNumber++;
                        }
                    }

                    if (SpawnedWeapons[0].pPhysics.UsesTwoSlots)//Spawning two-handed weapons
                    {
                        foreach (BodyPart bodyPart in ReqHands)
                        {
                            if (bodyPart.Equipped != null && bodyPart.Equipped.GetIntProperty("Natural", 0) == 0/* && (bodyPart.Equipped.GetBlueprint().InheritsFrom("MeleeWeapon"))*/)
                            {
                                GameObject equipped = bodyPart.Equipped;
                                this.ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", (object)bodyPart));
                            }
                        }
                        Event E2 = Event.New("CommandForceEquipObject", 0, 0, 0);
                        E2.AddParameter("Object", (object)SpawnedWeapons[0]);
                        E2.AddParameter("BodyPart", (object)ReqHands[0]);
                        this.ParentObject.FireEvent(E2);
                    }
                    else//Spawning one weapon
                    {
                        if (ReqHands[0].Equipped != null && ReqHands[0].Equipped.GetIntProperty("Natural", 0) == 0/* && (bodyPart.Equipped.GetBlueprint().InheritsFrom("MeleeWeapon"))*/)
                        {
                            GameObject equipped = ReqHands[0].Equipped;
                            this.ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", (object)ReqHands[0]));
                        }

                        Event E2 = Event.New("CommandForceEquipObject", 0, 0, 0);
                        E2.AddParameter("Object", (object)SpawnedWeapons[0]);
                        E2.AddParameter("BodyPart", (object)ReqHands[0]);
                        this.ParentObject.FireEvent(E2);
                    }
                    return true;
                }
                else
                {
                    if (this.ParentObject.IsPlayer())
                    {
                        if (SpawnedWeapons.Count > 1)
                            Popup.Show("You already have spawned weapons.", false);
                        else
                            Popup.Show("You already have a spawned weapon.", false);
                    }
                    return true;
                }
            }
            if (E.ID == "CommandSummonCreature")
            {

                return true;
            }
            if (E.ID == "CommandWeaponMenu")
            {
                this.ChosenWeaponBlueprint = Menu("Choose weapon", WeaponBlueprints);
                this.SummonWeaponActivatedAbility.DisplayName = "Summon weapon" + " [" + this.ChosenWeaponBlueprint + "]";

                return true;
            }
            if (E.ID == "CommandCreatureMenu")
            {
                this.ChosenCreatureBlueprint = Menu("Choose creature", CreatureBlueprints);
                this.SummonCreatureActivatedAbility.DisplayName = "Summon creature" + " [" + this.ChosenCreatureBlueprint + "]";
                return true;
            }
            if (E.ID == "CommandWeaponSpawnTwoHanded")
            {
                this.SpawnTwohandedActivatedAbility.ToggleState = !this.SpawnTwohandedActivatedAbility.ToggleState;
                return true;
            }
            if (E.ID == "CommandWeaponSpawnDual")
            {
                this.SpawnDualWeaponsActivatedAbility.ToggleState = !this.SpawnDualWeaponsActivatedAbility.ToggleState;
                return true;
            }
            if (E.ID == "EndTurn")
            {
                if (!this.ParentObject.HasEffect("MMM_EffectSummonedSomething") && this.SpawnedWeapons.Count > 0)
                    this.SpawnedWeapons.Clear();
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
                this.SummonWeaponActivatedAbilityID = part.AddAbility("Summon weapon", "CommandSummonWeapon", "Summoning");
                this.SummonWeaponActivatedAbility = part.AbilityByGuid[this.SummonWeaponActivatedAbilityID];
                this.SummonCreatureActivatedAbilityID = part.AddAbility("Summon creature", "CommandSummonCreature", "Summoning");
                this.SummonCreatureActivatedAbility = part.AbilityByGuid[this.SummonCreatureActivatedAbilityID];
                this.WeaponMenuActivatedAbilityID = part.AddAbility("Choose weapon", "CommandWeaponMenu", "Summoning");
                this.WeaponMenuActivatedAbility = part.AbilityByGuid[this.WeaponMenuActivatedAbilityID];
                this.CreatureMenuActivatedAbilityID = part.AddAbility("Choose creature", "CommandCreatureMenu", "Summoning");
                this.CreatureMenuActivatedAbility = part.AbilityByGuid[this.CreatureMenuActivatedAbilityID];
                this.SpawnTwohandedActivatedAbilityID = part.AddAbility("Spawn two-handed weapons", "CommandWeaponSpawnTwoHanded", "Summoning");
                this.SpawnTwohandedActivatedAbility = part.AbilityByGuid[this.SpawnTwohandedActivatedAbilityID];
                this.SpawnTwohandedActivatedAbility.Toggleable = true;
                this.SpawnTwohandedActivatedAbility.ToggleState = false;
                this.SpawnDualWeaponsActivatedAbilityID = part.AddAbility("Spawn dual weapons", "CommandWeaponSpawnDual", "Summoning");
                this.SpawnDualWeaponsActivatedAbility = part.AbilityByGuid[this.SpawnDualWeaponsActivatedAbilityID];
                this.SpawnDualWeaponsActivatedAbility.Toggleable = true;
                this.SpawnDualWeaponsActivatedAbility.ToggleState = false;
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if (this.SummonWeaponActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.SummonWeaponActivatedAbilityID);
                this.SummonWeaponActivatedAbilityID = Guid.Empty;
            }
            if (this.SummonCreatureActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.SummonCreatureActivatedAbilityID);
                this.SummonCreatureActivatedAbilityID = Guid.Empty;
            }
            if (this.WeaponMenuActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.WeaponMenuActivatedAbilityID);
                this.WeaponMenuActivatedAbilityID = Guid.Empty;
            }
            if (this.CreatureMenuActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.CreatureMenuActivatedAbilityID);
                this.CreatureMenuActivatedAbilityID = Guid.Empty;
            }
            if (this.SpawnTwohandedActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.SpawnTwohandedActivatedAbilityID);
                this.SpawnTwohandedActivatedAbilityID = Guid.Empty;
            }
            if (this.SpawnDualWeaponsActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.SpawnDualWeaponsActivatedAbilityID);
                this.SpawnDualWeaponsActivatedAbilityID = Guid.Empty;
            }
            return base.Unmutate(GO);
        }
    }
}