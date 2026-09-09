using System;
using System.Collections.Generic;
using System.Threading;
using ApexTelemetry.Models;

namespace ApexTelemetry.Services
{
    public class RaceTrackerService
    {
        private readonly List<TelemetryTracePoint> _traceHistory = new();
        private readonly List<Waypoint> _waypoints = new();
        private int _currentWaypointIndex = 0;

        private double? _lastLat;
        private double? _lastLon;
        private double? _lastAlt;
        private DateTime _lastCoordinateChangeTime;

        private double _activeSpeed = 0.0;
        private readonly Timer _decayTimer;

        public event Action<TelemetryTracePoint>? TraceUpdated;
        public event Action<Waypoint>? WaypointReached;

        public Waypoint? CurrentTargetWaypoint =>
            (_waypoints.Count > 0 && _currentWaypointIndex < _waypoints.Count)
            ? _waypoints[_currentWaypointIndex]
            : null;

        public RaceTrackerService()
        {
            _lastCoordinateChangeTime = DateTime.UtcNow;

            // Ticks every 100ms (10 times per second) for smooth rate-of-change decay
            _decayTimer = new Timer(OnDecayTick, null, 100, 100);
        }

        public void SetWaypoints(List<Waypoint> waypoints)
        {
            _waypoints.Clear();
            _waypoints.AddRange(waypoints);
            _currentWaypointIndex = 0;
        }

        public void ProcessStatusUpdate(StatusModel status)
        {
            if (!status.Latitude.HasValue || !status.Longitude.HasValue || !status.Altitude.HasValue)
                return;

            double lat = status.Latitude.Value;
            double lon = status.Longitude.Value;
            double alt = status.Altitude.Value;
            double planetRadius = status.PlanetRadius ?? 6371000.0;

            double refLat = _traceHistory.Count > 0 ? _traceHistory[0].Latitude : lat;
            double refLon = _traceHistory.Count > 0 ? _traceHistory[0].Longitude : lon;

            Vector3D enuPos = ConvertToLocalEnu(lat, lon, alt, refLat, refLon, planetRadius);
            Vector3D velocityVector = new(0, 0, 0);

            bool coordinatesChanged = (_lastLat != lat || _lastLon != lon || _lastAlt != alt);

            if (_lastLat == null)
            {
                _lastLat = lat;
                _lastLon = lon;
                _lastAlt = alt;
                _lastCoordinateChangeTime = DateTime.UtcNow;
            }
            else if (coordinatesChanged)
            {
                double timeDelta = (DateTime.UtcNow - _lastCoordinateChangeTime).TotalSeconds;

                if (timeDelta > 0 && _traceHistory.Count > 0)
                {
                    var prev = _traceHistory[^1];
                    double distanceMoved = enuPos.DistanceTo(prev.LocalEnuPosition);

                    velocityVector = new Vector3D(
                        (enuPos.East - prev.LocalEnuPosition.East) / timeDelta,
                        (enuPos.North - prev.LocalEnuPosition.North) / timeDelta,
                        (enuPos.Up - prev.LocalEnuPosition.Up) / timeDelta
                    );

                    // Re-calculate true physical speed on coordinate shift
                    _activeSpeed = distanceMoved / timeDelta;

                    CheckSegmentWaypointCollision(prev.LocalEnuPosition, enuPos, refLat, refLon, planetRadius);
                }

                _lastLat = lat;
                _lastLon = lon;
                _lastAlt = alt;
                _lastCoordinateChangeTime = DateTime.UtcNow;
            }

            var tracePoint = new TelemetryTracePoint
            {
                Timestamp = status.Timestamp,
                Latitude = lat,
                Longitude = lon,
                Altitude = alt,
                LocalEnuPosition = enuPos,
                VelocityVector = velocityVector,
                SpeedMS = _activeSpeed
            };

            _traceHistory.Add(tracePoint);
            TraceUpdated?.Invoke(tracePoint);
        }

        private void OnDecayTick(object? state)
        {
            double elapsedSeconds = (DateTime.UtcNow - _lastCoordinateChangeTime).TotalSeconds;

            // Grace period: Do nothing during the first 1.3 seconds
            if (elapsedSeconds <= 1.3 || _activeSpeed <= 0)
                return;

            // Decay phase: Calculate time spent inside the decay window (starts at 0.0 when elapsed = 1.3s)
            double decayTime = elapsedSeconds - 1.3;

            // Option A: Smooth Linear Reduction (e.g., lose 12 m/s per second of decay)
            // Adjust 12.0 to control how fast or slow it bleeds off.
            double decayAmount = 12.0 * 0.1; // 0.1s tick interval
            _activeSpeed = Math.Max(0.0, _activeSpeed - decayAmount);

            /* 
            // Option B: Gentler Exponential Decay (e.g., retain 85% per second instead of dropping 50%)
            // _activeSpeed *= Math.Pow(0.85, 0.1); 
            // if (_activeSpeed < 0.5) _activeSpeed = 0.0;
            */

            PublishStoppedTrace();
        }

        private void PublishStoppedTrace()
        {
            if (_traceHistory.Count > 0)
            {
                var last = _traceHistory[^1];
                var updatedTrace = new TelemetryTracePoint
                {
                    Timestamp = DateTime.UtcNow,
                    Latitude = last.Latitude,
                    Longitude = last.Longitude,
                    Altitude = last.Altitude,
                    LocalEnuPosition = last.LocalEnuPosition,
                    VelocityVector = new Vector3D(0, 0, 0),
                    SpeedMS = _activeSpeed
                };
                TraceUpdated?.Invoke(updatedTrace);
            }
        }

        private static Vector3D ConvertToLocalEnu(double lat, double lon, double alt, double refLat, double refLon, double radius)
        {
            double dLatRad = (lat - refLat) * (Math.PI / 180.0);
            double dLonRad = (lon - refLon) * (Math.PI / 180.0);
            double refLatRad = refLat * (Math.PI / 180.0);

            double north = dLatRad * radius;
            double east = dLonRad * radius * Math.Cos(refLatRad);
            double up = alt;

            return new Vector3D(east, north, up);
        }

        private void CheckSegmentWaypointCollision(Vector3D segStart, Vector3D segEnd, double refLat, double refLon, double planetRadius)
        {
            var target = CurrentTargetWaypoint;
            if (target == null) return;

            Vector3D targetEnu = ConvertToLocalEnu(target.Latitude, target.Longitude, target.TargetAltitude, refLat, refLon, planetRadius);

            double distToSegment = DistancePointToSegment(targetEnu, segStart, segEnd);

            if (distToSegment <= target.RadiusMeters)
            {
                target.IsReached = true;
                WaypointReached?.Invoke(target);
                _currentWaypointIndex++;
            }
        }

        private static double DistancePointToSegment(Vector3D p, Vector3D a, Vector3D b)
        {
            double abE = b.East - a.East;
            double abN = b.North - a.North;
            double abU = b.Up - a.Up;

            double apE = p.East - a.East;
            double apN = p.North - a.North;
            double apU = p.Up - a.Up;

            double ab2 = abE * abE + abN * abN + abU * abU;
            if (ab2 == 0) return a.DistanceTo(p);

            double t = (apE * abE + apN * abN + apU * apU) / ab2;
            t = Math.Clamp(t, 0.0, 1.0);

            Vector3D closest = new(a.East + t * abE, a.North + t * abN, a.Up + t * abU);
            return closest.DistanceTo(p);
        }
    }
}