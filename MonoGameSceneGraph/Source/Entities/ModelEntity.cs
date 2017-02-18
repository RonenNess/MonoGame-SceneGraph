#region File Description
//-----------------------------------------------------------------------------
// A basic renderable model.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


namespace MonoGameSceneGraph
{
    /// <summary>
    /// A basic renderable model.
    /// Note: this object is more of an example of how to create a renderable entity, you will probably
    /// need to implement something more sophisticated on your own.
    /// </summary>
    public class ModelEntity : IEntity
    {
        /// <summary>
        /// The model to render.
        /// </summary>
        public Model Model;

        /// <summary>
        /// Projection matrix to use with model.
        /// </summary>
        public Matrix Projection;

        /// <summary>
        /// View matrix to use with model.
        /// </summary>
        public Matrix View;

        /// <summary>
        /// Enable / disable default lighting on model.
        /// </summary>
        public bool EnableLighting = true;

        /// <summary>
        /// Create the model entity.
        /// </summary>
        /// <param name="model">Model to draw.</param>
        public ModelEntity(Model model)
        {
            // store model
            Model = model;

            // Create default Projection matrix (viewport)
            float aspectRatio = 1f;
            float fieldOfView = Microsoft.Xna.Framework.MathHelper.PiOver4;
            float nearClipPlane = 1;
            float farClipPlane = 200;
            Projection = Matrix.CreatePerspectiveFieldOfView(
                    fieldOfView, aspectRatio, nearClipPlane, farClipPlane);

            // Create default View matrix (camera)
            var cameraPosition = new Vector3(0, 20, 0);
            var cameraLookAtVector = Vector3.Zero;
            var cameraUpVector = Vector3.UnitZ;
            View = Matrix.CreateLookAt(cameraPosition, cameraLookAtVector, cameraUpVector);
        }

        /// <summary>
        /// Draw this model.
        /// </summary>
        /// <param name="parent">Parent node that's currently drawing this entity.</param>
        /// <param name="localTransformations">Local transformations from the direct parent node.</param>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        public void Draw(Node parent, Matrix localTransformations, Matrix worldTransformations)
        {
            // A model is composed of "Meshes" which are
            // parts of the model which can be positioned
            // independently, which can use different textures,
            // and which can have different rendering states
            // such as lighting applied.
            foreach (var mesh in Model.Meshes)
            {
                // "Effect" refers to a shader. Each mesh may
                // have multiple shaders applied to it for more
                // advanced visuals. 
                foreach (BasicEffect effect in mesh.Effects)
                {
                    // enable lights
                    if (EnableLighting)
                    {
                        // set default lightings
                        effect.EnableDefaultLighting();

                        // This makes lighting look more realistic on
                        // round surfaces, but at a slight performance cost:
                        effect.PreferPerPixelLighting = true;
                    }

                    // set world matrix
                    effect.World = worldTransformations;

                    // set view matrix
                    effect.View = View;

                    // set projection matrix
                    effect.Projection = Projection;
                }

                // Now that we've assigned our properties on the effects we can
                // draw the entire mesh
                mesh.Draw();
            }
        }
    }
}
