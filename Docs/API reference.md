RelationsInspectorAPI
=====================

```
void AddEntity(object entity, Vector2 position);
```
> Adds the entity to the graph. The position is in graph coordinates, not window coordinates. If unsure, pass Vector2.zero.

```
void AddRelation(object sourceEntity, object targetEntity, object tag);
```
> Adds relation between the given entities to the graph.

```
void ResetTargets(object[] targets);
```
> Clears the current graph and creates a new one for the given targets.

```
void AddTargets(object[] targets);
```
> Clears the current graph and creates a new one for the union of existing and added targets.

```
void ClearWindow();
```
> Wipes the graph.

```
void InitRelation(object[] sourceEntity, object tag);
```
> Makes the UI initiate the creation of a new relation. The user then gets to pick the target entity, which will result in call to the backend's `CreateEntity`.

```
void RemoveEntity(object entity);
```
> Removes entity and all its relations from the graph.

```
void RemoveRelation(object sourceEntity, object targetEntity, object tag);
```
> Removes the specified relation from the graph. If multiple matching relations exist, only the first one found will be removed.

```
void Repaint();
```
> Draws a fresh view of the graph.

```
void SetBackend(Type backendType);
```
> Enforces selection of the given backend type.

IGraphBackend
=============

Backend interface. The two generic parameters are the graph entity type T and relation type P.

```
void CreateEntity(Vector2 position);
```
> UI wants to create a new entity at the given graph position.

```
void CreateRelation(T source, T target, P tag);
```
> UI wants to create a new relation with the given properties.

```
Rect DrawContent(T entity, EntityDrawContext drawContext);
```
> UI needs a rect, visually representing the given entity.

```
Color GetRelationColor(P relationTagValue);
```
> UI needs the color in which to paint the relation marker.

```
IEnumerable<Tuple<T, P>> GetRelations(T entity);
```
> Graph construction needs the tags and targets of all relations in which the entity is the subject.

```
string GetTooltip(T entity);
```
> UI needs a string to use as entity widget tooltip.

```
IEnumerable<T> Init(IEnumerable<object> targets, RelationsInspectorAPI api);
```
> First call the backend gets. Returned entities are used as roots of graph contstruction. API parameter is for any graph manipulation the backend may want to make, like adding/removing entities or relations.

```
void OnEntityContextClick(IEnumerable<T> entities);
```
> UI got a context click on the given entities. Backend may want to show a menu.

```
void OnEntitySelectionChange(T[] selection);
```
> UI selection changed.

```
Rect OnGUI();
```
> UI is being drawn. Backend gets a chance to draw its own controls. Returns the remaining space as rect. The graph is then drawn in that rect.

```
void OnRelationContextClick(T source, T target, P tag);
```
> UI got a context click on the given relation's marker. Backend may wnat to show a menu.