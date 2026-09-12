using System;
using System.Collections;
using UnityEngine;

namespace Milex.GMS1.Core.Services
{
    /// <summary>
    /// Central optimization service to counteract progressive memory bloat and FPS degradation.
    /// Runs garbage collection and unloads unused Unity assets and VRAM resources asynchronously
    /// at player-imperceptible moments (Fast Travel, saving, laptop opening, menu pauses).
    /// </summary>
    public class MemoryCleanerService
    {
        public static MemoryCleanerService Instance { get; private set; }

        public struct MemoryCleanResult
        {
            public long BeforeBytes;
            public long AfterBytes;
            public long FreedBytes;
            public string Source;
            public bool Success;
        }

        public bool IsCleaning { get; private set; }
        public DateTime? LastCleanTime { get; private set; }
        public float LastCleanRealtime { get; private set; } = -9999f;
        public long LastFreedBytes { get; private set; }
        public long TotalFreedBytes { get; private set; }
        public int CleanCount { get; private set; }
        public string LastTriggerSource { get; private set; } = "None";

        public event Action<MemoryCleanResult> OnCleanCompleted;

        public static void Initialize()
        {
            if (Instance == null)
            {
                Instance = new MemoryCleanerService();
            }
        }

        /// <summary>
        /// Requests a memory and asset cleanup.
        /// </summary>
        /// <param name="force">If true, ignores the cooldown and disabled checks (used by manual UI button).</param>
        /// <param name="triggerSource">Identifier of the triggering event for telemetry.</param>
        /// <param name="onComplete">Optional completion callback.</param>
        /// <returns>True if the cleanup operation was started, false if skipped due to cooldown or concurrent run.</returns>
        public bool Clean(bool force = false, string triggerSource = "Manual", Action<MemoryCleanResult> onComplete = null)
        {
            if (IsCleaning)
            {
                CorePlugin.Instance?.LogInfo($"[MemoryCleaner] Cleanup already in progress, skipping request from '{triggerSource}'.");
                return false;
            }

            if (!force)
            {
                if (CorePlugin.EnableMemoryCleaner != null && !CorePlugin.EnableMemoryCleaner.Value)
                {
                    return false;
                }

                float cooldown = CorePlugin.MemoryCleanCooldownSeconds != null
                    ? CorePlugin.MemoryCleanCooldownSeconds.Value
                    : 60f;

                float elapsed = Time.realtimeSinceStartup - LastCleanRealtime;
                if (elapsed < cooldown)
                {
                    // Skipped silently due to cooldown
                    return false;
                }
            }

            if (CorePlugin.Instance == null)
            {
                return false;
            }

            IsCleaning = true;
            LastCleanRealtime = Time.realtimeSinceStartup;
            LastTriggerSource = triggerSource;

            CorePlugin.Instance.StartCoroutine(CleanRoutine(triggerSource, onComplete));
            return true;
        }

        private IEnumerator CleanRoutine(string triggerSource, Action<MemoryCleanResult> onComplete)
        {
            CorePlugin.Instance?.LogInfo($"[MemoryCleaner] Initiating memory & VRAM purge (Source: {triggerSource})...");

            long beforeBytes = GC.GetTotalMemory(false);

            // Phase 1: Collect managed heap and run finalizers so stale C# wrappers detach from C++ Unity objects
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Phase 2: Unload unused Unity engine assets (textures, meshes, audio) asynchronously
            AsyncOperation unloadOp = Resources.UnloadUnusedAssets();
            if (unloadOp != null)
            {
                yield return unloadOp;
            }

            // Phase 3: Final collection pass to sweep up any objects freed during asset unload
            GC.Collect();

            long afterBytes = GC.GetTotalMemory(true);
            long freedBytes = Math.Max(0L, beforeBytes - afterBytes);

            LastFreedBytes = freedBytes;
            TotalFreedBytes += freedBytes;
            CleanCount++;
            LastCleanTime = DateTime.Now;
            IsCleaning = false;

            float freedMb = freedBytes / (1024f * 1024f);
            float afterMb = afterBytes / (1024f * 1024f);

            CorePlugin.Instance?.LogInfo(
                $"[MemoryCleaner] Purge completed ({triggerSource}). Freed: {freedMb:F2} MB. Managed Heap: {afterMb:F2} MB (Runs: {CleanCount}).");

            var result = new MemoryCleanResult
            {
                BeforeBytes = beforeBytes,
                AfterBytes = afterBytes,
                FreedBytes = freedBytes,
                Source = triggerSource,
                Success = true
            };

            try
            {
                onComplete?.Invoke(result);
                OnCleanCompleted?.Invoke(result);
            }
            catch (Exception ex)
            {
                CorePlugin.Instance?.LogWarning($"[MemoryCleaner] Callback invocation error: {ex.Message}");
            }
        }
    }
}
