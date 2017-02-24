# MonoGame-SceneGraph
Nodes, Culling &amp; Entities for basic Scene Graphs in MonoGame.

## What is it
This lib implements basic Nodes, Transformations, and frustum-based culling required to implement a Scene Graphs in MonoGame.

### But what's a scene graph?

From [Wikipedia](https://en.wikipedia.org/wiki/Scene_graph):

A scene graph is a collection of nodes in a graph or tree structure. A tree node (in the overall tree structure of the scene graph) may have many children but often only a single parent, with the effect of a parent applied to all its child nodes; an operation performed on a group automatically propagates its effect to all of its members. In many programs, associating a geometrical transformation matrix (see also transformation and matrix) at each group level and concatenating such matrices together is an efficient and natural way to process such operations. A common feature, for instance, is the ability to group related shapes/objects into a compound object that can then be moved, transformed, selected, etc. as easily as a single object.

Scene graphs are useful for modern games using 3D graphics and increasingly large worlds or levels. In such applications, nodes in a scene graph (generally) represent entities or objects in the scene.
For instance, a game might define a logical relationship between a knight and a horse so that the knight is considered an extension to the horse. The scene graph would have a 'horse' node with a 'knight' node attached to it.
As well as describing the logical relationship, the scene graph may also describe the spatial relationship of the various entities: the knight moves through 3D space as the horse moves.

## Live example
To see a live example, open and execute the solution in this repo (make sure to build as ```Application``` and not ```Class Library```).

## Using MonoGame-SceneGraph

### Install

To install the lib you can use NuGet:

```
Install-Package MonoGame.SceneGraph
```

Or instead you can manually copy the source files from ```MonoGameSceneGraph/Source/``` into your project.

### Main objects
This lib contains 4 main classes you should know:

#### Node
A basic scene node with transformation. To build your scene create nodes and child-nodes and set their transformations.

Note: a Node does not have a graphic representation, eg it does not render anything. A node only handles the hierarchy and transformation and hold entities that render stuff.

#### Entity
An entity is a renderable object you attach to nodes. For example, the most basic Entity is a ModelEntity, which renders a 3D model loaded by the content manager.

Remember: Entities handle rendering, Nodes handle transformations.

#### ModelEntity
A basic Entity that renders a 3D model. 

Note: This entity uses the default MG effect, so you'll most likely want to impelement your own Model Entity to draw models with your own custom effects and camera. This class is just an example / reference.

#### Transformations
A set of transformations that a Node can have + helper functions to build a matrix.
Normally you don't need to use this class, it is used internally by the Nodes.

### How to use

As mentioned before, the most basic component of the scene graph is the Node.
To create a new node:

```cs
MonoGameSceneGraph.Node node = new MonoGameSceneGraph.Node();
```

Now that we have a node we can apply different transformations on it. For example:

```cs
// set node position
node.Position = new Vector3(10, 0, 0);

// rotate node on X axis
node.RotationX = 0.2f;

// scale node
node.Scale = Vector3.One * 2f;

// etc etc. see Node api for more options..
```

Or, we can start adding child nodes to it in order to build our scene:

```cs
MonoGameSceneGraph.Node childNode = new MonoGameSceneGraph.Node();
node.AddChildNode(childNode);
```

Now the transformations of the parent node will also apply the child node. But on top of that, the child node can have its own local transformations:

```cs
// set local position to (0, 10, 0).
// note: since our parent position is (10, 0, 0), our final position would be (10, 10, 0).
childNode.Position = new Vector3(0, 10, 0);
```

But what about actual rendering? As mentioned before nodes don't really handle rendering, they only handle transformations. To start drawing stuff we need to add entities to them, like the built-in ModelEntity:

```cs
// create a basic ModelEntity with a 'robot' model, and add to our child node from before.
MonoGameSceneGraph.entity = new MonoGameSceneGraph.ModelEntity(Content.Load<Model>("robot"));
childNode.AddEntity(entity);
```

And now that we have a scene with an entity, we can draw our scene:

```cs
// this should go inside the game Draw() function, after clearing the device buffers.
node.Draw();
```

If you used to code above properly (and have a robot model) you should now be able to see it on the screen.
If it doesn't work check out the sample code in ```Game1.cs```.

#### Creating Your Entities

The built-in ```ModelEntity``` is designed to be a reference and not really used in projects. It only renders static models with built-in lighting and constant camera. Clearly you need more than that.

In order to make your own renderable entities, inherit from the ```IEntity``` interface, and implement its ```Draw()``` function:

```cs
/// <summary>
/// Custom entity class for your game.
/// </summary>
public class MyEntityType : IEntity
{
	/// <summary>
	/// Draw this model.
	/// </summary>
	/// <param name="parent">Parent node that's currently drawing this entity.</param>
	/// <param name="localTransformations">Local transformations from the direct parent node.</param>
	/// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
	public void Draw(Node parent, Matrix localTransformations, Matrix worldTransformations)
	{
		// your drawing comes here..
	}
}
```

As you can see from the code above, when an entity is drawn you get 3 paramets: the parent node that's currently drawing it (an entity can be attached to multiple nodes), parent node's local transformations, and node's final world transformations, as calculated by the graph.

Using these params you should be able to draw everything you need. The rendering logic and type of entities is up to you.

#### Caching

Calculating transformations every frame can be heavy on CPU.

Because of that, MonoGame-SceneGraph Nodes only recalculate transformations when something changes (or when their parent node was changed).
This means that when using Nodes you should avoid changing stuff for the same value. For example, consider the following code:

```cs
// 'player' is a Node holding the player's graphics. 
// 'currSpeed' is a vector that's either 0 when standing still, or contain the value of the current player movement direction.
// `Translate()` is just a sugarcoat for `node.Position = node.Position + vector`.
player.Translate(currSpeed)
```

As you can see above, currSpeed can either be zero, or a movement vector when player is moving. However, because we set ```player.Position``` every frame, regardless of the value of ```currSpeed```, this means the ```player``` node will recalculate its matrix every single frame.

To avoid this unnecessary overhead, the code can be simply modified into this:

```cs
// 'player' is a Node holding the player's graphics. 
// 'currSpeed' is a vector that's either 0 when standing still, or contain the value of the current player movement direction.
if (currSpeed.Length() > 0)
{
	player.Translate(currSpeed)
}
```

And now the ```player``` node will only calculate its matrix when really needed.

### Culling

Culling objects that are outside the screen is an important optimization most games need.
MonoGame-SceneGraph comes with a built-in CullingNode that implements simple culling based on camera frustum and bounding boxes.

It works as follows:
1. Every time the camera changes, the static member CameraFrustum must be updated. The Culling Node will use this frustum to detect weather or not its in screen.
2. Whenever the transformations of this node update, it will also calculate the Bounding Box of itself with all its child nodes and entities.
3. When the node is drawn, it will first test collision between its bounding box and the camera frustum. If its found to be out of screen, the node will not draw itself or its children.

Use ```CullingNodes``` is just like using regular nodes, eg:

```cs
// create a culling node
MonoGameSceneGraph.CullingNode cullNode = new MonoGameSceneGraph.CullingNode();
```

But in addition to creating and drawing them, you also need to set the camera bounding frustum whenever it updates:

```cs
// set the camera frustum for all culling nodes (its a static member)
MonoGameSceneGraph.CullingNode.CameraFrustum = cameraFrustum;
```

Note that you can mix CullingNodes with regular Nodes, but make sure the plain nodes are only in edges and not between CullingNodes.

#### Using CullingNodes Properly
As mentioned above, a CullingNode bounding box is the combination of its own bounding box + all its children nodes and entities.
This means that the bounding box of a parent CullingNode contains in it the bounding boxes of all its child nodes, and their children, and their children, and so forth..

So its quite obvious that if you put a single CullingNode with lots of children scattered around the level the result would be a huge bounding box that will most likely always be in screen, and lots of small bounding boxes (per child) that needs to be checked every frame. This is far from optimal.

In order to fully enjoy the benefits of CullingNodes you need to build your scene properly, eg divide it into smaller chunks and distribute child nodes by regions.
The easiest example to explain this is a tilemap, made out of a grid of tiles; Instead of having one Node with *all* the tile nodes as its direct children, you should break the tilemap into smaller chunks (or smaller grids) so culling will occur on the chunk level before testing per-tile.


### Further Reading

In this doc we didn't cover much of the API, only the very basics needed to get you going. To learn more, please see the doc file in ```Help/Documentation.chm```, or check out the code documentation (mostly ```Node.cs```).

## Lisence
MonoGame-SceneGraph is distributed with the permissive MIT License. For more info, check out the ```LICENSE``` file in this repo.

## Changelog

### 1.0.0.1

- Added ```Find()``` to Nodes.
- Made some functions virtual to make Node more easily extendable.
- Fixed a bug when parent of a parent change transformations but middle parent don't.

### 1.1.0.0

- Improved general API and made Nodes and Entities more flexible and easier to extend.
- Added mechanism to calculate and cache Nodes Bounding Box.
- Added Culling Nodes to automatically cull by Bounding Box.
- Note: this version changed some public member names and added members to Entity API - might require changes on your code to adjust.

### 1.1.0.1

- Changed default build target to AnyCPU. No actual changes in code.