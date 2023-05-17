using Assimp;
using Assimp.Configs;
using Assimp.Unmanaged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Quaternion = Assimp.Quaternion;

namespace NowaAnimacjaRobocik
{
    internal class MyAnimations     //ZAKŁADAMY WCZYTANIE JEDNEJ ANIMACJI Z MODELU KTÓRY MA TYLKO JEDNĄ SIATKĘ
    {
        public AssimpContext importer;
        public Scene scene;
        public int numModelBones;   // 3 więcej niż Nodów
        public int numSceneBones;
        public int numChanelNodes;
        public Matrix[] boneTransforms;     //też 3 więcej niż Nodów
        public float animationTimeInTicks;
        public float animationTimeInSec;
        public double animationTime;
        public double actualTick;
        public Dictionary<string, int> modelBones;   //W boneMapping szukamy tylko po nazwie (kości modelu jest o 3 więcej niż powinno)
        public Dictionary<string, int> sceneBones;
        public Dictionary<string, int> sceneChanelNodes;
        //public Dictionary<string, int> positions;
        //public int numBones;
        List<List<QuaternionKey>> Rotations;
        List<List<VectorKey>> Positions;
        List<List<VectorKey>> Scaling;
        QuaternionKey lastQuad;
        VectorKey lastPos;
        VectorKey lastScale;
        Model model;
        Matrix4x4 GlobalInverseTransform;
        public MyAnimations(string filename, Model _model)
        {
            model = _model;
            importer = new AssimpContext();
            importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            modelBones = new Dictionary<string, int>();
            sceneBones = new Dictionary<string, int>();
            sceneChanelNodes = new Dictionary<string, int>();
            scene = importer.ImportFile(filename);
            animationTimeInTicks = (float)scene.Animations[0].DurationInTicks;
            animationTimeInSec = (float)(scene.Animations[0].DurationInTicks/scene.Animations[0].TicksPerSecond);
            numModelBones = model.Bones.Count;
            numSceneBones = scene.Meshes[0].BoneCount;
            numChanelNodes = scene.Animations[0].NodeAnimationChannels.Count;
            boneTransforms = new Matrix[numModelBones];

            Rotations = new List<List<QuaternionKey>>();
            Positions = new List<List<VectorKey>>();
            Scaling = new List<List<VectorKey>>();

            for (int i = 0; i < numModelBones; i++)
            {
                modelBones[model.Bones[i].Name] = i;
                boneTransforms[i] = Matrix.Identity;
            }
            for (int i = 0; i< numSceneBones; i++)
            {
                sceneBones[scene.Meshes[0].Bones[i].Name] = i;
            }
            for (int i =0; i< numChanelNodes; i++)
            {
                sceneChanelNodes[scene.Animations[0].NodeAnimationChannels[i].NodeName] = i;
                
            }


            //Cofnięcie optymalizacji QuaternionKey biblioteki Assimp 
            int o = 0;
            foreach (var chanel in scene.Animations[0].NodeAnimationChannels)       //DZIAŁA ROTA
            {
                Rotations.Add(new List<QuaternionKey>());
                for (int j = 0; j <= animationTimeInTicks; j++)
                {
                        bool hasKeyAtTime = chanel.RotationKeys.Exists(key=>key.Time==j);
                    if (hasKeyAtTime)
                    {
                        var szukanyindex = chanel.RotationKeys.Where(key => key.Time == j).First();

                        Rotations[o].Add(szukanyindex);
                        
                            lastQuad = new QuaternionKey(j, szukanyindex.Value);
                    }
                        else
                        {
                            QuaternionKey pakowany = new QuaternionKey(j,lastQuad.Value);
                            Rotations[o].Add(pakowany);
                        }
                }
                o++;
            }

            GlobalInverseTransform = scene.RootNode.Transform;
            GlobalInverseTransform.Inverse();

            //Cofnięcie optymalizacji QuaternionKey biblioteki Assimp 
            int z = 0;
            foreach (var chanel in scene.Animations[0].NodeAnimationChannels)       //DZIAŁA
            {
                Positions.Add(new List<VectorKey>());
                for (int j = 0; j <= animationTimeInTicks; j++)
                {
                    bool hasKeyAtTime = chanel.PositionKeys.Exists(key => key.Time == j);
                    if (hasKeyAtTime)
                    {
                        var szukanyindex = chanel.PositionKeys.Where(key => key.Time == j).First();

                        Positions[z].Add(szukanyindex);

                        lastPos = new VectorKey(j, szukanyindex.Value);
                    }
                    else
                    {
                        VectorKey pakowany = new VectorKey(j, lastPos.Value);
                        Positions[z].Add(pakowany);
                    }
                }
                z++;
            }
            //Cofnięcie optymalizacji QuaternionKey biblioteki Assimp 
            int x = 0;
            foreach (var chanel in scene.Animations[0].NodeAnimationChannels)       //DZIAŁA
            {
                Scaling.Add(new List<VectorKey>());
                for (int j = 0; j <= animationTimeInTicks; j++)
                {
                    bool hasKeyAtTime = chanel.ScalingKeys.Exists(key => key.Time == j);
                    if (hasKeyAtTime)
                    {
                        var szukanyindex = chanel.ScalingKeys.Where(key => key.Time == j).First();

                        Scaling[x].Add(szukanyindex);

                        lastScale = new VectorKey(j, szukanyindex.Value);
                    }
                    else
                    {
                        VectorKey pakowany = new VectorKey(j, lastScale.Value);
                        Scaling[x].Add(pakowany);
                    }
                }
                x++;
            }
            Debug.WriteLine(numModelBones);
            foreach(var kurwa in modelBones)
            {
                Debug.WriteLine(kurwa);
            }

            foreach (var chanel in Rotations)
            {
                Debug.WriteLine("node nr : " + (Rotations.FindIndex(key => key==chanel)+2));
                foreach (var rot in chanel)
                {
                    Debug.WriteLine(rot.Time + " : "+ Matrix.CreateFromQuaternion(QuatToQuat(rot.Value)));
                }
            }
        }

