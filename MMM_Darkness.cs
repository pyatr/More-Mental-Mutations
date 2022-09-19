using System;
using XRL.World.Parts.Effects;
using XRL.World;
using XRL.Messages;

namespace XRL.World.Parts
{
    [Serializable]
    public class MMM_Darkness : IPart
    {
        public MMM_Darkness()
        {
            this.Name = nameof(MMM_Darkness);
        }

        public override bool SameAs(IPart p)
        {
            return true;
        }

        public override void Register(GameObject Object)
        {
            Object.RegisterPartEvent((IPart)this, "EndTurn");
            Object.RegisterPartEvent((IPart)this, "ObjectEnteredCell");
        }

        public override bool FireEvent(Event E)
        {
            if (E.ID == "EndTurn")
            {
                this.ParentObject.pPhysics.CurrentCell.ParentZone.RemoveLight(this.ParentObject.pPhysics.CurrentCell.X, this.ParentObject.pPhysics.CurrentCell.Y, 0);                
                return true;
            }
            if (E.ID == "ObjectEnteredCell")
            {
                GameObject parameter = E.GetParameter("Object") as GameObject;

            }
            return base.FireEvent(E);
        }
    }
}
