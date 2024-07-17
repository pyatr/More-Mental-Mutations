using System;
using XRL;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace MoreMentalMutations.Effects
{
    [Serializable]
    public class MMM_EffectHighlightCreature : Effect
    {
        GameObject Seer;
        MMM_Obtenebration Source;

        public MMM_EffectHighlightCreature()
        {

        }

        public MMM_EffectHighlightCreature(GameObject _Seer, MMM_Obtenebration _Source) : this()
        {
            Seer = _Seer;
            Source = _Source;
            Duration = 1;
        }

        public override string GetDescription()
        {
            return null;
        }

        public bool CheckHighlight()
        {
            /*if(Source.litAreas[this.Object.Physics.CurrentCell.X, this.Object.Physics.CurrentCell.Y])
            {
                MessageQueue.AddPlayerMessage(this.Object.DisplayName + " is in a lit area");
                return true;
            }*/
            if (Seer == null || Seer.IsNowhere())
            {
                Seer = null;
                return true;
            }
            Physics part1 = Object.Physics;
            Physics part2 = Seer.Physics;
            if (part1.CurrentCell == null || part2.CurrentCell == null)
                return true;
            return false;
        }

        public override bool Apply(GameObject Object)
        {
            Brain part = Object.Brain;

            if (part != null)
            {
                part.Hibernating = false;
            }

            CheckHighlight();
            return true;
        }

        public override void Remove(GameObject Object)
        {
            base.Remove(Object);
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");

            base.Register(Object, Registrar);
        }

        public override bool FinalRender(RenderEvent E, bool bAlt)
        {
            if (Object == null)
                return true;
            Physics physicsPart = Object.Physics;

            if (physicsPart.CurrentCell == null || Object.IsInGraveyard()
                /*  || part.CurrentCell.IsLit()       */
                /*  && part.CurrentCell.IsExplored()  */
                /*  && part.CurrentCell.IsVisible()   */)
            {
                return true;
            }

            E.HighestLayer = 0;
            Object.ComponentRender(E);
            E.Tile = Object.Render.Tile;
            E.CustomDraw = true;

            return false;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn" && Object != null)
            {
                if (CheckHighlight())
                {
                    Duration--;
                }
            }
            return true;
        }
    }
}
