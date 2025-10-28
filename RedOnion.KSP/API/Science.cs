using KSP.Localization;
using RedOnion.Attributes;
using RedOnion.KSP.Parts;
using RedOnion.KSP.Utilities;
using RedOnion.ROS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RedOnion.KSP.API.Science;
using static System.FormattableString;

namespace RedOnion.KSP.API;

[WorkInProgress, Description("Science tools. See also [ship.science](Ship.md) and [PartScience](../Parts/PartScience.md).")]
public static class Science
{
	static string landedAt;
	internal static void update()
	{
		data.Dirty = true;
		if (!HighLogic.LoadedSceneIsFlight)
			goto clear;
		var active = FlightGlobals.ActiveVessel;
		if (!active) goto clear;
		var v = active.EVALadderVessel;
		var mbody = v.mainBody;
		if (!mbody) goto clear;
		var sit = ScienceUtil.GetExperimentSituation(v);
		var bio = ScienceUtil.GetExperimentBiome(mbody, v.latitude, v.longitude);
		var at = v.landedAt;
		if (body?.native == mbody && sit == situation && bio == mainBiome && at == landedAt)
			return;
		body = Bodies.Instance[mbody];
		situation = sit;
		mainBiome = bio;
		mainBiomeId = mainBiome.Replace(" ", "");
		landedAt = at;
		if (at?.Length > 0)
		{
			biome = subBiome = Vessel.GetLandedAtString(at);
			biomeId = subBiomeId = subBiome.Replace(" ", "");
			biomeName = Localizer.Format(v.displaylandedAt);
			if (biomeName == null || biomeName.Length == 0)
				biomeName = at;
		}
		else
		{
			biome = mainBiome;
			biomeId = mainBiomeId;
			subBiome = null;
			subBiomeId = null;
			biomeName = ScienceUtil.GetBiomedisplayName(mbody, biome);
			if (biomeName == null || biomeName.Length == 0)
				biomeName = biome;
		}
		situationId = body.name + situation.ToString() + biomeId;
		foreach (var action in situationChanged)
			action.Invoke();
		return;
	clear:
		body = null;
		situation = 0;
		biome = null;
		biomeId = null;
		data.Clear();
	}

	[Description("Current space/celestial body.")]
	public static SpaceBody body { get; private set; }
	[Description("Current science situation.")]
	public static ExperimentSituations situation { get; private set; }
	[Description("Current situation ID (`{body}{situation}{biomeId}`).")]
	public static string situationId { get; private set; }

	[Description("Event executed when `situationId` changes.")]
	public static readonly AutoRemoveList<Action> situationChanged = new AutoRemoveList<Action>();

	[Description("Current biome (display name).")]
	public static string biomeName { get; private set; }

	[Description("Current biome (tag, returns subBiome if landed).")]
	public static string biome { get; private set; }
	[Description("Current biome ID (spaces removed).")]
	public static string biomeId { get; private set; }

	[Description("Current main biome (tag, ignores landed).")]
	public static string mainBiome { get; private set; }
	[Description("Current main biome ID (spaces removed).")]
	public static string mainBiomeId { get; private set; }

	[Description("Current sub-biome (tag, valid only if landed).")]
	public static string subBiome { get; private set; }
	[Description("Current sub-biome ID (spaces removed).")]
	public static string subBiomeId { get; private set; }

	[Description("Career gain factor.")]
	public static double gain => HighLogic.CurrentGame.Parameters.Career.ScienceGainMultiplier;

	[Description("Science subject.")]
	public class Subject
	{
		[Unsafe, Description("[KSP API](https://kerbalspaceprogram.com/api/class_science_subject.html)")]
		public ScienceSubject native { get; private set; }
		public static implicit operator ScienceSubject(Subject sub) => sub?.native;

		[Description("Subject ID.")]
		public string id => native.id;