        public void Update(double gameTime)
        {
            animationTime += gameTime;
            if(animationTime >= animationTimeInSec)
            {
                animationTime = 0;
            }
            actualTick = (int)(animationTime * scene.Animations[0].TicksPerSecond);


            /*
            boneTransforms[3] = new Matrix(new Vector4(-0.105306506f, 0.13884193f, -0.98469967f, 0),
                                            new Vector4(0.13879219f, 0.98256576f, 0.12369822f, 0),
                                            new Vector4(0.9847067f, -0.123642385f, -0.122740746f, 0),
                                            new Vector4(0, 0, 0, 1));
            */
            /*
            boneTransforms[3] = new Matrix(new Vector4(0.99999547f, 0.0007575167f, -0.002921813f, 0),
                                            new Vector4(5.098991E-08f, 0.9679919f, 0.25098148f, 0),
                                            new Vector4(0.003018414f, -0.25098035f, 0.9679875f, 0),
                                            new Vector4(0, 0, 0, 1));
            */

            myFunk(scene.RootNode,Matrix.Identity,actualTick);


           
        }



        public Vector3 RotationToVec(Matrix3x3 mat)
        {
            float sy = (float)Math.Sqrt(mat.A1*mat.A1+mat.B1*mat.B1);
            bool singular = sy < 1e-6;
            float x, y, z;
            if(!singular)
            {
                x = (float)Math.Atan2(mat.C2, mat.C3);
                y = (float)Math.Atan2(-mat.C1, sy);
                z = (float)Math.Atan2(mat.B1, mat.A1);
            }
            else
            {
                x = (float)Math.Atan2(-mat.B3, mat.B2);
                y = (float)Math.Atan2(-mat.C1, sy);
                z = 0;
            }

            return new Vector3(x,y,z);
        }

        

