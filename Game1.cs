using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Assimp;
using Assimp.Configs;
using System.Diagnostics;
using Assimp.Unmanaged;
using Microsoft.Xna.Framework.Input;
using System.Xml.Linq;
using System.Reflection.Metadata;
using Quaternion = Microsoft.Xna.Framework.Quaternion;
using static System.Formats.Asn1.AsnWriter;

namespace NowaAnimacjaRobocik
{
    class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Assimp variables

        // MonoGame variables
        Model model;
        Matrix view;
        Matrix projection;
        Matrix world;
        Vector3 cameraPosition = new Vector3(0, -10, 10);
        Vector3 cameraTarget = new Vector3(0, 0, 0);
        Vector3 positionModel = Vector3.Zero;
        SkinnedEffect skinnedEffect;
        Texture2D texture2D;
        MyAnimations myAnimation;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            skinnedEffect = new SkinnedEffect(GraphicsDevice);
            skinnedEffect.EnableDefaultLighting(); // włącz domyślne oświetlenie
            skinnedEffect.WeightsPerVertex = 4; // ustaw ilość wag na wierzchołek


            // Assimp initialization
            world = Matrix.CreateWorld(Vector3.Zero, Vector3.Forward, Vector3.Up);
            view = Matrix.CreateLookAt(new Vector3(0, 10, 10), new Vector3(0, 0, 0), Vector3.UnitY);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800 / 480f, 0.1f, 10000f);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);


            texture2D = Content.Load<Texture2D>("StarSparrow_Green");
            model = Content.Load<Model>("fukyou");
            //Debug.Write(scene.Meshes[0].HasBones + "\n");
            myAnimation = new MyAnimations("../../../Content/fukyou.fbx", model);








            //Debug.Write(boneMapping.Count + "\n");



            //view = Matrix.CreateLookAt(new Vector3(0, 0, 900), Vector3.Zero, Vector3.Up);
            //projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 10000.0f);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart part in mesh.MeshParts)
                {
                    part.Effect = skinnedEffect;
                }
            }


            //Debug.WriteLine(boneMapping.Values);
            /*
            Debug.WriteLine(scene.Meshes[0].Bones[0].Name);
            Debug.WriteLine(model.Bones[0].Name);
            Debug.WriteLine(scene.Animations[0].DurationInTicks / scene.Animations[0].TicksPerSecond);
            foreach(VectorKey one in scene.Animations[0].NodeAnimationChannels[2].ScalingKeys)
            {
                Debug.WriteLine(one);
            }
                
            */

            //  Debug.WriteLine(scene.Animations[0].DurationInTicks);

            /*
            foreach (NodeAnimationChannel ani in scene.Animations[0].NodeAnimationChannels)
            {
               // Debug.WriteLine();
                Debug.WriteLine(ani.PositionKeys[0].Value);
                Debug.WriteLine(ani.ScalingKeys[0].Value);
                Debug.WriteLine(ani.RotationKeys[0].Value.GetMatrix());

                // Debug.WriteLine(" koniec ");
            }
            */


        }

        protected override void Update(GameTime gameTime)
        {

            myAnimation.Update(gameTime.ElapsedGameTime.TotalSeconds);
            /*
            if (scene.HasAnimations)
            {
                Animation animation = scene.Animations.FirstOrDefault(x => x.DurationInTicks > 0);


                if (animation != null)
                {
                    if (animationTime > scene.Animations[0].DurationInTicks / scene.Animations[0].TicksPerSecond)
                    {
                        //Debug.Write("tak");
                        animationTime = 0;
                    }
                    
                    Matrix4x4 nodeTransform = scene.RootNode.Transform;
                    Matrix nodeTransformMatrix = AssimpToXnaMatrix(nodeTransform);
                    string nodeName = scene.RootNode.Name;
                    int boneIndex = boneMapping[nodeName];
                    Matrix offsetMatrix = AssimpToXnaMatrix(scene.Meshes[0].Bones[boneIndex].OffsetMatrix);
                    Matrix animationTransform = nodeTransformMatrix * offsetMatrix;
                    Matrix finalTransform = animationTransform * Matrix.CreateScale(0.5f);
                    SecondMethod(model,finalTransform,animationTime);
                    


                    int i = 0;
                    foreach (var node in scene.RootNode.Children)
                    {
                       
                        foreach (var child in node.Children)
                        {
                            //Debug.WriteLine(i);
                            //Debug.WriteLine(child.Children.Count);
                            //Debug.WriteLine(child.Name);
                            myFunk(child, Matrix.Identity);
                            //i++;
                        }
                    }

                   
                    
                    foreach(Bone bone in scene.Meshes[0].Bones)
                    {
                        //boneTransforms[i] = Matrix.Lerp(Matrix.Identity, AssimpToXnaMatrix(bone.OffsetMatrix), animationTime);
                        boneTransforms[i] = Matrix.Identity;
                        i++;
                    }
                    

                    



                    //boneTransforms[30] = AssimpToXnaMatrix(scene.Meshes[0].Bones[30].OffsetMatrix);


                    
                    foreach (NodeAnimationChannel ani in scene.Animations[0].NodeAnimationChannels)
                    {
                        string name = scene.Animations[0].NodeAnimationChannels[i].NodeName;
                        int index = boneMapping[name];
                        Vector3 pos = new Vector3(ani.PositionKeys[0].Value.X, ani.PositionKeys[0].Value.Y, ani.PositionKeys[0].Value.Z);
                        Matrix rotation = ConvertMatrix3x3(ani.RotationKeys[0].Value.GetMatrix());

                        //Matrix allRotation = Matrix.CreateRotationX(rotX) * Matrix.CreateRotationY(rotY) * Matrix.CreateRotationZ(rotZ);
                        boneTransforms[index] = Matrix.Lerp(Matrix.Identity, Matrix.CreateTranslation(pos) * rotation, animationTime);
                        i++;
                        // Debug.WriteLine(" koniec ");
                    }
                    

                    // boneTransforms[0] = Matrix.Lerp(Matrix.Identity, AssimpToXnaMatrix(scene.Meshes[0].Bones[1].OffsetMatrix), animationTime);
                    //CalculateBoneTransforms(scene.RootNode, Matrix.Identity, animationTime);
            */


            KeyboardState keyboardState = Keyboard.GetState();


            float cameraSpeed = 10f;

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                cameraPosition.X -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                cameraTarget.X -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                cameraPosition.X += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                cameraTarget.X += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (keyboardState.IsKeyDown(Keys.Up))
            {
                cameraPosition.Y += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                cameraTarget.Y += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                cameraPosition.Y -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                cameraTarget.Y -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (keyboardState.IsKeyDown(Keys.Space))
            {
                cameraPosition.Z += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                cameraTarget.Z += cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }
            if (keyboardState.IsKeyDown(Keys.LeftControl))
            {
                cameraPosition.Z -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
                cameraTarget.Z -= cameraSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;
            }

            view = Matrix.CreateLookAt(cameraPosition, cameraTarget, Vector3.Up);

            // Update the animation time
            // animationTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

            base.Update(gameTime);
        }




        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the model using the bone transforms
           // modelTransforms = new Matrix[model.Bones.Count];
            //model.CopyAbsoluteBoneTransformsTo(modelTransforms);

            foreach(var one in myAnimation.boneTransforms)
            {
                //Debug.WriteLine(one);
            }
            //Debug.Write(boneTransforms.Length + "\n");

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (SkinnedEffect effect in mesh.Effects)
                {
                    //effect.View = view;
                    // effect.Projection = projection;

                    //Debug.Write(boneTransforms[i] + "\n");
                    effect.View = view;
                    effect.Projection = projection;
                    effect.World = world;
                    effect.SetBoneTransforms(myAnimation.boneTransforms);
                    effect.Texture = texture2D;

                    // Debug.Write(world * NormalizeMatrix(boneTransforms[mesh.ParentBone.Index]) * NormalizeMatrix(mesh.ParentBone.Transform) + "\n");
                    /*
                    for (int j = 0; j < boneTransforms.Length; j++)
                    {
                        effect.World = world * boneTransforms[j];
                    }
                    
                    for(int j=0; j< boneTransforms.Length; j++)
                    {
                       // Debug.Write(NormalizeMatrix(world * modelTransforms[mesh.ParentBone.Index] * boneTransforms[j]) + "\n");
                        effect.World = world * modelTransforms[mesh.ParentBone.Index] * boneTransforms[j];
                        
                    }
                    */

                    //effect.EnableDefaultLighting();
                    //effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }

            base.Draw(gameTime);
        }

    }
}