using UnityEngine;

namespace Milex.GMS1.Mods.ClaimMonitor.Diagnostics.Models
{
    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }

    public class ClaimAlert
    {
        public AlertSeverity Severity { get; set; }
        public string Category { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Vector3 Position { get; set; }
        public int SourceId { get; set; }
    }
}