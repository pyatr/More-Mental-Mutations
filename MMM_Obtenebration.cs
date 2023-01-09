using System;
using System.Collections.Generic;
using XRL.World;
using XRL.World.Parts;
using XRL.Core;
using XRL.World.Parts.Effects;
using XRL.Messages;
using XRL.UI;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_Obtenebration : BaseMutation
    {
        public Guid ObtenebrationActivatedAbilityID = Guid.Empty;
        public ActivatedAbilityEntry ObtenebrationActivatedAbility;
        public bool[,] litAreas;
        private int Density = 100;

        public MMM_Obtenebration()
        {
            this.DisplayName = "Obtenebration";
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "CommandObtenebration");
            Object.RegisterPartEvent((IPart)this, "EndTurn");
        }

        public override string GetDescription()
        {
            return "You see in the darkness and do some other stuff.";
        }

        public override string GetLevelText(int Level)
        {
            return "hmm";
        }

        public List<LightSource> GetLightSources()
        {
            List<GameObject> lightSourceObjects = new List<GameObject>();
            List<GameObject> Creatures = new List<GameObject>();
            Physics part = this.ParentObject.GetPart("Physics") as Physics;
            if (part != null && part.CurrentCell != null)
            {
                lightSourceObjects = part.CurrentCell.ParentZone.FastSquareSearch(part.CurrentCell.X, part.CurrentCell.Y, 80, "LightSource");
                Creatures = part.CurrentCell.ParentZone.FastSquareSearch(part.CurrentCell.X, part.CurrentCell.Y, 80, "Combat");
            }

            foreach (GameObject creature in Creatures)
            {
                List<GameObject> equippedItems = creature.GetEquippedObjects();
                foreach (GameObject lsi in equippedItems)
                {
                    LightSource ls1 = lsi.GetPart<LightSource>() as LightSource;
                    if (ls1 != null)
                    {
                        lightSourceObjects.Add(lsi);
                    }
                }
            }

            List<LightSource> lightSources = new List<LightSource>();
            foreach (GameObject GO in lightSourceObjects)
            {
                LightSource ls1 = GO.GetPart<LightSource>() as LightSource;
                if (!ls1.Darkvision && ls1.Lit)
                    lightSources.Add(ls1);
            }
            return lightSources;
        }

        public bool[,] GetLitCells()
        {
            bool[,] litAreas = new bool[80, 25];
            Physics part = this.ParentObject.GetPart("Physics") as Physics;
            if (part != null && part.CurrentCell != null)
            {
                Zone Z = part.CurrentCell.ParentZone;                
                int x = 0, y = 0;
                x = part.CurrentCell.X;
                y = part.CurrentCell.Y;
                int litCells = 0;//, allCells = 80 * 25;
                for (int i = 0; i < Z.Width; i++)
                {
                    for (int j = 0; j < Z.Height; j++)
                    {
                        /*
                        if (Z.GetLight(i, j) == LightLevel.Light)
                        {
                            litCells++;
                            litAreas[i, j] = true;
                        }
                        else
                            litAreas[i, j] = false;
                            */
                        litAreas[i, j] = false;
                    }
                }
                List<LightSource> lightSources = GetLightSources();
                foreach (LightSource ls in lightSources)
                {
                    //MessageQueue.AddPlayerMessage(ls.ParentObject.DisplayName);
                    int x1 = x;
                    int y1 = y;
                    int x1_1 = x - ls.Radius;
                    int x2 = x + ls.Radius;
                    int y1_1 = y - ls.Radius;
                    int y2 = y + ls.Radius;
                    part.CurrentCell.ParentZone.Constrain(ref x1_1, ref y1_1, ref x2, ref y2);
                    for (int x3 = x1_1; x3 <= x2; ++x3)
                    {
                        for (int y3 = y1_1; y3 <= y2; ++y3)
                        {
                            if (Math.Sqrt((double)((x3 - x1) * (x3 - x1) + (y3 - y1) * (y3 - y1))) <= (double)ls.Radius)
                            {
                                //if (!litAreas[x3, y3])
                                //{
                                litCells++;
                                litAreas[x3, y3] = true;
                                //}
                            }
                        }
                    }
                }
                //MessageQueue.AddPlayerMessage(litCells + "/" + allCells);
            }
            return litAreas;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn")
            {
                Physics part = this.ParentObject.GetPart("Physics") as Physics;
                if (part != null && part.CurrentCell != null)
                {
                    Zone Z = part.CurrentCell.ParentZone;
                    litAreas = GetLitCells();
                    int x = 0, y = 0;
                    x = part.CurrentCell.X;
                    y = part.CurrentCell.Y;
                    
                    List<GameObject> Creatures = new List<GameObject>();
                    Creatures = Z.FastSquareSearch(x, y, 80, "Physics");

                    foreach (GameObject gameObject2 in Creatures)
                    {
                        if (gameObject2.HasPart("Brain") && gameObject2.HasPart("Combat"))
                        {
                            if (gameObject2 != this.ParentObject && !gameObject2.HasEffect("MMM_EffectHighlightCreature"))
                            {
                                //Highlight creatures only in unlit areas 
                                if (!litAreas[gameObject2.pPhysics.CurrentCell.X, gameObject2.pPhysics.CurrentCell.Y])
                                {
                                    //MessageQueue.AddPlayerMessage(gameObject2.DisplayName);
                                    gameObject2.ApplyEffect((Effect)new MMM_EffectHighlightCreature());
                                }
                            }
                        }
                    }
                }
                return true;
            }
            if (E.ID == "CommandObtenebration")
            {
                List<Cell> adjacentCells = this.ParentObject.pPhysics.CurrentCell.GetAdjacentCells(true);
                adjacentCells.Add(this.ParentObject.pPhysics.CurrentCell);
                foreach (Cell cell in adjacentCells)
                {
                    foreach (Cell cell2 in cell.GetAdjacentCells(true))
                    {
                        GameObject GO = GameObjectFactory.Factory.CreateObject("MMM_DarknessGas");
                        Gas part2 = GO.GetPart("Gas") as Gas;
                        part2.Density = this.Density;
                        cell2.AddObject(GO);
                    }
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
            ActivatedAbilities part = GO.GetPart("ActivatedAbilities") as ActivatedAbilities;
            if (part != null)
            {
                this.ObtenebrationActivatedAbilityID = part.AddAbility("Obtenebration", "CommandObtenebration", "Mental Mutation");
                this.ObtenebrationActivatedAbility = part.AbilityByGuid[this.ObtenebrationActivatedAbilityID];
            }
            this.ChangeLevel(Level);
            return base.Mutate(GO, Level);
        }

        public override bool Unmutate(GameObject GO)
        {
            if (this.ObtenebrationActivatedAbilityID != Guid.Empty)
            {
                (GO.GetPart("ActivatedAbilities") as ActivatedAbilities).RemoveAbility(this.ObtenebrationActivatedAbilityID);
                this.ObtenebrationActivatedAbilityID = Guid.Empty;
            }
            return base.Unmutate(GO);
        }
    }
}