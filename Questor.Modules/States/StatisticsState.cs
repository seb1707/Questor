namespace Questor.Modules.States
{
    public enum StatisticsState
    {
        Idle,
        SessionLog,
        MissionLog,
        PocketLog,
        LogAllEntities,
        ListPotentialCombatTargets,
        ListLowValueTargets,
        ListHighValueTargets,
        ModuleInfo,
        ListIgnoredTargets,
        ListPrimaryWeaponPriorityTargets,
        ListClassInstanceInfo,
        ListDronePriorityTargets,
        ListTargetedandTargeting,
        PocketObjectStatistics,
        //AnomolyLog,
        //GateCampLog,
        //HourlyGridReportLog,
        //DailyGridReportLog,
        LocalStatistics,
        Done
    }
}