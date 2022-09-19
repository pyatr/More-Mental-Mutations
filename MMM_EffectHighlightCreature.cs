using System;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts.Mutation;
using XRL.Messages;

namespace XRL.World.Parts.Effects
{
    [Serializable]
    public class MMM_EffectHighlightCreature : Effect
    {
        GameObject Seer;
        MMM_Obtenebration Source;

        public MMM_EffectHighlightCreature()
        {
            this.Duration = 1;
        }

        public MMM_EffectHighlightCreature(GameObject _Seer, MMM_Obtenebration _Source)
        {
            this.Seer = _Seer;
            this.Source = _Source;
            this.Duration = 1;
        }

        public override string GetDescription()
        {
            return (string)null;
        }

        public bool CheckHighlight()
        {
            /*if(Source.litAreas[this.Object.pPhysics.CurrentCell.X, this.Object.pPhysics.CurrentCell.Y])
            {
                MessageQueue.AddPlayerMessage(this.Object.DisplayName + " is in a lit area");
                return true;
            }*/
            if (this.Seer == null || this.Seer.IsNowhere())
            {
                this.Seer = (GameObject)null;
                return true;
            }
            Physics part1 = this.Object.GetPart("Physics") as Physics;
            Physics part2 = this.Seer.GetPart("Physics") as Physics;
            if (part1.CurrentCell == null || part2.CurrentCell == null)
                return true;
            return false;
        }

        public override bool Apply(GameObject Object)
        {
            Brain part = Object.GetPart("Brain") as Brain;
            if (part != null)
                part.Hibernating = false;
            this.CheckHighlight();
            return true;
        }

        public override void Remove(GameObject Object)
        {
            base.Remove(Object);
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterEffectEvent((Effect)this, "EndTurn");
        }

        public override void Unregister(GameObject Object)
        {
            Object.UnregisterEffectEvent((Effect)this, "EndTurn");
        }

        public override bool FinalRender(RenderEvent E, bool bAlt)
        {
            if (this.Object == null)
                return true;
            Physics part = this.Object.GetPart("Physics") as Physics;
            if (part.CurrentCell == null || part.CurrentCell == XRLCore.Core.Game.Graveyard 
                /*  || part.CurrentCell.IsLit()       */
                /*  && part.CurrentCell.IsExplored()  */
                /*  && part.CurrentCell.IsVisible()   */)
                return true;
            E.HighestLayer = 0;
            this.Object.Render(E);
            E.Tile = this.Object.pRender.Tile;
            E.CustomDraw = true;
            return false;
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn" && this.Object != null)
            {
                //this.Duration--;
                bool result = this.CheckHighlight();
                if (result)
                {
                    this.Duration--;
                }
            }
            return true;
        }
    }
}
