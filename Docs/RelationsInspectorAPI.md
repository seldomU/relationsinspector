RelationsInspectorAPI
---------------------

``` csharp
void ResetTargets(object[] targets);
```
> Clears the current graph and creates a new one for the given targets.

``` csharp
void AddTargets(object[] targets);
```
> Clears the current graph and creates a new one for the union of existing and added targets.

``` csharp
void SetBackend(Type backendType);
```
> Enforces selection of the given backend type.

``` csharp
void Repaint();
```
> Draws a fresh view of the graph.

#### Graph manipulation
``` csharp
void AddEntity(object entity, Vector2 position);
```
> Adds the entity to the graph. The position is in graph coordinates, not window coordinates. If unsure, pass Vector2.zero.

``` csharp
void RemoveEntity(object entity);
```
> Removes entity and all its relations from the graph.

``` csharp
void AddRelation(object sourceEntity, object targetEntity, object tag);
```
> Adds relation between the given entities to the graph.

``` csharp
void RemoveRelation(object sourceEntity, object targetEntity, object tag);
```
> Removes the specified relation from the graph. If multiple matching relations exist, only the first one found will be removed.

``` csharp
void InitRelation(object[] sourceEntity, object tag);
```
> Makes the UI initiate the creation of a new relation. The user then gets to pick the target entity, which will result in call to the backend's `CreateEntity`.