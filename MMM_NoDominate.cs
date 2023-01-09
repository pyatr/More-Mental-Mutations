using System;
using XRL.World.Parts.Effects;

namespace XRL.World.Parts
{
    [Serializable]
    public class MMM_NoDominate : IPart
    {
        public MMM_NoDominate()
        {
            //Name = "Can't be dominated";
        }

        public override bool SameAs(IPart p)
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent(this, "ApplyDomination");
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "ApplyDomination")
            {
                /*if (this.ParentObject.HasEffect("Dominated"))
                {
                    Effect effect = this.ParentObject.GetEffect("Dominated") as Effect;
                    effect.Duration = 0;
                }
                
                return true;
                */

                return false;
            }
            return base.FireEvent(E);
        }
    }
}
