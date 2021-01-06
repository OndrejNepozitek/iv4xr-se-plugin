using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Sandbox;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Iv4xr.SePlugin.Custom
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class CustomSessionComponent : MySessionComponentBase
    {
        private bool isInit;
        private bool enableSensors = true;

        private Sensors sensors = new Sensors();
        private List<Sensors.RayCastResult> rayCastResults;

        private string behaviourDescriptorsFile;
        private List<Vector3D> behaviourDescriptors;
        private IEnumerator behaviourDescriptorsEnumerator;

        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();

            if (!isInit)
            {
                isInit = true;
                Init();
            }

            if (enableSensors)
            {
                ComputeSensors();
            }

            if (behaviourDescriptorsEnumerator != null)
            {
                var shouldContinue = behaviourDescriptorsEnumerator.MoveNext();
                if (!shouldContinue)
                {
                    behaviourDescriptorsEnumerator = null;
                }
            }
        }

        private void Init()
        {
            MyAPIGateway.Utilities.MessageEntered += MessageEntered;
        }

        protected override void UnloadData()
        {
            MyAPIGateway.Utilities.MessageEntered -= MessageEntered;
        }

        private void MessageEntered(string text, ref bool others)
        {
            if (text.StartsWith("/ToggleSensors", StringComparison.InvariantCultureIgnoreCase))
            {
                enableSensors = !enableSensors;
                MyAPIGateway.Utilities.ShowMessage("Helper", $"Sensors {(enableSensors ? "enabled" : "disabled")}");
            }

            if (text.StartsWith("/ToggleMaxSpeed", StringComparison.InvariantCultureIgnoreCase))
            {
                MySandboxGame.Static.EnableMaxSpeed = !MySandboxGame.Static.EnableMaxSpeed;
                MyAPIGateway.Utilities.ShowMessage("Helper",
                    $"Maximum simulation speed {(MySandboxGame.Static.EnableMaxSpeed ? "enabled" : "disabled")}");
            }

            if (text.StartsWith("/bds load", StringComparison.InvariantCultureIgnoreCase))
            {
                var prefix = "/bds load ";
                var prefixLength = prefix.Length;
                var file = text.Substring(prefixLength);

                if (!File.Exists(file))
                {
                    MyAPIGateway.Utilities.ShowMessage("Helper", $"The path \"{file}\" does not exist");
                }
                else
                {
                    MyAPIGateway.Utilities.ShowMessage("Helper", $"Behaviour descriptors file loaded");
                    behaviourDescriptorsFile = file;
                }
            }

            if (text.StartsWith("/bds show", StringComparison.InvariantCultureIgnoreCase))
            {
                if (behaviourDescriptorsFile == null)
                {
                    MyAPIGateway.Utilities.ShowMessage("Helper", $"Please call the \"/bds load filename\" first");
                }
                else
                {
                    behaviourDescriptorsEnumerator = ComputeBehaviourDescriptors();
                }
            }

            if (text.StartsWith("/bds stop", StringComparison.InvariantCultureIgnoreCase))
            {
                behaviourDescriptorsEnumerator = null;
            }

            if (text.StartsWith("/bds clear", StringComparison.InvariantCultureIgnoreCase))
            {
                behaviourDescriptors = null;
                behaviourDescriptorsEnumerator = null;
            }
        }

        public override void Draw()
        {
            base.Draw();

            if (enableSensors)
            {
                DrawSensors();
            }

            if (behaviourDescriptors != null)
            {
                DrawBehaviourDescriptors();
            }
        }

        private void DrawBehaviourDescriptors()
        {
            var bounds = 0.5 * Vector3D.One;
            var boundingBox = new BoundingBoxD(-1 * bounds, bounds);
            var color = Color.Red;
            var GIZMO_LINE_MATERIAL_WHITE = MyStringId.GetOrCompute("WeaponLaserIgnoreDepth");

            if (behaviourDescriptors != null)
            {
                foreach (Vector3D behaviourDescriptor in behaviourDescriptors)
                {
                    var matrix = MatrixD.CreateWorld(behaviourDescriptor);
                    MySimpleObjectDraw.DrawTransparentBox(ref matrix, ref boundingBox, ref color, MySimpleObjectRasterizer.SolidAndWireframe, 1, 0.25f, lineMaterial: GIZMO_LINE_MATERIAL_WHITE);
                }
            }
        }

        /// <summary>
        /// Compute sensory data
        /// </summary>
        private void ComputeSensors()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var player = players[0];

            rayCastResults = sensors.CastRaysAroundPlayer(player, 30, 16);
        }

        /// <summary>
        /// Draw sensory data there were previously computed by ComputeSensors()
        /// </summary>
        private void DrawSensors()
        {
            if (rayCastResults != null)
            {
                var GIZMO_LINE_MATERIAL_WHITE = MyStringId.GetOrCompute("WeaponLaserIgnoreDepth");

                foreach (Sensors.RayCastResult result in rayCastResults)
                {
                    var from = result.From;
                    var to = result.To;
                    var color = Color.White.ToVector4();

                    if (result.IsHit)
                    {
                        var fractionColor = (int)(255 * result.HitDistance.Value / result.Distance);
                        color = new Color(255, fractionColor, fractionColor);
                        to = result.HitPosition.Value;
                    }

                    MySimpleObjectDraw.DrawLine(from, to, GIZMO_LINE_MATERIAL_WHITE, ref color, 0.5f);
                }
            }
        }

        private IEnumerator ComputeBehaviourDescriptors()
        {
            var filename = behaviourDescriptorsFile;
            var lines = File.ReadAllLines(filename);

            for (var i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                var individuals = line.Split(';').ToList();
                var positions = new List<Vector3D>();

                foreach (string individual in individuals)
                {
                    var parts = individual.Split(',');

                    var x = double.Parse(parts[0]);
                    var z = double.Parse(parts[1]);
                    var position = new Vector3D(new Vector2D(x - 510.71, 377), z + 385.2);

                    positions.Add(position);
                }

                behaviourDescriptors = positions;

                var timer = new Stopwatch();
                timer.Start();

                // MyAPIGateway.Utilities.ShowMessage("Helper", $"Generation {i + 1}");
                MyAPIGateway.Utilities.ShowNotification($"Generation {i + 1}");

                while (timer.ElapsedMilliseconds < 1000)
                {
                    yield return null;
                }
            }
        }
    }
}