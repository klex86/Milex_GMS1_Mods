using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Helpers
{
    /// <summary>
    /// Helper to identify whether a washplant part belongs to the Tier 5 Orange Beast.
    /// Uses lock-free O(1) caching to eliminate frame-rate stutter in update loops.
    /// </summary>
    public static class OrangeBeastFilter
    {
        private static Type _orangeCounterType;
        private static readonly Dictionary<int, bool> Cache = new Dictionary<int, bool>();

        /// <summary>
        /// Returns true if the instance is part of an Orange Beast assembly.
        /// </summary>
        public static bool IsOrangeBeastPart(object instance)
        {
            if (!(instance is Component component) || component == null) return false;

            int id = component.GetInstanceID();
            if (Cache.TryGetValue(id, out bool cached)) return cached;

            if (_orangeCounterType == null)
            {
                _orangeCounterType = AccessTools.TypeByName("GoldDigger.OrangeBeastWashPlantGoldCounter");
            }

            bool isOrange = false;
            if (_orangeCounterType != null)
            {
                var counter = component.GetComponentInParent(_orangeCounterType);
                isOrange = counter != null;
            }

            Cache[id] = isOrange;
            return isOrange;
        }

        public static void Clear()
        {
            Cache.Clear();
        }
    }
}
