namespace RedOnion.KSP.Parts;

[WorkInProgress, Description("Parts per stage (by `decoupledin+1`).")]
public class Stages : ReadOnlyList<StagePartSet>
{
	[Description("Ship (vessel/vehicle) this list of parts belongs to.")]
	public Ship ship
	{
		get
		{
			Update();
			return _ship;
		}
		protected internal set => _ship = value;
	}
	protected Ship _ship;

	protected internal Stages(Ship ship, Action refresh) : base(refresh) => _ship = ship;
	protected internal override void SetDirty(bool value)
	{
		base.SetDirty(value);
		foreach (var stage in list)
			stage.Dirty = value;
	}
	protected internal override void Clear()
	{
		foreach (var stage in list)
			stage.Clear();
		//base.Clear(); - don't clear the list here, items reused
	}

	[Description("Current stage (highest index).")]
	public StagePartSet current => this[count - 1];
	[Description("Next stage (second highest index, null if last).")]
	public StagePartSet next => count >= 2 ? this[count - 2] : null;
	[Description("Last stage (index 0).")]
	public StagePartSet last => this[0];
}
[WorkInProgress, Description("Parts with same value of `decoupledin`.")]
public class StagePartSet : PartSet<PartBase>
{
	[Description("List of engines active in this stage.")]
	public readonly EngineSet engines;

	protected internal StagePartSet _next, _prev;
	[Description("Next stage (lower index).")]
	public StagePartSet next => _next;
	[Description("Previous stage (higher index).")]
	public StagePartSet prev => _prev;

	protected internal StagePartSet(Ship ship, Action refresh) : base(ship, refresh)
		=> engines = new EngineSet(ship, refresh);
	protected internal override void SetDirty(bool value)
	{
		updated = Time.never;
		engines.SetDirty(value);
		base.SetDirty(value);
	}
	protected internal override void Clear()
	{
		base.Clear();
		engines.Clear();
	}

	[Browsable(false)]
	public static TimeDelta UpdateInterval = new TimeDelta(0.2);
	protected TimeStamp updated = Time.never;

	protected double _thrust, _flow, _isp;
	public double thrust => Ensure()._thrust;
	public double flow => Ensure()._flow;
	public double isp => Ensure()._isp;

	protected StagePartSet Ensure()
	{
		Update();
		var now = Time.now;
		if (now - updated < UpdateInterval)
			return this;
		updated = now;
		_thrust = 0;
		_flow = 0;
		foreach (var e in engines)
		{
			var eisp = e.isp;
			if (eisp < EngineSet.minIsp)
				continue;
			var ethr = e.getThrust() * e.thrustPercentage * 0.01;
			_thrust += ethr;
			_flow += ethr / eisp;
		}
		_isp = _flow < EngineSet.minFlow ? 0.0 : _thrust / _flow;
		return this;
	}

}
