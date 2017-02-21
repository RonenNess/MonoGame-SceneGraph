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
            // visible by default
            Visible = true;

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
            // iterate model meshes
            foreach (var mesh in Model.Meshes)
            {
                // iterate effect in mesh
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

                // draw current mesh
                mesh.Draw();
            }
        }

        /// <summary>
        /// Return if the entity is currently visible.
        /// </summary>
        /// <returns>If the entity is visible or not.</returns>
        public bool Visible
        {
            get; set;
        }

        /// <summary>
        /// Get the bounding box of this entity.
        /// </summary>
        /// <param name="parent">Parent node that's currently drawing this entity.</param>
        /// <param name="localTransformations">Local transformations from the direct parent node.</param>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        /// <returns>Bounding box of the entity.</returns>
        public BoundingBox GetBoundingBox(Node parent, Matrix localTransformations, Matrix worldTransformations)
        {
            // initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // iterate over mesh parts
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // iterate through vertices (possibly) growing bounding box
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        // get curr position and update min / max
                        Vector3 currPosition = new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]);
                        currPosition = Vector3.Transform(currPosition, worldTransformations);
                        min = Vector3.Min(min, currPosition);
                        max = Vector3.Max(max, currPosition);
                    }
                }
            }

            // return bounding box
            return new BoundingBox(min, max);
        }
    }
}
