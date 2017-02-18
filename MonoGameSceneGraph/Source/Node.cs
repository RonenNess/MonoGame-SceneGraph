#region File Description
//-----------------------------------------------------------------------------
// A node is the basic container in the scene graph. Its basically a point in
// transformations that can contain child nodes (and inherit transformations), 
// and contain renderable entities to draw inside.
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
    /// MonoGameSceneGraph is the main namespace that contains all the MonoGame-SceneGraph entities.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGenerated]
    class NamespaceDoc
    {
    }

    /// <summary>
    /// A node with transformations, you can attach renderable entities to it, or append
    /// child nodes to inherit transformations.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Parent node.
        /// </summary>
        protected Node _parent = null;

        /// <summary>
        /// Node's transformations.
        /// </summary>
        protected Transformations _transformations = new Transformations();

        /// <summary>
        /// Is this node currently visible?
        /// </summary>
        public bool IsVisible = true;

        /// <summary>
        /// Optional identifier we can give to nodes.
        /// </summary>
        public string Identifier;

        /// <summary>
        /// Optional user data we can attach to nodes.
        /// </summary>
        public object UserData;

        /// <summary>
        /// The order in which we apply transformations when building the matrix for this node.
        /// </summary>
        protected TransformOrder _transformationsOrder = TransformOrder.ScaleRotationPosition;

        /// <summary>
        /// The order in which we apply rotation when building the matrix for this node.
        /// </summary>
        protected RotationOrder _rotationOrder = RotationOrder.RotateYXZ;

        /// <summary>
        /// Local transformations matrix, eg the result of the current local transformations.
        /// </summary>
        protected Matrix _localTransform = Matrix.Identity;

        /// <summary>
        /// World transformations matrix, eg the result of the local transformations multiplied with parent transformations.
        /// </summary>
        protected Matrix _worldTransform = Matrix.Identity;

        /// <summary>
        /// Child nodes under this node.
        /// </summary>
        protected List<Node> _childNodes = new List<Node>();

        /// <summary>
        /// Child entities under this node.
        /// </summary>
        protected List<IEntity> _childEntities = new List<IEntity>();

        /// <summary>
        /// Turns true when the transformations of this node changes.
        /// </summary>
        protected bool _isDirty = true;

        /// <summary>
        /// This number increment every time we update transformations.
        /// We use it to check if our parent's transformations had been changed since last
        /// time this node was rendered, and if so, we re-apply parent updated transformations.
        /// </summary>
        protected uint _transformVersion = 0;

        /// <summary>
        /// The last transformations version we got from our parent.
        /// </summary>
        protected uint _parentLastTransformVersion = 0;

        /// <summary>
        /// Get parent node.
        /// </summary>
        public Node Parent { get { return _parent; } }

        /// <summary>
        /// Draw the node and its children.
        /// </summary>
        public virtual void Draw()
        {
            // not visible? skip
            if (!IsVisible)
            {
                return;
            }

            // update transformations (only if needed, testing logic is inside)
            UpdateTransformations();

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
        /// Add an entity to this node.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public void AddEntity(IEntity entity)
        {
            _childEntities.Add(entity);
        }

        /// <summary>
        /// Remove an entity from this node.
        /// </summary>
        /// <param name="entity">Entity to add.</param>
        public void RemoveEntity(IEntity entity)
        {
            _childEntities.Remove(entity);
        }

        /// <summary>
        /// Add a child node to this node.
        /// </summary>
        /// <param name="node">Node to add.</param>
        public void AddChildNode(Node node)
        {
            // node already got a parent?
            if (node._parent != null)
            {
                throw new System.Exception("Can't add a node that already have a parent.");
            }

            // add node to children list
            _childNodes.Add(node);

            // set self as node's parent, and make sure it will update world transformations next draw call
            node._parent = this;
            node._parentLastTransformVersion = _transformVersion - 1;
        }

        /// <summary>
        /// Remove a child node from this node.
        /// </summary>
        /// <param name="node">Node to add.</param>
        public void RemoveChildNode(Node node)
        {
            // make sure the node is a child of this node
            if (node._parent != this)
            {
                throw new System.Exception("Can't remove a node that don't belong to this parent.");
            }

            // remove node from children list
            _childNodes.Remove(node);

            // clear node parent pointer and set parent transformations to 1, to make sure it will recalc world transform next draw
            node._parent = null;
            node._parentLastTransformVersion = 1;
        }

        /// <summary>
        /// Remove this node from its parent.
        /// </summary>
        public void RemoveFromParent()
        {
            // don't have a parent?
            if (_parent == null)
            {
                throw new System.Exception("Can't remove an orphan node from parent.");
            }

            // remove from parent
            _parent.RemoveChildNode(this);
        }

        /// <summary>
        /// Set this node as "dirty", eg that we need to update local transformations.
        /// </summary>
        protected void OnWorldMatrixChange()
        {
            _isDirty = true;
        }

        /// <summary>
        /// Calc final transformations for current frame.
        /// This uses an indicator to know if an update is needed, so no harm is done if you call it multiple times.
        /// </summary>
        protected void UpdateTransformations()
        {
            // if local transformations are dirty, we need to update them
            if (_isDirty)
            {
                _localTransform = _transformations.BuildMatrix(_transformationsOrder, _rotationOrder);
                _transformVersion++;
            }
            
            // if local transformations are dirty, or parent transformations are out-of-date, update global transformations
            if (_isDirty || 
                (_parent != null && _parentLastTransformVersion != _parent._transformVersion) |
                (_parent == null && _parentLastTransformVersion != 0))
            {
                // if we got parent, apply its transformations
                if (_parent != null)
                {
                    _worldTransform = _localTransform * _parent._worldTransform;
                    _parentLastTransformVersion = _parent._transformVersion;
                }
                // if not, world transformations are the same as local, and reset parent last transformations version
                else
                {
                    _worldTransform = _localTransform;
                    _parentLastTransformVersion = 0;
                }
            }

            // no longer dirty
            _isDirty = false;
        }

        /// <summary>
        /// Return local transformations matrix (note: will recalculate if needed).
        /// </summary>
        public Matrix LocalTransformations
        {
            get { UpdateTransformations(); return _localTransform; }
        }

        /// <summary>
        /// Return world transformations matrix (note: will recalculate if needed).
        /// </summary>
        public Matrix WorldTransformations
        {
            get { UpdateTransformations(); return _worldTransform; }
        }

        /// <summary>
        /// Reset all local transformations.
        /// </summary>
        public void ResetTransformations()
        {
            _transformations = new Transformations();
            OnWorldMatrixChange();
        }

        /// <summary>
        /// Get / Set the order in which we apply local transformations in this node.
        /// </summary>
        public TransformOrder TransformationsOrder
        {
            get { return _transformationsOrder; }
            set { _transformationsOrder = value;  OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Get / Set the order in which we apply local rotation in this node.
        /// </summary>
        public RotationOrder RotationOrder
        {
            get { return _rotationOrder; }
            set { _rotationOrder = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Get / Set node local position.
        /// </summary>
        public Vector3 Position
        {
            get { return _transformations.Position; }
            set { _transformations.Position = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Get / Set node local scale.
        /// </summary>
        public Vector3 Scale
        {
            get { return _transformations.Scale; }
            set { _transformations.Scale = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Get / Set node local rotation.
        /// </summary>
        public Vector3 Rotation
        {
            get { return _transformations.Rotation; }
            set { _transformations.Rotation = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Alias to access rotation X directly.
        /// </summary>
        public float RotationX
        {
            get { return _transformations.Rotation.X; }
            set { _transformations.Rotation.X = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Alias to access rotation Y directly.
        /// </summary>
        public float RotationY
        {
            get { return _transformations.Rotation.Y; }
            set { _transformations.Rotation.Y = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Alias to access rotation Z directly.
        /// </summary>
        public float RotationZ
        {
            get { return _transformations.Rotation.Z; }
            set { _transformations.Rotation.Z = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Alias to access scale X directly.
        /// </summary>
        public float ScaleX
        {
            get { return _transformations.Scale.X; }
            set { _transformations.Scale.X = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Alias to access scale Y directly.
        /// </summary>
        public float ScaleY
        {
            get { return _transformations.Scale.Y; }
            set { _transformations.Scale.Y = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Alias to access scale Z directly.
        /// </summary>
        public float ScaleZ
        {
            get { return _transformations.Scale.Z; }
            set { _transformations.Scale.Z = value; OnWorldMatrixChange(); }
        }


        /// <summary>
        /// Alias to access position X directly.
        /// </summary>
        public float PositionX
        {
            get { return _transformations.Position.X; }
            set { _transformations.Position.X = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Alias to access position Y directly.
        /// </summary>
        public float PositionY
        {
            get { return _transformations.Position.Y; }
            set { _transformations.Position.Y = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Alias to access position Z directly.
        /// </summary>
        public float PositionZ
        {
            get { return _transformations.Position.Z; }
            set { _transformations.Position.Z = value; OnWorldMatrixChange(); }
        }

        /// <summary>
        /// Move position by vector.
        /// </summary>
        /// <param name="moveBy">Vector to translate by.</param>
        public void Translate(Vector3 moveBy)
        {
            _transformations.Position += moveBy;
            OnWorldMatrixChange();
        }
    }
}
