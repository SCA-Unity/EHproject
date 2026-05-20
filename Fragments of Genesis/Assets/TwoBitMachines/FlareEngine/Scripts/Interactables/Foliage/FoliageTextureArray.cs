using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TwoBitMachines.FlareEngine.Interactables
{
        [System.Serializable]
        public class FoliageTextureArray
        {
                [SerializeField] public int PPU = 8;
                [SerializeField] public Vector2 size = Vector2.one;
                [SerializeField] public Vector2Int textureSize = new Vector2Int(16, 8);

                [SerializeField] public Mesh mesh;
                [SerializeField] public Material material;
                // MaterialPropertyBlock is not reliably serializable in Unity.
                // Keep it runtime-only to avoid Unity serialization issues.
                [System.NonSerialized] public MaterialPropertyBlock property;
                [SerializeField] public List<FoliageTexture> textures = new List<FoliageTexture>(); //*  all Textures MUST be the same size
                [SerializeField] public List<FoliageInstance> foliage = new List<FoliageInstance>();
                [SerializeField] public Matrix4x4[] matrixList = new Matrix4x4[] { };
                [SerializeField] public float[] instance_textureArrayIndex;
                [SerializeField] public float[] instance_offset;
                [SerializeField] public float[] instance_orientation;

                [SerializeField] private bool createTexture = false;

                public bool GroupTextures ()
                {
                        if (textures.Count == 0 || textures[0].texture == null)
                                return false;

                        Texture2D baseTexture = textures[0].texture;
                        TextureFormat baseFormat = baseTexture.format;

                        for (int i = 0; i < textures.Count; i++)
                        {
                                Texture2D texture = textures[i].texture;
                                if (texture == null)
                                {
                                        Debug.LogWarning("Foliage could not create texture array. Texture is null");
                                        return false;
                                }
                                if (texture.width != textureSize.x || texture.height != textureSize.y)
                                {
                                        Debug.LogWarning("Foliage could not create texture array. Texture " + texture.name + " is not of size " + textureSize + " but of size (" + texture.width + "," + texture.height + ")");
                                        return false;
                                }
                                // Graphics.CopyTexture requires matching texture formats for the same array target.
                                if (texture.format != baseFormat)
                                {
                                        Debug.LogWarning("Foliage could not create texture array. Texture " + texture.name + " has different format (" + texture.format + ") than base texture (" + baseFormat + ").");
                                        return false;
                                }
                        }

                        for (int i = 0; i < foliage.Count; i++)
                        {
                                foliage[i].textureIndex = Mathf.Clamp(foliage[i].textureIndex, 0, textures.Count - 1);
                        }

                        foliage = foliage.OrderBy(w => textures[w.textureIndex].z).ToList(); //  reorder depth
                        Texture2DArray textureArray = CreateTextureArray(textures[0].texture);

                        if (material != null)
                        {
                                material.SetTexture("_Textures", textureArray); //        set texture array
                                if (mesh == null)
                                {
                                        mesh = QuadMesh.Create();
                                        mesh.MarkDynamic();
                                }
                        }
                        return true;
                }

                private Texture2DArray CreateTextureArray (Texture2D texture)
                {
                        Texture2DArray textureArray = new Texture2DArray(texture.width, texture.height, textures.Count + 1, texture.format, false);
                        textureArray.filterMode = FilterMode.Point;

                        Graphics.CopyTexture(BlankTexture(texture), 0, 0, textureArray, 0, 0);

                        for (int i = 0; i < textures.Count; i++)
                        {
                                if (textures[i].texture == null)
                                        textures[i].texture = texture;
                                Graphics.CopyTexture(textures[i].texture, 0, 0, textureArray, i + 1, 0);
                        }
                        return textureArray;
                }

                private Texture2D BlankTexture (Texture2D texture)
                {
                        Texture2D blankTexture = new Texture2D(texture.width, texture.height, texture.format, false); //* create a clear texture to set as first texture for textureArray. This will avoid weird artifacts when rending other textures in scene view.
                        for (int y = 0; y < texture.height; y++)
                                for (int x = 0; x < texture.width; x++)
                                {
                                        blankTexture.SetPixel(x, y, Color.clear);
                                }
                        blankTexture.Apply();
                        return blankTexture;
                }

                public void ClearDataLists ()
                {
                        instance_textureArrayIndex = new float[0];
                        instance_offset = new float[0];
                        instance_orientation = new float[0];
                        matrixList = new Matrix4x4[0];
                }

                public void SetInstanceDataArrays ()
                {
                        if (textures.Count == 0 || foliage.Count == 0)
                                return;

                        PPU = PPU <= 0 ? 1 : PPU;
                        size.x = textures[0].texture.width / (float) PPU;
                        size.y = textures[0].texture.height / (float) PPU;

                        instance_textureArrayIndex = new float[foliage.Count];
                        instance_offset = new float[foliage.Count];
                        instance_orientation = new float[foliage.Count];
                        matrixList = new Matrix4x4[foliage.Count];

                        for (int i = 0; i < foliage.Count; ++i)
                        {
                                foliage[i].position.z = textures[foliage[i].textureIndex].z;
                                matrixList[i].SetTRS(foliage[i].position, Quaternion.identity, new Vector3(size.x, size.y, 0));
                                instance_textureArrayIndex[i] = foliage[i].textureIndex + 1; // offset  1
                                instance_orientation[i] = (int) textures[foliage[i].textureIndex].orientation;
                        }
                }

                public void Initialize ()
                {
                        // Optional: if enabled, build the texture array + instance buffers at runtime.
                        if (createTexture)
                        {
                                if (GroupTextures())
                                {
                                        SetInstanceDataArrays();
                                }
                        }

                        CreatePropertyBlock();
                }

                public void CreatePropertyBlock ()
                {
                        if (instance_orientation == null || instance_orientation.Length == 0)
                        {
                                return;
                        }

                        property = new MaterialPropertyBlock();
                        property.SetFloatArray("_TextureIdx", instance_textureArrayIndex);
                        property.SetFloatArray("_Offset", instance_offset);
                        property.SetFloatArray("_Top", instance_orientation);
                }

                public void DrawMeshes ()
                {
                        if (SystemInfo.supportsInstancing && matrixList.Length > 0 && matrixList.Length < 1024 && material != null)
                        {
                                if (property == null)
                                {
                                        property = new MaterialPropertyBlock();
                                        // If Initialize() wasn't called, make a best-effort to populate arrays.
                                        CreatePropertyBlock();
                                }
                                Graphics.DrawMeshInstanced(mesh, 0, material, matrixList, matrixList.Length, property, UnityEngine.Rendering.ShadowCastingMode.Off);
                        }
                }
        }
}
