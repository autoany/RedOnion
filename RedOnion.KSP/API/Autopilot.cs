using RedOnion.Attributes;
using RedOnion.ROS.Utilities;
using System;
using System.ComponentModel;

namespace RedOnion.KSP.API
{
	[Description("Autopilot (throttle and steering) for a ship (vehicle/vessel).")]
	public class Autopilot : IDisposable
	{
		protected Ship _ship;
		protected Vessel _hooked;

		// inputs
		protected float _throttle, _rawPitch, _rawYaw, _rawRoll;
		protected float _userFactor, _userPitch, _userYaw, _userRoll;
		protected double _pitch, _heading, _roll;
		protected Vector _direction;
		protected bool _killRot, _pylink;
		protected TimeStamp _lastUpdate = TimeStamp.never;
		protected TimeStamp _warpLock = TimeStamp.never;

		protected internal Autopilot(Ship ship)
		{
			_ship = ship;
			disable();
			reset();
		}

		[Description("Disable the autopilot, setting all values to NaN.")]
		public void disable()
		{
			_throttle = float.NaN;
			_rawPitch = float.NaN;
			_rawYaw = float.NaN;
			_rawRoll = float.NaN;
			_pitch = double.NaN;
			_heading = double.NaN;
			_roll = double.NaN;
			_direction = new Vector(double.NaN, double.NaN, double.NaN);
			_killRot = false;
			Unhook();
		}

		[Description("Reset the autopilot to default settings.")]
		public void reset()
		{
			pids.reset();
			_userFactor = 0.8f;
			_userPitch = float.NaN;
			_userYaw = float.NaN;
			_userRoll = float.NaN;
			_pylink = true;
		}

		~Autopilot() => Dispose(false);
		[Browsable(false)]
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		protected virtual void Dispose(bool disposing)
		{
			_ship = null;
			if (disposing)
				Unhook();
			else if (_hooked != null)
				UI.Collector.Add(this);
		}

		[Description("Throttle control (0..1). NaN for releasing the control.")]
		public float throttle
		{
			get => _throttle;
			set => Check(_throttle = RosMath.Clamp(value, 0f, 1f));
		}

		[Description("Raw pitch control (up-down, -1..+1). NaN for releasing the control.")]
		public float rawPitch
		{
			get => _rawPitch;
			set => Check(_rawPitch = RosMath.Clamp(value, -1f, +1f));
		}
		[Description("Raw yaw control (left-right, -1..+1). NaN for releasing the control.")]
		public float rawYaw
		{
			get => _rawYaw;
			set => Check(_rawYaw = RosMath.Clamp(value, -1f, +1f));
		}
		[Description("Raw roll control (rotation, -1..+1). NaN for releasing the control.")]
		public float rawRoll
		{
			get => _rawRoll;
			set => Check(_rawRoll = RosMath.Clamp(value, -1f, +1f));
		}

		[Description("Target direction vector."
			+ " NaN/vector.none for releasing the control.")]
		public Vector direction
		{
			get
			{
				if (double.IsNaN(_direction.x)
					|| double.IsNaN(_direction.y)
					|| double.IsNaN(_direction.z))
				{
					var pitch = _pitch;
					if (double.IsNaN(pitch))
						pitch = _ship.pitch;
					var heading = _heading;
					if (double.IsNaN(heading))
						heading = _ship.heading;
					return _ship.north.rotate(-pitch, _ship.east).rotate(heading, _ship.away);
				}
				return _direction;
			}
			set => Check((_direction = double.IsNaN(value.x)
				|| double.IsNaN(value.y) || double.IsNaN(value.z)
				? new Vector(double.NaN, double.NaN, double.NaN)
				: value).x);
		}

