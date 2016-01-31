using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//=================================================================================================================

namespace jUtility.Geo
{

	public static class MeshUtil 
	{
        public static void MergeSkinnedMeshes(SkinnedMeshRenderer targetRenderer, SkinnedMeshRenderer[] meshesToMerge)
        {
            List<CombineInstance> combineInstances = new List<CombineInstance>();
            List<BoneWeight> boneWeights = new List<BoneWeight>();
            List<Transform> bones = new List<Transform>();

            int boneOffset = 0;


            for (var i = 0; i < meshesToMerge.Length; ++i)
            {
                SkinnedMeshRenderer currentRenderer = meshesToMerge[i];
                Mesh currentMesh = currentRenderer.sharedMesh;

                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = currentMesh;
                combineInstances.Add(combineInstance);

                var currentBoneWeights = currentMesh.boneWeights;
                foreach (BoneWeight bw in currentBoneWeights)
                {
                    BoneWeight bWeight = bw;

                    bWeight.boneIndex0 += boneOffset;
                    bWeight.boneIndex1 += boneOffset;
                    bWeight.boneIndex2 += boneOffset;
                    bWeight.boneIndex3 += boneOffset;

                    boneWeights.Add(bWeight);
                }

                boneOffset += currentRenderer.bones.Length;

                Transform[] currentMeshBones = currentRenderer.bones;
                foreach (var bone in currentMeshBones)
                {
                    bones.Add(bone);
                }
            }

            //List<Matrix4x4> bindposes = new List<Matrix4x4>();

            //for (int i = 0; i < bones.Count; i++)
            //{
            //    bindposes.Add(bones[i].worldToLocalMatrix * targetRenderer.transform.worldToLocalMatrix);
            //}

            var resultingMesh = new Mesh();
            resultingMesh.CombineMeshes(combineInstances.ToArray(), true, false);
            resultingMesh.boneWeights = boneWeights.ToArray();
            //resultingMesh.bindposes = bindposes.ToArray();
            resultingMesh.RecalculateBounds();
            targetRenderer.sharedMesh = resultingMesh;
            targetRenderer.bones = bones.ToArray();
            
        }

        public static Mesh MergeStaticMeshes(Mesh[] meshesToMerge)
        {
            List<CombineInstance> combineInstances = new List<CombineInstance>();

            for (var i = 0; i < meshesToMerge.Length; ++i)
            {
                Mesh currentMesh = meshesToMerge[i];
                CombineInstance combineInstance = new CombineInstance();
                combineInstance.mesh = currentMesh;
                combineInstances.Add(combineInstance);
            }

            var resultingMesh = new Mesh();
            resultingMesh.CombineMeshes(combineInstances.ToArray(), true, false);
            resultingMesh.RecalculateBounds();
            return resultingMesh;
        }

	}

}


//=================================================================================================================