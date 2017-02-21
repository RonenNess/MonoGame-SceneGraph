#region File Description
//-----------------------------------------------------------------------------
// This type of node implements basic Bounding-Box based culling.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MonoGameSceneGraph
{
    /// <summary>
    /// A culling node is a special node that cull itself if out of screen, based on camera frustum and node bounding box.
    /// </summary>
    /// <remarks>
    /// It works as follows:
    ///     1. Every time the camera changes, the static member CameraFrustum must be updated. The Culling Node will use this frustum to detect weather or not its in screen.
    ///     2. Whenever the transformations of this node update, it will also calculate the Bounding Box of itself with all its child nodes and entities.
    ///     3. When the node is drawn, it will first test collision between its bounding box and the camera frustum. If its found to be out of screen, the node will not draw itself or its children.
    ///     
    /// Its important to note that using Culling Nodes properly require some non-trivial designing for the scene graph.
    ///
    /// For example, if you have a tilemap that is made out of 100x100 nodes grid (every node = single tile), and you put all the tile nodes under a single node (eg a node with 10,000 children), 
    /// the result would be that the parent node will have a huge bounding box that will most likely always be in screen, and then the Culling Nodes will have to test for 10,000 bounding boxes to detect
    /// exactly which tiles are in screen and which are not.. Not only that this won't really boost up performance, it might even slow your game down (depending on CPU/GPU strength).
    /// So in the example above, in order to really enjoy the culling, you should break the 100x100 grid into smaller grids and break those grids as well.
    /// For example, break the parent node into 4 nodes with 50x50 tiles each, and then break those nodes into 5x5 nodes with 10 tiles each. This way, the Culling Node will first test 4 large bounding box,
    /// with an actual chance of culling some of them. Then we'll have 25 bounding boxes to check per chunk, and some of them will cull out as well. For larger maps, just break into more chunks.
    /// 
    /// Another thing you need to remember is that you can combine Culling Nodes with regular nodes, but the regular nodes must be the edges (eg [culling_node] -> [plain_node] -> [culling_node] will not work properly). 
    /// For example if you implemet particles or doodads (like grass) with lots of small nodes, sometimes its enough to just test their parent node.
    /// </remarks>
    public class CullingNode : Node
    {
        /// <summary>
        /// The camera frustum to cull by. You need to update this every time the camera frustum changes in order
        /// to make the culling work currectly.
        /// </summary>
        public static BoundingFrustum CameraFrustum = null;

        /// <summary>
        /// Last calculated bounding box for this node.
        /// </summary>
        protected BoundingBox _boundingBox;

        /// <summary>
        /// Do we need to update the bounding box?
        /// </summary>
        private bool _isBoundingBoxDirty = true;

        /// <summary>
        /// Draw the node and its children.
        /// </summary>
        public override void Draw()
        {
            // if not visible skip
            if (!IsVisible)
            {
                return;
            }

            // if camera frustum is not defined, draw this node as a regular node
            if (CameraFrustum == null)
            {
                base.Draw();
                return;
            }

            // update transformations (only if needed, testing logic is inside)
            UpdateTransformations();

            // update bounding box (only if needed, testing logic is inside)
            UpdateBoundingBox();

            // if this node is out of screen, don't draw it
            if (CameraFrustum.Contains(_boundingBox) == ContainmentType.Disjoint)
            {
                return;
            }

            // if got here it means this node is in screen and should be rendered. draw it.

            // draw all child nodes
            foreach (Node node in _childNodes)
            {
                node.Draw();
            }

            // draw all child entities
            foreach (IEntity entity in _childEntities)
            {
                entity.Draw(this, _localTransform, _worldTransform);
            }
        }

        /// <summary>
        /// Called every time one of the child nodes recalculate world transformations.
        /// </summary>
        /// <param name="node">The child node that updated.</param>
        public override void OnChildWorldMatrixChange(Node node)
        {
            // mark bounding box as needing update
            _isBoundingBoxDirty = true;

            // pass message to parent, because it needs to update bounding box as well
            if (_parent != null)
            {
                _parent.OnChildWorldMatrixChange(node);
            }
        }

        /// <summary>
        /// Get bounding box of this node and all its child nodes.
        /// </summary>
        /// <param name="includeChildNodes">If true, will include bounding box of child nodes. If false, only of entities directly attached to this node.</param>
        /// <returns>Bounding box of the node and its children.</returns>
        public override BoundingBox GetBoundingBox(bool includeChildNodes = true)
        {
            // update bounding box (note: only if needed, tested inside)
            UpdateBoundingBox();

            // return bounding box
            return _boundingBox;
        }

        /// <summary>
        /// Called when the world matrix of this node is actually recalculated (invoked after the calculation).
        /// </summary>
        protected override void OnWorldMatrixChange()
        {
            // call base function
            base.OnWorldMatrixChange();

            // set bounding box to dirty
            _isBoundingBoxDirty = true;
        }

        /// <summary>
        /// Update the bounding box of this Culling Node.
        /// </summary>
        protected void UpdateBoundingBox()
        {
            // if bounding box is not dirty, skip
            if (!_isBoundingBoxDirty)
            {
                return;
            }

            // update bounding box
            _boundingBox = base.GetBoundingBox(true);

            // bounding box no longer dirty
            _isBoundingBoxDirty = false;
        }
    }
}
