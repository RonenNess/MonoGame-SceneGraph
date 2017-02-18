#region File Description
//-----------------------------------------------------------------------------
// A class containing all the basic transformations a renderable object can have.
// This include: Translation, Rotation, and Scale.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameSceneGraph
{
    /// <summary>
    /// Different way to build matrix from transformations.
    /// </summary>
    public enum TransformOrder
    {
        /// <summary>
        /// Apply position, then rotation, then scale.
        /// </summary>
        PositionRotationScale,

        /// <summary>
        /// Apply position, then scale, then rotation.
        /// </summary>
        PositionScaleRotation,

        /// <summary>
        /// Apply scale, then position, then rotation.
        /// </summary>
        ScalePositionRotation,

        /// <summary>
        /// Apply scale, then rotation, then position.
        /// </summary>
        ScaleRotationPosition,

        /// <summary>
        /// Apply rotation, then scale, then position.
        /// </summary>
        RotationScalePosition,

        /// <summary>
        /// Apply rotation, then position, then scale.
        /// </summary>
        RotationPositionScale,
    }

    /// <summary>
    /// Different ways to apply rotation (order in which we rotate the different axis).
    /// </summary>
    public enum RotationOrder
    {
        /// <summary>
        /// Rotate by axis order X, Y, Z.
        /// </summary>
        RotateXYZ,

        /// <summary>
        /// Rotate by axis order X, Z, Y.
        /// </summary>
        RotateXZY,

        /// <summary>
        /// Rotate by axis order Y, X, Z.
        /// </summary>
        RotateYXZ,

        /// <summary>
        /// Rotate by axis order Y, Z, X.
        /// </summary>
        RotateYZX,

        /// <summary>
        /// Rotate by axis order Z, X, Y.
        /// </summary>
        RotateZXY,

        /// <summary>
        /// Rotate by axis order Z, Y, X.
        /// </summary>
        RotateZYX,
    }

    /// <summary>
    /// Contain all the possible node transformations.
    /// </summary>
    public class Transformations
    {

        /// <summary>
        /// Node position / translation.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Node rotation.
        /// </summary>
        public Vector3 Rotation;

        /// <summary>
        /// Node scale.
        /// </summary>
        public Vector3 Scale;

        /// <summary>
        /// Create new default transformations.
        /// </summary>
        public Transformations()
        {
            Position = Vector3.Zero;
            Rotation = Vector3.Zero;
            Scale = Vector3.One;
        }

        /// <summary>
        /// Clone transformations.
        /// </summary>
        public Transformations(Transformations other)
        {
            Position = other.Position;
            Rotation = other.Rotation;
            Scale = other.Scale;
        }

        /// <summary>
        /// Build and return just the rotation matrix for this treansformations.
        /// </summary>
        /// <param name="rotationOrder">In which order to apply rotation (axis order) when applying rotation.</param>
        /// <returns></returns>
        public Matrix BuildRotationMatrix(RotationOrder rotationOrder = RotationOrder.RotateYXZ)
        {
            switch (rotationOrder)
            {
                case RotationOrder.RotateXYZ:
                    return Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationZ(Rotation.Z);

                case RotationOrder.RotateXZY:
                    return Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationZ(Rotation.Z) * Matrix.CreateRotationY(Rotation.Y);

                case RotationOrder.RotateYXZ:
                    return Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z);

                case RotationOrder.RotateYZX:
                    return Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationZ(Rotation.Z) * Matrix.CreateRotationX(Rotation.X);

                case RotationOrder.RotateZXY:
                    return Matrix.CreateRotationZ(Rotation.Z) * Matrix.CreateRotationX(Rotation.X) * Matrix.CreateRotationY(Rotation.Y);

                case RotationOrder.RotateZYX:
                    return Matrix.CreateRotationZ(Rotation.Z) * Matrix.CreateRotationY(Rotation.Y) * Matrix.CreateRotationX(Rotation.X);

                default:
                    throw new System.Exception("Unknown rotation order!");
            }
        }

        /// <summary>
        /// Build and return a matrix from current transformations.
        /// </summary>
        /// <param name="transformOrder">In which order to apply transformations to produce final matrix.</param>
        /// <param name="rotationOrder">In which order to apply rotation (axis order) when applying rotation.</param>
        /// <returns>Matrix with all transformations applied.</returns>
        public Matrix BuildMatrix(TransformOrder transformOrder = TransformOrder.ScaleRotationPosition, 
            RotationOrder rotationOrder = RotationOrder.RotateYXZ)
        {
            // create the matrix parts
            Matrix pos = Matrix.CreateTranslation(Position);
            Matrix rot = BuildRotationMatrix(rotationOrder);
            Matrix scale = Matrix.CreateScale(Scale);

            // build and return matrix based on order
            switch (transformOrder)
            {
                case TransformOrder.PositionRotationScale:
                    return pos * rot * scale;

                case TransformOrder.PositionScaleRotation:
                    return pos * scale * rot;

                case TransformOrder.ScalePositionRotation:
                    return scale * pos * rot;

                case TransformOrder.ScaleRotationPosition:
                    return scale * rot * pos;

                case TransformOrder.RotationScalePosition:
                    return rot * scale * pos;

                case TransformOrder.RotationPositionScale:
                    return rot * pos * scale;

                default:
                    throw new System.Exception("Unknown build matrix order!");
            }
        }
    }
}
