using System.Globalization;
using System.Text;

namespace RedOnion.ROS.Tests;

public class StatementTests : CoreTests
{
	public void Test(ExitCode exit, object value, string script, int countdown = 1000)
	{
		Test(script, countdown);
		Assert.AreEqual(exit, Exit, "Test: <{0}>", script);
		var result = Result.Box();
		Assert.AreEqual(value, result, "Different result: <{0}>", script);
		Assert.AreEqual(value?.GetType(), result?.GetType(), "Different type: <{0}>", script);
	}
	public void Lines(ExitCode exit, object value, params string[] lines)
		=> Test(exit, value, string.Join(Environment.NewLine, lines));
	public void Lines(ExitCode exit, object value, int countdown, params string[] lines)
		=> Test(exit, value, string.Join(Environment.NewLine, lines), countdown);

	public void Yield(string script, int countdown = 1000)
	{
		try
		{
			Code = Compile(script);
			if (Globals == null)
				Globals = new Globals();
			Assert.IsFalse(Execute(countdown));
			do UpdatePhysics();
			while (Paused);
		}
		catch (Exception e)
		{
			throw new Exception(String.Format("{0} in Eval: {1}; IN: <{2}>",
				e.GetType().ToString(), e.Message, script), e);
		}
	}
	public void Yield(ExitCode exit, object value, string script, int countdown = 1000)
	{
		Yield(script, countdown);
		Assert.AreEqual(exit, Exit, "Test: <{0}>", script);
		var result = Result.Box();
		Assert.AreEqual(value, result, "Different result: <{0}>", script);
		Assert.AreEqual(value?.GetType(), result?.GetType(), "Different type: <{0}>", script);
	}
	public void YieldLines(params string[] lines)
		=> Yield(string.Join(Environment.NewLine, lines));
	public void YieldLines(ExitCode exit, object value, params string[] lines)
		=> Yield(exit, value, string.Join(Environment.NewLine, lines));
	public void YieldLines(ExitCode exit, object value, int countdown, params string[] lines)
		=> Yield(exit, value, string.Join(Environment.NewLine, lines), countdown);
}
[TestFixture]
public class ROS_Statements : StatementTests
{
	string lastPrint;
	public ROS_Statements()
		=> Print = (msg) => lastPrint = msg;

	[TearDown]
	public override void Reset() => base.Reset();

	[Test]
	public void ROS_Stts01_Return()
	{
		Test(ExitCode.Return, null, "return");
		Test(ExitCode.Return, 1234, "return 1234");
		Test(ExitCode.Return, 12/5, "return 12/5");
	}

	[Test]
	public void ROS_Stts02_IfElse()
	{
		Test(ExitCode.Return, true, "if true then return true");
		Test(ExitCode.Return, false, "if false: return true else: return false");
		Test(ExitCode.Return, 1, "if true; return 1; else return 2");
		Test(ExitCode.Return, 2, "unless true: return 1; else return 2");

		Lines(ExitCode.Return, 1,
			"if true; return 1",
			"else if true; return 2",
			"else return 3");
		Lines(ExitCode.Return, 2,
			"if false; return 1",
			"else if true; return 2",
			"else return 3");
		Lines(ExitCode.Return, 3,
			"if false; return 1",
			"else if false; return 2",
			"else return 3");

		Lines(ExitCode.Return, 3,
			"if true",
			"  if false; return 1",
			"else return 2",
			"return 3");
	}

	[Test]
	public void ROS_Stts03_Loops()
	{
		Lines(ExitCode.Return, 5,
			"var counter = 0",
			"while counter < 5",
			"  counter++",
			"return counter");
		Lines(ExitCode.Return, 3,
			"counter = 0",
			"until counter > 2",
			"  counter++",
			"return counter");
		Lines(ExitCode.Return, 2,
			"counter = 0",
			"do counter++ while counter < 2",
			"return counter");
		Lines(ExitCode.Return, 6,
			"counter = 0",
			"do",
			"  if ++counter > 5; break",
			"  if counter > 3; continue",
			"  counter++",
			"  continue",
			"  counter = 10",
			"until counter > 10",
			"return counter");

		Lines(ExitCode.Return, 1,
			"var it = 0",
			"until (",
			"  false)",
			"  it = 1",
			"  return it",
			"return it+2");
	}

	[Test]
	public void ROS_Stts04_For()
	{
		Lines(ExitCode.Return, "321",
			"var s = \"\"",
			"for var i = 3; i; i -= 1; s += i",
			"return s");
		Lines(ExitCode.Return, "135",
			"s = \"\"",
			"for var i = 0; i < 10; i++",
			"  if i&1 == 0; continue",
			"  s += i",
			"  if i == 5; break",
			"return s");
	}

	[Test]
	public void ROS_Stts05_ForEach()
	{
		Lines(ExitCode.None, "hello world",
			"var s = \"\"",
			"foreach var e in [\"hello\", \" \", \"world\"]",
			"  s += e",
			"s");
		Lines(ExitCode.None, "hello world",
			"var s = \"\"",
			"foreach var e in [\"hello\", \" \", \"world\"]: s += e",
			"s");
		Lines(ExitCode.Return, "123",
			"var s = \"\"",
			"for var e in [1,2,3]",
			"  s += e",
			"return s");
	}

