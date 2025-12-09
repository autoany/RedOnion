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
		for (int i = 0; ; )
		{
			msg.IndexOf('{', i);
			var call = new object[args.Length-1];
			for (int j = 1; j < args.Length; j++)
				call[j-1] = args[j].Box();
			msg = string.Format(Value.Culture, msg, call);
			args.Processor?.Print?.Invoke(msg);
			result = msg;
			return true;
		}
	}
}
