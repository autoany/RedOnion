namespace RedOnion.ROS;
using static ROS.Functions.Print;

public partial class Descriptor
{
	internal class OfString : Simple
	{
		public OfString()
			: base("string", typeof(string), ExCode.String, TypeCode.String, props) { }
		public override string ToString(ref Value self, string format, IFormatProvider provider, bool debug)
			=> self.obj.ToString();

		public override bool Call(ref Value result, object self, in Arguments args)
		{
			if (result.obj != (object)typeof(string))
				return false;
			if (args.Length == 0)
			{
				result = new Value(this, "");
				return true;
			}
			var msg = args[0].ToStr();
			if (args.Length == 1)
			{
				result = new Value(this, msg);
				return true;
			}
			if (IsFormatString(msg))
			{
				var call = new object[args.Length-1];
				for (int j = 1; j < args.Length; j++)
					call[j-1] = args[j].Box();
				msg = Format(Value.Culture, msg, call);
				result = msg;
				return true;
			}
			var sb = new StringBuilder(msg);
			for (int i = 1; i < args.Length; i++)
				sb.Append(", ").Append(args[i].ToStr());
			string output = sb.ToString();
			result = output;
			return true;
		}

		protected string ConvertCommas(string str)
			=> Value.Culture == CultureInfo.InvariantCulture ? 
				str.Replace(',', '.') : str;
		public override bool Convert(ref Value self, Descriptor to, CallFlags flags = CallFlags.Convert)
		{
			var str = self.obj.ToString();
			switch (to.Primitive)
			{
			case ExCode.String:
				self = str;
				return true;
			case ExCode.Char:
			case ExCode.WideChar:
				self = str.Length == 0 ? '\0' : str[0];
				return true;
			case ExCode.Byte:
				self = byte.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.UShort:
				self = ushort.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.UInt:
				self = uint.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.ULong:
				self = ulong.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.SByte:
				self = sbyte.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.Short:
				self = short.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.Int:
				self = int.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.Long:
				self = long.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.Float:
				self = float.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.Number:
			case ExCode.Double:
				self = double.Parse(ConvertCommas(str), Value.Culture);
				return true;
			case ExCode.Bool:
				foreach (char c in str)
				{
					if (char.IsWhiteSpace(c) || char.IsSymbol(c))
						continue;
					self = c == 't' || c == 'T' // true
						|| c == 'y' || c == 'Y' // yes
						|| c == 'a' || c == 'A' // ano
						|| c == 'e' || c == 'E' // enable
						|| c == 'p' || c == 'P' // povol
						|| (c > '0' && c <= '9');
					return true;
				}
				self = false;
				return true;
			}
			return false;
		}

		public override bool Binary(ref Value lhs, OpCode op, ref Value rhs)
		{
			if (lhs.desc.Primitive != ExCode.String && !lhs.desc.Convert(ref lhs, this))
				return false;
			if (rhs.desc.Primitive != ExCode.String && !rhs.desc.Convert(ref rhs, this))
				return false;
			switch (op)
			{
			case OpCode.Add:
				lhs.obj = lhs.obj.ToString() + rhs.obj.ToString();
				return true;

			case OpCode.Equals:
				lhs = string.Compare(lhs.obj.ToString(), rhs.obj.ToString(), StringComparison.OrdinalIgnoreCase) == 0;
				return true;
			case OpCode.Differ:
				lhs = string.Compare(lhs.obj.ToString(), rhs.obj.ToString(), StringComparison.OrdinalIgnoreCase) != 0;
				return true;

			case OpCode.Less:
				lhs = string.Compare(lhs.obj.ToString(), rhs.obj.ToString(), StringComparison.OrdinalIgnoreCase) < 0;
				return true;
			case OpCode.More:
				lhs = string.Compare(lhs.obj.ToString(), rhs.obj.ToString(), StringComparison.OrdinalIgnoreCase) > 0;
				return true;
			case OpCode.LessEq:
				lhs = string.Compare(lhs.obj.ToString(), rhs.obj.ToString(), StringComparison.OrdinalIgnoreCase) <= 0;
				return true;
			case OpCode.MoreEq:
				lhs = string.Compare(lhs.obj.ToString(), rhs.obj.ToString(), StringComparison.OrdinalIgnoreCase) >= 0;
				return true;
			}
			return true;
		}

		static readonly Props props = new StringProps();
		class StringProps : Props
		{
			static Prop M(string name, Func<string, Value, Value> fn)
				=> new Prop(name, new Value(new Method1<string>(name), fn));
			public StringProps() : base(new Prop[]
			{
				new Prop("length", new Value(0)),						//0
				new Prop("substring", new Value(new Substring())),		//1
				M("compare", (it, arg) => it.CompareTo(arg.ToStr())),	//2
				M("equals", (it, arg) => it.Equals(arg.ToStr())),		//3
				M("contains", (it, arg) => it.Contains(arg.ToStr())),	//4
				M("starts", (it, arg) => it.StartsWith(arg.ToStr()))	//5
			}, new Prop[] {
				new Prop("format", new Value(new Format()))
			})
			{
				Alias("count", 0);
				Alias("substr", 1);
				Alias("compareTo", 2);
				Alias("startsWith", 5);
			}
		};

		public override void Get(Core core, ref Value self)
		{
			if (self.obj != (object)typeof(string))
			{
				if (self.idx is string name
					&& name.Equals("length", StringComparison.OrdinalIgnoreCase))
				{
					self = self.obj.ToString().Length;
					return;
				}
				if (self.IsIntIndex)
				{
					self = self.obj.ToString()[self.num.Int];
					return;
				}
			}
			base.Get(core, ref self);
		}

		public override IEnumerable<Value> Enumerate(object self)
		{
			foreach (var c in self.ToString())
				yield return new Value(c);
		}

		class Substring : Descriptor
		{
			public override bool Call(ref Value result, object self, in Arguments args)
			{
				var str = self.ToString();
				if (args.Length == 0)
				{
					result = str;
					return true;
				}
				var arg = args[0].ToInt();
				if (args.Length == 1)
				{
					result = str.Substring(arg);
					return true;
				}
				var arg2 = args[1].ToInt();
				result = str.Substring(arg, arg2);
				return true;
			}
		}
		class Format : Descriptor
		{
			public override bool Call(ref Value result, object self, in Arguments args)
			{
				if (self == typeof(string))
				{
					if (args.Length == 0)
						return false;
					var msg = args[0].ToStr();
					var call = new object[args.Length-1];
					for (int i = 1; i < args.Length; i++)
						call[i-1] = args[i].Box();
					msg = Format(Value.Culture, msg, call);
					result = msg;
					return true;
				}
				else
				{
					var msg = self.ToString();
					var call = new object[args.Length];
					for (int i = 0; i < args.Length; i++)
						call[i] = args[i].Box();
					msg = Format(Value.Culture, msg, call);
					result = msg;
					return true;
				}
			}
		}
	}
}
