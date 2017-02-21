#region File Description
//-----------------------------------------------------------------------------
// An entity is the basic renderable entity, eg something you can draw.
// Entities don't have transformations of their own; instead, you put them inside
// nodes which handle matrices and transformations for them.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;


namespace MonoGameSceneGraph
{
    /// <summary>
    /// A basic renderable entity.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Draw this entity.
        /// </summary>
        /// <param name="parent">Parent node that's currently drawing this entity.</param>
        /// <param name="localTransformations">Local transformations from the direct parent node.</param>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        void Draw(Node parent, Matrix localTransformations, Matrix worldTransformations);

        /// <summary>
        /// Get the bounding box of this entity (in world space).
        /// </summary>
        /// <param name="parent">Parent node that's currently drawing this entity.</param>
        /// <param name="localTransformations">Local transformations from the direct parent node.</param>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        /// <returns>Bounding box of the entity, in world space.</returns>
        BoundingBox GetBoundingBox(Node parent, Matrix localTransformations, Matrix worldTransformations);

        /// <summary>
        /// Return if the entity is currently visible.
        /// </summary>
        /// <returns>If the entity is visible or not.</returns>
        bool Visible { get; set; }
    }
}
