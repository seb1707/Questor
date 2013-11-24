namespace Questor.Modules.States
{
    public enum InnerspaceCommandsState
    {
        Idle,
        LogAllEntities,
        ListEntitiesThatHaveUsLocked,
        ListPrimaryWeaponPriorityTargets,
        AddPWPT,
        AddDPT,
        ListCachedPocketInfo,
        SlaveToMaster_WhatIsLocationIDofMaster,
        SlaveToMaster_WhatIsCoordofMaster,
        SlaveToMaster_WhatIsCurrentMissionAction,
        SlaveToMaster_WhatAmmoShouldILoad,
        MasterToSlaves_SetDestinationLocationID,
        MasterToSlaves_MasterIsWarpingTo,
        MasterToSlaves_SlavesGotoBase,
        MasterToSlaves_DoThisMissionAction,
        MasterToSlaves_DoNotLootItemName,
        MasterToSlaves_SetAutoStart,
        MasterToSlaves_WhereAreYou,
        MasterToSlaves_WhatAreYouShooting,
        MasterToSlaves_ShootThisEntityID,
        Done
    }
}