﻿
namespace MunSharp.Interpreter.Tree
{
	interface IVariable
	{
		void CompileAssignment(Execution.VM.ByteCode bc, int stackofs, int tupleidx);
	}
}
