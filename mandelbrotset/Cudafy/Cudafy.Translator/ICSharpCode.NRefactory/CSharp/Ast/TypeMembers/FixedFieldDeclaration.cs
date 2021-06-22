﻿// 
// FixedFieldDeclaration.cs
//
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2011 Novell, Inc (http://www.novell.com)
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
using System;

namespace ICSharpCode.NRefactory.CSharp
{
	public class FixedFieldDeclaration : AttributedNode
	{
		public static readonly Role<FixedVariableInitializer> VariableRole = new Role<FixedVariableInitializer> ("FixedVariable");
		
		public override NodeType NodeType {
			get { return NodeType.Member; }
		}
		
		public CSharpTokenNode FixedToken {
			get { return GetChildByRole (Roles.Keyword); }
		}

		public AstType ReturnType {
			get { return GetChildByRole (Roles.Type); }
			set { SetChildByRole (Roles.Type, value); }
		}
		
		public AstNodeCollection<FixedVariableInitializer> Variables {
			get { return GetChildrenByRole (VariableRole); }
		}

		public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitFixedFieldDeclaration (this, data);
		}

		protected internal override bool DoMatch (AstNode other, PatternMatching.Match match)
		{
			var o = other as FixedFieldDeclaration;
			return o != null && this.MatchAttributesAndModifiers (o, match)
				&& this.ReturnType.DoMatch (o.ReturnType, match) && this.Variables.DoMatch (o.Variables, match);
		}
	}
}

