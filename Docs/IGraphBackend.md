IGraphBackend members
-------------

Backend interface. The two generic parameters are the graph entity type T and relation type P.

``` csharp
IEnumerable<T> Init(IEnumerable<object> targets, RelationsInspectorAPI api);
```
> First call the backend gets. Returned entities are used as roots of graph contstruction. API parameter is for any graph manipulation the backend may want to make, like adding/removing entities or relations.

``` csharp
IEnumerable<Tuple<T, P>> GetRelations(T entity);
```
> Graph construction needs the tags and targets of all relations in which the entity is the subject.

#### Graph modification

``` csharp
void CreateEntity(Vector2 position);
```
> UI wants to create a new entity at the given graph position.

``` csharp
void CreateRelation(T source, T target, P tag);
```
> UI wants to create a new relation with the given properties.

#### Content drawing

``` csharp
Rect DrawContent(T entity, EntityDrawContext drawContext);
```
> UI needs a rect, visually representing the given entity.

``` csharp
Color GetRelationColor(P relationTagValue);
```
> UI needs the color in which to paint the relation marker.

``` csharp
string GetTooltip(T entity);
```
> UI needs a string to use as entity widget tooltip.

``` csharp
Rect OnGUI();
```
> UI is being drawn. Backend gets a chance to draw its own controls. Returns the remaining space as rect. The graph is then drawn in that rect.

#### Other events

``` csharp
void OnEntityContextClick(IEnumerable<T> entities);
```
> UI got a context click on the given entities. Backend may want to show a menu.

``` csharp
void OnEntitySelectionChange(T[] selection);
```
> UI selection changed.

``` csharp
void OnRelationContextClick(T source, T target, P tag);
```
> UI got a context click on the given relation's marker. Backend may wnat to show a menu.