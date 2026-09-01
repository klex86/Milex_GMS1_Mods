using System;
using System.Collections.Generic;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Helpers
{
    /// <summary>
    /// Tracks baseline (vanilla) values for game instances to ensure clean re-tuning,
    /// prevent cumulative multiplication drift, and enable seamless in-game restoration.
    /// </summary>
    public static class OriginalValueStore
    {
        private class TrackedEntry
        {
            public object TargetInstance { get; set; }
            public float OriginalFloat { get; set; }
            public Vector3 OriginalVector3 { get; set; }
            public int OriginalInt { get; set; }
            public Action<object, float> FloatReapplyAction { get; set; }
            public Action<object, Vector3> Vector3ReapplyAction { get; set; }
            public Action<object, int> IntReapplyAction { get; set; }
            public Action<object> CustomRestoreAction { get; set; }
        }

        private static readonly Dictionary<(int instanceId, string key), TrackedEntry> Entries =
            new Dictionary<(int instanceId, string key), TrackedEntry>();

        private static readonly object SyncRoot = new object();

        /// <summary>
        /// Registers a baseline float value if not already tracked, returning the preserved original value.
        /// </summary>
        public static float GetOrRegisterFloat(
            object instance,
            string key,
            float currentValue,
            Action<object, float> reapplyAction,
            Action<object> customRestoreAction = null)
        {
            if (instance == null) return currentValue;

            int id = GetInstanceId(instance);
            var entryKey = (id, key);

            lock (SyncRoot)
            {
                if (Entries.TryGetValue(entryKey, out var entry))
                {
                    return entry.OriginalFloat;
                }

                entry = new TrackedEntry
                {
                    TargetInstance = instance,
                    OriginalFloat = currentValue,
                    FloatReapplyAction = reapplyAction,
                    CustomRestoreAction = customRestoreAction
                };
                Entries[entryKey] = entry;
                return currentValue;
            }
        }

        /// <summary>
        /// Registers a baseline Vector3 value if not already tracked, returning the preserved original value.
        /// </summary>
        public static Vector3 GetOrRegisterVector3(
            object instance,
            string key,
            Vector3 currentValue,
            Action<object, Vector3> reapplyAction,
            Action<object> customRestoreAction = null)
        {
            if (instance == null) return currentValue;

            int id = GetInstanceId(instance);
            var entryKey = (id, key);

            lock (SyncRoot)
            {
                if (Entries.TryGetValue(entryKey, out var entry))
                {
                    return entry.OriginalVector3;
                }

                entry = new TrackedEntry
                {
                    TargetInstance = instance,
                    OriginalVector3 = currentValue,
                    Vector3ReapplyAction = reapplyAction,
                    CustomRestoreAction = customRestoreAction
                };
                Entries[entryKey] = entry;
                return currentValue;
            }
        }

        /// <summary>
        /// Registers a baseline integer value if not already tracked, returning the preserved original value.
        /// </summary>
        public static int GetOrRegisterInt(
            object instance,
            string key,
            int currentValue,
            Action<object, int> reapplyAction,
            Action<object> customRestoreAction = null)
        {
            if (instance == null) return currentValue;

            int id = GetInstanceId(instance);
            var entryKey = (id, key);

            lock (SyncRoot)
            {
                if (Entries.TryGetValue(entryKey, out var entry))
                {
                    return entry.OriginalInt;
                }

                entry = new TrackedEntry
                {
                    TargetInstance = instance,
                    OriginalInt = currentValue,
                    IntReapplyAction = reapplyAction,
                    CustomRestoreAction = customRestoreAction
                };
                Entries[entryKey] = entry;
                return currentValue;
            }
        }

        /// <summary>
        /// Reapplies current configuration multipliers across all active tracked instances.
        /// </summary>
        public static void ReapplyAll(Func<string, float> multiplierProvider)
        {
            if (multiplierProvider == null) return;

            lock (SyncRoot)
            {
                CleanupDeadReferences();

                foreach (var kvp in Entries)
                {
                    var entry = kvp.Value;
                    if (entry.TargetInstance == null || IsUnityObjectDead(entry.TargetInstance)) continue;

                    string key = kvp.Key.key;
                    float multiplier = multiplierProvider(key);

                    if (entry.FloatReapplyAction != null)
                    {
                        entry.FloatReapplyAction(entry.TargetInstance, entry.OriginalFloat * multiplier);
                    }
                }
            }
        }

        /// <summary>
        /// Restores all tracked instances back to their exact original vanilla values.
        /// Called when the mod is disabled in-game.
        /// </summary>
        public static void RestoreAll()
        {
            lock (SyncRoot)
            {
                CleanupDeadReferences();

                foreach (var entry in Entries.Values)
                {
                    if (entry.TargetInstance == null || IsUnityObjectDead(entry.TargetInstance)) continue;

                    if (entry.CustomRestoreAction != null)
                    {
                        entry.CustomRestoreAction(entry.TargetInstance);
                    }
                    else if (entry.FloatReapplyAction != null)
                    {
                        entry.FloatReapplyAction(entry.TargetInstance, entry.OriginalFloat);
                    }
                    else if (entry.Vector3ReapplyAction != null)
                    {
                        entry.Vector3ReapplyAction(entry.TargetInstance, entry.OriginalVector3);
                    }
                    else if (entry.IntReapplyAction != null)
                    {
                        entry.IntReapplyAction(entry.TargetInstance, entry.OriginalInt);
                    }
                }

                Entries.Clear();
            }
        }

        /// <summary>
        /// Removes destroyed Unity objects from tracking to prevent memory leaks.
        /// </summary>
        public static void CleanupDeadReferences()
        {
            var deadKeys = new List<(int, string)>();
            foreach (var kvp in Entries)
            {
                if (kvp.Value.TargetInstance == null || IsUnityObjectDead(kvp.Value.TargetInstance))
                {
                    deadKeys.Add(kvp.Key);
                }
            }

            foreach (var key in deadKeys)
            {
                Entries.Remove(key);
            }
        }

        private static int GetInstanceId(object instance)
        {
            if (instance is UnityEngine.Object unityObj)
            {
                return unityObj.GetInstanceID();
            }
            return instance.GetHashCode();
        }

        private static bool IsUnityObjectDead(object instance)
        {
            if (instance is UnityEngine.Object unityObj)
            {
                return unityObj == null;
            }
            return false;
        }
    }
}
