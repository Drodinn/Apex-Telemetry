using System;

namespace ApexTelemetry.Models
{
    // Local Tangent Plane (East, North, Up) in meters relative to a reference point
    public struct Vector3D
    {
        public double East { get; set; }   // X (Meters)
        public double North { get; set; }  // Y (Meters)
        public double Up { get; set; }     // Z (Meters - Raycasted Radar Altitude)

        public Vector3D(double east, double north, double up)
        {
            East = east;
            North = north;
            Up = up;
        }

        public double DistanceTo(Vector3D target)
        {
            double de = East - target.East;
            double dn = North - target.North;
            double du = Up - target.Up;
            return Math.Sqrt(de * de + dn * dn + du * du);
        }
    }

    public class Waypoint
    {
        public string Name { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double TargetAltitude { get; set; }  // Target radar altitude (meters above ground)
        public double RadiusMeters { get; set; } = 150.0; // 3D Detection Sphere Radius
        public bool IsReached { get; set; }
    }

    public class TelemetryTracePoint
    {
        public DateTime Timestamp { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Altitude { get; set; }
        public Vector3D LocalEnuPosition { get; set; }
        public Vector3D VelocityVector { get; set; }
        public double SpeedMS { get; set; }
    }
}