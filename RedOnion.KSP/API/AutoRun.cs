using RedOnion.KSP.Settings;
using System.Text;

namespace RedOnion.KSP.API;

[Description("Used to get, set, or modify the current list of scripts (file names, paths)"
+ "that are to be executed first whenever an engine is initialized or reset.")]
public class AutoRun : IList<string>
{
	public static readonly AutoRun Instance = new AutoRun();
	protected AutoRun() { }

	protected const string AutoRunSettingName = "AutoRun";
	protected ListCore<string> list;
	protected bool loaded;

	protected AutoRun Load()
	{
		if (!loaded)
		{
			loaded = true;
			list.AddRange(SavedSettings.LoadListSetting(AutoRunSettingName));
		}
		return this;
	}
	protected void Save()
	{
		SavedSettings.SaveListSetting(AutoRunSettingName, this);
	}

	[Description("Clears the list and saves the empty list.")]
	public void Clear()
	{
		list.Clear();
		loaded = true;
		Save();
	}

	[Description("Adds a new script to the list.")]
	public void Add(string script)
	{
		Load();
		list.Add(script);
		Save();
	}

	[Description("Removes the given script from the list.")]
	public bool Remove(string script)
	{
		Load();
		bool was = list.Remove(script);
		Save();
		return was;
	}

	[Description("Inserts a new script to the list at the specified index.")]
	public void Insert(int index, string script)
	{
		Load();
		list.Insert(index, script);
		Save();
	}

	[Description("Removes the script at the specified index.")]
	public void RemoveAt(int index)
	{
		Load();
		list.RemoveAt(index);
		Save();
	}

	[Description("Number of scripts in the list.")]
	public int Count => Load().list.Count;

	[Description("Get script by index. Will throw exception if index is out of range.")]
	public string this[int index]
	{
		get => Load().list[index];
		set
		{
			Load();
			list[index] = value;
			Save();
		}
	}
	[Description("Get index of script. -1 if not found.")]
	public int IndexOf(string script) => Load().list.IndexOf(script);

	[Description("Test wether the list contains specified script.")]
	public bool Contains(string script) => Load().list.Contains(script);

	[Browsable(false)]
	public void CopyTo(string[] array, int index)
	{
		Load();
		list.CopyTo(array, index);
	}

	[Browsable(false)]
	public IEnumerator<string> GetEnumerator() => Load().list.GetEnumerator();
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.Append("[");
		for (int i = 0; i < Count; i++)
		{
			if (sb.Length >= 64)
			{
				sb.Append(", ...");
				break;
			}
			if (i > 0)
				sb.Append(", ");
			sb.Append(this[i]);
		}
		sb.Append("]");
		return sb.ToString();
	}

	bool ICollection<string>.IsReadOnly => false;
}
