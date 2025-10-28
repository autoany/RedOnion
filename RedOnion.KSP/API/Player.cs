using RedOnion.ROS.Utilities;
using System.ComponentModel;

namespace RedOnion.KSP.API
{
	[Description("User/player controls.")]
	public static class Player
	{
		[Description("Throttle control. \\[0, 1]")]
		public static float throttle
		{
			get => FlightInputHandler.state.mainThrottle;
			set
			{
				if (!float.IsNaN(value))
					FlightInputHandler.state.mainThrottle = RosMath.Clamp(value, 0f, 1f);
			}
		}

		[Description("Pitch raw control. \\[-1, +1]")]
		public static float pitch
		{
			get => FlightInputHandler.state.pitch;
			set
			{
				if (!float.IsNaN(value))
					FlightInputHandler.state.pitch = RosMath.Clamp(value, -1f, +1f);
			}
		}
		[Description("Yaw raw control. \\[-1, +1]")]
		public static float yaw
		{
			get => FlightInputHandler.state.yaw;
			set
			{
				if (!float.IsNaN(value))
					FlightInputHandler.state.yaw = RosMath.Clamp(value, -1f, +1f);
			}
		}
		[Description("Roll raw control. \\[-1, +1]")]
		public static float roll
		{
			get => FlightInputHandler.state.roll;
			set
			{
				if (!float.IsNaN(value))
					FlightInputHandler.state.roll = RosMath.Clamp(value, -1f, +1f);
			}
		}

		[Description("Pitch trim control. \\[-1, +1]")]
		public static float pitchTrim
		{
			get => FlightInputHandler.state.pitchTrim;
			set
			{
				if (!float.IsNaN(value))
					FlightInputHandler.state.pitchTrim = RosMath.Clamp(value, -1f, +1f);
			}
		}
		[Description("Yaw trim control. \\[-1, +1]")]
		public static float yawTrim
		{
			get => FlightInputHandler.state.yawTrim;
			set
			{
				if (!float.IsNaN(value))
					FlightInputHandler.state.yawTrim = RosMath.Clamp(value, -1f, +1f);
			}
		}
		[Description("Roll trim control. \\[-1, +1]")]
		public static float rollTrim
		{
			get => FlightInputHandler.state.rollTrim;
			set
			{
				if (!float.IsNaN(value))
					FlightInputHandler.state.rollTrim = RosMath.Clamp(value, -1f, +1f);
			}
		}
	}
}
