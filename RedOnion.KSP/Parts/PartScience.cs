using Experience.Effects;

namespace RedOnion.KSP.Parts
{
	[WorkInProgress, Description("State of science module.")]
	public enum ScienceState
	{
		[Description("Can perform experiment now.")]
		ready,
		[Description("Module is full of data.")]
		full,
		[Description("Module inoperable.")]
		inoperable,
		[Description("Module is shielded and cannot perform experiments now.")]
		shielded,
		[Description("Experiment not available in this situation.")]
		unavailable,
		[Description("Ship is currently not controllable (and module requires it).")]
		noControl,
		[Description("No crew in ship or the part (as required by the module).")]
		noCrew,
		[Description("No scientist in ship or the part (as required by the module).")]
		noScientist,
		[Description("Module needs some time before operation.")]
		cooldown,
		[Description("Unknown state, probably cannot perform experiments now.")]
		unknown,
		[Description("Unknown state, exception in API.")]
		error,
	}

	[Description("Available science through one part.")]
	public class PartScience
	{
		[Description("The part this science belongs to.")]
		public PartBase part { get; }
		[Unsafe, KspApi("class_module_science_experiment.html")]
		public ModuleScienceExperiment native { get; }
		public static implicit operator ModuleScienceExperiment(PartScience sci) => sci?.native;

		[Unsafe, KspApi("class_science_experiment.html")]
		public virtual ScienceExperiment experiment => native.experiment;

		[Description("Experiment ID."), Aliases("xid")]
		public string experimentId => native.experimentID; // do not use experiment.id

		[Description("Experiment title."), Aliases("title", "xtitle")]
		public string experimentTitle => experiment.experimentTitle;

		Science.Subject _subject;
		TimeStamp _subjectUpdate = TimeStamp.never;
		[Description("Science subject for the experiment and current situation.")]
		public Science.Subject subject
		{
			get
			{
				if (_subject == null)
				{
					_subject = new Science.Subject(experiment);
					_subjectUpdate = Time.now;
				}
				else
				{
					var now = Time.now;
					if (now > _subjectUpdate)
					{
						_subjectUpdate = now;
						_subject.Update();
					}
				}
				return _subject;
			}
		}
		[Description("Science subject ID for the experiment and current situation.")]
		public string id => subject.id;

		[Description("Science returned to KSC.")]
		public double completed => subject.completed;
		[Description("Total obtainable science.")]
		public double capacity => subject.capacity;
		[Description("Science value (when returned to KSC).")]
		public double value => subject.value;
		[Description("Next science value (when returned to KSC).")]
		public double nextValue => subject.nextValue;

		// note: may need to create linked list for multiple modules
		protected PartScience(PartBase part, ModuleScienceExperiment module)
		{
			this.part = part;
			native = module;
		}

		[Browsable(false)]
		public static readonly HashSet<string> ExcludedExperimentIds = new HashSet<string>()
		{
			"evaScience", // needs "EVAExperimentsKit" in ModuleInventoryPart?
			"ROCScience", // Breaking Ground RobotArmScanner?
			"WBIEmptyExperiment"
		};
		internal static PartScience Create(PartBase part, ModuleScienceExperiment module)
			=> Validate(TryCreate(part, module));
		private static PartScience Validate(PartScience it)
			=> it?.experiment != null ? it : null;
		private static PartScience TryCreate(PartBase part, ModuleScienceExperiment module)
		{
			if (module == null || ExcludedExperimentIds.Contains(module.experimentID))
				return null;
			var type = module.GetType();
			if (DMagic.api1?.type.IsAssignableFrom(type) == true)
				return new DMagic(part, module, DMagic.api1);
			if (DMagic.api2?.type.IsAssignableFrom(type) == true)
				return new DMagic(part, module, DMagic.api2);
			if (module.experiment == null)
				return null;
			return new PartScience(part, module);
		}

		public override string ToString()
			=> Value.Format($"{part?.name??("EVA")}/{native.experimentID}");

