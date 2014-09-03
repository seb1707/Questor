
namespace Questor.Modules.Actions
{
    using System;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;
    
    public class SwitchShip
    {
        public void ProcessState()
        {
            if (!Cache.Instance.InStation)
                return;

            if (Cache.Instance.InSpace)
                return;

            if (DateTime.UtcNow < Time.Instance.LastInSpace.AddSeconds(20)) // we wait 20 seconds after we last thought we were in space before trying to do anything in station
                return;
            
            switch (_States.CurrentSwitchShipState)
            {
                case SwitchShipState.Idle:
                    break;

                case SwitchShipState.Done:
                    break;

                case SwitchShipState.ActivateCombatShip:
                    Arm.ProcessState();
                    if (_States.CurrentArmState == ArmState.Done)
                    {
                        _States.CurrentSwitchShipState = SwitchShipState.Done;
                    }

                    break;
                    
            }
        }
    }
}