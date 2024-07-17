using System;

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

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("ApplyDomination");

            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "ApplyDomination")
            {
                return false;
            }

            return base.FireEvent(E);
        }
    }
}
