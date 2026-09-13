using System.Collections.Generic;
using Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics
{
    public interface IClaimScanner
    {
        List<ClaimAlert> ActiveAlerts { get; }
        int PlantCount { get; }
        int MatCount { get; }
        int VehicleCount { get; }
        void ForceScan();
        void StartScanning();
        void StopScanning();
    }
}