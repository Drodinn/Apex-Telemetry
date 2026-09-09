using System;
using Newtonsoft.Json;

namespace ApexTelemetry.Models
{
    public class StatusModel
    {
        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("event")]
        public string Event { get; set; } = string.Empty;

        [JsonProperty("Flags")]
        public long Flags { get; set; }

        [JsonProperty("Flags2")]
        public long Flags2 { get; set; }

        [JsonProperty("Pips")]
        public int[]? Pips { get; set; }

        [JsonProperty("Firegroup")]
        public int Firegroup { get; set; }

        [JsonProperty("GuiFocus")]
        public int GuiFocus { get; set; }

        [JsonProperty("Latitude")]
        public double? Latitude { get; set; }

        [JsonProperty("Longitude")]
        public double? Longitude { get; set; }

        [JsonProperty("Heading")]
        public double? Heading { get; set; }

        [JsonProperty("Altitude")]
        public double? Altitude { get; set; }

        [JsonProperty("PlanetRadius")]
        public double? PlanetRadius { get; set; }

        // --- Complete Flags (32-Bit Bitmask Mappings) ---

        public bool Docked => (Flags & 0x00000001L) != 0;                 // Bit 0
        public bool Landed => (Flags & 0x00000002L) != 0;                 // Bit 1
        public bool LandingGearDown => (Flags & 0x00000004L) != 0;        // Bit 2
        public bool ShieldsUp => (Flags & 0x00000008L) != 0;              // Bit 3
        public bool Supercruise => (Flags & 0x00000010L) != 0;            // Bit 4
        public bool FlightAssistOff => (Flags & 0x00000020L) != 0;        // Bit 5
        public bool HardpointsDeployed => (Flags & 0x00000040L) != 0;     // Bit 6
        public bool InWing => (Flags & 0x00000080L) != 0;                 // Bit 7
        public bool LightsOn => (Flags & 0x00000100L) != 0;               // Bit 8
        public bool CargoScoopDeployed => (Flags & 0x00000200L) != 0;     // Bit 9
        public bool SilentRunning => (Flags & 0x00000400L) != 0;          // Bit 10
        public bool ScoopingFuel => (Flags & 0x00000800L) != 0;           // Bit 11
        public bool SrvHandbrake => (Flags & 0x00001000L) != 0;           // Bit 12
        public bool SrvTurretView => (Flags & 0x00002000L) != 0;          // Bit 13
        public bool SrvTurretRetracted => (Flags & 0x00004000L) != 0;     // Bit 14
        public bool SrvDriveAssist => (Flags & 0x00008000L) != 0;         // Bit 15
        public bool MassLocked => (Flags & 0x00010000L) != 0;             // Bit 16
        public bool FsdCharging => (Flags & 0x00020000L) != 0;            // Bit 17
        public bool FsdCooldown => (Flags & 0x00040000L) != 0;            // Bit 18
        public bool LowFuel => (Flags & 0x00080000L) != 0;                // Bit 19
        public bool OverHeating => (Flags & 0x00100000L) != 0;            // Bit 20
        public bool HasLatLong => (Flags & 0x00200000L) != 0;             // Bit 21
        public bool IsInDanger => (Flags & 0x00400000L) != 0;             // Bit 22
        public bool BeingInterdicted => (Flags & 0x00800000L) != 0;       // Bit 23
        public bool InMainShip => (Flags & 0x01000000L) != 0;             // Bit 24
        public bool InFighter => (Flags & 0x02000000L) != 0;              // Bit 25
        public bool InSrv => (Flags & 0x04000000L) != 0;                  // Bit 26
        public bool HudAnalysisMode => (Flags & 0x08000000L) != 0;        // Bit 27
        public bool NightVision => (Flags & 0x10000000L) != 0;           // Bit 28
        public bool AltitudeFromAverageRadius => (Flags & 0x20000000L) != 0; // Bit 29
        public bool FsdJump => (Flags & 0x40000000L) != 0;                // Bit 30
        public bool SrvHighBeam => (Flags & 0x80000000L) != 0;            // Bit 31

        public bool IsLanded => Docked || Landed;

        // --- Complete Flags2 Bitmask Mappings ---

        public bool OnFoot => (Flags2 & 0x00000001L) != 0;                // Bit 0
        public bool InTaxi => (Flags2 & 0x00000002L) != 0;                // Bit 1
        public bool InMulticrew => (Flags2 & 0x00000004L) != 0;           // Bit 2
        public bool OnFootInStation => (Flags2 & 0x00000008L) != 0;       // Bit 3
        public bool OnFootOnPlanet => (Flags2 & 0x00000010L) != 0;        // Bit 4
        public bool AimDownSight => (Flags2 & 0x00000020L) != 0;          // Bit 5
        public bool LowOxygen => (Flags2 & 0x00000040L) != 0;             // Bit 6
        public bool LowHealth => (Flags2 & 0x00000080L) != 0;             // Bit 7
        public bool Cold => (Flags2 & 0x00000100L) != 0;                  // Bit 8
        public bool Hot => (Flags2 & 0x00000200L) != 0;                   // Bit 9
        public bool VeryCold => (Flags2 & 0x00000400L) != 0;              // Bit 10
        public bool VeryHot => (Flags2 & 0x00000800L) != 0;               // Bit 11
        public bool GlideMode => (Flags2 & 0x00001000L) != 0;             // Bit 12
        public bool OnFootInHangar => (Flags2 & 0x00002000L) != 0;        // Bit 13
        public bool OnFootSocialSpace => (Flags2 & 0x00004000L) != 0;     // Bit 14
        public bool OnFootExterior => (Flags2 & 0x00008000L) != 0;        // Bit 15
        public bool BreathableAtmosphere => (Flags2 & 0x00010000L) != 0;  // Bit 16
        public bool TelepresenceMulticrew => (Flags2 & 0x00020000L) != 0; // Bit 17
        public bool PhysicalMulticrew => (Flags2 & 0x00040000L) != 0;     // Bit 18
        public bool FsdHyperdriveCharging => (Flags2 & 0x00080000L) != 0;// Bit 19
    }
}