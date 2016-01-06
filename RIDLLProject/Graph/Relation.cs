
using System;

namespace RelationsInspector
{
	public class Relation<T, P> : IEquatable<Relation<T, P>> where T : class
	{
		public T Source { get; private set; }
		public T Target { get; private set; }
		public P Tag { get; private set; }
		public bool IsSelfRelation { get { return Source == Target; } }

		public Relation( T source, T target, P tag )
		{
			this.Source = source;
			this.Target = target;
			this.Tag = tag;
		}

		public T Opposite( T entity )
		{
			if ( entity == Source ) return Target;
			if ( entity == Target ) return Source;
			throw new System.ArgumentException( "entity is not part of the relation" );
		}

		public override int GetHashCode()
		{
			int hash = 13;
			hash = ( hash * 7 ) + Source.GetHashCode();
			hash = ( hash * 7 ) + Target.GetHashCode();
			hash = ( hash * 7 ) + Tag.GetHashCode();
			return hash;
		}

		public bool Equals( Relation<T, P> other )
		{
			return (
				other != null &&
				Source.Equals( other.Source ) &&
				Target.Equals( other.Target ) &&
				Tag.Equals( other.Tag ) );
		}

		public override bool Equals( object otherObj )
		{
			var otherRelation = otherObj as Relation<T, P>;
			return ( otherRelation == null ) ? false : Equals( otherRelation );
		}

		public bool Matches( T source, T target, P tag )
		{
			return Source == source && Target == target && ( Tag == null && tag == null || Tag != null && Tag.Equals( tag ) );
		}

		public Relation<T, P> Copy()
		{
			return new Relation<T, P>( Source, Target, Tag );
		}

		public override string ToString()
		{
			return string.Format( "source: {0}, target: {1}, tag: {2}, hash {3}", Source, Target, Tag, GetHashCode() );
		}
	}
}
