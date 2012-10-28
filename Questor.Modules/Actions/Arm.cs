// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using System.Xml.Linq;
    using System.Globalization;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Lookup;
    using global::Questor.Modules.States;
    using global::Questor.Modules.Logging;
    using Questor.Modules.BackgroundTasks;

    public class Arm
    {
        private bool _missionItemMoved;
        private bool _optionalMissionItemMoved;
        private DateTime _lastPulse;
        private DateTime _lastArmAction;

        public Arm()
        {
            AmmoToLoad = new List<Ammo>();
        }

        // Bleh, we don't want this here, can we move it to cache?
        public long AgentId { get; set; }

        public List<Ammo> AmmoToLoad { get; private set; }

        private bool DefaultFittingChecked; //false; //flag to check for the correct default fitting before using the fitting manager
        private bool DefaultFittingFound = true; //Did we find the default fitting?
        private bool TryMissionShip = true;  // Used in the event we can't find the ship specified in the missionfittings
        private bool UseMissionShip; //false; // Were we successful in activating the mission specific ship?
        private bool CustomFittingFound;
        private bool WaitForFittingToLoad = true;
        
        public void LoadSpecificAmmo(IEnumerable<DamageType> damageTypes)
        {
            AmmoToLoad.Clear();
            AmmoToLoad.AddRange(Settings.Instance.Ammo.Where(a => damageTypes.Contains(a.DamageType)).Select(a => a.Clone()));
        }

        public void ProcessState()
        {
            // Only pulse state changes every 1.5s
            if (DateTime.Now.Subtract(_lastPulse).TotalMilliseconds < Time.Instance.QuestorPulse_milliseconds) //default: 1500ms
                return;
            _lastPulse = DateTime.Now;

            if (!Cache.Instance.InStation)
                return;

            if (Cache.Instance.InSpace)
                return;

            if (DateTime.Now < Cache.Instance.LastInSpace.AddSeconds(20)) // we wait 20 seconds after we last thought we were in space before trying to do anything in station
                return;

            switch (_States.CurrentArmState)
            {
                case ArmState.Idle:
                    break;

                case ArmState.Cleanup:
                    if (!Cleanup.CloseInventoryWindows()) break;
                    _States.CurrentArmState = ArmState.Done;
                    break;

                case ArmState.Done:
                    break;

                case ArmState.NotEnoughDrones:
                    //This is logged in questor.cs - do not double log
                    //Logging.Log("Arm","Armstate.NotEnoughDrones");
                    //State = ArmState.Idle;
                    break;

                case ArmState.NotEnoughAmmo:
                    //This is logged in questor.cs - do not double log
                    //Logging.Log("Arm","Armstate.NotEnoughAmmo");
                    //State = ArmState.Idle;
                    break;

                case ArmState.Begin:
                    if (!Cleanup.CloseInventoryWindows()) break;
                    Cache.Instance.ArmLoadedCache = false;
                    TryMissionShip = true;           // Used in the event we can't find the ship specified in the missionfittings
                    UseMissionShip = false;          // Were we successful in activating the mission specific ship?
                    DefaultFittingChecked = false;   //flag to check for the correct default fitting before using the fitting manager
                    DefaultFittingFound = false;      //Did we find the default fitting?
                    CustomFittingFound = false;
                    WaitForFittingToLoad = false;
                    _States.CurrentArmState = ArmState.OpenShipHangar;
                    _States.CurrentCombatState = CombatState.Idle;
                    Cache.Instance.NextArmAction = DateTime.Now;
                    break;

                case ArmState.OpenShipHangar:
                case ArmState.SwitchToTransportShip:
                case ArmState.SwitchToSalvageShip:
                    if (DateTime.Now > Cache.Instance.NextArmAction) //default 10 seconds
                    {
                        if (!Cache.Instance.ReadyShipsHangar("Arm")) break;

                        if (_States.CurrentArmState == ArmState.OpenShipHangar)
                        {
                            Logging.Log("Arm", "Activating combat ship", Logging.White);
                            _States.CurrentArmState = ArmState.ActivateCombatShip;
                        }
                        else if (_States.CurrentArmState == ArmState.SwitchToTransportShip)
                        {
                            Logging.Log("Arm", "Activating transport ship", Logging.White);
                            _States.CurrentArmState = ArmState.ActivateTransportShip;
                        }
                        else
                        {
                            Logging.Log("Arm", "Activating salvage ship", Logging.White);
                            _States.CurrentArmState = ArmState.ActivateSalvageShip;
                        }
                        break;
                    }
                    break;

                case ArmState.ActivateTransportShip:
                    if (DateTime.Now < Cache.Instance.NextArmAction) return;
                    string transportshipName = Settings.Instance.TransportShipName.ToLower();

                    if (string.IsNullOrEmpty(transportshipName))
                    {
                        _States.CurrentArmState = ArmState.NotEnoughAmmo;
                        Logging.Log("Arm.ActivateTransportShip", "Could not find transportshipName: " + transportshipName + " in settings!", Logging.Orange);
                        return;
                    }

                    if (Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != transportshipName)
                    {
                        if (!Cache.Instance.ReadyShipsHangar("Arm")) break;

                        List<DirectItem> ships = Cache.Instance.ShipHangar.Items;
                        foreach (DirectItem ship in ships.Where(ship => ship.GivenName != null && ship.GivenName.ToLower() == transportshipName))
                        {
                            Logging.Log("Arm", "Making [" + ship.GivenName + "] active", Logging.White);
                            ship.ActivateShip();
                            Cache.Instance.NextArmAction = DateTime.Now.AddSeconds(Time.Instance.SwitchShipsDelay_seconds);
                        }
                        return;
                    }

                    if (DateTime.Now > Cache.Instance.NextArmAction) //default 7 seconds
                    {
                        if (Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() == transportshipName)
                        {
                            Logging.Log("Arm.ActivateTransportShip", "Done", Logging.White);
                            _States.CurrentArmState = ArmState.Cleanup;
                            return;
                        }
                    }

                    break;

                case ArmState.ActivateSalvageShip:
                    string salvageshipName = Settings.Instance.SalvageShipName.ToLower();

                    if (DateTime.Now > Cache.Instance.NextArmAction) //default 10 seconds
                    {
                        if (string.IsNullOrEmpty(salvageshipName))
                        {
                            _States.CurrentArmState = ArmState.NotEnoughAmmo;
                            Logging.Log("Arm.ActivateSalvageShip", "Could not find salvageshipName: " + salvageshipName + " in settings!", Logging.Orange);
                            return;
                        }

                        if ((!string.IsNullOrEmpty(salvageshipName) && Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != salvageshipName.ToLower()))
                        {
                            if (DateTime.Now > Cache.Instance.NextArmAction)
                            {
                                if (!Cache.Instance.ReadyShipsHangar("Arm")) break;

                                List<DirectItem> ships = Cache.Instance.ShipHangar.Items;
                                foreach (DirectItem ship in ships.Where(ship => ship.GivenName != null && ship.GivenName.ToLower() == salvageshipName.ToLower()))
                                {
                                    Logging.Log("Arm", "Making [" + ship.GivenName + "] active", Logging.White);
                                    ship.ActivateShip();
                                    Cache.Instance.NextArmAction = DateTime.Now.AddSeconds(Time.Instance.SwitchShipsDelay_seconds);
                                }
                                return;
                            }
                            return;
                        }

                        if (DateTime.Now > Cache.Instance.NextArmAction && (!string.IsNullOrEmpty(salvageshipName) && Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != salvageshipName))
                        {
                            _States.CurrentArmState = ArmState.OpenShipHangar;
                            break;
                        }

                        if (DateTime.Now > Cache.Instance.NextArmAction)
                        {
                            Logging.Log("Arm", "Done", Logging.White);
                            _States.CurrentArmState = ArmState.Cleanup;
                            return;
                        }
                    }
                    break;

                case ArmState.ActivateCombatShip:                    
                    if (DateTime.Now < Cache.Instance.NextArmAction) 
                        return;

                    string shipNameToUseNow = Settings.Instance.CombatShipName.ToLower();
                    if (string.IsNullOrEmpty(shipNameToUseNow))
                    {
                        _States.CurrentArmState = ArmState.NotEnoughAmmo;
                        Logging.Log("Arm.ActivateCombatShip", "Could not find CombatShipName: " + shipNameToUseNow + " in settings!", Logging.Orange);
                        return;
                    }

                    if (!Cache.Instance.ArmLoadedCache)
                    {
                        _missionItemMoved = false;
                        if (_States.CurrentQuestorState == QuestorState.CombatMissionsBehavior)
                        {
                            Cache.Instance.RefreshMissionItems(AgentId);
                        }
                        Cache.Instance.ArmLoadedCache = true;
                    }

                    //
                    // If we have a mission-specific ship defined, switch to it
                    //
                    if (!string.IsNullOrEmpty(Cache.Instance.MissionShip) &&  TryMissionShip)
                    {
                        shipNameToUseNow = Cache.Instance.MissionShip.ToLower();
                        TryMissionShip = true;
                    }
                    else
                    {
                        TryMissionShip = false;
                    }
                     
                    //
                    // if we have a ship to use defined and we are not currently in that defined ship. change to that ship
                    //
                    if ((!string.IsNullOrEmpty(shipNameToUseNow) && Cache.Instance.DirectEve.ActiveShip.GivenName.ToLower() != shipNameToUseNow))
                    {
                        if (!Cache.Instance.ReadyShipsHangar("Arm")) break;

                        List<DirectItem> shipsInShipHangar = Cache.Instance.ShipHangar.Items;
                        DirectItem shipToUseNow = shipsInShipHangar.FirstOrDefault(s => s.GivenName != null && s.GivenName.ToLower() == shipNameToUseNow.ToLower());
                        if (shipToUseNow != null)
                        {
                            Logging.Log("Arm", "Making [" + shipToUseNow.GivenName + "] active", Logging.White);
                            shipToUseNow.ActivateShip();
                            Cache.Instance.NextArmAction = DateTime.Now.AddSeconds(Time.Instance.SwitchShipsDelay_seconds);
                            if (TryMissionShip)
                            {
                                UseMissionShip = true;
                            }
                                
                            if (TryMissionShip && !UseMissionShip)
                            {
                                Logging.Log("Arm", "Unable to find the ship specified in the missionfitting.  Using default combat ship and default fitting.", Logging.Orange);
                                TryMissionShip = false;
                                Cache.Instance.Fitting = Cache.Instance.DefaultFitting;
                            }
                        }
                        else
                        {
                            _States.CurrentArmState = ArmState.NotEnoughAmmo;
                            Logging.Log("Arm", "Found the following ships:", Logging.White);
                            foreach (DirectItem shipInShipHangar in shipsInShipHangar)
                            {
                                Logging.Log("Arm", "[" + shipInShipHangar.GivenName + "]", Logging.White);
                            }
                            Logging.Log("Arm", "Could not find [" + shipNameToUseNow + "] ship!", Logging.Red);
                            return;
                        }
                        
                    }

                    if (TryMissionShip)
                    {
                        UseMissionShip = true;
                    }

                    if (AmmoToLoad.Count == 0 && string.IsNullOrEmpty(Cache.Instance.BringMissionItem))
                    {
                        Logging.Log("Arm", "Done", Logging.White);
                        _States.CurrentArmState = ArmState.Cleanup;
                    }
                    else
                    {
                        _States.CurrentArmState = ArmState.RepairShop;
                    }

                    break;

                case ArmState.LoadSavedFitting:

                    if (DateTime.Now < Cache.Instance.NextArmAction)
                        return;

                    //If we are already loading a fitting...
                    if (WaitForFittingToLoad) 
                    {
                        if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                        {
                            //we should be done fitting, proceed to the next state
                            if(!Cache.Instance.CloseFitting("Arm")) return;

                            WaitForFittingToLoad = false;
                            _States.CurrentArmState = ArmState.MoveItems;
                            Logging.Log("Arm", "Done Loading Saved Fitting", Logging.White);
                            return; 
                        }

                        
                        if (DateTime.Now.Subtract(_lastArmAction).TotalSeconds > 120)
                        {
                            Logging.Log("Arm", "Loading Fitting timed out, clearing item locks", Logging.Orange);
                            Cache.Instance.DirectEve.UnlockItems();
                            _lastArmAction = DateTime.Now.AddSeconds(-10);
                            _States.CurrentArmState = ArmState.Begin;
                            break;
                        }

                        //let's wait 10 seconds if we still have locked items
                        Logging.Log("Arm", "Waiting for fitting. locked items = " + Cache.Instance.DirectEve.GetLockedItems().Count, Logging.White);
                        Cache.Instance.NextArmAction = DateTime.Now.AddSeconds(Time.Instance.FittingWindowLoadFittingDelay_seconds);
                        return;
                    }
                    
                    if (_States.CurrentQuestorState == QuestorState.CombatMissionsBehavior)
                    {
                        if ((!Settings.Instance.UseFittingManager || !DefaultFittingFound) ||
                            !(UseMissionShip && !Cache.Instance.ChangeMissionShipFittings))
                        {
                            _States.CurrentArmState = ArmState.MoveItems;
                            return;
                        }

                        //let's check first if we need to change fitting at all
                        Logging.Log("Arm", "Fitting: " + Cache.Instance.Fitting + " - currentFit: " + Cache.Instance.CurrentFit, Logging.White);
                        if (Cache.Instance.Fitting.Equals(Cache.Instance.CurrentFit))
                        {
                            Logging.Log("Arm", "Current fit is now correct", Logging.White);
                            _States.CurrentArmState = ArmState.MoveItems;
                            return;
                        }

                        if (!Cache.Instance.OpenFittingWindow("Arm")) return;

                        DefaultFittingFound = false;
                        if (!DefaultFittingChecked)
                        {
                            DefaultFittingChecked = true;
                            Logging.Log("Arm", "Looking for Default Fitting " + Cache.Instance.DefaultFitting, Logging.White);

                            foreach (DirectFitting fitting in Cache.Instance.FittingWindow.Fittings)
                            {
                                //ok found it
                                if (fitting.Name.ToLower().Equals(Cache.Instance.DefaultFitting.ToLower()))
                                {
                                    DefaultFittingFound = true;
                                    Logging.Log("Arm", "Found Default Fitting " + fitting.Name, Logging.White);
                                }
                            }

                            if (!DefaultFittingFound)
                            {
                                Logging.Log("Arm", "Error! Could not find Default Fitting.  Disabling fitting manager.", Logging.Orange);
                                DefaultFittingFound = false;
                                Settings.Instance.UseFittingManager = false;
                                Logging.Log("Arm", "Closing Fitting Manager", Logging.White);
                                Cache.Instance.FittingWindow.Close();

                                _States.CurrentArmState = ArmState.MoveItems;
                                return;
                            }
                        }

                        if (!Cache.Instance.OpenFittingWindow("Arm")) return;

                        Logging.Log("Arm", "Looking for fitting " + Cache.Instance.Fitting, Logging.White);

                        foreach (DirectFitting fitting in Cache.Instance.FittingWindow.Fittings)
                        {
                            //ok found it
                            DirectActiveShip CurrentShip = Cache.Instance.DirectEve.ActiveShip;
                            if (Cache.Instance.Fitting.ToLower().Equals(fitting.Name.ToLower()) && fitting.ShipTypeId == CurrentShip.TypeId)
                            {
                                Cache.Instance.NextArmAction = DateTime.Now.AddSeconds(Time.Instance.SwitchShipsDelay_seconds);
                                Logging.Log("Arm", "Found fitting [ " + fitting.Name + " ][" + Math.Round(Cache.Instance.NextArmAction.Subtract(DateTime.Now).TotalSeconds, 0) + "sec]", Logging.White);
                                //switch to the requested fitting for the current mission
                                fitting.Fit();
                                _lastArmAction = DateTime.Now;
                                WaitForFittingToLoad = true;
                                Cache.Instance.CurrentFit = fitting.Name;
                                CustomFittingFound = true;
                                break;
                            }
                        }

                        //if we did not find it, we'll set currentfit to default
                        //this should provide backwards compatibility without trying to fit always
                        if (!CustomFittingFound)
                        {
                            if (UseMissionShip)
                            {
                                Logging.Log("Arm", "Could not find fitting for this ship typeid.  Using current fitting.", Logging.Orange);
                                _States.CurrentArmState = ArmState.MoveItems;
                                break;
                            }

                            Logging.Log("Arm", "Could not find fitting - switching to default", Logging.Orange);
                            Cache.Instance.Fitting = Cache.Instance.DefaultFitting;
                            break;
                        }
                        _States.CurrentArmState = ArmState.MoveItems;
                        Logging.Log("Arm", "Closing Fitting Manager", Logging.White);
                        Cache.Instance.FittingWindow.Close();
                        return;
                    }
                    
                    _States.CurrentArmState = ArmState.MoveItems;
                    break;

                case ArmState.RepairShop:
                    if (DateTime.Now < Cache.Instance.NextArmAction)
                        return;

                    if (Settings.Instance.UseStationRepair && Cache.Instance.RepairAll)
                    {
                        if (!Cache.Instance.RepairItems("Repair All")) break; //attempt to use repair facilities if avail in station
                    }
                    else if (Settings.Instance.UseStationRepair)
                    {
                        if (!Cache.Instance.RepairDrones("Repair Drones")) break; //attempt to use repair facilities if avail in station        
                    }

                    _States.CurrentArmState = ArmState.MoveDrones;
                    break;

                case ArmState.MoveDrones:

                    if (!Settings.Instance.UseDrones || (Cache.Instance.DirectEve.ActiveShip.GroupId == 31 || Cache.Instance.DirectEve.ActiveShip.GroupId == 28 || Cache.Instance.DirectEve.ActiveShip.GroupId == 380))
                    {
                        _States.CurrentArmState = ArmState.LoadSavedFitting;
                        break;
                    }

                    if (Cache.Instance.DirectEve.GetLockedItems().Count != 0)
                    {
                        if (DateTime.Now.Subtract(_lastArmAction).TotalSeconds > 120)
                        {
                            Logging.Log("Arm", "Moving Drones timed out, clearing item locks", Logging.Orange);
                            Cache.Instance.DirectEve.UnlockItems();
                            _lastArmAction = DateTime.Now.AddSeconds(-10);
                            _States.CurrentArmState = ArmState.Begin;
                            break;
                        }
                        return;
                    }

                    if (!Cache.Instance.ReadyAmmoHangar("Arm")) break;
                    if (!Cache.Instance.OpenItemsHangar("Arm")) break;

                    DirectItem drone = Cache.Instance.ItemHangar.Items.OrderBy(i => i.IsSingleton).ThenBy(i => i.Quantity).FirstOrDefault(i => i.TypeId == Settings.Instance.DroneTypeId) ??
                                       Cache.Instance.AmmoHangar.Items.OrderBy(i => i.IsSingleton).ThenBy(i => i.Quantity).FirstOrDefault(i => i.TypeId == Settings.Instance.DroneTypeId);

                    if (drone == null || drone.Stacksize < 1)
                    {
                        string ammoHangarName = string.IsNullOrEmpty(Settings.Instance.AmmoHangar) ? "ItemHangar" : Settings.Instance.AmmoHangar.ToString(CultureInfo.InvariantCulture);
                        Logging.Log("Arm", "Out of drones with typeID [" + Settings.Instance.DroneTypeId + "] in [" + ammoHangarName + "]", Logging.Orange);
                        _States.CurrentArmState = ArmState.NotEnoughDrones;
                        break;
                    }

                    if (!Cache.Instance.ReadyDroneBay("Arm")) break;

                    double neededDrones = Math.Floor((Cache.Instance.DroneBay.Capacity - Cache.Instance.DroneBay.UsedCapacity) / drone.Volume);
                    Logging.Log("Arm", "neededDrones: " + neededDrones, Logging.White);

                    if ((int)neededDrones == 0)
                    {
                        Logging.Log("Arm", "Fitting", Logging.White);
                        _States.CurrentArmState = ArmState.LoadSavedFitting;
                        break;
                    }

                    // Move needed drones
                    Logging.Log("Arm", "Move [ " + (int)Math.Min(neededDrones, drone.Stacksize) + " ] Drones into drone bay", Logging.White);
                    _lastArmAction = DateTime.Now;
                    Cache.Instance.DroneBay.Add(drone, (int)Math.Min(neededDrones, drone.Stacksize));
                    break;

                case ArmState.MoveItems:
                    
                    string bringItem = Cache.Instance.BringMissionItem;
                    if (string.IsNullOrEmpty(bringItem))
                        _missionItemMoved = true;

                    int bringitemQuantity = Math.Max(Cache.Instance.BringMissionItemQuantity, 1);

                    string bringOptionalItem = Cache.Instance.BringOptionalMissionItem;
                    if (string.IsNullOrEmpty(bringOptionalItem))
                        _optionalMissionItemMoved = true;

                    int bringOptionalitemQuantity = Math.Max(Cache.Instance.BringOptionalMissionItemQuantity, 1);

                    if (!_missionItemMoved)
                    {
                        if (!Cache.Instance.OpenCargoHold("Arm")) break;
                        if (!Cache.Instance.ReadyAmmoHangar("Arm")) break;
                        if (!Cache.Instance.OpenItemsHangar("Arm")) break;
                        
                        DirectItem missionItem = Cache.Instance.ItemHangar.Items.FirstOrDefault(i => (i.TypeName ?? string.Empty).ToLower() == bringItem) ??
                                                 Cache.Instance.AmmoHangar.Items.FirstOrDefault(i => (i.TypeName ?? string.Empty).ToLower() == bringItem);

                        if (missionItem != null && !string.IsNullOrEmpty(missionItem.TypeName.ToString(CultureInfo.InvariantCulture)))
                        {
                            Logging.Log("Arm", "Moving MissionItem [" + missionItem.TypeName + "] to CargoHold", Logging.White);

                            Cache.Instance.CargoHold.Add(missionItem, bringitemQuantity);
                            _missionItemMoved = true;
                            break;
                        }
                    }

                    if (!_optionalMissionItemMoved)
                    {
                        if (!Cache.Instance.OpenCargoHold("Arm")) break;
                        if (!Cache.Instance.ReadyAmmoHangar("Arm")) break;
                        if (!Cache.Instance.OpenItemsHangar("Arm")) break;
                    
                        DirectItem optionalmissionItem = Cache.Instance.ItemHangar.Items.FirstOrDefault(i => (i.TypeName ?? string.Empty).ToLower() == bringOptionalItem) ??
                                                         Cache.Instance.AmmoHangar.Items.FirstOrDefault(i => (i.TypeName ?? string.Empty).ToLower() == bringOptionalItem);

                        if (optionalmissionItem != null && !string.IsNullOrEmpty(optionalmissionItem.TypeName.ToString(CultureInfo.InvariantCulture)))
                        {
                            Logging.Log("Arm", "Moving MissionItem [" + optionalmissionItem.TypeName + "] to CargoHold", Logging.White);

                            Cache.Instance.CargoHold.Add(optionalmissionItem, bringOptionalitemQuantity);
                            _optionalMissionItemMoved = true;
                            break;
                        }
                    }

                    bool ammoMoved = false;
                    if (Cache.Instance.MissionAmmo.Count() != 0)
                    {
                        AmmoToLoad = new List<Ammo>(Cache.Instance.MissionAmmo);
                    }

                    //
                    // load ammo
                    //
                    if (!Cache.Instance.OpenCargoHold("Arm")) break;
                    if (!Cache.Instance.ReadyAmmoHangar("Arm")) break;

                    foreach (DirectItem item in Cache.Instance.AmmoHangar.Items.OrderBy(i => i.IsSingleton).ThenBy(i => i.Quantity))
                    {
                        if (item.ItemId <= 0 || item.Volume == 0.00 || item.Quantity == 0)
                            continue;

                        Ammo ammo = AmmoToLoad.FirstOrDefault(a => a.TypeId == item.TypeId);
                        if (ammo == null || ammo.Quantity == 0)
                            continue;

                        int moveAmmoQuantity = Math.Min(item.Quantity, ammo.Quantity);
                        moveAmmoQuantity = Math.Max(moveAmmoQuantity, 1);
                        Cache.Instance.CargoHold.Add(item, moveAmmoQuantity);

                        Logging.Log("Arm", "Moving [" + moveAmmoQuantity + "] units of Ammo  [" + item.TypeName + "] from [ AmmoHangar ] to CargoHold", Logging.White);

                        ammo.Quantity -= moveAmmoQuantity;
                        if (ammo.Quantity <= 0)
                        {
                            Cache.Instance.MissionAmmo.RemoveAll(a => a.TypeId == item.TypeId);
                            AmmoToLoad.RemoveAll(a => a.TypeId == item.TypeId);
                        }
                        ammoMoved = true;
                        break;
                    }

                    if (AmmoToLoad.Count == 0 && _missionItemMoved)
                    {
                        Cache.Instance.NextArmAction = DateTime.Now.AddSeconds(Time.Instance.WaitforItemstoMove_seconds);

                        Logging.Log("Arm", "Waiting for items", Logging.White);
                        _States.CurrentArmState = ArmState.WaitForItems;
                    }
                    else if (!ammoMoved)
                    {
                        if (AmmoToLoad.Count > 0)
                        {
                            foreach (Ammo ammo in AmmoToLoad)
                            {
                                Logging.Log("Arm", "Missing [" + ammo.Quantity + "] units of ammo: [ " + ammo.Description + " ] with TypeId [" + ammo.TypeId + "]", Logging.Orange);
                            }
                        }

                        if (!_missionItemMoved)
                        {
                            Logging.Log("Arm", "Missing mission item [" + bringItem + "]", Logging.Orange);
                        }

                        _States.CurrentArmState = ArmState.NotEnoughAmmo;
                    }
                    break;

                case ArmState.WaitForItems:
                    // Wait 5 seconds after moving
                    if (DateTime.Now < Cache.Instance.NextArmAction)
                        break;

                    if (!Cache.Instance.OpenCargoHold("Arm")) break;

                    if (Cache.Instance.CargoHold.Items.Count == 0)
                        break;

                    if (Settings.Instance.UseDrones && (Cache.Instance.DirectEve.ActiveShip.GroupId != 31 && Cache.Instance.DirectEve.ActiveShip.GroupId != 28 && Cache.Instance.DirectEve.ActiveShip.GroupId != 380))
                    {
                        // Close the drone bay, its not required in space.
                        //if (Cache.Instance.DroneBay.IsReady) //why is not .isready and .isvalid working at the moment? 4/2012
                        Cache.Instance.CloseDroneBay("Arm");
                    }

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        Logging.Log("Arm", "Done", Logging.White);

                        if (_States.CurrentQuestorState == QuestorState.CombatMissionsBehavior)
                        {
                            //reload the ammo setting for combat
                            try
                            {
                                DirectAgentMission mission = Cache.Instance.DirectEve.AgentMissions.FirstOrDefault(m => m.AgentId == AgentId);
                                if (mission == null)
                                    return;

                                Cache.Instance.SetmissionXmlPath(Cache.Instance.FilterPath(mission.Name));

                                XDocument missionXml = XDocument.Load(Cache.Instance.MissionXmlPath);
                                Cache.Instance.MissionAmmo = new List<Ammo>();
                                if (missionXml.Root != null)
                                {
                                    XElement ammoTypes = missionXml.Root.Element("missionammo");
                                    if (ammoTypes != null)
                                        foreach (XElement ammo in ammoTypes.Elements("ammo"))
                                            Cache.Instance.MissionAmmo.Add(new Ammo(ammo));
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.Log("Arms.WaitForItems",
                                            "Unable to load missionammo from mission XML for: [" +
                                            Cache.Instance.MissionName + "], " + ex.Message, Logging.Orange);
                                Cache.Instance.MissionAmmo = new List<Ammo>();
                            }
                        }

                        _States.CurrentArmState = ArmState.Cleanup;
                        break;
                    }

                    // Note, there's no unlock here as we *always* want our ammo!
                    break;
            }
        }
    }
}