using System;
using Kerbalui.Controls;
using Kerbalui.Layout;
using LiveRepl.Decorators;
using RedOnion.KSP.API;

namespace LiveRepl.Parts
{
	/// <summary>
	/// The Center Group between the Editor and Repl.
	/// Will tend to contain functionality for the overall ScriptWindow
	/// </summary>
	public class CenterGroup : VerticalSpacer
	{
		ScriptWindowParts uiparts;

		public CenterGroup(ScriptWindowParts uiparts)
		{
			this.uiparts=uiparts;

			var disableableStuff=new ScriptDisabledButtonsGroup();

			AddMinSized(new Button("<<", uiparts.scriptWindow.ToggleRepl));
			AddMinSized(new Button(">>", uiparts.scriptWindow.ToggleEditor));
			AddMinSized(new ScriptDisabledElement(uiparts, 
				new Button("Run", uiparts.scriptWindow.RunEditorScript)));
			AddMinSized(new Button("Terminate", uiparts.scriptWindow.Terminate));
			disableableStuff.AddMinSized(new Button("Reset Engine", uiparts.scriptWindow.ResetEngine));
			disableableStuff.AddMinSized(new Button("Reset Pilot", Ship.DisableAutopilot));
			disableableStuff.AddMinSized(new Button("Reset All", uiparts.scriptWindow.ResetAll));
			AddMinSized(new ScriptDisabledElement(uiparts, disableableStuff));
			AddMinSized(uiparts.scriptEngineSelector=new ScriptEngineSelector(uiparts));
		}
	}
}
