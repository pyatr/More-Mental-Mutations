using System;
using System.Collections.Generic;
using System.Linq;
using XRL.UI;
using XRL.Rules;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Vicissitude : MMM_BaseMutation
    {
        public Guid DisassembleActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry DisassembleActivatedAbility;

        private List<string> BodyParts = new List<string>();

        public MMM_Vicissitude()
        {
            DisplayName = "Vicissitude";
            BodyParts.Add("Bones");
            BodyParts.Add("Sinew");
            BodyParts.Add("Muscle");
            BodyParts.Add("Nerve");
            BodyParts.Add("Skin");
        }

        public static GameObject Menu(string Title, Dictionary<string, GameObject> Dic)
        {
            if (Dic.Count > 0)
            {
                ScreenBuffer scrapBuffer1 = ScreenBuffer.GetScrapBuffer1(false);
                int MenuPosition = 0;
                string line = "";
                string emptyline = "";
                int i, max = 1;
                bool FinishedChoosing = false;
                Keys keys = Keys.None;
                for (i = 0; i < Dic.Count; i++)
                    if (Dic.Keys.ElementAt(i).Length > max)
                        max = Dic.Keys.ElementAt(i).Length;
                if (Title.Length > max)
                    max = Title.Length;

                for (i = 0; i <= max + 2; i++)
                    emptyline += " ";

                scrapBuffer1.SingleBox(40 - (2 + max / 2) - 1, 12 - (1 + Dic.Count / 2) - 1, 40 + 2 + max / 2 + 1, 12 + 0 + Dic.Count / 2 + Dic.Count % 2 + 1, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));

                scrapBuffer1.Goto(40 - (2 + max / 2), 12 - (1 + Dic.Count / 2));
                scrapBuffer1.Write(emptyline);
                scrapBuffer1.Goto(40 - (2 + max / 2), 12 + 1 + Dic.Count / 2 + 2);
                scrapBuffer1.Write(emptyline);

                string actualtitle = "[";
                actualtitle += Title += "]";
                scrapBuffer1.Goto(40 - (2 + max / 2) + 1, 12 - (1 + Dic.Count / 2) - 1);
                scrapBuffer1.Write(actualtitle);

                while (!FinishedChoosing)
                {
                    for (i = 0; i < Dic.Count; i++)
                    {
                        line = "";
                        if (i == MenuPosition)
                            line += ">";
                        else
                            line += " ";
                        line += " ";
                        line += Dic.Keys.ElementAt(i);
                        scrapBuffer1.Goto(40 - (2 + max / 2), 12 - (1 + Dic.Count / 2) + i + 1);
                        scrapBuffer1.Write(emptyline);
                        scrapBuffer1.Goto(40 - (2 + max / 2) + 1, 12 - (1 + Dic.Count / 2) + i + 1);
                        scrapBuffer1.Write(line);
                        Popup._TextConsole.DrawBuffer(scrapBuffer1, null, false);
                    }
                    keys = Keyboard.getvk(Options.GetOption("OptionMapDirectionsToKeypad", string.Empty) == "Yes", true);
                    if (keys == Keys.Enter || keys == Keys.Space)
                        FinishedChoosing = true;
                    if (keys == Keys.NumPad2)
                    {
                        if (MenuPosition >= Dic.Count - 1)
                            MenuPosition = 0;
                        else
                            MenuPosition++;
                    }
                    if (keys == Keys.NumPad8)
                    {
                        if (MenuPosition <= 0)
                            MenuPosition = Dic.Count - 1;
                        else
                            MenuPosition--;
                    }
                    keys = Keys.None;
                }
                return Dic.Values.ElementAt(MenuPosition);
            }
            else return null;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent(this, "CommandTakeCorpseApart");

            base.Register(Object, Registrar);
        }

        public override string GetDescription()
        {
            return "";
        }

        public override string GetLevelText(int Level)
        {
            return "";
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "CommandTakeCorpseApart")
            {
                Inventory inventory = ParentObject.GetPart<Inventory>();
                Dictionary<string, GameObject> Corpses = new Dictionary<string, GameObject>();

                foreach (GameObject GO in inventory.GetObjects())
                {
                    if (GO.Physics.Category == "Corpse")
                    {
                        Corpses.Add(GO.DisplayName, GO);
                    }
                }
                if (Corpses.Count > 0)
                {
                    GameObject body = Menu("Choose body to take apart", Corpses);
                    if (body != null)
                    {
                        int i = 0;

                        if (body.DisplayName.Contains("amoeba"))
                        {
                            Popup.Show("There is nothing valuable in that body");
                            return true;
                        }

                        if (body.DisplayName.Contains(" head "))
                        {
                            ParentObject.GetPart<Inventory>().AddObject("Brain", true);
                            i = Stat.Random(1, 32);
                            if (i < 7)
                                ParentObject.GetPart<Inventory>().AddObject("Nerve", true);
                            if (i < 14)
                                ParentObject.GetPart<Inventory>().AddObject("Nerve", true);
                            ParentObject.FlingBlood();
                            body.Destroy();
                            ParentObject.UseEnergy(1000, "Physical");
                            return true;
                        }

                        if (body.DisplayName.Contains(" arm ") || body.DisplayName.Contains(" feet "))
                        {
                            i = Stat.Random(1, 32);
                            if (i < 9)
                                ParentObject.GetPart<Inventory>().AddObject("Bones", true);
                            else if (i >= 9 && i < 16)
                                ParentObject.GetPart<Inventory>().AddObject("Skin", true);
                            else if (i >= 16 && i < 25)
                                ParentObject.GetPart<Inventory>().AddObject("Muscle", true);
                            else
                                ParentObject.GetPart<Inventory>().AddObject("Sinew", true);

                            ParentObject.FlingBlood();
                            body.Destroy();
                            ParentObject.UseEnergy(1000, "Physical");
                            return true;
                        }

                        bool BrainAdded = false;
                        for (int j = 5; j < body.Physics.Weight; j += 25 - Level)
                        {
                            ParentObject.FlingBlood();

                            ParentObject.UseEnergy(1000, "Physical");
                            i = Stat.Random(0, 39);
                            if (i == 0 && BrainAdded == false)
                            {
                                ParentObject.GetPart<Inventory>().AddObject("Brain", true);
                                BrainAdded = true;
                            }
                            else if (i < 9 && i > 0)
                                ParentObject.GetPart<Inventory>().AddObject("Bones", true);
                            else if (i >= 9 && i < 16)
                                ParentObject.GetPart<Inventory>().AddObject("Skin", true);
                            else if (i >= 16 && i < 27)
                                ParentObject.GetPart<Inventory>().AddObject("Muscle", true);
                            else if (i >= 27 && i < 34)
                                ParentObject.GetPart<Inventory>().AddObject("Sinew", true);
                            else
                                ParentObject.GetPart<Inventory>().AddObject("Nerve", true);
                        }
                        body.Destroy();
                        return true;
                    }
                }
                else
                {
                    Popup.Show("You don't have any corpses to disassemble.");
                    return true;
                }
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
                DisassembleActivatedAbilityID = part.AddAbility("Disassemble body", "CommandTakeCorpseApart", "Vicissitude");
                DisassembleActivatedAbility = part.AbilityByGuid[DisassembleActivatedAbilityID];
            }

            ChangeLevel(Level);

            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            RemoveMutationByGUID(GO, ref DisassembleActivatedAbilityID);

            return base.Unmutate(GO);
        }
    }
}
