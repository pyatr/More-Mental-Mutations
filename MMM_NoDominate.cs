using System;
using XRL;
using XRL.World;

namespace MoreMentalMutations.Parts
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

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Object.RegisterPartEvent(this, "ApplyDomination");

            base.Register(Object, Registrar);
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