	[Test]
	public void ROS_Stts06_Break()
	{
		Lines(ExitCode.Return, "12",
			"var s = \"\"",
			"for var e in [1,2,3]",
			"  s += e",
			"  if e == 2",
			"    break",
			"return s");
		Lines(ExitCode.Return, "234",
			"var s = \"\"",
			"for var i in [1,2,3]",
			"  for var j in [1,2,3]",
			"    s += i+j",
			"    if j == i",
			"      break",
			"  if i == 2",
			"    break",
			"return s");
	}

	[Test]
	public void ROS_Stts07_Yield()
	{
		Yield("yield");
		YieldLines(ExitCode.Return, "done",
			"for var i = 0",
			"  if ++i > 3; return \"done\"",
			"  wait");
		YieldLines(ExitCode.None, 3,
			"var i = 0",
			"function f",
			"  i++",
			"  yield",
			"  i++",
			"  wait",
			"  return ++i",
			"f");
	}

	string Build(string fn, params object[] args)
	{
		var script = new StringBuilder(fn);
		for (int i = 0; i < args.Length; i++)
		{
			script.Append(i == 0 ? ' ' : ',');
			if (args[i] is string s)
				script.Append('\"').Append(s.Replace("\"", "\\\"")).Append('\"');
			else if (args[i] is char c)
				script.Append('\'').Append(c == '\'' ? "\\'" : c.ToString()).Append('\'');
			else if (args[i] is null)
				script.Append("null");
			else if (args[i] is bool b)
				script.Append(b ? "true" : "false");
			else if (args[i] is IFormattable f)
				script.Append(f.ToString(null, CultureInfo.InvariantCulture));
			else script.Append(args[i].ToString());
		}
		return script.ToString();
	}
	public void TestString(string value, params object[] args)
	{
		var script = Build("string", args);
		Test(value, script);
		script = Build("string.format", args);
		Test(value, script);

		script = Build("print", args);
		Test(value, script);
		Assert.AreEqual(value, lastPrint, "Different print: <{0}>", script);
	}
	public void TestFormat(string value, params object[] args)
	{
		var script = Build("string", args);
		Test(value, script);
		script = Build("string.format", args);
		Expect<FormatException>(script);

		script = Build("print", args);
		Test(value, script);
		Assert.AreEqual(value, lastPrint, "Different print: <{0}>", script);
	}
	public void TestFormat2(string value, string value2, params object[] args)
	{
		var script = Build("string", args);
		Test(value, script);
		script = Build("string.format", args);
		Test(value2, script);

		script = Build("print", args);
		Test(value, script);
		Assert.AreEqual(value, lastPrint, "Different print: <{0}>", script);
	}
	public void TestString<Ex>(params object[] args) where Ex : Exception
	{
		var script = Build("string", args);
		Expect<Ex>(script);
		script = Build("string.format", args);
		Expect<Ex>(script);
		script = Build("print", args);
		Expect<Ex>(script);
	}
	[Test]
	public void ROS_Stts08_Format()
	{
		// no argument
		Test("", "print");
		Assert.AreEqual("", lastPrint, "Different print: <{0}>", "print");
		Test("", "string");

		// basic
		TestString("", "");
		TestString("1", 1);
		TestString("1+2", "{0}+{1}", 1, 2);
		TestFormat2("1, 2", "1", 1, 2);
		TestString("{hello world}", "{{hello {0}}}", "world");
		TestFormat2("{{hello world}}", "{hello world}", "{{hello world}}");
		TestFormat2("{{hello}}, world", "{hello}", "{{hello}}", "world");

		// one arg, no formatting, no errors
		TestString("", "");
		TestString(" ", " ");
		TestFormat("{}", "{}");
		TestFormat("{0}", "{0}");
		TestFormat2("{{0}}", "{0}", "{{0}}");
		TestFormat("hello {0", "hello {0");

		// formatting
		TestString("a=1, b=true", "a={0}, b={1}", 1, true);
		TestString("x=10, y=20, z=30", "x={0}, y={1}, z={2}", 10, 20.0, 30f);
		TestString("<null>", "<{0}>", null);
		TestString("3.14159", "{0:F5}", Math.PI);
		TestFormat2("true, 3.14, null", "true", true, 3.14, null);
		TestString("¤42.00", "{0:C}", 42);
		TestString("123", "1{1}3", 1, 2, 3);
		TestString("66666", "6{0}6", 666);

		// errors
		TestString<FormatException>("hello {0", 42);
		TestString<FormatException>("hello {5}", 1,2,3);

		// escaping
		TestFormat2("hello {{0}}, 42", "hello {0}", "hello {{0}}", 42);
		TestString("{42}", "{{{0}}}", 42);
		TestFormat2("a{{b}}c, 42", "a{b}c", "a{{b}}c", 42);
	}
}
