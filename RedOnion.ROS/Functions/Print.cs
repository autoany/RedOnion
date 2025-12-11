namespace RedOnion.ROS.Functions;

public class Print(UserObject baseClass) : UserObject("Print", baseClass)
{
	public override bool Call(ref Value result, object self, in Arguments args)
	{
		if (args.Length == 0)
		{
			args.Processor?.Print?.Invoke("");
			result = "";
			return true;
		}
		var msg = args[0].ToStr();
		if (args.Length == 1)
		{
			args.Processor?.Print?.Invoke(msg);
			result = msg;
			return true;
		}
		if (IsFormatString(msg))
		{
			var call = new object[args.Length-1];
			for (int j = 1; j < args.Length; j++)
				call[j-1] = args[j].Box();
			msg = Format(Value.Culture, msg, call);
			args.Processor?.Print?.Invoke(msg);
			result = msg;
			return true;
		}
		var sb = new StringBuilder(msg);
		for (int i = 1; i < args.Length; i++)
			sb.Append(", ").Append(args[i].ToStr());
		string output = sb.ToString();
		args.Processor?.Print?.Invoke(output);
		result = output;
		return true;
	}

	public static bool IsFormatString(string format)
	{
		if (string.IsNullOrEmpty(format))
			return false;
		for (int i = 0; i < format.Length - 1;)
		{
			if (format[i++] != '{')
				continue;
			if (format[i++] != '{')
				return true;
		}
		return false;
	}

	public static string Format(IFormatProvider provider, string format, params object[] args)
	{
		if (args == null || args.Length == 0)
			return format == null ? "null" : string.Format(provider, format);
		for (int i = 0; i < args.Length; i++)
		{
			var arg = args[i];
			if (arg is bool b)
				args[i] = b ? "true" : "false";
			else if (arg is null)
				args[i] = "null";
		}
		return string.Format(provider, format, args);
	}
}