        public void myFunk(Node node, Matrix parentTransforms,double tick)
        {
            string nodename = node.Name;
            int boneIndex = modelBones[nodename];
           
            Vector3D pos;
            Quaternion rot;
            Vector3D scale;
            Matrix nodeTransformation = AssimpToXnaMatrix(node.Transform);

            if (modelBones.ContainsKey(nodename))
            {
                Matrix rotation = Matrix.Identity;
                Matrix position = Matrix.Identity;
                Matrix scaling = Matrix.Identity;
                
                foreach(var chanel in scene.Animations[0].NodeAnimationChannels)
                {
                    // Debug.WriteLine(o + " : " + chanel.NodeName);
                    if (chanel.NodeName == nodename)
                    {
                        //Debug.WriteLine(nodename);
                        foreach (VectorKey vec in Positions[scene.Animations[0].NodeAnimationChannels.IndexOf(chanel)])
                        {
                            if (vec.Time == tick)
                            {
                                pos = vec.Value;
                                position = Matrix.CreateTranslation(pos.X, pos.Y, pos.Z);
                                // lastPosition = position;
                            }
                        }
                        foreach (QuaternionKey vec in Rotations[scene.Animations[0].NodeAnimationChannels.IndexOf(chanel)])
                        {
                            //rotation = lastRotation;
                            if (vec.Time == tick)
                            {
                                rot = vec.Value;
                                //Debug.WriteLine(o + " ; "+rot + " ; " + nodename);
                                rotation = Matrix.CreateFromQuaternion(QuatToQuat(rot));
                                //lastRotation = rotation;
                            }

                        }
                        foreach (VectorKey vec in Scaling[scene.Animations[0].NodeAnimationChannels.IndexOf(chanel)])
                        {
                            if (vec.Time == tick)
                            {
                                scale = vec.Value;
                                scale.Normalize();
                                //lastScaleing = scale.;
                                scaling = Matrix.CreateScale(scale.X, scale.Y, scale.Z);

                            }
                        }
                        //AllTransforms[o] *= rotation;


                        //globalTransformation *= parentTransform;
                        nodeTransformation = position * rotation * scaling;
                    }

                        // Debug.WriteLine("dla noda: " + modelBones[nodename] +" tick: "+tick+" : "+ scaling);
                }
                   
                    
            }

            Matrix GlobalTransformation = parentTransforms * nodeTransformation;
          
            if (sceneBones.ContainsKey(nodename))
            {
                
                    boneTransforms[boneIndex-2] = AssimpToXnaMatrix(GlobalInverseTransform) * GlobalTransformation * AssimpToXnaMatrix(scene.Meshes[0].Bones[sceneBones[nodename]].OffsetMatrix);
                    //  boneTransforms[boneIndex - 2] = nodeTransformation * GlobalTransformation * offsetMatrix;
                    //Debug.WriteLine(" index; " + (boneIndex) + " tick: " + tick + " : " + rotation + " nodename; " + nodename);
                    //  Debug.WriteLine(" index; " + (boneIndex) + " tick: " + tick + " : " + parentTransforms + " nodename; " + nodename);
                
            }
            //Debug.WriteLine("dla noda: " + modelBones[nodename] + " tick: " + tick + " : " + offsetMatrix);
            //Debug.WriteLine(boneIndex);
            //Debug.WriteLine(boneIndex);
            //Debug.WriteLine(scene.Meshes[0].Bones[boneIndex].Name);
            // model.Bones[boneIndex].Transform = globalTransformation * AssimpToXnaMatrix(scene.Meshes[0].Bones[boneIndex].OffsetMatrix);
            // Debug.WriteLine(nodename + " : " + boneIndex);
            //Debug.WriteLine(modelBones[nodename]);



            //boneTransforms[boneIndex] = parentTransform;
            //Debug.WriteLine(" index; " + boneIndex  + " tick: " + tick + " : " + rotation + " nodename; " + nodename);
            // Debug.WriteLine(" index; " + (boneIndex) + " tick: " + tick + " : " + parentTransform + " nodename; " + nodename);



            //Debug.WriteLine(boneIndex + " : " + boneTransforms[boneIndex]);
            foreach (var child in node.Children)
                {
                    myFunk(child, GlobalTransformation, tick);
                }
        }

