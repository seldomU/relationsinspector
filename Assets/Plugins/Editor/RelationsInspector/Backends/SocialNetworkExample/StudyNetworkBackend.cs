using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector.Backend.SocialNetwork
{
	public class StudyNetworkBackend : ScriptableObjectBackend<Person, string>
	{
		public override IEnumerable<Relation<Person, string>> GetRelations( Person person )
		{
			if ( person.studyPartners == null )
				yield break;

			foreach ( var partner in person.studyPartners )
				yield return new Relation<Person, string>( person, partner, string.Empty );
		}

		public override void CreateRelation( Person source, Person target )
		{
			if ( source.studyPartners == null )
				source.studyPartners = new List<Person>();

			Undo.RecordObject( source, "adding study partner" );
			source.studyPartners.Add( target );
			EditorUtility.SetDirty( source );
			api.AddRelation( source, target, string.Empty );
		}

		public override void DeleteRelation( Person source, Person target, string tag )
		{
			var targetEntries = source.studyPartners.Where( partner => target == partner );

			if ( !targetEntries.Any() )
			{
				Debug.LogError( "RemoveRelation: source is not related to target" );
				return;
			}

			Undo.RecordObject( source, "removing study partner" );
			source.studyPartners.Remove( targetEntries.First() );
			EditorUtility.SetDirty( source );
			api.RemoveRelation( source, target, tag );
		}
	}
}
