using Kerbalui.Controls;
using Kerbalui.EventHandling;
using Kerbalui.Layout;
using UnityEngine;

namespace LiveRepl.Parts
{
	/// <summary>
	/// The Group that holds the Repl and related functionality.
	/// </summary>
	public class ReplGroup : VerticalSpacer
	{
		public ScriptWindowParts uiparts;

		public ReplGroup(ScriptWindowParts uiparts)
		{
			this.uiparts=uiparts;

			var replControls=new HorizontalSpacer();

			replControls.AddWeighted(1, new Filler());
			replControls.AddFixed(ScriptWindow.baseCenterGroupWidth, new Button("Clear Repl", uiparts.scriptWindow.ClearRepl));
			replControls.AddFixed(ScriptWindow.baseCenterGroupWidth, new Button("Show Hotkeys", uiparts.scriptWindow.PrintKeyBindingsInOutputArea));

#if DEBUG
			replControls.AddFixed(ScriptWindow.baseCenterGroupWidth*2, new PrintQueuesArea(uiparts));
#endif


			AddMinSized(replControls);
			AddWeighted(1, uiparts.repl=new Repl(uiparts));

			uiparts.replInputArea.keybindings.Add(new EventKey(KeyCode.E, true), uiparts.scriptWindow.EvaluateReplText);
			uiparts.replInputArea.keybindings.Add(new EventKey(KeyCode.Return), uiparts.scriptWindow.SubmitReplText);

			uiparts.replInputArea.keybindings.Add(new EventKey(KeyCode.LeftBracket, true), () =>
			{
				//Debug.Log("history up");
				uiparts.replInputArea.Text = uiparts.scriptWindow.CurrentEngine.History.Up();
				uiparts.replInputArea.SelectIndex = uiparts.replInputArea.Text.Length;
				uiparts.replInputArea.CursorIndex = uiparts.replInputArea.Text.Length;
			});
			uiparts.replInputArea.keybindings.Add(new EventKey(KeyCode.Quote, true), () =>
			{
				//Debug.Log("history down");
				uiparts.replInputArea.Text = uiparts.scriptWindow.CurrentEngine.History.Down();
				uiparts.replInputArea.SelectIndex = uiparts.replInputArea.Text.Length;
				uiparts.replInputArea.CursorIndex = uiparts.replInputArea.Text.Length;
			});
			uiparts.replInputArea.keybindings.Add(new EventKey(KeyCode.UpArrow, true), () =>
			{
				//Debug.Log("history up");
				uiparts.replInputArea.Text = uiparts.scriptWindow.CurrentEngine.History.Up();
				uiparts.replInputArea.SelectIndex = uiparts.replInputArea.Text.Length;
				uiparts.replInputArea.CursorIndex = uiparts.replInputArea.Text.Length;
			});
			uiparts.replInputArea.keybindings.Add(new EventKey(KeyCode.DownArrow, true), () =>
			{
				//Debug.Log("history down");
				uiparts.replInputArea.Text = uiparts.scriptWindow.CurrentEngine.History.Down();
				uiparts.replInputArea.SelectIndex = uiparts.replInputArea.Text.Length;
				uiparts.replInputArea.CursorIndex = uiparts.replInputArea.Text.Length;
			});
		}
	}
}
