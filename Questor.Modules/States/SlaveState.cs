namespace Questor.Modules.States
{
    public enum SlaveState
    {
        Idle,
        Begin,
        AddPriorityTargets,
        TravelToMasterLocationID,
        FindMaster,
        IsMasterDocked,
        Done,
        SlaveToMaster_WhatIsLocationIDofMaster,
        SlaveToMaster_WhatIsCoordofMaster,
        SlaveToMaster_WhatIsCurrentMissionAction,
        SlaveToMaster_WhatAmmoShouldILoad
    }
}