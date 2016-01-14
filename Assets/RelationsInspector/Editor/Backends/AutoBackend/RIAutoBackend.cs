using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RelationsInspector.Backend.AutoBackend
{
	public class RIAutoBackend<T> : MinimalBackend<T, string> where T : class
	{
		IEnumerable<FieldInfo> relatedFields;
		IEnumerable<FieldInfo> relatingFields;

		public override void Awake( GetAPI getAPI )
		{
			relatingFields = ReflectionUtil.GetAttributeFields<T, RelatingAttribute>();
			relatedFields = ReflectionUtil.GetAttributeFields<T, RelatedAttribute>();
			if ( !relatingFields.Any() && !relatedFields.Any() )
				Debug.LogError( "Type has auto backend attribute, but no fields marked as related or relating: " + typeof( T ) );
			base.Awake( getAPI );
		}

		public override IEnumerable<Relation<T, string>> GetRelations( T entity )
		{
			var outRelations = relatedFields
				.SelectMany( fInfo => ReflectionUtil.GetValues<T>( fInfo, entity ) )
				.Select( other => new Relation<T, string>( entity, other, string.Empty ) );

			var inRelations = relatingFields
				.SelectMany( fInfo => ReflectionUtil.GetValues<T>( fInfo, entity ) )
				.Select( other => new Relation<T, string>( other, entity, string.Empty ) );

			return outRelations.Concat( inRelations );
		}
	}
}
