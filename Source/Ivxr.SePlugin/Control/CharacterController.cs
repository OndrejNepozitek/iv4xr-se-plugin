﻿using Iv4xr.SePlugin.WorldModel;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Text;
using VRageMath;

namespace Iv4xr.SePlugin.Control
{
	public interface ICharacterController
	{
		void Move(Vector3 move, Vector2 rotation, float roll);
		void Move(MoveAndRotateArgs args);
		void Interact(InteractionArgs args);
        void Teleport(Vector3 position);
    }

	public class CharacterController : ICharacterController
	{
		private IGameSession m_session;

		public CharacterController(IGameSession session)
		{
			m_session = session;
		}

		public void Move(MoveAndRotateArgs args)
		{
			Move(args.Movement, args.Rotation, (float)args.Roll);
		}

		public void Move(Vector3 movement, Vector2 rotation, float roll)
		{
			var entityController = GetEntityController();

			entityController.ControlledEntity.MoveAndRotate(movement, rotation, roll);
		}

		public void Interact(InteractionArgs args)
		{
			if (args.InteractionType == InteractionType.EQUIP)
			{
				EquipToolbarItem(args.Slot);
			}
			else if (args.InteractionType == InteractionType.PLACE)
			{
				PlaceItem();
			}
			else
			{
				throw new ArgumentException("Unknown or not implemented interaction type.");
			}
		}

		private void PlaceItem()
		{
			var entityController = GetEntityController();

			entityController.ControlledEntity.BeginShoot(MyShootActionEnum.PrimaryAction);
		}

		private void EquipToolbarItem(int slot)
		{
			var currentToolbar = MyToolbarComponent.CurrentToolbar;

			currentToolbar.ActivateItemAtSlot(slot);
		}

		private MyEntityController GetEntityController()
		{
			if (m_session.Character is null)
				throw new NullReferenceException("I'm out of character!");  // Should not happen.

			var entityController = m_session.Character.ControllerInfo.Controller;

			if (entityController is null)  // Happens when the character enters a vehicle, for example.
				throw new NotSupportedException("Entity control not possible now.");

			return entityController;
		}

        public void Teleport(Vector3 position)
        {
            var matrix = MatrixD.Identity;
			m_session.Character.PositionComp.SetWorldMatrix(ref matrix);
            m_session.Character.PositionComp.SetPosition(position);
        }
	}
}
