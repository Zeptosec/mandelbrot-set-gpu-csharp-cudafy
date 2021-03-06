// Copyright (c) 2010 AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace ICSharpCode.NRefactory.TypeSystem
{
	#if WITH_CONTRACTS
	[ContractClass(typeof(IEntityContract))]
	#endif
	public interface IEntity : INamedElement, IFreezable
	{
		EntityType EntityType { get; }
		
		/// <summary>
		/// Gets the complete entity region (including header+body)
		/// </summary>
		DomRegion Region { get; }
		
		/// <summary>
		/// Gets the entity body region.
		/// </summary>
		DomRegion BodyRegion { get; }
		
		/// <summary>
		/// Gets the declaring class.
		/// For members, this is the class that contains the member.
		/// For nested classes, this is the outer class. For top-level entities, this property returns null.
		/// </summary>
		ITypeDefinition DeclaringTypeDefinition { get; }
		
		IList<IAttribute> Attributes { get; }
		
		string Documentation { get; }
		
		/// <summary>
		/// Gets the accessibility of this entity.
		/// </summary>
		Accessibility Accessibility { get; }
		
		/// <summary>
		/// Gets whether this entity is static.
		/// Returns true if either the 'static' or the 'const' modifier is set.
		/// </summary>
		bool IsStatic { get; }
		
		/// <summary>
		/// Returns whether this entity is abstract.
		/// </summary>
		/// <remarks>Static classes also count as abstract classes.</remarks>
		bool IsAbstract { get; }
		
		/// <summary>
		/// Returns whether this entity is sealed.
		/// </summary>
		/// <remarks>Static classes also count as sealed classes.</remarks>
		bool IsSealed { get; }
		
		/// <summary>
		/// Gets whether this member is declared to be shadowing another member with the same name.
		/// </summary>
		bool IsShadowing { get; }
		
		/// <summary>
		/// Gets whether this member is generated by a macro/compiler feature.
		/// </summary>
		bool IsSynthetic { get; }
		
		/// <summary>
		/// The assembly in which this entity is defined.
		/// This property never returns null.
		/// </summary>
		IProjectContent ProjectContent { get; }
		
		//bool IsAccessible(IClass callingClass, bool isAccessThoughReferenceOfCurrentClass);
	}
	
	#if WITH_CONTRACTS
	[ContractClassFor(typeof(IEntity))]
	abstract class IEntityContract : INamedElementContract, IEntity
	{
		EntityType IEntity.EntityType {
			get { return default(EntityType); }
		}
		
		DomRegion IEntity.Region {
			get { return DomRegion.Empty; }
		}
		
		DomRegion IEntity.BodyRegion {
			get { return DomRegion.Empty; }
		}
		
		ITypeDefinition IEntity.DeclaringTypeDefinition {
			get { return null; }
		}
		
		IList<IAttribute> IEntity.Attributes {
			get {
				Contract.Ensures(Contract.Result<IList<IAttribute>>() != null);
				return null;
			}
		}
		
		string IEntity.Documentation {
			get { return null; }
		}
		
		bool IEntity.IsStatic {
			get { return false; }
		}
		
		Accessibility IEntity.Accessibility {
			get { return default(Accessibility); }
		}
		
		bool IEntity.IsAbstract {
			get { return false; }
		}
		
		bool IEntity.IsSealed {
			get { return false; }
		}
		
		bool IEntity.IsShadowing {
			get { return false; }
		}
		
		bool IEntity.IsSynthetic {
			get { return false; }
		}
		
		IProjectContent IEntity.ProjectContent {
			get {
				Contract.Ensures(Contract.Result<IProjectContent>() != null);
				return null;
			}
		}
		
		bool IFreezable.IsFrozen {
			get { return false; }
		}
		
		void IFreezable.Freeze()
		{
		}
	}
	#endif
}
