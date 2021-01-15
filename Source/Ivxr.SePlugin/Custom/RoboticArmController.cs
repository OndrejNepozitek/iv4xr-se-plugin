using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Cube;
using Sandbox.Game.GUI;
using Sandbox.ModAPI;
using VRage;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRageMath;

namespace Iv4xr.SePlugin.Custom
{
    public class RoboticArmController
    {
        private MyMotorAdvancedStator rotor1;
        private MyMotorAdvancedStator rotor2;
        private MyMotorAdvancedStator rotor3;
        private MyShipConnector connector;
        private List<MyMotorAdvancedStator> rotors;
        
        public void Init()
        {
            var entities = GetEntities();

            rotor1 = FindRotor("Arm Rotor 1", entities);
            rotor2 = FindRotor("Arm Rotor 2", entities);
            rotor3 = FindRotor("Arm Rotor 3", entities);
            rotors = GetRotors(entities);

            var connectors = GetEntitiesOfType<MyShipConnector>(entities);
            connector = connectors.SingleOrDefault(x => x.CustomName.ToString() == "Arm Connector 1");

            entities.Clear();
        }

        public void Set(float rotor1Velocity, float rotor2Velocity, float rotor3Velocity)
        {
            rotor1.TargetVelocityRPM = rotor1Velocity;
            rotor2.TargetVelocityRPM = rotor2Velocity;

            if (rotor3 != null)
            {
                rotor3.TargetVelocityRPM = rotor3Velocity;
            }
        }

        private List<MyEntity> GetEntities()
        {
            var players = new List<IMyPlayer>();
            MyAPIGateway.Players.GetPlayers(players);
            var player = players[0];
            var playerPosition = player.Character.PositionComp.GetPosition();
            var sphere = new BoundingSphereD(playerPosition, radius: 100.0);
            var entities = MyEntities.GetEntitiesInSphere(ref sphere);

            return entities;
        }

        public MyMotorAdvancedStator GetRotor(string name)
        {
            return rotors.SingleOrDefault(x => x?.CustomName.ToString() == name);
        }

        private List<MyMotorAdvancedStator> GetRotors(List<MyEntity> entities)
        {
            return entities
                .Where(x => x is MyMotorAdvancedStator)
                .Cast<MyMotorAdvancedStator>()
                .ToList();
        }

        private List<T> GetEntitiesOfType<T>(List<MyEntity> entities)
        {
            return entities
                .Where(x => x is T)
                .Cast<T>()
                .ToList();
        }

        private MyMotorAdvancedStator FindRotor(string name, List<MyEntity> entities)
        {
            return entities
                .Where(x => x is MyMotorAdvancedStator)
                .Cast<MyMotorAdvancedStator>()
                .SingleOrDefault(x => x.CustomName.ToString() == name);
        }

        public void Throw()
        {
            connector.ThrowOut.ValidateAndSet(true);
        }

        public void ResetBall()
        {
            var entities = GetEntities();
            var floatingObjects = GetEntitiesOfType<MyFloatingObject>(entities);
            entities.Clear();

            connector.ThrowOut.ValidateAndSet(false);
            var inventory = connector.GetInventoryBase();

            if (inventory.GetItemsCount() == 0)
            {
                var ores = new MyObjectBuilder_Ore()
                {
                    SubtypeName = "Iron",
                };
                inventory.AddItems(2000, ores);
            }

            foreach (var floatingObject in floatingObjects)
            {
                MyEntities.Remove(floatingObject);
            }
        }

        public IEnumerator Reset()
        {
            var entities = GetEntities();
            var floatingObjects = GetEntitiesOfType<MyFloatingObject>(entities);

            foreach (var floatingObject in floatingObjects)
            {
                MyEntities.Remove(floatingObject);
            }

            foreach (var entity in entities)
            {
                if (entity is MyCubeGrid)
                {
                    var grid = (MyCubeGrid) entity;

                    //if (grid.Hierarchy != null)
                    //{
                    //    grid.Hierarchy.GetTopMostParent().Delete();
                    //}

                    //if (grid.DisplayName == "ThrowerArm - WithHinges")
                    //{
                    //    entity.Close();
                    //}

                    grid.SendGridCloseRequest();

                    // entity.Close();
                }
            }

            entities.Clear();

            yield return null;

            MyVisualScriptLogicProvider.SpawnLocalBlueprint("ThrowerArm - WithHinges", new Vector3D(new Vector3(-300, 300, 300)));
        }

        //public IEnumerator Reset()
        //{
        //    var rotorSettings = new Dictionary<MyMotorAdvancedStator, RotorSettings>();

        //    foreach (var rotor in rotors)
        //    {
        //        rotorSettings.Add(rotor, new RotorSettings()
        //        {
        //            MinAngle = rotor.MinAngle,
        //            MaxAngle = rotor.MaxAngle,
        //        });

        //        var angles = ((IMyMotorStator)rotor).Angle;

        //        if (angles < 0)
        //        {
        //            rotor.MaxAngle = 0;
        //        }
        //        else
        //        {
        //            rotor.MinAngle = 0;
        //        }
        //    }

        //    ResetBall();

        //    yield return null;

        //    var rotorsInProgress = rotors.ToList();

        //    while (true)
        //    {
        //        foreach (var rotor in rotorsInProgress.ToList())
        //        {
        //            var angle = ((IMyMotorStator) rotor).Angle;

        //            if (Math.Abs(angle) < 0.01f)
        //            {
        //                var settings = rotorSettings[rotor];

        //                rotor.TargetVelocityRPM = 0;
        //                rotor.MinAngle = settings.MinAngle;
        //                rotor.MaxAngle = settings.MaxAngle;
        //                rotorsInProgress.Remove(rotor);
        //            }
        //            else
        //            {
        //                var velocity = GetTargetVelocityRPM(angle);
        //                rotor.TargetVelocityRPM = velocity;
        //            }
        //        }

        //        if (rotorsInProgress.Count == 0)
        //        {
        //            break;
        //        }

        //        yield return null;
        //    }
        //}

        private float GetTargetVelocityRPM(float angle)
        {
            var degrees = angle / (float) (Math.PI / 180.0f);
            var absDegrees = Math.Abs(degrees);
            var velocity = 30f;

            if (absDegrees > 60)
            {
                velocity = 30;
            }
            else if (absDegrees > 30)
            {
                velocity = 15;
            } 
            else if (absDegrees > 10)
            {
                velocity = 5;
            }
            else
            {
                velocity = absDegrees / 2;
            }

            return Math.Sign(angle) * velocity * -1;
        }

        private class RotorSettings
        {
            public float MinAngle { get; set; }

            public float MaxAngle { get; set; }
        }
    }
}