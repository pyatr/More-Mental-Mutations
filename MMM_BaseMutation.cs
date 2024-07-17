using System;

namespace XRL.World.Parts.Mutation
{
    [Serializable]
    public class MMM_BaseMutation : BaseMutation
    {
        protected void RemoveMutationByGUID(GameObject GO, ref Guid guid)
        {
            if (guid != Guid.Empty)
            {
                GetActivatedAbilities(GO).RemoveAbility(guid);
                guid = Guid.Empty;
            }
        }

        protected ActivatedAbilities GetActivatedAbilities(GameObject gameObject)
        {
            return gameObject.GetPart<ActivatedAbilities>();
        }
    }
}
