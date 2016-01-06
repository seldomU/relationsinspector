using System.Collections.Generic;
using UnityEngine;

namespace RelationsInspector
{
	public interface IRelationDrawer<T, P> where T : class
	{
		Dictionary<Relation<T, P>, Rect> DrawRelation( IEnumerable<Relation<T, P>> toEdges, IEnumerable<Relation<T, P>> fromEdges, EdgePlacement placement, bool isSelfEdge, bool highLight, bool includeMarker, RelationDrawerStyle style, System.Func<P, Color> GetMarkerColor );

		void DrawPseudoRelation( EdgePlacement placement, bool isSelfEdge, RelationDrawerStyle relationDrawerStyle );
	}
}
