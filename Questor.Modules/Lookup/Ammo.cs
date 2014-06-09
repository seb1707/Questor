// ------------------------------------------------------------------------------
//   <copyright from='2010' to='2015' company='THEHACKERWITHIN.COM'>
//     Copyright (c) TheHackerWithin.COM. All Rights Reserved.
//
//     Please look in the accompanying license.htm file for the license that
//     applies to this source code. (a copy can also be found at:
//     http://www.thehackerwithin.com/license.htm)
//   </copyright>
// -------------------------------------------------------------------------------

namespace Questor.Modules.Lookup
{
    using System;
    using System.Xml.Linq;
    using System.Linq;
    using DirectEve;
    using global::Questor.Modules.Caching;
    using global::Questor.Modules.Logging;

    public class Ammo
    {
        public Ammo()
        {
        }

        public Ammo(XElement ammo)
        {
            try
            {
                TypeId = (int)ammo.Attribute("typeId");
                DamageType = (DamageType)Enum.Parse(typeof(DamageType), (string)ammo.Attribute("damageType"));
                Range = (int)ammo.Attribute("range");
                Quantity = (int)ammo.Attribute("quantity");
                Description = (string)ammo.Attribute("description") ?? (string)ammo.Attribute("typeId");
                //
                // the above is pulling from XML, not eve... the below is what we want to pull from eve
                //
                DirectInvType __directInvTypeItem = null;
                Cache.Instance.DirectEve.InvTypes.TryGetValue(TypeId, out __directInvTypeItem);
                if (__directInvTypeItem != null)
                {
                    DirectItem __directItem = null;
                    __directItem = (DirectItem)__directInvTypeItem;
                    ItemCache __item = null;
                    __item = new ItemCache(__directItem);

                    Name = __item.Name;
                    maxVelocity = __item.maxVelocity;

                    emDamage = __item.emDamage;
                    explosiveDamage = __item.explosiveDamage;
                    kineticDamage = __item.kineticDamage;
                    thermalDamage = __item.thermalDamage;
                    metaLevel = __item.metaLevel;
                    hp = __item.hp;
                    techLevel = __item.techLevel;
                    radius = __item.radius;

                    //
                    // only useful for missiles should we not pull this info for items that wont have these attributes?!
                    //
                    aoeDamageReductionFactor = __item.aoeDamageReductionFactor;
                    detonationRange = __item.detonationRange;
                    aoeCloudSize = __item.aoeCloudSize;
                    aoeVelocity = __item.aoeVelocity;
                    agility = __item.agility;
                    explosionDelay = __item.explosionDelay;
                    maxVelocityBonus = __item.maxVelocityBonus;
                    
                    //
                    // only useful for AutoCannon / Artillery and Blaster/RailGun ammo should we not pull this info for items that wont have these attributes?!
                    //
                    fallofMultiplier = __item.fallofMultiplier;
                    weaponRangeMultiplier = __item.weaponRangeMultiplier;
                    trackingSpeedMultiplier = __item.trackingSpeedMultiplier;
                    powerNeedMultiplier = __item.powerNeedMultiplier;
                }
                if (Settings.Instance.DebugAmmo)
                {
                    Logging.Log("Ammo", " [01] Name [" + Name + "] - derived from XML", Logging.Debug);
                    Logging.Log("Ammo", " [01] TypeId [" + TypeId + "] - from XML", Logging.Debug);
                    Logging.Log("Ammo", " [02] DamageType [" + DamageType + "] - from XML", Logging.Debug);
                    Logging.Log("Ammo", " [03] Range [" + Range + "] - from XML", Logging.Debug);
                    Logging.Log("Ammo", " [04] Quantity [" + Quantity + "] - from XML", Logging.Debug);
                    Logging.Log("Ammo", " [05] Description [" + Description + "] - from XML", Logging.Debug);
                    Logging.Log("Ammo", " [06] -------- EVE Attributes Below ------------", Logging.Debug); 
                    Logging.Log("Ammo", " [07] maxVelocity [" + maxVelocity + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [08] emDamage [" + emDamage + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [09] explosiveDamage [" + explosiveDamage + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [10] kineticDamage [" + kineticDamage + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [11] thermalDamage [" + thermalDamage + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [12] metaLevel [" + metaLevel + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [13] hp [" + hp + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [14] techLevel [" + techLevel + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [15] radius [" + radius + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [16] -------- Missile Related  Attributes Below ----------", Logging.Debug);
                    Logging.Log("Ammo", " [17] aoeDamageReductionFactor [" + aoeDamageReductionFactor + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [18] detonationRange [" + detonationRange + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [19] aoeCloudSize [" + aoeCloudSize + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [20] aoeVelocity [" + aoeVelocity + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [21] agility [" + agility + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [22] explosionDelay [" + explosionDelay + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [23] maxVelocityBonus [" + maxVelocityBonus + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [24] -------- Hybrid/Projectile/Laser Related  Attributes Below ---", Logging.Debug);
                    Logging.Log("Ammo", " [25] fallofMultiplier [" + fallofMultiplier + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [26] weaponRangeMultiplier [" + weaponRangeMultiplier + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [27] trackingSpeedMultiplier [" + trackingSpeedMultiplier + "] - from eve no skills applied", Logging.Debug);
                    Logging.Log("Ammo", " [28] powerNeedMultiplier [" + powerNeedMultiplier + "] - from eve no skills applied", Logging.Debug);
                }
            }
            catch (Exception exception)
            {
                Logging.Log("Ammo","Exception [" + exception + "]",Logging.Debug);
            }
        }

        public string Name { get; set; }
        public int TypeId { get; private set; }
        public DamageType DamageType { get; private set; }
        public int Range { get; private set; }
        public int Quantity { get; set; }
        public string Description { get; set; }

        public int maxVelocity { get; set; } //(int)ammo.Attribute("maxVelocity");
        public int emDamage { get; set; } //(int)ammo.Attribute("emDamage");
        public int explosiveDamage { get; set; } //(int)ammo.Attribute("explosiveDamage");
        public int kineticDamage { get; set; } //(int)ammo.Attribute("kineticDamage");
        public int thermalDamage { get; set; } //(int)ammo.Attribute("thermalDamage");
        public int metaLevel { get; set; } //(int)ammo.Attribute("metaLevel");
        public int hp { get; set; } //(int)ammo.Attribute("hp");
        public int techLevel { get; set; } //(int)ammo.Attribute("techLevel");
        public int radius { get; set; } //(int)ammo.Attribute("radius");

        //
        // only useful for missiles
        //
        public int aoeDamageReductionFactor { get; set; } //(int)ammo.Attribute("aoeDamageReductionFactor");
        public int detonationRange { get; set; } //(int)ammo.Attribute("detonationRange");
        public int aoeCloudSize { get; set; } //(int)ammo.Attribute("aoeCloudSize");
        public int aoeVelocity { get; set; } //(int)ammo.Attribute("aoeVelocity");
        public int agility { get; set; } //(int)ammo.Attribute("agility");
        public int explosionDelay { get; set; } //(int)ammo.Attribute("explosionDelay");
        public int maxVelocityBonus { get; set; } //(int)ammo.Attribute("maxVelocityBonus");

        //
        // only useful for AutoCannons and RailGuns
        //
        public int fallofMultiplier { get; set; } //(int)ammo.Attribute("fallofMultiplier");
        public int weaponRangeMultiplier { get; set; } //(int)ammo.Attribute("weaponRangeMultiplier");
        public int trackingSpeedMultiplier { get; set; } //(int)ammo.Attribute("trackingSpeedMultiplier");
        public int powerNeedMultiplier { get; set; } //(int)ammo.Attribute("powerNeedMultiplier");


        public Ammo Clone()
        {
            Ammo _ammo = new Ammo
                {
                    TypeId = TypeId,
                    DamageType = DamageType,
                    Range = Range,
                    Quantity = Quantity,
                    Description = Description,
                    Name = Name,
                    maxVelocity = maxVelocity,
                    emDamage = emDamage,
                    explosiveDamage = explosiveDamage,
                    kineticDamage = kineticDamage,
                    thermalDamage = thermalDamage,
                    metaLevel = metaLevel,
                    hp = hp,
                    techLevel = techLevel,
                    radius = radius,
                    aoeDamageReductionFactor = aoeDamageReductionFactor,
                    detonationRange = detonationRange,
                    aoeCloudSize = aoeCloudSize,
                    aoeVelocity = aoeVelocity,
                    agility = agility,
                    explosionDelay = explosionDelay,
                    maxVelocityBonus = maxVelocityBonus,
                    fallofMultiplier = fallofMultiplier,
                    weaponRangeMultiplier = weaponRangeMultiplier,
                    trackingSpeedMultiplier = trackingSpeedMultiplier,
                    powerNeedMultiplier = powerNeedMultiplier
                };
            return _ammo;
        }
    }
}