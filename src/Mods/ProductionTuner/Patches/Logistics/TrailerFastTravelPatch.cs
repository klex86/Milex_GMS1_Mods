using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Logistics
{
    /// <summary>
    /// Smooths Fast Travel with hitched trailers:
    /// In vanilla, MapMenu.FastTravel forcibly calls connectedTrailer.DisconnectInstantly()
    /// and leaves the trailer uncoupled 4 meters behind the vehicle.
    /// This patch tracks the coupled trailer and automatically re-hitches it after teleportation.
    /// </summary>
    public static class TrailerFastTravelPatch
    {
        private static Pickup _pendingPickup;
        private static GoldDigger.Trailer _pendingTrailer;

        /// <summary>
        /// Timestamp until which suspension compression damage (potholes) is suppressed
        /// to prevent teleport drops and hitching tension from destroying vehicle/trailer wheels.
        /// </summary>
        public static float FastTravelProtectionUntil { get; private set; }

        [HarmonyPatch(typeof(GoldDigger.MapMenu), "FastTravel")]
        public static class FastTravelHook
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                try
                {
                    // Extend protection grace window during travel and landing
                    FastTravelProtectionUntil = Time.realtimeSinceStartup + 15f;

                    if (!Singleton<Player>.IsInstanced()) return;
                    var machine = Singleton<Player>.Instance.GetControlledMachine();
                    if (machine is Pickup pickup && pickup.ConnectedTrailer != null)
                    {
                        _pendingPickup = pickup;
                        _pendingTrailer = pickup.ConnectedTrailer;
                        ProductionTunerPlugin.Instance?.LogInfo(
                            $"[FastTravel] Captured connected trailer '{_pendingTrailer.name}' for pickup hitching after travel.");
                    }
                }
                catch (Exception ex)
                {
                    ProductionTunerPlugin.Instance?.LogWarning($"[FastTravel] Prefix error: {ex.Message}");
                }
            }
        }

        [HarmonyPatch(typeof(GoldDigger.MenuLoading), "Hide")]
        public static class MenuLoadingHideHook
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                // Set grace period to allow physics to settle after teleport
                FastTravelProtectionUntil = Mathf.Max(FastTravelProtectionUntil, Time.realtimeSinceStartup + 4f);

                if (_pendingPickup != null && _pendingTrailer != null)
                {
                    var pickup = _pendingPickup;
                    var trailer = _pendingTrailer;
                    _pendingPickup = null;
                    _pendingTrailer = null;

                    ProductionTunerPlugin.Instance?.StartCoroutine(AutoReconnectTrailerCor(pickup, trailer));
                }
            }
        }

        /// <summary>
        /// Prevents instantaneous suspension compression spikes during teleport landing
        /// and joint hitching from destroying wheels. Normal driving wear from genuine potholes
        /// remains 100% intact once the vehicle settles.
        /// </summary>
        [HarmonyPatch(typeof(CheckAndRepair), "UpdateDurability")]
        public static class WheelLandingProtectionPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(CheckAndRepair.EWearConditions reason)
            {
                if (reason == CheckAndRepair.EWearConditions.DrivingThroughHoles && Time.realtimeSinceStartup < FastTravelProtectionUntil)
                {
                    return false;
                }
                return true;
            }
        }

        private static IEnumerator AutoReconnectTrailerCor(Pickup pickup, GoldDigger.Trailer trailer)
        {
            // Allow physics and terrain mesh colliders to settle after teleport
            yield return new WaitForSeconds(0.8f);

            if (pickup == null || trailer == null) yield break;

            try
            {
                if (trailer._Hook != null && !trailer._Hook.IsConnected)
                {
                    if (trailer._Hook.PickupCanConnect == null)
                    {
                        trailer._Hook.PickupCanConnect = pickup;
                    }

                    // Keep protection active through hitching snap
                    FastTravelProtectionUntil = Mathf.Max(FastTravelProtectionUntil, Time.realtimeSinceStartup + 3f);

                    // Attempt official connection
                    trailer.ConnectToPickup();

                    // Fallback if closed trunk check or timing prevented official connection
                    if (!trailer._Hook.IsConnected)
                    {
                        MethodInfo connectMethod = AccessTools.Method(typeof(GoldDigger.Trailer), "Connect");
                        connectMethod?.Invoke(trailer, null);
                    }

                    if (trailer._Hook.IsConnected)
                    {
                        ProductionTunerPlugin.Instance?.LogInfo(
                            $"[FastTravel] Successfully auto-reconnected trailer '{trailer.name}' to Pickup!");
                    }
                }
            }
            catch (Exception ex)
            {
                ProductionTunerPlugin.Instance?.LogWarning($"[FastTravel] Auto-reconnect failed: {ex.Message}");
            }
        }
    }
}
