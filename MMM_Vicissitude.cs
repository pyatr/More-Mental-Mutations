using System;
using System.Collections.Generic;
using System.Linq;
using XRL.Core;
using XRL.Messages;
using XRL.UI;
using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using ConsoleLib.Console;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Vicissitude : BaseMutation
    {
        public Guid DisassembleActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry DisassembleActivatedAbility;

        private List<string> BodyParts = new List<string>();

        public MMM_Vicissitude()
        {
            this.DisplayName = "Vicissitude";
            BodyParts.Add("MMM_Bones");
            BodyParts.Add("MMM_Sinew");
            BodyParts.Add("MMM_Muscle");
            BodyParts.Add("MMM_Nerve");
            BodyParts.Add("MMM_Skin");
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

                scrapBuffer1.SingleBox(40 - (2 + max / 2) - 1, 12 - (1 + Dic.Count / 2) - 1, 40 + (2 + max / 2) + 1, 12 + (0 + Dic.Count / 2 + Dic.Count % 2) + 1, ColorUtility.MakeColor(TextColor.Grey, TextColor.Black));

                scrapBuffer1.Goto(40 - (2 + max / 2), 12 - (1 + Dic.Count / 2));
                scrapBuffer1.Write(emptyline);
                scrapBuffer1.Goto(40 - (2 + max / 2), 12 + (1 + Dic.Count / 2) + 2);
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
                        Popup._TextConsole.DrawBuffer(scrapBuffer1, (IScreenBufferExtra)null, false);
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
            else return (GameObject) null;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "CommandTakeCorpseApart");
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
                Inventory inventory = this.ParentObject.GetPart<Inventory>();
                Dictionary<string, GameObject> Corpses = new Dictionary<string, GameObject>();

                foreach (GameObject GO in inventory.GetObjects())
                {
                    if (GO.pPhysics.Category == "Corpse")
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
                            Popup.Show("There is nothing valuable in that body", false);
                            return true;
                        }

                        if (body.DisplayName.Contains(" head "))
                        {
                            this.ParentObject.GetPart<Inventory>().AddObject("MMM_Brain", true);
                            i = Stat.Random(1, 32);
                            if (i < 7)
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Nerve", true);
                            if (i < 14)
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Nerve", true);
                            this.ParentObject.FlingBlood();
                            body.Destroy();
                            this.ParentObject.UseEnergy(1000, "Physical");
                            return true;
                        }

                        if (body.DisplayName.Contains(" arm ") || body.DisplayName.Contains(" feet "))
                        {
                            i = Stat.Random(1, 32);
                            if (i < 9)
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Bones", true);
                            else if (i >= 9 && i < 16)
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Skin", true);
                            else if (i >= 16 && i < 25)
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Muscle", true);
                            else
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Sinew", true);

                            this.ParentObject.FlingBlood();
                            body.Destroy();
                            this.ParentObject.UseEnergy(1000, "Physical");
                            return true;
                        }

                        bool BrainAdded = false;
                        for (int j = 5; j < body.pPhysics.Weight; j += 25 - this.Level)
                        {
                            this.ParentObject.FlingBlood();
                                
                            this.ParentObject.UseEnergy(1000, "Physical");
                            i = Stat.Random(0, 39);
                            if (i == 0 && BrainAdded == false)
                            {
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Brain", true);
                                BrainAdded = true;
                            }
                            else if (i < 9 && i > 0)
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Bones", true);
                            else if (i >= 9 && i < 16)
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Skin", true);
                            else if (i >= 16 && i < 27)
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Muscle", true);
                            else if (i >= 27 && i < 34)
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Sinew", true);
                            else
                                this.ParentObject.GetPart<Inventory>().AddObject("MMM_Nerve", true);
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
            ActivatedAbilities part = GO.GetPart("ActivatedAbilities") as ActivatedAbilities;
            if (part != null)
            {
                this.DisassembleActivatedAbilityID = part.AddAbility("Disassemble body", "CommandTakeCorpseApart", "Vicissitude");
                this.DisassembleActivatedAbility = part.AbilityByGuid[this.DisassembleActivatedAbilityID];
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if (this.DisassembleActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.DisassembleActivatedAbilityID);
                this.DisassembleActivatedAbilityID = Guid.Empty;
            }
            return base.Unmutate(GO);
        }
    }
}
