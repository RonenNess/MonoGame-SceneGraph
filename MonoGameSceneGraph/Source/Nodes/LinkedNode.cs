#region File Description
//-----------------------------------------------------------------------------
// A scene node that can be linked to an external source and copy its transformations.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace GeonBit.Core.Graphics
{
    /// <summary>
    /// An external transformations source we can attach to a LinkedNode to update its transformations.
    /// This is used to connect a node to a physical body, like a Bullet3d rigid body etc.
    /// </summary>
    public interface ITransformationsSource
    {
        /// <summary>
        /// Return if transformations are dirty and need update.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Get body transformations.
        /// </summary>
        Matrix WorldTransform { get; }

        /// <summary>
        /// Invoked after the node took the transformations from the source.
        /// </summary>
        void NodeAcceptedTransform();
    }

    /// <summary>
    /// A scene node designed to be integrated into GeonBit scene and receive updates from external source,
    /// like physical body. This is the default node we use everywhere in the engine.
    /// </summary>
    public class LinkedNode : CullingNode
    {
        /// <summary>
        /// Option to bind external transformations for this node, like a physical body etc.
        /// </summary>
        public ITransformationsSource TransformsBind = null;

        /// <summary>
        /// If true, will not copy scale transformations from source
        /// </summary>
        public bool KeepScale = true;

        /// <summary>
        /// Clone this scene node.
        /// </summary>
        /// <returns>Node copy.</returns>
        public override Node Clone()
        {
            LinkedNode ret = new LinkedNode();
            ret._transformations = _transformations.Clone();
            ret.Visible = Visible;
            return ret;
        }

        /// <summary>
        /// Calc final transformations for current frame.
        /// This uses an indicator to know if an update is needed, so no harm is done if you call it multiple times.
        /// </summary>
        protected override void UpdateTransformations()
        {
            // if got no transformations bind, call base update
            if (TransformsBind == null)
            {
                base.UpdateTransformations();
                return;
            }

            // if got here it means we need to get transformations from external source.
            // check if dirty
            if (TransformsBind.IsDirty)
            {
                // update world transform from bind
                if (KeepScale)
                {
                    Vector3 prevScale = _parent.WorldScale * _transformations.Scale;
                    _worldTransform = Matrix.CreateScale(prevScale) * TransformsBind.WorldTransform;
                }
                else
                {
                    _worldTransform = TransformsBind.WorldTransform;
                }

                // notify parent that we accepted transformation and send world matrix change event
                TransformsBind.NodeAcceptedTransform();
                OnWorldMatrixChange();
            }

            // no longer dirty
            _isDirty = false;
        }
    }
}
