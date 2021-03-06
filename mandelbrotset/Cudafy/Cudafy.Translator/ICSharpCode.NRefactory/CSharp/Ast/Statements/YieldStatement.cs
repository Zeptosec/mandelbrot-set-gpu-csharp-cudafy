// 
// YieldStatement.cs
//
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2009 Novell, Inc (http://www.novell.com)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

namespace ICSharpCode.NRefactory.CSharp
{
	/// <summary>
	/// yield return Expression;
	/// </summary>
	public class YieldStatement : Statement
	{
		public static readonly Role<CSharpTokenNode> YieldKeywordRole = new Role<CSharpTokenNode>("YieldKeyword", CSharpTokenNode.Null);
		public static readonly Role<CSharpTokenNode> ReturnKeywordRole = new Role<CSharpTokenNode>("ReturnKeyword", CSharpTokenNode.Null);
		
		public CSharpTokenNode YieldToken {
			get { return GetChildByRole (YieldKeywordRole); }
		}
		
		public CSharpTokenNode ReturnToken {
			get { return GetChildByRole (ReturnKeywordRole); }
		}
		
		public Expression Expression {
			get { return GetChildByRole (Roles.Expression); }
			set { SetChildByRole (Roles.Expression, value); }
		}
		
		public CSharpTokenNode SemicolonToken {
			get { return GetChildByRole (Roles.Semicolon); }
		}
		
		public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitYieldStatement (this, data);
		}
		
		protected internal override bool DoMatch(AstNode other, PatternMatching.Match match)
		{
			YieldStatement o = other as YieldStatement;
			return o != null && this.Expression.DoMatch(o.Expression, match);
		}
	}
}