		[Description("Target heading [0..360]."
			+ " NaN for releasing the control.")]
		public double heading
		{
			get => _heading;
			set => Check(_heading = RosMath.Clamp360(value));
		}
		[Description("Target pitch/elevation [-180..+180]."
			+ " Values outside -90..+90 flip heading."
			+ " NaN for releasing the control.")]
		public double pitch
		{
			get => _pitch;
			set => Check(_pitch = RosMath.ClampS180(value));
		}
		[Description("Target roll/bank [-180..+180]."
			+ " NaN for releasing the control.")]
		public double roll
		{
			get => _roll;
			set => Check(_roll = RosMath.ClampS180(value));
		}
		[Description("Stop ship rotation. (A bit stronger alternative to stock SAS.)")]
		public bool killRot
		{
			get => _killRot;
			set => Check((_killRot = value) ? 1.0 : double.NaN);
		}
		[Description("Limit pitch/yaw by their ratio needed to achieve direction. This leads to better 2D/3D behaviour.")]
		public bool pylink
		{
			get => _pylink;
			set => _pylink = value;
		}

		[Description("SAS: Stability Assist System. This is stock (weaker) alternative to `killRot`.")]
		public bool sas
		{
			get => _ship.sas;
			set => _ship.sas = value;
		}
		[Description("Reset stock SAS - resets its PIDs and quickly disables and re-enables it if it is currently enabled.")]
		public void resetSAS()
		{
			var ap = _ship.native.Autopilot;
			if (ap == null) return;
			ap.SAS.ResetAllPIDS();
			if (ap.Enabled)
			{
				ap.Disable();
				ap.Enable();
			}
		}

		[Description("RCS: Reaction Control System.")]
		public bool rcs
		{
			get => _ship.rcs;
			set => _ship.rcs = value;
		}

		[WorkInProgress, Description("General strength of user override/correction of controls. \\[0, 1] 0.8 by default.")]
		public float userFactor
		{
			get => _userFactor;
			set => _userFactor = float.IsNaN(value) ? 0f : RosMath.Clamp(value, 0f, 1f);
		}
		[WorkInProgress, Description("Strength of user pitch-override/correction. \\[0, 1] or `nan` - `userFactor` used if `nan` (which is by default).")]
		public float userPitchFactor
		{
			get => _userPitch;
			set => _userPitch = RosMath.Clamp(value, 0f, 1f);
		}
		[WorkInProgress, Description("Strength of user yaw-override/correction. \\[0, 1] or `nan` - `userFactor` used if `nan` (which is by default).")]
		public float userYawFactor
		{
			get => _userYaw;
			set => _userYaw = RosMath.Clamp(value, 0f, 1f);
		}
		[WorkInProgress, Description("Strength of user roll-override/correction. \\[0, 1] or `nan` - `userFactor` used if `nan` (which is by default).")]
		public float userRollFactor
		{
			get => _userRoll;
			set => _userRoll = RosMath.Clamp(value, 0f, 1f);
		}

		[WorkInProgress, Description("Set of PID(R) controllers used by the autopilot.")]
		public PIDs pids { get; } = new PIDs();

		protected PID pitchPID => pids._pitch;
		protected PID yawPID => pids._yaw;
		protected PID rollPID => pids._roll;

		public class PidParams : API.PidParams
		{
			public double strength { get; set; }
			// todo: maximal angular velocity and maximal stopping time
			public double angular { get; set; }
			public double time { get; set; }

			protected internal PidParams() => reset();
			public virtual void reset()
			{
				P = 1.00; // direct control        (better leave that as 1.0 for fast responses)
				I = 0.10; // error-correcting, *dt (this can be small but not zero)
				D = 0.03; // change-resisting, /dt (R&D can be zero, but at least one should not)
				R = 0.02; // cumulated change
				outputChangeLimit = 5; //*dt => 100% in 1/5s
				targetChangeLimit = 5; //*dt => 100% in 1/5s
				accumulatorLimit = 0.2; // abs(accu) <= 20%
				errorLimit = 1; // abs(err) <= 100%
				strength = 4.0;
				angular = 10.0;
				time = 3.0;
			}

