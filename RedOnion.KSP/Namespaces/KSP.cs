using KSP.UI.Screens;
using RedOnion.KSP.UnsafeAPI;
using UE = UnityEngine;

namespace RedOnion.KSP.Namespaces;

[DisplayName("KSP"), DocBuild("RedOnion.KSP/Namespaces/KSP")]
[SafeProps, Unsafe, Description("Unsafe KSP API - see [CommonScriptApi](../../CommonScriptApi.md)")]
public static class KSP_Namespace
{
	[Description("KSP Versioning info.")]
	public static Versioning versioning => Versioning.fetch;

	[Description("UnityEngine.Time")]
	public static readonly Type Time = typeof(UE.Time);
	[Description("UnityEngine.Random")]
	public static readonly Type Random = typeof(UE.Random);
	[Description("UnityEngine.Mathf")]
	public static readonly Type Mathf = typeof(UE.Mathf);

	[Description("A map of planet names to planet bodies. (Unsafe API)")]
	public static BodiesDictionary bodies => BodiesDictionary.Instance;

	[KspApi("class_vessel.html")]
	public static readonly Type Vessel = typeof(Vessel);
	[KspApi("_vessel_8cs.html#afa39c7ec7cc0926b332fcd2d77425edb", "Vessel Type (enum).")]
	public static readonly Type VesselType = typeof(VesselType);

	[KspApi("class_flight_ctrl_state.html", "Flight Control State (class for fly-by-wire/autopilot).")]
	public static readonly Type FlightCtrlState = typeof(FlightCtrlState);
	[KspApi("class_flight_globals.html", "Flight Globals (for autopilot).")]
	public static FlightGlobals FlightGlobals => FlightGlobals.fetch;
	[KspApi("class_flight_driver.html")]
	public static FlightDriver FlightDriver => FlightDriver.fetch;

	[KspApi("class_k_s_p_1_1_u_i_1_1_screens_1_1_stage_manager.html", "Staging logic.")]
	public static readonly Type StageManager = typeof(StageManager);

	[KspApi("_high_logic_8cs.html#a0687e907db3af3681f90377d69f32090", "Game scenes (enum).")]
	public static readonly Type GameScenes = typeof(GameScenes);
	[KspApi("class_high_logic.html", "LoadedScene indicator and other global state.")]
	public static HighLogic HighLogic => HighLogic.fetch;
	[KspApi("class_game.html", "State of the game.")]
	public static Game CurrentGame => HighLogic.CurrentGame;
	[KspApi("class_game_parameters.html", "Parameters of the game.")]
	public static GameParameters GameParameters => CurrentGame.Parameters;

	[KspApi("class_game_parameters_1_1_career_params.html", "Career parameters.")]
	public static GameParameters.CareerParams Career => GameParameters.Career;

	[KspApi("class_editor_logic.html", "For use in editor (VAB/SPH).")]
	public static readonly Type EditorLogic = typeof(EditorLogic);
	[KspApi("class_k_s_p_1_1_u_i_1_1_screens_1_1_editor_panels.html")]
	public static readonly Type EditorPanels = typeof(EditorPanels);
	[KspApi("class_ship_construction.html")]
	public static readonly Type ShipConstruction = typeof(ShipConstruction);
	[KspApi("class_part_loader.html")]
	public static readonly Type PartLoader = typeof(PartLoader);
	[KspApi("class_part_resource_library.html")]
	public static PartResourceLibrary PartResourceLibrary => PartResourceLibrary.Instance;

	[KspApi("class_input_lock_manager.html", "For locking input.")]
	public static readonly Type InputLockManager = typeof(InputLockManager);
	[Description("Alias to `InputLockManager`.")]
	public static readonly Type InputLock = typeof(InputLockManager);

	[KspApi("class_game_settings.html", "Various KSP settings.")]
	public static readonly Type GameSettings = typeof(GameSettings);
	[KspApi("class_game_events.html", "Various KSP events.")]
	public static readonly Type GameEvents = typeof(GameEvents);
	[KspApi("class_game_variables.html", "Various KSP variables.")]
	public static GameVariables GameVariables => GameVariables.Instance;
	[KspApi("class_game_database.html")]
	public static GameDatabase GameDatabase => GameDatabase.Instance;
	[KspApi("class_assembly_loader.html", "Assembly loader, list of assemblies and types.")]
	public static readonly Type AssemblyLoader = typeof(AssemblyLoader);

	[KspApi("class_research_and_development.html", "Science stuff.")]
	public static ResearchAndDevelopment ResearchAndDevelopment => ResearchAndDevelopment.Instance;
	[KspApi("class_research_and_development.html", "Science stuff.")]
	public static ResearchAndDevelopment RnD => ResearchAndDevelopment.Instance;
	[KspApi("class_science_util.html", "Science utilities.")]
	public static readonly Type ScienceUtil = typeof(ScienceUtil);
	[KspApi("_science_8cs.html", "Experiment situation flags.")]
	public static readonly Type ExperimentSituations = typeof(ExperimentSituations);


	public static PlanetariumCamera PlanetariumCamera => PlanetariumCamera.fetch;
}
