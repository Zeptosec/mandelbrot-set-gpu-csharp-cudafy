// 
// FullTypeName.cs
//
// Author:
//       Mike Krüger <mkrueger@novell.com>
// 
// Copyright (c) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Generic;
using System.Linq;

namespace ICSharpCode.NRefactory.CSharp
{
	public class PrimitiveType : AstType
	{
		public string Keyword { get; set; }
		public AstLocation Location { get; set; }
        public String OriginalType { get; set; }
		public PrimitiveType()
		{
            OriginalType = string.Empty;
		}
		
		public PrimitiveType(string keyword)
		{
			this.Keyword = keyword;
		}
		
		public PrimitiveType(string keyword, AstLocation location)
		{
			this.Keyword = keyword;
			this.Location = location;
		}
		
		public override AstLocation StartLocation {
			get {
				return Location;
			}
		}
		public override AstLocation EndLocation {
			get {
				return new AstLocation (Location.Line, Location.Column + (Keyword != null ? Keyword.Length : 0));
			}
		}
		
		public override S AcceptVisitor<T, S> (IAstVisitor<T, S> visitor, T data)
		{
			return visitor.VisitPrimitiveType (this, data);
		}
		
		protected internal override bool DoMatch(AstNode other, PatternMatching.Match match)
		{
			PrimitiveType o = other as PrimitiveType;
			return o != null && MatchString(this.Keyword, o.Keyword);
		}
		
		public override string ToString()
		{
			return Keyword ?? base.ToString();
		}
	}
}