        public void UpdateTime(float gameTime)
        {
            animationTime += gameTime;

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
                    /*
                    Matrix4x4 nodeTransform = scene.RootNode.Transform;
                    Matrix nodeTransformMatrix = AssimpToXnaMatrix(nodeTransform);
                    string nodeName = scene.RootNode.Name;
                    int boneIndex = boneMapping[nodeName];
                    Matrix offsetMatrix = AssimpToXnaMatrix(scene.Meshes[0].Bones[boneIndex].OffsetMatrix);
                    Matrix animationTransform = nodeTransformMatrix * offsetMatrix;
                    Matrix finalTransform = animationTransform * Matrix.CreateScale(0.5f);
                    SecondMethod(model,finalTransform,animationTime);
                    */


                    int i = 0;
                    foreach (var node in scene.RootNode.Children)
                    {

                        foreach (var child in node.Children)
                        {
                            //Debug.WriteLine(i);
                            //Debug.WriteLine(child.Children.Count);
                            //Debug.WriteLine(child.Name);
                            //myFunk(child, Matrix.Identity);
                            //i++;
                        }
                    }
                }
            }
        }

        public void CalculateBoneTransforms(Node node, Matrix parentTransform, float time)
        {
            string nodeName = node.Name;
            Matrix4x4 nodeTransform = node.Transform;
            Matrix nodeTransformMatrix = AssimpToXnaMatrix(nodeTransform);
            
            // Debug.Write(nodeTransformMatrix + "\n");
            if (modelBones.ContainsKey(nodeName))
            {
                //Debug.Write("robi sie");
                int boneIndex = modelBones[nodeName];

                Matrix offsetMatrix = AssimpToXnaMatrix(scene.Meshes[0].Bones[boneIndex].OffsetMatrix);

                Matrix animationTransform = nodeTransformMatrix * offsetMatrix;
                Matrix finalTransform = animationTransform * parentTransform * Matrix.CreateScale(0.005f);
                Matrix interpolatedMatrix = Matrix.Lerp(Matrix.Identity, finalTransform, time);

                boneTransforms[boneIndex] = interpolatedMatrix;

            }

            for (int i = 0; i < node.ChildCount; i++)
            {
                CalculateBoneTransforms(node.Children[i], nodeTransformMatrix * parentTransform, time);
            }
        }
        public static Matrix AssimpToXnaMatrix(Matrix4x4 assimpMatrix)
        {
            Matrix xnaMatrix = new Matrix();

            xnaMatrix.M11 = assimpMatrix.A1;
            xnaMatrix.M12 = assimpMatrix.A2;
            xnaMatrix.M13 = assimpMatrix.A3;
            xnaMatrix.M14 = assimpMatrix.A4;

            xnaMatrix.M21 = assimpMatrix.B1;
            xnaMatrix.M22 = assimpMatrix.B2;
            xnaMatrix.M23 = assimpMatrix.B3;
            xnaMatrix.M24 = assimpMatrix.B4;

            xnaMatrix.M31 = assimpMatrix.C1;
            xnaMatrix.M32 = assimpMatrix.C2;
            xnaMatrix.M33 = assimpMatrix.C3;
            xnaMatrix.M34 = assimpMatrix.C4;

            xnaMatrix.M41 = assimpMatrix.D1;
            xnaMatrix.M42 = assimpMatrix.D2;
            xnaMatrix.M43 = assimpMatrix.D3;
            xnaMatrix.M44 = assimpMatrix.D4;

            return xnaMatrix;
        }

        public static Matrix ConvertMatrix3x3(Matrix3x3 matrix)
        {
            Matrix result = new Matrix();
            result.M11 = matrix.A1;
            result.M12 = matrix.A2;
            result.M13 = matrix.A3;
            result.M14 = 0;

            result.M21 = matrix.B1;
            result.M22 = matrix.B2;
            result.M23 = matrix.B3;
            result.M24 = 0;

            result.M31 = matrix.C1;
            result.M32 = matrix.C2;
            result.M33 = matrix.C3;
            result.M34 = 0;

            result.M41 = 0;
            result.M42 = 0;
            result.M43 = 0;
            result.M44 = 1;

            return result;

        }

        public Vector3 ToVector3(Vector3D vec)
        {
            return new Vector3(vec.X, vec.Y, vec.Z);
        }


        public static Matrix4x4 QuaternionToRotationMatrix(Quaternion q)
        {
            // normalize the quaternion
            q.Normalize();
           
            // compute the matrix elements
            float xx = q.X * q.X;
            float xy = q.X * q.Y;
            float xz = q.X * q.Z;
            float xw = q.X * q.W;
            float yy = q.Y * q.Y;
            float yz = q.Y * q.Z;
            float yw = q.Y * q.W;
            float zz = q.Z * q.Z;
            float zw = q.Z * q.W;

            // construct the matrix
            Matrix4x4 matrix = new Matrix4x4(
                1 - 2 * (yy + zz), 2 * (xy - zw), 2 * (xz + yw), 0,
                2 * (xy + zw), 1 - 2 * (xx + zz), 2 * (yz - xw), 0,
                2 * (xz - yw), 2 * (yz + xw), 1 - 2 * (xx + yy), 0,
                0, 0, 0, 1
            );

            return matrix;
        }
        public static Matrix4x4 QuaternionToRotationMatrix2(Quaternion q)
        {
            // normalize the quaternion
            q.Normalize();

            // compute the matrix elements
            float s = q.W;
            float xx = q.X * q.X;
            float xy = q.X * q.Y;
            float xz = q.X * q.Z;
            float xw = q.X * q.W;
            float yy = q.Y * q.Y;
            float yz = q.Y * q.Z;
            float yw = q.Y * q.W;
            float zz = q.Z * q.Z;
            float zw = q.Z * q.W;

            // construct the matrix
            Matrix4x4 matrix = new Matrix4x4(
                1 - 2 * (yy - zz), 2 * (xy + zw), 2 * (xz - yw), 0,
                2 * (xy - zw), 1 - 2 * (xx - zz), 2 * (yz + xw), 0,
                2 * (xz + yw), 2 * (yz - xw), 1 - 2 * (xx - yy), 0,
                0, 0, 0, 1
            );

            return matrix;
        }

        public List<Vector3> QuaternionToRotationVectors(Quaternion q)
        {
            List<Vector3> result = new List<Vector3>();
            Vector3 xVector,yVector,zVector;

            // normalize the quaternion
            q.Normalize();

            // compute the matrix elements
            float xx = q.X * q.X;
            float xy = q.X * q.Y;
            float xz = q.X * q.Z;
            float xw = q.X * q.W;
            float yy = q.Y * q.Y;
            float yz = q.Y * q.Z;
            float yw = q.Y * q.W;
            float zz = q.Z * q.Z;
            float zw = q.Z * q.W;

            // compute the x vector
            xVector = new Vector3(
                1 - 2 * (yy + zz),
                2 * (xy + zw),
                2 * (xz - yw)
            );

            // compute the y vector
            yVector = new Vector3(
                2 * (xy - zw),
                1 - 2 * (xx + zz),
                2 * (yz + xw)
            );

            // compute the z vector
            zVector = new Vector3(
                2 * (xz + yw),
                2 * (yz - xw),
                1 - 2 * (xx + yy)
            );

            result.Add(xVector);
            result.Add(yVector);
            result.Add(zVector);

            return result;
        }

        public Microsoft.Xna.Framework.Quaternion QuatToQuat(Quaternion quad)
        {
            Microsoft.Xna.Framework.Quaternion quaternion = new Microsoft.Xna.Framework.Quaternion();
            quaternion.X = quad.X;
            quaternion.Y = quad.Y;
            quaternion.Z = quad.Z;
            quaternion.W = quad.W;
            return quaternion;
        }
    }
}
