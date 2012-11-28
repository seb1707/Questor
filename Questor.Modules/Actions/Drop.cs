namespace Questor.Modules.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;
    using Questor.Modules.Lookup;
    using global::Questor.Modules.States;

    public class Drop
    {
        public int Item { get; set; }

        public int Unit { get; set; }

        public string DestinationHangarName { get; set; }

        private DateTime _lastAction;

        public void ProcessState()
        {
            if (!Cache.Instance.InStation)
                return;

            if (Cache.Instance.InSpace)
                return;

            if (DateTime.UtcNow < Cache.Instance.LastInSpace.AddSeconds(20)) // we wait 20 seconds after we last thought we were in space before trying to do anything in station
                return;

            DirectContainer dropHangar = null;

            if (!Cache.Instance.ReadyItemsHangar("Drop")) return;
            if (!Cache.Instance.OpenShipsHangar("Drop")) return;

            if (DestinationHangarName == "Local Hangar")
            {
                dropHangar = Cache.Instance.ItemHangar;
            }
            else if (DestinationHangarName == "Ship Hangar")
            {
                dropHangar = Cache.Instance.ShipHangar;
            }
            //else
                //_hangar = Cache.Instance.DirectEve.GetCorporationHangar(Hangar); //this needs to be fixed

            switch (_States.CurrentDropState)
            {
                case DropState.Idle:
                case DropState.Done:
                    break;

                case DropState.Begin:
                    _States.CurrentDropState = DropState.ReadyItemhangar;
                    break;

                case DropState.ReadyItemhangar:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2) return;

                    if (DestinationHangarName == "Local Hangar")
                    {
                        if (!Cache.Instance.ReadyItemsHangar("Drop")) return;
                    }
                    else if (DestinationHangarName == "Ship Hangar")
                    {
                        if (!Cache.Instance.OpenShipsHangar("Drop")) return;
                    }
                    else
                    {
                        if (dropHangar != null && dropHangar.Window == null)
                        {
                            // No, command it to open
                            //Cache.Instance.DirectEve.OpenCorporationHangar();
                            break;
                        }

                        if (dropHangar != null && !dropHangar.Window.IsReady) return;
                    }

                    Logging.Log("Drop", "Opening Hangar", Logging.White);
                    _States.CurrentDropState = DropState.OpenCargo;
                    break;

                case DropState.OpenCargo:

                    if (!Cache.Instance.OpenCargoHold("Drop")) return;

                    Logging.Log("Drop", "Opening Cargo Hold", Logging.White);
                    _States.CurrentDropState = Item == 00 ? DropState.AllItems : DropState.MoveItems;

                    break;

                case DropState.MoveItems:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2) return;

                    DirectItem dropItem;

                    if (Unit == 00)
                    {
                        try
                        {
                            if (Settings.Instance.DebugQuestorManager) Logging.Log("Drop", "[]", Logging.Debug);
                            if (Settings.Instance.DebugQuestorManager) Logging.Log("Drop", "Item = [" + Item + "]",Logging.Debug);
                            if (Settings.Instance.DebugQuestorManager) Logging.Log("Drop", "[]", Logging.Debug);
                            //dropItem = Cache.Instance.CargoHold.Items.FirstOrDefault(i => (i.TypeId == Item));
                            dropItem = Cache.Instance.CargoHold.Items.FirstOrDefault(i => i.TypeId == Item);
                            if (dropItem != null)
                            {
                                if (dropHangar != null) dropHangar.Add(dropItem, dropItem.Quantity);
                                Logging.Log("Drop", "Moving all the items", Logging.White);
                                _lastAction = DateTime.UtcNow;
                                _States.CurrentDropState = DropState.WaitForMove;
                                return;
                            }
                        }
                        catch (Exception exception)
                        {
                            Logging.Log("Drop","MoveItems (all): Exception [" + exception + "]",Logging.Debug);
                        }
                        return;
                    }

                    try
                    {
                        if (Settings.Instance.DebugQuestorManager) Logging.Log("Drop", "Item = [" + Item + "]", Logging.Debug);
                        dropItem = Cache.Instance.CargoHold.Items.FirstOrDefault(i => (i.TypeId == Item));
                        if (dropItem != null)
                        {
                            if (Settings.Instance.DebugQuestorManager) Logging.Log("Drop", "Unit = [" + Unit + "]", Logging.Debug);
                            
                            if (dropHangar != null) dropHangar.Add(dropItem, Unit);
                            Logging.Log("Drop", "Moving item", Logging.White);
                            _lastAction = DateTime.UtcNow;
                            _States.CurrentDropState = DropState.WaitForMove;
                            return;
                        }
                    }
                    catch (Exception exception)
                    {
                        Logging.Log("Drop", "MoveItems: Exception [" + exception + "]", Logging.Debug);
                    }
                    
                    break;

                case DropState.AllItems:

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 2) return;

                    List<DirectItem> allItem = Cache.Instance.CargoHold.Items;
                    if (allItem != null)
                    {
                        if (dropHangar != null) dropHangar.Add(allItem);
                        Logging.Log("Drop", "Moving item", Logging.White);
                        _lastAction = DateTime.UtcNow;
                        _States.CurrentDropState = DropState.WaitForMove;
                        return;
                    }

                    break;

                case DropState.WaitForMove:
                    if (Cache.Instance.CargoHold.Items.Count != 0)
                    {
                        _lastAction = DateTime.UtcNow;
                        break;
                    }

                    // Wait 5 seconds after moving
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5) return;

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        _States.CurrentDropState = DropState.StackItemsHangar;
                        return;
                    }

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds > 120)
                    {
                        Logging.Log("Drop", "Moving items timed out, clearing item locks", Logging.White);
                        Cache.Instance.DirectEve.UnlockItems();

                        _States.CurrentDropState = DropState.StackItemsHangar;
                        return;
                    }
                    break;

                case DropState.StackItemsHangar:
                    // Do not stack until 5 seconds after the cargo has cleared
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5) return;

                    // Stack everything
                    if (dropHangar != null && dropHangar.Window.IsReady)
                    {
                        Logging.Log("Drop", "Stacking items", Logging.White);
                        dropHangar.StackAll();
                        _lastAction = DateTime.UtcNow;
                        _States.CurrentDropState = DropState.WaitForStacking;
                        return;
                    }
                    break;

                case DropState.WaitForStacking:
                    // Wait 5 seconds after stacking
                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds < 5) return;

                    if (Cache.Instance.DirectEve.GetLockedItems().Count == 0)
                    {
                        Logging.Log("Drop", "Done", Logging.White);
                        _States.CurrentDropState = DropState.Done;
                        return;
                    }

                    if (DateTime.UtcNow.Subtract(_lastAction).TotalSeconds > 120)
                    {
                        Logging.Log("Drop", "Stacking items timed out, clearing item locks", Logging.White);
                        Cache.Instance.DirectEve.UnlockItems();

                        Logging.Log("Drop", "Done", Logging.White);
                        _States.CurrentDropState = DropState.Done;
                        return;
                    }
                    break;
            }
        }
    }
}