		[Description("Ready to perform experiment.")]
		public bool ready => state == ScienceState.ready;
		[Description("Experiment deployed (full of data).")]
		public bool deployed => native.Deployed;
		[Description("Experiment deployed (full of data).")]
		public bool full => native.Deployed;
		[Description("Experiment inoperable.")]
		public bool inoperable => native.Inoperable;
		[Description("Experiment shielded and not available as such.")]
		public bool shielded => !native.availableShielded && native.part.ShieldedFromAirstream;
		[Description("Experiment is available in current situation.")]
		public bool available => astate == ScienceState.ready;
		[Description("Experiment cooldown.")]
		public double cooldown => native.useCooldown ? native.cooldownTimer : 0.0;

		[Description("State/readiness of science module.")]
		public ScienceState state
		{
			get
			{
				if (native.Deployed)
					return ScienceState.full;
				if (native.Inoperable)
					return ScienceState.inoperable;
				if (!native.availableShielded && native.part.ShieldedFromAirstream)
					return ScienceState.shielded;
				var value = astate;
				if (value != ScienceState.ready)
					return value;
				if (native.useCooldown && native.cooldownTimer > 0.0)
					return ScienceState.cooldown;
				if (capacity > 0.0)
					return ScienceState.ready;
				return ScienceState.unknown;
			}
		}
		[Description("Experiment availability in current situation.")]
		public virtual ScienceState astate
		{
			get
			{
				try
				{
					var experiment = this.experiment;
					if (experiment == null)
						return ScienceState.error;
					if (!experiment.IsAvailableWhile(ScienceUtil.GetExperimentSituation(native.vessel), native.vessel.mainBody))
						return ScienceState.unavailable;
					var usage = (ExperimentUsageReqs)native.usageReqMaskInternal;
					string msg = null;
					if (!ScienceUtil.RequiredUsageInternalAvailable(
						native.vessel, native.part, usage, native.experiment, ref msg))
					{
						if (usage == ExperimentUsageReqs.Never)
							return ScienceState.unknown;
						if ((usage & ExperimentUsageReqs.VesselControl) != 0
							&& !native.vessel.IsControllable)
							return ScienceState.noControl;
						if ((usage & ExperimentUsageReqs.CrewInVessel) != 0
							&& !native.vessel.GetVesselCrew().Any(x =>
							x.type == ProtoCrewMember.KerbalType.Crew))
							return ScienceState.noCrew;
						if ((usage & ExperimentUsageReqs.CrewInPart) != 0
							&& !native.part.protoModuleCrew.Any(x =>
							x.type == ProtoCrewMember.KerbalType.Crew))
							return ScienceState.noCrew;
						if ((usage & ExperimentUsageReqs.ScientistCrew) != 0
							&& !((usage & ExperimentUsageReqs.CrewInPart) != 0
							? native.part.protoModuleCrew
							: native.vessel.GetVesselCrew()).Any(x =>
							x.HasEffect<SpecialExperimentSkill>()))
							return ScienceState.noScientist;
						return ScienceState.unknown;
					}
					return ScienceState.ready;
				}
				catch (Exception ex)
				{
					MainLogger.Log($"Exception checking science module state for {this}: {ex}");
					return ScienceState.error;
				}
			}
		}

		[Description("Perform the experiment (may take some time). Shows the dialog by default."
			+ " Note that suppressing the dialog is rather experimental and may not work for some parts.")]
		public virtual bool perform(bool dialog = true)
		{
			if (state != ScienceState.ready)
				return false;
			if (dialog)
				native.DeployExperiment();
			else if (native.useStaging && native.stagingEnabled)
				native.OnActive();
			else
			{
				var useStaging = native.useStaging;
				var stagingEnabled = native.stagingEnabled;
				native.useStaging = true;
				native.stagingEnabled = true;
				native.OnActive();
				native.useStaging = useStaging;
				native.stagingEnabled = stagingEnabled;
			}
			return true;
		}

