using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector.Backend.SocialNetwork
{
	public class SocialNetworkBackend : ScriptableObjectBackend<Person, Feeling>
	{
		public override IEnumerable<Relation<Person, Feeling>> GetRelations( Person person )
		{
			if ( person.acquaintances == null )
				yield break;

			foreach ( var acq in person.acquaintances )
				yield return new Relation<Person, Feeling>( person, acq.person, acq.feeling );
		}

		public override void CreateRelation( Person source, Person target )
		{
			if ( source.acquaintances == null )
				source.acquaintances = new List<Acquaintance>();

			var tag = Feeling.Indifference;
			Undo.RecordObject( source, "adding acquaintance" );
			source.acquaintances.Add( new Acquaintance() { person = target, feeling = tag } );
			EditorUtility.SetDirty( source );
			api.AddRelation( source, target, tag );
		}

		public override void DeleteRelation( Person source, Person target, Feeling tag )
		{
			var targetEntries = source.acquaintances.Where( acq => acq.person == target && acq.feeling == tag );

			if ( !targetEntries.Any() )
			{
				Debug.LogError( "RemoveRelation: source is not related to target" );
				return;
			}

			Undo.RecordObject( source, "removing acquaintance" );
			source.acquaintances.Remove( targetEntries.First() );
			EditorUtility.SetDirty( source );
			api.RemoveRelation( source, target, tag );
		}

		// map relation tag value to color
		public override Color GetRelationColor( Feeling feeling )
		{
			switch ( feeling )
			{
				case Feeling.Indifference:
					return Color.white;

				case Feeling.Love:
					return Color.red;

				case Feeling.Hate:
					return Color.green;

				case Feeling.ItsComplicated:
				default:
					return Color.magenta;
			}
		}
	}
}