			// not that it would be so important, simple PI would do,
			// but we want roll-pid to stabilize a bit quicker
			// (albeit through little damped oscillation)
			internal class Roll : PidParams
			{
				public override void reset()
				{
					base.reset();
					I = 0.2;
					D = 0.1;
					R = 0.15;
				}
			}
		}
		public class PID : API.PID<PidParams>
		{
			protected internal PID() : base(new PidParams()) => Init();
			protected internal PID(PidParams param) : base(param) => Init();
			protected internal void Init()
			{
				param.reset();
				maxInput = +1.0;
				minInput = -1.0;
				maxOutput = +1.0;
				minOutput = -1.0;
				reset();
			}
			public double strength
			{
				get => param.strength;
				set => param.strength = value;
			}
			public double angular
			{
				get => param.angular;
				set => param.angular = value;
			}
			public double time
			{
				get => param.time;
				set => param.time = value;
			}
		}
		[WorkInProgress, Description(
@"Set of PID(R) controllers used by the autopilot. Simple PI-regulator with small `I`
would do (some non-zero `I` is needed to eliminate final offset, especially for roll)
as these are used to modify raw controls (-1..+1). Other parameters were itegrated,
the most important probably being `strength` which determines how aggressive the autopilot is.")]
		public class PIDs
		{
			protected internal PIDs() { }

			protected internal PID _pitch = new PID();
			protected internal PID _yaw = new PID();
			protected internal PID _roll = new PID(new PidParams.Roll());

			[Description("Pitch control PID(R) parameters.")]
			public PidParams pitch => _pitch.param;
			[Description("Yaw control PID(R) parameters.")]
			public PidParams yaw => _yaw.param;
			[Description("Roll control PID(R) parameters.")]
			public PidParams roll => _roll.param;

			double combine(double a, double b)
				=> a == b ? b : double.NaN;
			double combine(double a, double b, double c)
				=> a == b && b == c ? c : double.NaN;

			[Description("Proportional factor (strength of direct control) for all three angles (`NaN` if not same).")]
			public double P
			{
				get => combine(pitch.P, yaw.P, roll.P);
				set => roll.P = yaw.P = pitch.P = value;
			}
			[Description("Integral factor (dynamic error-correction, causes oscillation as side-effect) for all three angles (`NaN` if not same).")]
			public double I
			{
				get => combine(pitch.I, yaw.I, roll.I);
				set => roll.I = yaw.I = pitch.I = value;
			}
			[Description("Derivative factor (dampening - applied to output, reduces the oscillation) for all three angles (`NaN` if not same).")]
			public double D
			{
				get => combine(pitch.D, yaw.D, roll.D);
				set => roll.D = yaw.D = pitch.D = value;
			}
			[Description("Reduction factor for accumulator for all three angles (`NaN` if not same;"
				+ "dampening - applied to accumulator used by integral factor,"
				+ " works well against both oscillation and windup).")]
			public double R
			{
				get => combine(pitch.R, yaw.R, roll.R);
				set => roll.R = yaw.R = pitch.R = value;
			}
			[Description("Common strength/aggressiveness of control (`NaN` if not same`).")]
			public double strength
			{
				get => combine(pitch.strength, yaw.strength, roll.strength);
				set => roll.strength = yaw.strength = pitch.strength = value;
			}
			public double angular
			{
				get => combine(pitch.angular, yaw.angular, roll.angular);
				set => roll.angular = yaw.angular = pitch.angular = value;
			}
			public double time
			{
				get => combine(pitch.time, yaw.time, roll.time);
				set => roll.time = yaw.time = pitch.time = value;
			}

			public void reset()
			{
				_pitch.Init();
				_yaw.Init();
				_roll.Init();
			}
		}

		protected void Hook()
		{
			if (_hooked != null)
				return;
			_hooked = _ship?.native;
			if (_hooked == null)
				return;
			_hooked.OnFlyByWire += Callback;
			pitchPID.reset();
			yawPID.reset();
			rollPID.reset();
			_lastUpdate = TimeStamp.never;
			_warpLock = TimeStamp.never;
		}
		protected void Unhook()
		{
			if (_hooked == null)
				return;
			_hooked.OnFlyByWire -= Callback;
			_hooked = null;
		}
		protected void Check(double value)
		{
			if (!double.IsNaN(value))
				Hook();
			else if (double.IsNaN(_throttle)
				&& double.IsNaN(_pitch) && double.IsNaN(_heading) && double.IsNaN(_roll)
				&& double.IsNaN(_direction.x * _direction.y * _direction.z)
				&& float.IsNaN(_rawPitch) && float.IsNaN(_rawYaw) && float.IsNaN(_rawRoll))
				Unhook();
		}

		/// <summary>
		/// Here is where all the magic happens.
		/// </summary>
		/// <param name="st">Vessel control state (in-out).</param>
		protected virtual void Callback(FlightCtrlState st)
		{
			if (_hooked == null)
				return;
			if (!float.IsNaN(_throttle))
				st.mainThrottle = RosMath.Clamp(_throttle, 0f, 1f);

			if (Time.since(_lastUpdate) > 0.2)
			{// note that this callback is not executed during on-rails warp
				pitchPID.resetAccu();
				yawPID.resetAccu();
				rollPID.resetAccu();
				_warpLock = Time.now;
			}
			_lastUpdate = Time.now;

			var angvel = _ship.angularVelocity;
			var maxang = _ship.maxAngular;
			if (!double.IsNaN(_direction.x)
				|| !double.IsNaN(_pitch)
				|| !double.IsNaN(_heading))
			{
				// translate the direction into local space
				// (like if we were looking at the vector from cockpit)
				var want = _ship.local(direction).normalized;
				// now get the angles in respective planes and feed it into the PIDs
				// ship.forward => (0,1,0)  <= transform.up       (up in VAB is future forward)
				// ship.right   => (1,0,0)  <= transform.right    (right is as it should be)
				// ship.up      => (0,0,-1) <= -transform.forward (future up/above is towards the screen in VAB, but Z is away - left-handed coords)
				var pitchDiff = RosMath.Deg.Atan2(-want.z, want.y);
				var yawDiff = RosMath.Deg.Atan2(want.x, want.y);
				// try to keep the ratio
				var pitchScale = 1.0;
				var yawScale = 1.0;
				if (_pylink)
				{// look into AngularControl for why these equations
					var pr = Math.Abs(pitchDiff + angvel.x*Math.Abs(angvel.x)/maxang.x);
					var yr = Math.Abs(yawDiff   + angvel.z*Math.Abs(angvel.z)/maxang.z);
					if (pr >= 0.5 && yr >= 0.5)
					{// use the ratio-based limiter only after certain threshold
						var ratio = Math.Pow(RosMath.Clamp(pr / yr, 0.1, 10.0),
							0.95 - 0.25/Math.Max(pr, yr)); // + smooth transition
						var smooth = 2.0*Math.Min(pr, yr) - 1.0;
						if (smooth < 1.0)
							ratio = 1.0 + (ratio - 1.0) * smooth;
						if (ratio < 1.0)
							pitchScale = ratio;
						else yawScale = 1.0/ratio;
					}
				}
				// compute control inputs (X=pitch, Z=yaw - the one not used in the atan2 above)
				var pitch = AngularControl(pitchPID, pitchDiff, angvel.x, maxang.x, _userPitch, Player.pitch, pitchScale);
				var yaw = AngularControl(yawPID, yawDiff, angvel.z, maxang.z, _userYaw, Player.yaw, yawScale);
				// set the controls
				if (!double.IsNaN(pitch))
					st.pitch = ControlValue(pitch, _userPitch, Player.pitch);
				if (!double.IsNaN(yaw))
					st.yaw = ControlValue(yaw, _userYaw, Player.yaw);
			}
			else if (killRot)
			{
				// compute control inputs
				var pitch = AngularControl(pitchPID, 0, angvel.x, maxang.x, _userPitch, Player.pitch);
				var yaw = AngularControl(yawPID, 0, angvel.z, maxang.z, _userYaw, Player.yaw);
				// set the controls
				if (!double.IsNaN(pitch))
					st.pitch = ControlValue(pitch, _userPitch, Player.pitch);
				if (!double.IsNaN(yaw))
					st.yaw = ControlValue(yaw, _userYaw, Player.yaw);
			}
			if (!double.IsNaN(_roll) || killRot)
			{
				// a bit of control to speed-up killRot stabilization (through damped oscillation)
				var rollDiff = 0.5*angvel.y;

				if (!double.IsNaN(_roll))
				{
					var apitch = Math.Abs(_ship.pitch);
					var sroll = apitch <= 89.0 ? _ship.roll : double.NaN;
					if (!double.IsNaN(_heading) && apitch >= 30.0)
					{
						var hroll = RosMath.ClampS180(180.0 +
							_ship.up.exclude(_ship.away).angle(
							_ship.north.rotate(_heading, _ship.away), _ship.away));
						if (apitch >= 60.0)
							sroll = hroll;
						else
						{
							if (Math.Abs(sroll - hroll) > 180.0)
								hroll = RosMath.ClampS180(hroll + 180.0);
							sroll = ((apitch-30.0)*hroll + (60.0-apitch)*sroll) / 30.0;
						}
					}
					if (!double.IsNaN(sroll))
						rollDiff = RosMath.ClampS180(_roll - sroll);
				}
				var roll = AngularControl(rollPID, rollDiff, angvel.y, maxang.y, _userRoll, Player.roll);
				if (!double.IsNaN(roll))
					st.roll = ControlValue(roll, _userRoll, Player.roll);
			}
			if (!float.IsNaN(_rawPitch))
				st.pitch = ControlValue(_rawPitch, _userPitch, Player.pitch);
			if (!float.IsNaN(_rawYaw))
				st.yaw = ControlValue(_rawYaw, _userYaw, Player.yaw);
			if (!float.IsNaN(_rawRoll))
				st.roll = ControlValue(_rawRoll, _userRoll, Player.roll);
		}

		protected virtual float ControlValue(double input, double factor, double user)
		{
			if (_ship.native != FlightGlobals.ActiveVessel)
				user = 0.0;
			else
			{
				if (double.IsNaN(factor))
					factor = _userFactor;
				user *= factor;
			}
			return (float)RosMath.Clamp(input + user, -1.0, +1.0);
		}

		/// <summary>
		/// Calculate PYR control input for given parameters. 
		/// </summary>
		/// <param name="pid">Associated PID(R) controller.</param>
		/// <param name="angle">Angle towards target (signed).</param>
		/// <param name="speed">Angular speed (deg/s, signed).</param>
		/// <param name="accel">Angular acceleration (deg/s/s, unsigned) at full control.</param>
		/// <returns>New control fraction (0-100%).</returns>
		protected virtual double AngularControl(
			PID pid, double angle,
			double speed, double accel,
			double factor, double user,
			double scale = 1.0)
		{
			// double the angle we will still travel if we try to stop immediately.
			// abs(speed)/accel is the time needed, 0.5*speed would be average speed,
			// we use double of that because we always add the angle difference.
			var stop = speed * (Math.Abs(speed)/accel + 0.3 + 10 * pid.dt);

			if (_ship.native != FlightGlobals.ActiveVessel)
				user = 0.0;
			else
			{
				if (double.IsNaN(factor))
					factor = _userFactor;
				user *= factor;
			}
			var warpDelta = Time.since(_warpLock);
			if (warpDelta < 0.2)
				scale = warpDelta <= 0.1 ? 0.0 : scale * (warpDelta - 0.1);
			pid.input = RosMath.Clamp(pid.strength
				* (angle + stop) / accel, pid.minInput, pid.maxInput)
				* scale * (1.0 - Math.Abs(user)) + user;
			return pid.update() - user;
		}
	}
}
