using System;
using System.Collections.Generic;
using System.IO;
using Kerbalui.Controls;
using MunOS;
using MunOS.Repl;
using RedOnion.KSP.API;
using RedOnion.KSP.ROS;
using RedOnion.KSP.Settings;

namespace LiveRepl
{
	public partial class ScriptWindow
	{
		// this needs some better logic because there is no way to know if there is any native callback (that may spawn a thread)
		public bool ScriptRunning => CurrentProcess?.Count > 0;

		public void SetCurrentEngineProcess(string engineName)
		{
			CurrentEngine = engines[engineName];
			uiparts.scriptEngineLabel.SetEngine(engineName);
		}

		Dictionary<string, ScriptManager> engines=new Dictionary<string, ScriptManager>();
		public ScriptManager CurrentEngine { get; protected set; }
		public MunProcess CurrentProcess => CurrentEngine?.Process;
		public OutputBuffer CurrentBuffer => CurrentEngine?.OutputBuffer;

		/* unused
		private MunProcess GetEngineProcessByExtension(string extension)
		{
			foreach(var engineProcessEntry in engines)
			{
				if (engineProcessEntry.Value.ScriptManager.Extension.ToLower()==extension.ToLower())
				{
					return engineProcessEntry.Value;
				}
			}
			return null;
		}
		*/

		public bool DisableElements;
		public void FixedUpdate()
		{
			//if (ScriptRunning)
			//{
			//	// make a delay after script starts running before disabling elements
			//	if (!disableClock.IsRunning) 
			//	{
			//		disableClock.Start();
			//	}
			//	if (disableClock.ElapsedMilliseconds>50)
			//	{
			//		DisableElements=true;
			//		disableClock.Reset();
			//	}

			//	//var currentEvaluation = evaluationList[0];
			//	//if (currentEvaluation.Evaluate())
			//	//{
			//	//	uiparts.replOutoutArea.AddReturnValue(currentEvaluation.Result);
			//	//	evaluationList.RemoveAt(0);
			//	//	completionManager.DisplayCurrentCompletions();
			//	//}
			//}
			//else
			//{
			//	disableClock.Reset();
			//	// have a delay after script ends before enabling elements
			//	if (!enableClock.IsRunning) 
			//	{
			//		enableClock.Start();
			//	}
			//	if (enableClock.ElapsedMilliseconds>50)
			//	{
			//		DisableElements=false;
			//		enableClock.Reset();
			//	}
			//}

			//foreach (var engineName in replEvaluators.Keys)
			//{
			//	var replEvaluator = replEvaluators[engineName];
			//	try
			//	{
			//		replEvaluator.FixedUpdate();
			//	}
			//	catch (Exception ex)
			//	{
			//		Debug.Log("Exception in REPL.FixedUpdate: " + ex.Message);
			//		replEvaluator.ResetEngine();
			//		//RunStartupScripts(engineName);
			//	}
			//}
		}


		void InitEvaluation()
		{
			ApiMain.Init();
			engines["ROS"] = new RosManager();

			string lastEngineName = SavedSettings.LoadSetting("lastEngine", "ROS");
			if (engines.ContainsKey(lastEngineName))
			{
				SetCurrentEngineProcess(lastEngineName);
			}
			else
			{
				foreach (var engineName in engines.Keys)
				{
					SetCurrentEngineProcess(engineName);
					SavedSettings.SaveSetting("lastEngine", engineName);
					break;
				}
			}

			foreach (var engineName in engines.Keys)
			{
				uiparts.scriptEngineSelector.AddMinSized(new Button(engineName, () =>
				{
					SetCurrentEngineProcess(engineName);
					SavedSettings.SaveSetting("lastEngine", engineName);
				}));
			}

			RunAutorunScripts();
		}

		public IList<string> GetAutorunScripts(string extensionToMatch)
		{
			var extensionScriptNames = new List<string>();
			foreach (var scriptname in AutoRun.Instance)
			{
				var extension = Path.GetExtension(scriptname);
				if (extension.Equals(extensionToMatch, StringComparison.OrdinalIgnoreCase))
					extensionScriptNames.Add(scriptname);
			}
			return extensionScriptNames;
		}
	}
}
