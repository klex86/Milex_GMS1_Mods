using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace Milex.GMS1.Mods.ProductionTuner.Patches.Vehicles
{
    /// <summary>
    /// Provides rock-solid chassis stabilization for excavators and tracked machinery,
    /// preventing vehicles from tipping over onto their sides when parked or uncrewed.
    /// Also resolves the physics joint entanglement glitch on the DLC Mini Excavator (SmallExcavator)
    /// during claim transport and provides automatic in-place upright recovery upon vehicle entry.
    /// </summary>
    [HarmonyPatch(typeof(TrackMachineController), "Update")]
    public static class ExcavatorStabilityPatch
    {
        private static readonly HashSet<Rigidbody> TrackedRigidbodies = new HashSet<Rigidbody>();
        private static readonly Dictionary<int, float> TeleportSettleUntil = new Dictionary<int, float>();
        private static readonly MethodInfo ReloadObjectsMethod = AccessTools.Method(typeof(SmallExcavator), "ReloadObjects");
        private static readonly MethodInfo ForceChangeLimitsMethod = AccessTools.Method(typeof(SmallExcavator), "ForceChangeLimits");
        private static float _lastMiniExcFixTime = -1f;

        public static void MarkTeleported(MachineController machine, float settleSeconds = 2.5f)
        {
            if (machine == null) return;
            TeleportSettleUntil[machine.GetInstanceID()] = Time.time + settleSeconds;
        }

        [HarmonyPostfix]
        public static void Postfix(TrackMachineController __instance)
        {
            if (__instance == null) return;

            var rb = __instance.MyRigidbody ?? __instance.GetComponent<Rigidbody>();
            if (rb == null) return;

            TrackedRigidbodies.Add(rb);

            bool isControlled = __instance.IsControlled();

            // DLC Mini Excavator (SmallExcavator) tumble & infinite spin watchdog
            if (__instance is SmallExcavator smallExc && !isControlled)
            {
                if (rb.angularVelocity.sqrMagnitude > 4f || rb.velocity.sqrMagnitude > 16f)
                {
                    if (Time.time - _lastMiniExcFixTime > 1.0f)
                    {
                        _lastMiniExcFixTime = Time.time;
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                        ReloadMiniExcavator(smallExc);
                    }
                }
            }

            // Check if vehicle is grounded (wheel contact or raycast beneath chassis)
            bool isGrounded = __instance.IsGrounded || Physics.Raycast(rb.worldCenterOfMass, Vector3.down, 2.5f);

            // A vehicle that is in mid-air or actively settling after teleport must never be frozen!
            bool isSettling = TeleportSettleUntil.TryGetValue(__instance.GetInstanceID(), out float until) && Time.time < until;
            bool inAirOrTeleporting = isSettling || !isGrounded;

            if (inAirOrTeleporting)
            {
                if (rb.constraints != RigidbodyConstraints.None)
                {
                    rb.constraints = RigidbodyConstraints.None;
                }
                return;
            }

            // Chassis stabilization: freeze undercarriage ONLY when on the ground and (handbrake is engaged OR uncrewed & settled)
            bool shouldFreeze = __instance.HandbrakeOn ||
                                (!isControlled && rb.velocity.sqrMagnitude < 0.3f && __instance.transform.up.y > 0.2f);

            if (shouldFreeze)
            {
                if (rb.constraints != RigidbodyConstraints.FreezeAll)
                {
                    rb.constraints = RigidbodyConstraints.FreezeAll;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                // Quench residual oscillation in cantilever arm joints when uncrewed
                if (!isControlled && __instance is Koparka koparka && koparka.ControledJoints?.Arms != null)
                {
                    for (int i = 0; i < koparka.ControledJoints.Arms.Length; i++)
                    {
                        var armObj = koparka.ControledJoints.Arms[i]?.Arm;
                        if (armObj != null)
                        {
                            var armRb = armObj.GetComponent<Rigidbody>();
                            if (armRb != null && armRb.angularVelocity.sqrMagnitude > 0.001f)
                            {
                                armRb.angularVelocity = Vector3.zero;
                                armRb.velocity = Vector3.zero;
                            }
                        }
                    }
                }
            }
            else if (isControlled && !__instance.HandbrakeOn)
            {
                if (rb.constraints != RigidbodyConstraints.None)
                {
                    rb.constraints = RigidbodyConstraints.None;
                }
            }
        }

        public static void ReloadMiniExcavator(SmallExcavator smallExc)
        {
            if (smallExc == null) return;

            if (smallExc.MyRigidbody != null)
            {
                smallExc.MyRigidbody.velocity = Vector3.zero;
                smallExc.MyRigidbody.angularVelocity = Vector3.zero;
            }

            if (ReloadObjectsMethod != null)
            {
                var routine = (IEnumerator)ReloadObjectsMethod.Invoke(smallExc, null);
                if (routine != null)
                {
                    smallExc.StartCoroutine(routine);
                }
            }
            ForceChangeLimitsMethod?.Invoke(smallExc, null);
        }

        public static void RestoreVanilla()
        {
            foreach (var rb in TrackedRigidbodies)
            {
                if (rb != null)
                {
                    rb.constraints = RigidbodyConstraints.None;
                }
            }
            TrackedRigidbodies.Clear();
        }

        public static void Reset()
        {
            RestoreVanilla();
            TeleportSettleUntil.Clear();
        }
    }

    /// <summary>
    /// Teleport hook to reset joint colliders and limits on the DLC Mini Excavator after claim transport.
    /// In vanilla, ReloadObjects is only called in OnEnable, leaving transported mini excavators in a tangled physics loop.
    /// </summary>
    [HarmonyPatch(typeof(MachineController), "Teleport", new Type[] { typeof(Vector3), typeof(Quaternion), typeof(GoldDigger.DepotPlace) })]
    public static class MachineTeleportPatch
    {
        [HarmonyPostfix]
        public static void Postfix(MachineController __instance)
        {
            if (__instance == null) return;

            ExcavatorStabilityPatch.MarkTeleported(__instance);

            var rb = __instance.MyRigidbody ?? __instance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
            }

            if (__instance is SmallExcavator smallExc)
            {
                ExcavatorStabilityPatch.ReloadMiniExcavator(smallExc);
            }
        }
    }

    /// <summary>
    /// Intercepts vehicle entry sanity checks when an excavator or vehicle is overturned.
    /// Instead of beaming the vehicle across the claim back to the entrance depot, rights it upright in-place.
    /// </summary>
    [HarmonyPatch(typeof(MachineController), "SanityCheck")]
    public static class MachineUprightRecoveryPatch
    {
        [HarmonyPrefix]
        public static bool Prefix(MachineController __instance)
        {
            if (__instance == null) return true;

            // When entering an overturned vehicle (tipped on side or roof)
            if (__instance.IsControlled() && __instance.transform.up.y < 0.2f)
            {
                Vector3 uprightPos = __instance.transform.position + Vector3.up * 1.5f;
                Quaternion uprightRot = Quaternion.Euler(0f, __instance.transform.eulerAngles.y, 0f);

                __instance.Teleport(uprightPos, uprightRot, null);

                if (__instance is SmallExcavator smallExc)
                {
                    ExcavatorStabilityPatch.ReloadMiniExcavator(smallExc);
                }

                return false; // Skip vanilla's claim teleport!
            }

            return true;
        }
    }
}