		[Unsafe, Description("[KSP API](https://kerbalspaceprogram.com/api/class_science_experiment.html)")]
		public ScienceExperiment experiment { get; }

		[Description("Title of the experiment")]
		public string title => experiment.experimentTitle;

		[Description("Subject space/celestial body.")]
		public SpaceBody body { get; private set; }
		[Description("Subject situation.")]
		public ExperimentSituations situation { get; private set; }
		[Description("Subject biome (if relevant).")]
		public string biome { get; private set; }
		[Description("Subject biome ID (if relevant).")]
		public string biomeId { get; private set; }

		internal Subject(ScienceExperiment exp)
		{
			experiment = exp;
			Update();
		}
		internal void Update()
		{
			body = Science.body;
			situation = Science.situation;
			string biomeName = null;
			if (experiment.BiomeIsRelevantWhile(situation))
			{
				biome = Science.biome;
				biomeId = Science.biomeId;
				biomeName = Science.biomeName;
			}
			else
			{
				biome = null;
				biomeId = null;
			}
			var id = Invariant(
				$"{experiment.id}@{body.name}{situation}{biomeId}");
			native = ResearchAndDevelopment.GetSubjectByID(id) ??
				(native?.id == id ? native :
				new ScienceSubject(experiment, situation, body, biome, biomeName));
		}

		[Description("Science returned to KSC.")]
		public double completed => native.science * gain;
		[Description("Total obtainable science.")]
		public double capacity => native.scienceCap * gain;
		[Description("Science value (when returned to KSC).")]
		public double value => gain * ResearchAndDevelopment.GetScienceValue(
			experiment.baseValue * experiment.dataScale, this);
		[Description("Next science value (when returned to KSC).")]
		public double nextValue => gain * ResearchAndDevelopment.GetNextScienceValue(
			experiment.baseValue * experiment.dataScale, this);

		public override string ToString() => Value.Format(
			$@"{completed,4:F1}/{capacity,4:F1}; {value,4:F1}|{nextValue,4:F1}: {
				title}, {body.name}, {situation}, {biomeName}");
	}

	[Description("Science data of current vessel.")]
	public static DataList data { get; } = new();
	public class DataList : ReadOnlyList<Data>
	{
		protected Dictionary<string, Data> dict = new();
		public bool Contains(object id) => this[id] != null;
		public Data this[object id] => GetData(id);
		Data GetData(object id)
		{
			if (id is null)
				return null;
			Update();
			if (id is string s)
				return dict.TryGetValue(s, out var it) ? it : null;
			if (id is PartScience sci)
				return dict.TryGetValue(sci.id, out var it) ? it : null;
			if (id is Data d)
				return dict.TryGetValue(d.id, out var it) ? it : null;
			return null;
		}

		protected internal override void Clear()
		{
			list.Clear();
			dict.Clear();
		}

		protected override void DoRefresh()
		{
			Dirty = false;
			Clear();
			if (!HighLogic.LoadedSceneIsFlight)
				return;
			var active = FlightGlobals.ActiveVessel;
			if (!active)
				return;
			var v = active.EVALadderVessel;
			foreach (var c in v.FindPartModulesImplementing<IScienceDataContainer>())
			{
				var list = c.GetData();
				if (list == null)
					continue;
				foreach (var d in list)
				{
					var it = new Data(d);
					data.Add(it);
					dict[it.id] = it;
				}
			}
			Refresh?.Invoke();
		}
	}

	public static bool Contains(object id) => data.Contains(id);

	[Description("Science data.")]
	public class Data
	{
		[Unsafe]
		public ScienceData native { get; private set; }
		public static implicit operator ScienceData(Data data) => data?.native;

		[Description("Subject ID.")]
		public string id => native.subjectID;
		[Description("Title of the experiment")]
		public string title => native.title;
		[Description("Science value.")]
		public double value => native.labValue;

		internal Data(ScienceData data)
		{
			native = data;
		}
	}
}
