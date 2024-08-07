using System;

namespace XRL.World.Parts
{
    [Serializable]
    public class MMM_Darkness : IPart
    {
        public MMM_Darkness()
        {
            //this.Name = nameof(Darkness);
        }

        public override bool SameAs(IPart p)
        {
            return true;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("EndTurn");
            Registrar.Register("ObjectEnteredCell");

            base.Register(Object, Registrar);
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn")
            {
                ParentObject.Physics.CurrentCell.ParentZone.RemoveLight(ParentObject.Physics.CurrentCell.X, ParentObject.Physics.CurrentCell.Y, 0);

                return true;
            }
            if (E.ID == "ObjectEnteredCell")
            {
                // GameObject parameter = E.GetParameter("Object") as GameObject;
            }
            return base.FireEvent(E);
        }
    }
}
