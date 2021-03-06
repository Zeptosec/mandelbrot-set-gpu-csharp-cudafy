// Copyright (c) 2011 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Linq;
using ICSharpCode.NRefactory.CSharp;
using ICSharpCode.NRefactory.PatternMatching;
using Mono.Cecil;

namespace ICSharpCode.Decompiler.Ast.Transforms
{
	/// <summary>
	/// If the first element of a constructor is a chained constructor call, convert it into a constructor initializer.
	/// </summary>
	public class ConvertConstructorCallIntoInitializer : DepthFirstAstVisitor<object, object>, IAstTransform
	{
		public override object VisitConstructorDeclaration(ConstructorDeclaration constructorDeclaration, object data)
		{
			ExpressionStatement stmt = constructorDeclaration.Body.Statements.FirstOrDefault() as ExpressionStatement;
			if (stmt == null)
				return null;
			InvocationExpression invocation = stmt.Expression as InvocationExpression;
			if (invocation == null)
				return null;
			MemberReferenceExpression mre = invocation.Target as MemberReferenceExpression;
			if (mre != null && mre.MemberName == ".ctor") {
				ConstructorInitializer ci = new ConstructorInitializer();
				if (mre.Target is ThisReferenceExpression)
					ci.ConstructorInitializerType = ConstructorInitializerType.This;
				else if (mre.Target is BaseReferenceExpression)
					ci.ConstructorInitializerType = ConstructorInitializerType.Base;
				else
					return null;
				// Move arguments from invocation to initializer:
				invocation.Arguments.MoveTo(ci.Arguments);
				// Add the initializer: (unless it is the default 'base()')
				if (!(ci.ConstructorInitializerType == ConstructorInitializerType.Base && ci.Arguments.Count == 0))
					constructorDeclaration.Initializer = ci.WithAnnotation(invocation.Annotation<MethodReference>());
				// Remove the statement:
				stmt.Remove();
			}
			return null;
		}
		
		static readonly ExpressionStatement fieldInitializerPattern = new ExpressionStatement {
			Expression = new AssignmentExpression {
				Left = new NamedNode("fieldAccess", new MemberReferenceExpression { Target = new ThisReferenceExpression() }),
				Operator = AssignmentOperatorType.Assign,
				Right = new AnyNode("initializer")
			}
		};
		
		static readonly AstNode thisCallPattern = new ExpressionStatement(new ThisReferenceExpression().Invoke(".ctor", new Repeat(new AnyNode())));
		
		public override object VisitTypeDeclaration(TypeDeclaration typeDeclaration, object data)
		{
			var instanceCtors = typeDeclaration.Members.OfType<ConstructorDeclaration>().Where(c => (c.Modifiers & Modifiers.Static) == 0).ToArray();
			var instanceCtorsNotChainingWithThis = instanceCtors.Where(ctor => !thisCallPattern.IsMatch(ctor.Body.Statements.FirstOrDefault())).ToArray();
			if (instanceCtorsNotChainingWithThis.Length > 0 && typeDeclaration.ClassType == NRefactory.TypeSystem.ClassType.Class) {
				// Recognize field initializers:
				// Convert first statement in all ctors (if all ctors have the same statement) into a field initializer.
				bool allSame;
				do {
					Match m = fieldInitializerPattern.Match(instanceCtorsNotChainingWithThis[0].Body.FirstOrDefault());
					if (!m.Success)
						break;
					
					FieldDefinition fieldDef = m.Get<AstNode>("fieldAccess").Single().Annotation<FieldReference>().ResolveWithinSameModule();
					if (fieldDef == null)
						break;
					AttributedNode fieldOrEventDecl = typeDeclaration.Members.FirstOrDefault(f => f.Annotation<FieldDefinition>() == fieldDef);
					if (fieldOrEventDecl == null)
						break;
					
					allSame = true;
					for (int i = 1; i < instanceCtorsNotChainingWithThis.Length; i++) {
						if (!instanceCtors[0].Body.First().IsMatch(instanceCtorsNotChainingWithThis[i].Body.FirstOrDefault()))
							allSame = false;
					}
					if (allSame) {
						foreach (var ctor in instanceCtorsNotChainingWithThis)
							ctor.Body.First().Remove();
						fieldOrEventDecl.GetChildrenByRole(AstNode.Roles.Variable).Single().Initializer = m.Get<Expression>("initializer").Single().Detach();
					}
				} while (allSame);
			}
			
			// Now convert base constructor calls to initializers:
			base.VisitTypeDeclaration(typeDeclaration, data);
			
			// Remove single empty constructor:
			if (instanceCtors.Length == 1) {
				ConstructorDeclaration emptyCtor = new ConstructorDeclaration();
				emptyCtor.Modifiers = ((typeDeclaration.Modifiers & Modifiers.Abstract) == Modifiers.Abstract ? Modifiers.Protected : Modifiers.Public);
				emptyCtor.Body = new BlockStatement();
				if (emptyCtor.IsMatch(instanceCtors[0]))
					instanceCtors[0].Remove();
			}
			
			// Convert static constructor into field initializers if the class is BeforeFieldInit
			var staticCtor = typeDeclaration.Members.OfType<ConstructorDeclaration>().FirstOrDefault(c => (c.Modifiers & Modifiers.Static) == Modifiers.Static);
			if (staticCtor != null) {
				TypeDefinition typeDef = typeDeclaration.Annotation<TypeDefinition>();
				if (typeDef != null && typeDef.IsBeforeFieldInit) {
					while (true) {
						ExpressionStatement es = staticCtor.Body.Statements.FirstOrDefault() as ExpressionStatement;
						if (es == null)
							break;
						AssignmentExpression assignment = es.Expression as AssignmentExpression;
						if (assignment == null || assignment.Operator != AssignmentOperatorType.Assign)
							break;
						FieldDefinition fieldDef = assignment.Left.Annotation<FieldReference>().ResolveWithinSameModule();
						if (fieldDef == null || !fieldDef.IsStatic)
							break;
						FieldDeclaration fieldDecl = typeDeclaration.Members.OfType<FieldDeclaration>().FirstOrDefault(f => f.Annotation<FieldDefinition>() == fieldDef);
						if (fieldDecl == null)
							break;
						fieldDecl.Variables.Single().Initializer = assignment.Right.Detach();
						es.Remove();
					}
					if (staticCtor.Body.Statements.Count == 0)
						staticCtor.Remove();
				}
			}
			return null;
		}
		
		void IAstTransform.Run(AstNode node)
		{
			node.AcceptVisitor(this, null);
		}
	}
}
