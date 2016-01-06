using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector.Extensions
{
	public static class LinqExtensions
	{
		public static HashSet<T> ToHashSet<T>( this IEnumerable<T> sequence )
		{
			if ( sequence == null )
				throw new System.ArgumentException( "sequence" );

			return new HashSet<T>( sequence );
		}

		public static void Enqueue<T>( this Queue<T> queue, IEnumerable<T> items )
		{
			if ( queue == null )
				throw new System.ArgumentException( "queue" );
			if ( items == null )
				throw new System.ArgumentException( "items" );

			foreach ( var item in items )
				queue.Enqueue( item );
		}

		public static string ToDelimitedString<T>( this IEnumerable<T> sequence )
		{
			if ( sequence == null )
				return "null";

			var sb = new System.Text.StringBuilder();

			sb.Append( "(" );
			//sequence.Aggregate(sb, (builder, item) => { builder.Append(", " + item.ToString()); return builder; } );

			bool firstvalue = true;
			foreach ( var value in sequence )
			{
				if ( !firstvalue )
					sb.Append( ", " );

				firstvalue = false;
				sb.Append( value );
			}
			sb.Append( ")" );

			return sb.ToString();
		}

		public static void UnionWith<T, P>( this Dictionary<T, P> dict, Dictionary<T, P> other )
		{
			if ( dict == null )
				throw new System.ArgumentNullException( "dict" );

			if ( other == null )
				throw new System.ArgumentNullException( "other" );

			foreach ( var pair in other )
			{
				if ( dict.ContainsKey( pair.Key ) )
					throw new System.ArgumentException( "dict keys are not disjunct" );

				dict[ pair.Key ] = pair.Value;
			}
		}

		public static void RemoveWhere<T>( this ICollection<T> collection, System.Func<T, bool> condition )
		{
			List<T> toRemove = collection.Where( condition ).ToList();

			foreach ( T obj in toRemove )
			{
				collection.Remove( obj );
			}
		}

		// avoid naming conflict with linq's IEnumerable.Reverse (which is slower)
		public static IEnumerable<T> FastReverse<T>( this LinkedList<T> list )
		{
			var el = list.Last;
			while ( el != null )
			{
				yield return el.Value;
				el = el.Previous;
			}
		}
	}
}
