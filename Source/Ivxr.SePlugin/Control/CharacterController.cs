﻿using Iv4xr.SePlugin.WorldModel;
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
			if (m_session.Character is null)
				throw new NullReferenceException("I'm out of character!");  // Should not happen.
			
			var entityController = m_session.Character.ControllerInfo.Controller;

			if (entityController is null)  // Happens when the character enters a vehicle, for example.
				throw new NotSupportedException("Entity control not possible now."); 

			entityController.ControlledEntity.MoveAndRotate(movement, rotation, roll);
		}
	}
}