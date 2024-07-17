using System;
using System.Collections.Generic;
using XRL.UI;
using XRL.World.Anatomy;
using ConsoleLib.Console;
using MoreMentalMutations.Effects;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Summoning : MMM_BaseMutation
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
            DisplayName = "Summoning";
            Type = "Mental";
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

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");
            Registrar.Register("CommandSummonWeapon");
            Registrar.Register("CommandSummonCreature");
            Registrar.Register("CommandWeaponMenu");
            Registrar.Register("CommandCreatureMenu");
            Registrar.Register("CommandWeaponSpawnTwoHanded");
            Registrar.Register("CommandWeaponSpawnDual");

            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            return "You may summon extradimensional creatures or weapons for " + SummonDuration.ToString() + " turns.";
        }

        public override string GetLevelText(int Level)
        {
            string stri = "";

            if (Level != this.Level)
            {
                stri += "Summon better weapons and creatures.";
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

                scrapBuffer1.SingleBox(40 - (2 + max / 2) - 1, 12 - (1 + Entries.Count / 2) - 1, 40 + 2 + max / 2 + 1, 12 + 0 + Entries.Count / 2 + Entries.Count % 2 + 1, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));

                scrapBuffer1.Goto(40 - (2 + max / 2), 12 - (1 + Entries.Count / 2));
                scrapBuffer1.Write(emptyline);
                scrapBuffer1.Goto(40 - (2 + max / 2), 12 + 1 + Entries.Count / 2 + 2);
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
                        Popup._TextConsole.DrawBuffer(scrapBuffer1, null, false);
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
            int Dices = 1, Sides = 1, Bonus = 0;
            GameObject GO = GameObjectFactory.Factory.CreateObject("SummonedMeleeWeapon", 0, 0, null);//Making sure it doesn't spawn with mods
            GO.Physics.Weight = 12;

            if (ChosenWeaponBlueprint == "long sword")
            {
                GO.GetPart<MeleeWeapon>().Skill = "LongBlades";
                GO.Render.Tile = "items/sw_sword.bmp";
                Dices = Level / 6 + 1;
                Sides = 1 + Level % 6;
                if (SpawnTwohandedActivatedAbility.ToggleState == true) { GO.Physics.UsesTwoSlots = true; Dices++; Sides += 2; }
            }

            if (ChosenWeaponBlueprint == "short sword")
            {
                GO.GetPart<MeleeWeapon>().Skill = "ShortBlades";
                GO.Render.Tile = "items/sw_dagger1.bmp";
                GO.Physics.Weight /= 2;
                Dices = 1;
                if (Level <= 9)
                {
                    Sides = 3 + Level;
                }
                else
                {
                    Sides = 12;
                    Bonus = Level - 9;
                }
            }
            if (ChosenWeaponBlueprint == "axe")
            {
                GO.GetPart<MeleeWeapon>().Skill = "Axe";
                GO.GetPart<Render>().Tile = "Assets_Content_Textures_Items_sw_axe.bmp";
                if (Level <= 6)
                    Sides += Level + 1;
                else
                {
                    Sides = 8;
                    Bonus += Level - 6;
                }
                if (SpawnTwohandedActivatedAbility.ToggleState == true)
                {
                    GO.Physics.UsesTwoSlots = true;
                    Bonus += 2;
                }
            }
            if (ChosenWeaponBlueprint == "dagger")
            {
                GO.GetPart<MeleeWeapon>().Skill = "ShortBlades";
                GO.GetPart<Render>().Tile = "items/sw_dagger1.bmp";
                GO.Physics.Weight /= 2;
                Dices = 1;
                if (Level <= 9) { Sides = 3 + Level; }
                else { Sides = 12; Bonus = Level - 9; }
            }
            if (ChosenWeaponBlueprint == "hammer")
            {
                GO.GetPart<MeleeWeapon>().Skill = "Cudgel";
                GO.GetPart<Render>().RenderString = "/";
                Dices = 2 + Level / 10;
                Sides = 1 + Level % 12;
                if (SpawnTwohandedActivatedAbility.ToggleState == true) { GO.Physics.UsesTwoSlots = true; Bonus = 2; }
            }
            if (ChosenWeaponBlueprint == "mace")
            {
                GO.GetPart<MeleeWeapon>().Skill = "Cudgel";
                GO.GetPart<Render>().RenderString = "/";
                Dices = 2 + Level / 6;
                Sides = 1 + Level % 3;
                if (SpawnTwohandedActivatedAbility.ToggleState == true) { GO.Physics.UsesTwoSlots = true; Bonus = 3; }
            }
            if (ChosenWeaponBlueprint == "staff")
            {
                GO.GetPart<MeleeWeapon>().Skill = "Cudgel";
                GO.GetPart<Render>().RenderString = "/";
                Dices = 2;
                Sides = 1 + Level / 2;
                if (SpawnTwohandedActivatedAbility.ToggleState == true)
                {
                    GO.Physics.UsesTwoSlots = true; Dices++;
                }
            }

            GO.GetPart<Description>().Short = "";//Adds to the mystery
            GO.GetPart<MeleeWeapon>().MaxStrengthBonus = Level;
            GO.GetPart<MeleeWeapon>().BaseDamage = Dices.ToString() + "d" + Sides.ToString();
            if (Bonus > 0) GO.GetPart<MeleeWeapon>().BaseDamage += "+" + Bonus.ToString();
            if (GO.Physics.UsesTwoSlots == true)
            {
                GO.DisplayName = "&oextradimensional &btwo-handed " + ChosenWeaponBlueprint;
                GO.Physics.Weight *= 10;
                GO.Physics.Weight /= 7;
            }
            else
            {
                GO.DisplayName = "&oextradimensional &b" + ChosenWeaponBlueprint;
            }

            GO.Render.ColorString = "&b";
            GO.AddPart<XRL.World.Parts.Temporary>(new XRL.World.Parts.Temporary(SummonDuration), true);
            ParentObject.ApplyEffect(new MMM_EffectSummonedSomething(SummonDuration, GO.DisplayName));
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
                    if (SpawnedWeapons[0].Physics.UsesTwoSlots == false && SpawnDualWeaponsActivatedAbility.ToggleState)
                        SpawnedWeapons.Add(SpawnWeapon());

                    Body part1 = ParentObject.Body;

                    foreach (BodyPart bodyPart in part1.GetParts())
                    {
                        if (bodyPart.Type == "Hand")
                        {
                            if (ReqHands.Count < 1 || (SpawnTwohandedActivatedAbility.ToggleState || SpawnDualWeaponsActivatedAbility.ToggleState) && ReqHands.Count < 2)
                                ReqHands.Add(bodyPart);
                        }
                    }

                    if (SpawnedWeapons[0].Physics.UsesTwoSlots != true && SpawnDualWeaponsActivatedAbility.ToggleState)//Spawning dual weapons
                    {
                        int HandNumber = 0;
                        //MessageQueue.AddPlayerMessage(ReqHands.Count.ToString());
                        foreach (BodyPart bodyPart in ReqHands)
                        {
                            if (bodyPart.Equipped != null && bodyPart.Equipped.GetIntProperty("Natural", 0) == 0/* && (bodyPart.Equipped.GetBlueprint().InheritsFrom("MeleeWeapon"))*/)
                            {
                                GameObject equipped = bodyPart.Equipped;
                                ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", bodyPart));
                            }

                            Event E2 = Event.New("CommandForceEquipObject", 0, 0, 0);
                            E2.AddParameter("Object", SpawnedWeapons[HandNumber]);
                            E2.AddParameter("BodyPart", bodyPart);
                            ParentObject.FireEvent(E2);
                            HandNumber++;
                        }
                    }

                    if (SpawnedWeapons[0].Physics.UsesTwoSlots)//Spawning two-handed weapons
                    {
                        foreach (BodyPart bodyPart in ReqHands)
                        {
                            if (bodyPart.Equipped != null && bodyPart.Equipped.GetIntProperty("Natural", 0) == 0/* && (bodyPart.Equipped.GetBlueprint().InheritsFrom("MeleeWeapon"))*/)
                            {
                                GameObject equipped = bodyPart.Equipped;
                                ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", bodyPart));
                            }
                        }
                        Event E2 = Event.New("CommandForceEquipObject", 0, 0, 0);
                        E2.AddParameter("Object", SpawnedWeapons[0]);
                        E2.AddParameter("BodyPart", ReqHands[0]);
                        ParentObject.FireEvent(E2);
                    }
                    else//Spawning one weapon
                    {
                        if (ReqHands[0].Equipped != null && ReqHands[0].Equipped.GetIntProperty("Natural", 0) == 0/* && (bodyPart.Equipped.GetBlueprint().InheritsFrom("MeleeWeapon"))*/)
                        {
                            GameObject equipped = ReqHands[0].Equipped;
                            ParentObject.FireEvent(Event.New("CommandForceUnequipObject", "BodyPart", ReqHands[0]));
                        }

                        Event E2 = Event.New("CommandForceEquipObject", 0, 0, 0);
                        E2.AddParameter("Object", SpawnedWeapons[0]);
                        E2.AddParameter("BodyPart", ReqHands[0]);
                        ParentObject.FireEvent(E2);
                    }
                    return true;
                }
                else
                {
                    if (ParentObject.IsPlayer())
                    {
                        string message = "You already have spawned weapons.";

                        if (SpawnedWeapons.Count == 1)
                        {
                            message = "You already have a spawned weapon.";
                        }

                        Popup.Show(message);
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
                ChosenWeaponBlueprint = Menu("Choose weapon", WeaponBlueprints);
                SummonWeaponActivatedAbility.DisplayName = "Summon weapon" + " [" + ChosenWeaponBlueprint + "]";

                return true;
            }

            if (E.ID == "CommandCreatureMenu")
            {
                ChosenCreatureBlueprint = Menu("Choose creature", CreatureBlueprints);
                SummonCreatureActivatedAbility.DisplayName = "Summon creature" + " [" + ChosenCreatureBlueprint + "]";
                return true;
            }

            if (E.ID == "CommandWeaponSpawnTwoHanded")
            {
                SpawnTwohandedActivatedAbility.ToggleState = !SpawnTwohandedActivatedAbility.ToggleState;
                return true;
            }

            if (E.ID == "CommandWeaponSpawnDual")
            {
                SpawnDualWeaponsActivatedAbility.ToggleState = !SpawnDualWeaponsActivatedAbility.ToggleState;
                return true;
            }

            if (E.ID == "EndTurn")
            {
                if (!ParentObject.HasEffect<MMM_EffectSummonedSomething>() && SpawnedWeapons.Count > 0)
                {
                    SpawnedWeapons.Clear();
                }

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
                SummonWeaponActivatedAbilityID = part.AddAbility("Summon weapon", "CommandSummonWeapon", "Summoning");
                SummonWeaponActivatedAbility = part.AbilityByGuid[SummonWeaponActivatedAbilityID];
                SummonCreatureActivatedAbilityID = part.AddAbility("Summon creature", "CommandSummonCreature", "Summoning");
                SummonCreatureActivatedAbility = part.AbilityByGuid[SummonCreatureActivatedAbilityID];
                WeaponMenuActivatedAbilityID = part.AddAbility("Choose weapon", "CommandWeaponMenu", "Summoning");
                WeaponMenuActivatedAbility = part.AbilityByGuid[WeaponMenuActivatedAbilityID];
                CreatureMenuActivatedAbilityID = part.AddAbility("Choose creature", "CommandCreatureMenu", "Summoning");
                CreatureMenuActivatedAbility = part.AbilityByGuid[CreatureMenuActivatedAbilityID];
                SpawnTwohandedActivatedAbilityID = part.AddAbility("Spawn two-handed weapons", "CommandWeaponSpawnTwoHanded", "Summoning");
                SpawnTwohandedActivatedAbility = part.AbilityByGuid[SpawnTwohandedActivatedAbilityID];
                SpawnTwohandedActivatedAbility.Toggleable = true;
                SpawnTwohandedActivatedAbility.ToggleState = false;
                SpawnDualWeaponsActivatedAbilityID = part.AddAbility("Spawn dual weapons", "CommandWeaponSpawnDual", "Summoning");
                SpawnDualWeaponsActivatedAbility = part.AbilityByGuid[SpawnDualWeaponsActivatedAbilityID];
                SpawnDualWeaponsActivatedAbility.Toggleable = true;
                SpawnDualWeaponsActivatedAbility.ToggleState = false;
            }

            ChangeLevel(Level);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMutationByGUID(GO, ref SummonWeaponActivatedAbilityID);
            RemoveMutationByGUID(GO, ref SummonCreatureActivatedAbilityID);
            RemoveMutationByGUID(GO, ref WeaponMenuActivatedAbilityID);
            RemoveMutationByGUID(GO, ref CreatureMenuActivatedAbilityID);
            RemoveMutationByGUID(GO, ref SpawnTwohandedActivatedAbilityID);
            RemoveMutationByGUID(GO, ref SpawnDualWeaponsActivatedAbilityID);

            return base.Unmutate(GO);
        }
    }
}