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
    /// Another thing you need to remember is that you can combine Culling Nodes with regular nodes. For example if you implemet particles or doodads (like grass) with lots of small nodes, sometimes its
    /// enough to just test their parent node.
    /// </remarks>
    public class CullingNode : Node
    {
        /// <summary>
        /// The camera frustum to cull by. You need to update this every time the camera frustum changes in order
        /// to make the culling work currectly.
        /// </summary>
        public static BoundingFrustum CameraFrustum;

        /// <summary>
        /// Last calculated bounding box for this node.
        /// </summary>
        protected BoundingBox _currBoundingBox;

        /// <summary>
        /// Do we need to recalculate the bounding box of this node?
        /// </summary>
        private bool _isBoundingBoxDirty = true;

        /// <summary>
        /// Called when the world matrix of this node is actually recalculated (invoked after the calculation).
        /// </summary>
        protected override void OnWorldMatrixChange()
        {
            base.OnWorldMatrixChange();
            _isBoundingBoxDirty = true;
        }

        /// <summary>
        /// Draw the node and its children.
        /// </summary>
        public override void Draw()
        {
            // if not visible or camera frustum is not set, skip (eg don't draw it)
            if (!IsVisible || CameraFrustum == null)
            {
                return;
            }

            // check if need to recalculate bounding box
            if (_isBoundingBoxDirty)
            {
                _currBoundingBox = GetBoundingBox(true);
                _isBoundingBoxDirty = false;
            }

            // if this node is out of screen, don't draw it
            if (!_currBoundingBox.Intersects(CameraFrustum))
            {
                return;
            }

            // if got here it means this node is in screen and should be rendered. draw it.
            base.Draw();
        }
    }
}
