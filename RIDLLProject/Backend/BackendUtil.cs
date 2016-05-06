using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	internal static class BackendTypeUtil
	{
		// the assemblies in which we search for backends
		static readonly Assembly[] backendSearchAssemblies = new[]
		{
			TypeUtil.GetAssemblyByName("Assembly-CSharp-Editor-firstpass"),
			TypeUtil.GetAssemblyByName("Assembly-CSharp-Editor")
		};

		internal static readonly Dictionary<Type, Type> backendToDecorator = new Dictionary<Type, Type>
		{
			{ typeof(IGraphBackend<,>), typeof(BackendDecoratorV1<,>) },
			{ typeof(IGraphBackend2<,>), typeof(BackendDecoratorV2<,>) }
		};

		internal static readonly HashSet<Type> backEndInterfaces = backendToDecorator.Keys.ToHashSet();

		public static readonly Type[] backendTypes = backendSearchAssemblies
			.Where(asm=>asm != null )
			.SelectMany( asm => asm.GetTypes() )
			.Where( t => IsBackendType( t ) )
			.ToArray();

		static readonly Type openAutoBackendType = backendTypes
			.Where( t => t.Name.StartsWith( ProjectSettings.AutoBackendClassName ) )
			.SingleOrDefault();

		internal static Type GetBackendInterface( Type potentialBackendType )
		{
			return potentialBackendType
				.GetInterfaces()
				.Where( i => i.IsGenericType && backEndInterfaces.Contains( i.GetGenericTypeDefinition() ) )
				.SingleOrDefault();
		}

		// returns true if candidateType implements one of the backend interfaces
		// (the interface might have generic arguments)
		internal static bool IsBackendType( Type candidateType )
		{
			return GetBackendInterface( candidateType ) != null;
		}

		internal static bool IsOpenBackendType( Type backendType )
		{
			return GetGenericArguments( backendType )
				.Any( arg => arg.IsGenericParameter );
		}

		// returns the type parameters of IGraphBackend that backend uses
		// assumes that backend implements IGraphBackend
		internal static Type[] GetGenericArguments( Type backendType )
		{
			var backendInterface = GetBackendInterface( backendType );
			if ( backendInterface == null )
				throw new ArgumentException( backendType + " does not implement a backend interface" );

			return backendInterface.GetGenericArguments();
		}

		internal static IEnumerable<Type> CreateAutoBackendTypes( IEnumerable<Type> entityTypes )
		{
			if ( openAutoBackendType == null )
				return Enumerable.Empty<Type>();

			return entityTypes
				.Where( IsMarkedAsAutoBackend )
				.Select( type => openAutoBackendType.MakeGenericType( new[] { type } ) );
		}

		static bool IsMarkedAsAutoBackend( Type t )
		{
			return t
				.GetCustomAttributes( false )
				.Any( attr => attr.GetType().Name == ProjectSettings.AutoBackendAttributeName );
		}

		internal static bool? GetLayoutSavingChoice( Type backendType )
		{
			var attributes = backendType.GetCustomAttributes<SaveLayoutAttribute>( true );
			if ( !attributes.Any() )
				return null;
			return attributes.First().doSave;
		}

		internal static Type GetDecoratorInterface( Type backendType )
		{
			Type bInterface = GetBackendInterface( backendType );
			if ( bInterface == null )
				throw new ArgumentException( "Expected a backend type:" + backendType );

			Type decoratorInterface;
			if ( !backendToDecorator.TryGetValue( bInterface.GetGenericTypeDefinition(), out decoratorInterface ) )
				throw new ArgumentException( "No decorator found for backend interface " + bInterface );

			return decoratorInterface;
		}

		internal static object CreateBackendDecorator( Type backendType )
		{
			Type decoratorType = GetDecoratorInterface( backendType ).MakeGenericType( GetGenericArguments( backendType ) );
			var ctorArgs = new object[] { Activator.CreateInstance( backendType ) };
			return Activator.CreateInstance( decoratorType, ctorArgs );
		}


		internal static Type GetMostSpecificBackendType( IList<Type> backendTypes )
		{
			if ( backendTypes == null || backendTypes.Count() == 0 )
				return null;

			// prefer auto-backends
			var autoBackendTypes = backendTypes
				.Where( t => t.IsGenericType && t.GetGenericTypeDefinition() == openAutoBackendType );

			if ( autoBackendTypes.Any() )
				return autoBackendTypes.First();

			var groups = backendTypes.GroupBy( backend => GetGenericArguments( backend ).First() );
			var entityTypes = groups.Select( group => group.Key ).ToHashSet();

			var bestEntityType = TypeUtil.GetMostSpecificType( entityTypes );
			var bestEntityTypeGroup = groups.Single( group => group.Key == bestEntityType );
			return bestEntityTypeGroup.First();
		}

		internal static Type GetEntityType( Type backendType )
		{
			return GetGenericArguments( backendType ).First();
		}

		internal static bool IsEntityTypeAssignableFromAny( Type backendType, IEnumerable<Type> types )
		{
			var entityType = GetEntityType( backendType );
			return entityType != null && types.Where( t => entityType.IsAssignableFrom( t ) ).Any();
		}

		// true if any of the given types can be passed as a RI attribute type
		internal static bool BackendAttributeFitsAny( Type backendType, IEnumerable<Type> types )
		{
			var attributes = backendType.GetCustomAttributes<AcceptTargetsAttribute>( true );
			return attributes.Any( attr => types.Any( t => attr.type.IsAssignableFrom( t ) ) );
		}

		internal static Type BackendAttrType( Type backendType )
		{
			var attr = backendType.GetCustomAttributes<AcceptTargetsAttribute>( true ).FirstOrDefault();
			return ( attr == null ) ? null : attr.type;
		}

		internal static string GetTitle( Type backendType )
		{
			string title = GetAttributeTitle( backendType );
			if ( string.IsNullOrEmpty( title ) )
				title = TypeName( backendType );
			return title;
		}

		internal static string TypeName( Type t )
		{
			if ( !t.IsGenericType )
				return t.Name;
			return t.Name.Remove( t.Name.IndexOf( '`' ) ) + " of " + t.GetGenericArguments()[ 0 ].Name;
		}

		internal static string GetAttributeTitle( Type backendType )
		{
			var attr = backendType.GetCustomAttributes<TitleAttribute>( false ).FirstOrDefault();
			return (attr == null) ? null : attr.title;
		}

		internal static string GetDocumentationURL( Type backendType )
		{
			var attr = backendType.GetCustomAttributes<DocumentationAttribute>( false ).FirstOrDefault();
			return ( attr == null ) ? null : attr.url;
		}

		internal static string GetVersion( Type backendType )
		{
			var attr = backendType.GetCustomAttributes<VersionAttribute>( false ).FirstOrDefault();
			return ( attr == null ) ? null : attr.version;
		}

		internal static string GetDescription( Type backendType )
		{
			var attr = backendType.GetCustomAttributes<DescriptionAttribute>( false ).FirstOrDefault();
			return ( attr == null ) ? null : attr.description;
		}

		internal static string GetIconPath( Type backendType )
		{
			var attr = backendType.GetCustomAttributes<IconAttribute>( false ).FirstOrDefault();
			return attr == null ? null : attr.iconPath;
		}

		internal static bool DoHide( Type backendType )
		{
			return backendType.GetCustomAttributes<HideAttribute>( false ).Any();
		}
	}
}
