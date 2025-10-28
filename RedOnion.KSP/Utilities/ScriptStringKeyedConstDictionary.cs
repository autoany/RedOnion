using System;
using System.Collections.Generic;
using System.Linq;
using RedOnion.Common.Completion;

namespace RedOnion.KSP.Utilities
{
	/// <summary>
	/// The purpose of this is to provide functionality like
	/// dictionary.keyname in-game, for example, for a mapping of planets accessed
	/// by planetname that is not to be modified.
	/// 
	/// Implementing this for ROS and Lua will allow us to add dictionaries like the
	/// global "bodies" in the future without having to create a custom per-engine
	/// definition per dictionary-like-thing.
	/// </summary>
	public class ScriptStringKeyedConstDictionary<T> : Dictionary<string, T>, ICompletable
	{
		public ScriptStringKeyedConstDictionary()
		{
		}

		public IList<string> PossibleCompletions => Keys.ToList();

		public bool TryGetCompletion(string completionName, out object completion)
		{
			if(TryGetValue(completionName, out T completionT))
			{
				completion = completionT;
				return true;
			}
			completion = null;
			return false;
		}
	}
}
