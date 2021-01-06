using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;

namespace Iv4xr.SePlugin.Custom
{
    public class Sensors
    {
        public List<RayCastResult> CastRaysAroundPlayer(IMyPlayer player, double distance, int raysCount)
        {
            var position = player.GetPosition();
            var orientation = player.Character.PositionComp.GetOrientation();
            var forward = orientation.Forward;

            return CastRaysAroundPosition(position + orientation.Up, forward, distance, raysCount);
        }

        public List<RayCastResult> CastRaysAroundPlayer(IMyCharacter character, double distance, int raysCount)
        {
            var position = character.GetPosition();
            var orientation = character.PositionComp.GetOrientation();
            var forward = orientation.Forward;

            return CastRaysAroundPosition(position + orientation.Up, forward, distance, raysCount);
        }


        public List<RayCastResult> CastRaysAroundPosition(Vector3D position, Vector3D forward, double distance, int raysCount)
        {
            var results = new List<RayCastResult>();

            for (int i = 0; i < raysCount; i++)
            {
                var rotation = MatrixD.CreateRotationY((2 * Math.PI / raysCount) * i);
                
                var direction = Vector3D.Rotate(forward, rotation);
                var from = position;
                var to = from + distance * direction;

                MyAPIGateway.Physics.CastRay(from, to, out var hitInfo);

                var result = new RayCastResult()
                {
                    From = from,
                    To = to,
                    Distance = distance,
                };

                if (hitInfo != null)
                {
                    result.IsHit = true;
                    result.HitDistance = hitInfo.Fraction * distance;
                    result.HitPosition = from + result.HitDistance.Value * direction;
                }

                results.Add(result);
            }

            return results;
        }

        public class RayCastResult
        {
            public Vector3D From { get; set; }

            public Vector3D To { get; set; }

            public double Distance { get; set; }

            public bool IsHit { get; set; }

            public double? HitDistance { get; set; }

            public Vector3D? HitPosition { get; set; }
        }
    }
}