		protected class DMagic : PartScience
		{
			protected internal static API api1, api2;

			static DMagic()
			{
				foreach (var asm in AssemblyLoader.loadedAssemblies)
				{
					switch (asm.name)
					{
					// DMagic's Orbital Science (Magnetometer etc.)
					case "DMagic":
						try
						{
							api1 = new API(asm.typesDictionary
								[typeof(PartModule)]
								["DMModuleScienceAnimate"]);
						}
						catch (Exception ex)
						{
							MainLogger.Log("Could not reflect DMagic's DMModuleScienceAnimate: " + ex.ToString());
						}
						break;
					// DMagic's generic module used e.g. by ReStock+ (if both installed)
					case "DMModuleScienceAnimateGeneric":
						try
						{
							api2 = new API(asm.typesDictionary
								[typeof(PartModule)]
								["DMModuleScienceAnimateGeneric"]);
						}
						catch (Exception ex)
						{
							MainLogger.Log("Could not reflect DMagic's DMModuleScienceAnimateGeneric: " + ex.ToString());
						}
						break;
					}
				}
			}

			public class API
			{
				public readonly Type type;
				public readonly Func<ModuleScienceExperiment, ScienceExperiment> scienceExp;
				public readonly Func<ModuleScienceExperiment, bool> canConduct;
				public readonly Action<ModuleScienceExperiment, bool> gatherScienceData;

				public API(Type type)
				{
					this.type = type;
					const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

					var module = Expression.Parameter(
						typeof(ModuleScienceExperiment), "module");
					try
					{
						canConduct = Expression.Lambda<Func<ModuleScienceExperiment, bool>>(
							Expression.Call(Expression.Convert(module, type),
							type.GetMethod("canConduct", flags, null, new Type[0], null)),
							module).Compile();
					}
					catch (Exception ex)
					{
						MainLogger.Log($"Could not reflect DMagic's `canConduct` in {type.Name}: {ex}");
					}

					var silent = Expression.Parameter(
						typeof(bool), "silent");
					try
					{
						gatherScienceData = Expression.Lambda<Action<ModuleScienceExperiment, bool>>(
							Expression.Call(Expression.Convert(module, type),
							type.GetMethod("gatherScienceData", flags, null, new Type[] { typeof(bool) }, null),
							silent), module, silent).Compile();
					}
					catch (Exception ex)
					{
						MainLogger.Log($"Could not reflect DMagic's `gatherScienceData` in {type.Name}: {ex}");
					}

					try
					{
						scienceExp = Expression.Lambda<Func<ModuleScienceExperiment, ScienceExperiment>>(
							Expression.Field(Expression.Convert(module, type),
							type.GetField("scienceExp", BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic)),
							module).Compile();
					}
					catch (Exception ex)
					{
						MainLogger.Log($"Could not reflect DMagic's `scienceExp` in {type.Name}: {ex}");
					}
				}
			}

			protected readonly Func<ModuleScienceExperiment, ScienceExperiment> _scienceExp;
			protected readonly Func<ModuleScienceExperiment, bool> _canConduct;
			protected readonly Action<ModuleScienceExperiment, bool> _gatherScienceData;

			protected internal DMagic(PartBase part, ModuleScienceExperiment module, API api)
				: base(part, module)
			{
				_scienceExp = api.scienceExp;
				_canConduct = api.canConduct;
				_gatherScienceData = api.gatherScienceData;
			}

			public override ScienceExperiment experiment
				=> _scienceExp != null ? _scienceExp(native) : base.experiment;

			public override ScienceState astate
			{
				get
				{
					if (_canConduct == null)
						return base.astate;
					return _canConduct(native) ? ScienceState.ready : ScienceState.unknown;
				}
			}

			public override bool perform(bool dialog = true)
			{
				if (_gatherScienceData == null)
					return base.perform(dialog);
				if (state != ScienceState.ready)
					return false;
				_gatherScienceData(native, !dialog);
				return true;
			}
		}
	}
}
