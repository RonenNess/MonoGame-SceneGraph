using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace MonoGameSceneGraph
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        // graphics manager
        GraphicsDeviceManager graphics;

        // sprite batch (for 2d drawings)
        SpriteBatch spriteBatch;

        // create test nodes and a model to draw
        Node root;
        Node node;
        Node nodeContainer;
        Node nodeContainerContainer;
        ModelEntity entity;

        /// <summary>
        /// Init game.
        /// </summary>
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // create nodes
            root = new Node();
            nodeContainerContainer = new Node();
            nodeContainer = new Node();
            node = new Node();

            // arrange them in hirerchy
            root.AddChildNode(nodeContainerContainer);
            nodeContainerContainer.AddChildNode(nodeContainer);
            nodeContainer.AddChildNode(node);

            // for debug
            root.Identifier = "root";
            nodeContainerContainer.Identifier = "ncc";
            nodeContainer.Identifier = "nc";
            node.Identifier = "n";

            // create entity and attach to bottom node
            entity = new ModelEntity(Content.Load<Model>("robot"));
            node.AddEntity(entity);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // clear screen
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // draw scene
            root.Draw();

            // rotate inner node on X axis
            node.RotationZ = node.RotationZ + 0.01f;

            // scale node container
            nodeContainer.ScaleZ = (1.0f + (float)System.Math.Cos(gameTime.TotalGameTime.TotalSeconds * 2f) / 4f);

            // move top node left and right
            nodeContainerContainer.PositionX = (float)System.Math.Sin(gameTime.TotalGameTime.TotalSeconds) * 4f;

            // call base draw
            base.Draw(gameTime);
        }
    }
}
