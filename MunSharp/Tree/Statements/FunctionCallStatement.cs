﻿using System;
using MunSharp.Interpreter.Execution;
using MunSharp.Interpreter.Execution.VM;
using MunSharp.Interpreter.Tree.Expressions;

namespace MunSharp.Interpreter.Tree.Statements
{
	class FunctionCallStatement : Statement
	{
		FunctionCallExpression m_FunctionCallExpression;

		public FunctionCallStatement(ScriptLoadingContext lcontext, FunctionCallExpression functionCallExpression)
			: base(lcontext)
		{
			m_FunctionCallExpression = functionCallExpression;
			lcontext.Source.Refs.Add(m_FunctionCallExpression.SourceRef);
		}


		public override void Compile(ByteCode bc)
		{
			using (bc.EnterSource(m_FunctionCallExpression.SourceRef))
			{
				m_FunctionCallExpression.Compile(bc);
				RemoveBreakpointStop(bc.Emit_Pop());
			}
		}

		private void RemoveBreakpointStop(Instruction instruction)
		{
			instruction.SourceCodeRef = null;
		}
	}
}
