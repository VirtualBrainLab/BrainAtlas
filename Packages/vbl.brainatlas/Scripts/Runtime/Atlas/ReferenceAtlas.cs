using BrainAtlas.ScriptableObjects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace BrainAtlas
{

    /// <summary>
    /// A Reference Atlas is a collection of metadata about the dimensions and axis directions
    /// of an atlas. To use, instantiate a copy and set the Dimensions, AxisDirections, 
    /// </summary>
    public class ReferenceAtlas
    {
        #region Variables
        private ReferenceAtlasData _data;
        private Texture3D _annotationTexture3D;
        private Texture3D _referenceTexture3D;
        private int[] _annotationIDs;
        private Material _defaultMaterial;
        #endregion

        #region Properties
        public string Name { get => _data.name; }

        /// <summary>
        /// Dimensions of the Atlas
        /// </summary>
        public Vector3 Dimensions { get => _data.Dimensions; }

        public Vector3 DimensionsIdx
        {
            get
            {
                return Vector3.Scale(Dimensions, ResolutionInverse) * 1000f;
            }
        }

        /// <summary>
        /// The resolution of the atlas is the um each voxel takes up on (x,y,z)
        /// </summary>
        public Vector3 Resolution { get => _data.Resolution; }

        private Vector3 ResolutionInverse { get { return new Vector3(
            1000f / _data.Resolution.x, 
            1000f / _data.Resolution.y, 
            1000f / _data.Resolution.z); } }

        /// <summary>
        /// The zero offset indicates the position of the (0,0,0) coordinate in the ReferenceAtlas
        /// relative to the world (0,0,0) coordinate.
        /// 
        /// It should be used by the W2A and A2W functions to transform coordinates
        /// </summary>
        public Vector3 ZeroOffset { get => _data.ZeroOffset; }

        /// <summary>
        /// The reference coordinate is the point from which all transformed coordinates will be measured
        /// 
        /// For example, in the CCF this defaults to Bregma.
        /// </summary>
        public Vector3 ReferenceCoordinate
        { 
            get => _data.ReferenceCoordinate; 
            set => _data.ReferenceCoordinate = value; 
        }

        public Transform ParentT;

        public Dictionary<int, Vector3> MeshCenters;

        public Vector3 MeshCentersIdx(int meshID)
        {
            return Vector3.Scale(MeshCenters[meshID], ResolutionInverse);
        }

        public Ontology Ontology;

        public Texture3D AnnotationTexture { get {
                if (_annotationTexture3D == null)
                    throw new Exception("(RA) Annotation texture is not loaded");
                return _annotationTexture3D; 
            } }

        public Texture3D ReferenceTexture { get
            {
                if (_annotationTexture3D == null)
                    throw new Exception("(RA) Annotation texture is not loaded");
                return _referenceTexture3D; } }

        public int[] DefaultAreas { get { return _data.DefaultAreas; } }
        #endregion

        /// <summary>
        /// Create a new ReferenceAtlas object using the Data
        /// 
        /// Child mesh objects will be attached to the parentT transform and will initialize with the defaultMaterial
        /// </summary>
        /// <param name="referenceAtlasData"></param>
        /// <param name="parentT"></param>
        /// <param name="defaultMaterial"></param>
        public ReferenceAtlas(ReferenceAtlasData referenceAtlasData, Transform parentT, Material defaultMaterial,
            bool loadAnnotations = false, bool loadAnnotationTex = false, bool loadReferenceTex = false)
        {
            _data = referenceAtlasData;
            ParentT = parentT;
            _defaultMaterial = defaultMaterial;

            Load_Deserialize();

            if (loadAnnotations)
                _ = LoadAnnotations();
            if (loadAnnotationTex)
                _ = LoadAnnotationTexture();
            if (loadReferenceTex)
                _ = LoadReferenceTexture();
        }

        #region Loading
        /// <summary>
        /// Some of the data in the ReferenceAtlasData needs to be de-serialized from lists for easier access, we do that here
        /// </summary>
        private void Load_Deserialize()
        {
            MeshCenters = new();
            foreach (var tuple in _data._privateMeshCenters)
                MeshCenters.Add(tuple.i, tuple.v3);

            Ontology = new Ontology(_data._privateOntologyData, ParentT, _defaultMaterial);
        }

        public async Task<Texture3D> LoadAnnotationTexture()
        {
            var loadHandler = AddressablesRemoteLoader.LoadTexture(true);
            await loadHandler;

            _annotationTexture3D = loadHandler.Result;

            return _annotationTexture3D;
        }

        public async Task<Texture3D> LoadReferenceTexture()
        {
            var loadHandler = AddressablesRemoteLoader.LoadTexture(false);
            await loadHandler;

            _referenceTexture3D = loadHandler.Result;

            return _referenceTexture3D;
        }

        public async Task<int[]> LoadAnnotations()
        {
            var loadHandler = AddressablesRemoteLoader.LoadAnnotationIDs();
            await loadHandler;

            _annotationIDs = loadHandler.Result;

            return _annotationIDs;
        }
        #endregion

        /// <summary>
        /// Move a coordinate from world space into the ReferenceAtlas space in um
        /// </summary>
        /// <param name="worldCoord"></param>
        /// <returns></returns>
        public Vector3 World2Atlas(Vector3 worldCoord)
        {
            return World2Atlas_Vector(worldCoord + ReferenceCoordinate) - ZeroOffset;
        }

        /// <summary>
        /// Move a coordinate from world space into the ReferenceAtlas voxel space
        /// </summary>
        /// <param name="worldCoord"></param>
        /// <returns></returns>
        public Vector3 World2AtlasIdx(Vector3 worldCoord)
        {
            return World2Atlas(Vector3.Scale(ResolutionInverse, worldCoord));
        }

        /// <summary>
        /// Move a coordinate from the ReferenceAtlas space into world space
        /// </summary>
        /// <param name="atlasCoord"></param>
        /// <returns></returns>
        public Vector3 Atlas2World(Vector3 atlasCoord)
        {
            return Atlas2World_Vector(atlasCoord + ZeroOffset) - ReferenceCoordinate;
        }

        public Vector3 AtlasIdx2World(Vector3 atlasIdxCoord)
        {
            return Vector3.Scale(Atlas2World(atlasIdxCoord), Resolution);
        }

        /// <summary>
        /// Rotate a normalized vector in the ReferenceAtlas space into world space
        /// </summary>
        /// <param name="normalizedAtlasVector"></param>
        /// <returns></returns>
        public Vector3 Atlas2World_Vector(Vector3 normalizedAtlasVector)
        {
            return new Vector3(normalizedAtlasVector.y, -normalizedAtlasVector.z, -normalizedAtlasVector.x);
        }

        /// <summary>
        /// Flip the Unity (x,y,z) axis directions of a vector in the world space
        /// into the ReferenceAtlas space
        /// 
        /// We define all reference atlases as being in (ap, ml, dv)
        /// </summary>
        /// <param name="normalizedWorldVector"></param>
        /// <returns></returns>
        public Vector3 World2Atlas_Vector(Vector3 normalizedWorldVector)
        {
            return new Vector3(-normalizedWorldVector.z, normalizedWorldVector.x, -normalizedWorldVector.y);
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is ReferenceAtlas space &&
                   Name == space.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        /// <summary>
        /// Get the annotation value at a specified index coordinate
        /// </summary>
        /// <param name="coordIdx"></param>
        /// <returns></returns>
        public int AnnotationIdx(Vector3 coordIdx)
        {
            if (_annotationIDs == null)
                throw new Exception("(RA) Annotations are not loaded");
            return _annotationIDs[Mathf.RoundToInt(coordIdx.z * (Dimensions.z * Dimensions.y) + coordIdx.y * Dimensions.y + coordIdx.x)];
        }

        /// <summary>
        /// Get the annotation value at a specified mm coordinate
        /// </summary>
        /// <param name="coordmm"></param>
        /// <returns></returns>
        public int Annotation(Vector3 coordmm)
        {
            if (_annotationIDs == null)
                throw new Exception("(RA) Annotations are not loaded");
            return _annotationIDs[Mathf.RoundToInt(Mathf.RoundToInt(coordmm.z / Resolution.z) * (Dimensions.z * Dimensions.y) + Mathf.RoundToInt(coordmm.y / Resolution.y) * Dimensions.y + Mathf.RoundToInt(coordmm.x / Resolution.x))];
        }
    }

    /// <summary>
    /// An ontology is a hierarchical tree of brain region structures
    /// 
    /// The ontology doesn't represent the tree directly, it just represents the mappings between region ID #s and
    /// colors, parents, and children. The ontology nodes themselves hold links to the actual mesh files
    /// </summary>
    public class Ontology
    {
        private Dictionary<int, (string acronym, string name, Color color, int[] path)> _ontologyData;
        private Dictionary<string, int> _acronym2id;

        private Material _defaultMaterial;

        public Color ID2Color(int areaID)
        {
            return _ontologyData[areaID].color;
        }

        public string ID2Acronym(int areaID)
        {
            return _ontologyData[areaID].acronym;
        }

        public int Acronym2ID(string acronym)
        {
            return _acronym2id[acronym];
        }

        public string ID2Name(int areaID)
        {
            return _ontologyData[areaID].name;
        }

        public int[] ID2Path(int areaID)
        {
            return _ontologyData[areaID].path;
        }

        private Dictionary<int, OntologyNode> _nodes;

        public OntologyNode ID2Node(int areaID)
        {
            return _nodes[areaID];
        }

        /// <summary>
        /// 
        /// </summary>
        public Ontology(List<OntologyTuple> ontologyData, Transform parentT, Material defaultMaterial)
        {
            _defaultMaterial = defaultMaterial;

            _ontologyData = new();

            foreach (OntologyTuple ontologyTuple in ontologyData)
                _ontologyData.Add(ontologyTuple.id,
                    (ontologyTuple.acronym,
                    ontologyTuple.name,
                    ontologyTuple.color,
                    ontologyTuple.structure_id_path));

            ParseData(parentT);
        }

        private void ParseData(Transform parentT)
        {
            _acronym2id = new();
            _nodes = new();

            foreach (var dataKVP in _ontologyData)
            {
                var nodeData = dataKVP.Value;
                // build the reverse dictionary
                _acronym2id.Add(nodeData.acronym, dataKVP.Key);

                // build the ontology tree
                OntologyNode cNode = new OntologyNode();
                cNode.SetData(dataKVP.Key, nodeData.acronym, nodeData.color, parentT, _defaultMaterial);

                _nodes.Add(dataKVP.Key, cNode);
            }
        }
    }

    [Serializable]
    public class OntologyNode
    {
        public enum OntologyNodeSide
        {
            Left = -1,
            Full = 0,
            Right = 1,
            All = 3
        }

        #region Private vars
        private int _id;
        private string _acronym;
        private Transform _atlasParentT;

        private Color _defaultColor;
        private Color _overrideColor;

        private Material _defaultMaterial;

        private TaskCompletionSource<bool> _sideLoadedSource = new();
        private TaskCompletionSource<bool> _fullLoadedSource = new();

        #endregion

        #region Public accessors
        public Task SideLoaded { get { return _sideLoadedSource.Task; } }
        public Task FullLoaded { get { return _fullLoadedSource.Task; } }

        public Transform Transform { get { return ParentGO.transform; } }

        public Color Color { get
            {
                if (_overrideColor != null)
                    return _overrideColor;
                else
                    return _defaultColor;
            } 
        }
        #endregion

        #region GameObjects
        private GameObject _parentGO;
        private GameObject _leftGO;
        private GameObject _rightGO;
        private GameObject _fullGO;

        public GameObject ParentGO { get { return _parentGO; } private set { _parentGO = value; } }
        public GameObject LeftGO { get { return _leftGO; } private set { _leftGO = value; } }
        public GameObject RightGO { get { return _rightGO; } private set { _rightGO = value; } }
        public GameObject FullGO { get { return _fullGO; } private set { _fullGO = value; } }
        #endregion

        public void SetData(int id, string acronym, Color defaultColor, Transform parentTransform, 
            Material defaultMaterial)
        {
            _id = id;
            _acronym = acronym;
            _defaultColor = defaultColor;
            _atlasParentT = parentTransform;

            _defaultMaterial = defaultMaterial;
        }

        /// <summary>
        /// 
        /// </summary>
        public Task<bool> LoadMesh(OntologyNodeSide side)
        {
            if (ParentGO == null)
            {
                ParentGO = new GameObject(_acronym);
                ParentGO.transform.SetParent(_atlasParentT, false);
            }

            switch (side)
            {
                case OntologyNodeSide.Left:
                    LoadSided(true, false);
                    return _sideLoadedSource.Task;

                case OntologyNodeSide.Right:
                    LoadSided(false, true);
                    return _sideLoadedSource.Task;

                case OntologyNodeSide.Full:
                    LoadFull();
                    return _fullLoadedSource.Task;

                case OntologyNodeSide.All:
                    LoadFull();
                    // Load the sided models, but don't show them
                    LoadSided(false, false);
                    return _fullLoadedSource.Task;

                default:
                    throw new Exception("(RA) Unreachable code");
            }
        }

        /// <summary>
        /// Reset the node's color to it's default
        /// </summary>
        public void ResetColor(OntologyNodeSide side = OntologyNodeSide.All)
        {
            switch (side)
            {
                case OntologyNodeSide.All:
                    if (_fullGO != null)
                        _fullGO.GetComponent<Renderer>().material.color = _defaultColor;
                    if (_leftGO != null)
                    {
                        _leftGO.GetComponent<Renderer>().material.color = _defaultColor;
                        _rightGO.GetComponent<Renderer>().material.color = _defaultColor;
                    }
                    _overrideColor = _defaultColor;
                    break;

                case OntologyNodeSide.Left:
                    _leftGO.GetComponent<Renderer>().material.color = _defaultColor;
                    break;

                case OntologyNodeSide.Right:
                    _rightGO.GetComponent<Renderer>().material.color = _defaultColor;
                    break;

                case OntologyNodeSide.Full:
                    _fullGO.GetComponent<Renderer>().material.color = _defaultColor;
                    break;
            }
        }

        public void SetColor(Color color, OntologyNodeSide side = OntologyNodeSide.All)
        {
            switch (side)
            {
                case OntologyNodeSide.All:
                    if (_fullGO != null)
                        _fullGO.GetComponent<Renderer>().material.color = color;
                    if (_leftGO != null)
                    {
                        _leftGO.GetComponent<Renderer>().material.color = color;
                        _rightGO.GetComponent<Renderer>().material.color = color;
                    }
                    break;

                case OntologyNodeSide.Left:
                    _leftGO.GetComponent<Renderer>().material.color = color;
                    break;

                case OntologyNodeSide.Right:
                    _rightGO.GetComponent<Renderer>().material.color = color;
                    break;

                case OntologyNodeSide.Full:
                    _fullGO.GetComponent<Renderer>().material.color = color;
                    break;
            }
            _overrideColor = color;
        }

        public void SetMaterial(Material material, OntologyNodeSide side = OntologyNodeSide.All)
        {
            switch (side)
            {
                case OntologyNodeSide.All:
                    if (_fullGO != null)
                        _fullGO.GetComponent<Renderer>().material = material;
                    if (_leftGO != null)
                    {
                        _leftGO.GetComponent<Renderer>().material = material;
                        _rightGO.GetComponent<Renderer>().material = material;
                    }
                    break;

                case OntologyNodeSide.Left:
                    _leftGO.GetComponent<Renderer>().material = material;
                    break;

                case OntologyNodeSide.Right:
                    _rightGO.GetComponent<Renderer>().material = material;
                    break;

                case OntologyNodeSide.Full:
                    _fullGO.GetComponent<Renderer>().material = material;
                    break;
            }
        }

        public void ResetMaterial(OntologyNodeSide side = OntologyNodeSide.All)
        {
            switch (side)
            {
                case OntologyNodeSide.All:
                    if (_fullGO != null)
                        _fullGO.GetComponent<Renderer>().material = _defaultMaterial;
                    if (_leftGO != null)
                    {
                        _leftGO.GetComponent<Renderer>().material = _defaultMaterial;
                        _rightGO.GetComponent<Renderer>().material = _defaultMaterial;
                    }
                    break;

                case OntologyNodeSide.Left:
                    _leftGO.GetComponent<Renderer>().material = _defaultMaterial;
                    break;

                case OntologyNodeSide.Right:
                    _rightGO.GetComponent<Renderer>().material = _defaultMaterial;
                    break;

                case OntologyNodeSide.Full:
                    _fullGO.GetComponent<Renderer>().material = _defaultMaterial;
                    break;
            }
        }

        public void SetShaderProperty(string property, Vector4 value, OntologyNodeSide side = OntologyNodeSide.All)
        {
            switch (side)
            {
                case OntologyNodeSide.All:
                    if (_fullGO != null)
                        _fullGO.GetComponent<Renderer>().material.SetVector(property, value);
                    if (_leftGO != null)
                    {
                        _leftGO.GetComponent<Renderer>().material.SetVector(property, value);
                        _rightGO.GetComponent<Renderer>().material.SetVector(property, value);
                    }
                    break;

                case OntologyNodeSide.Left:
                    _leftGO.GetComponent<Renderer>().material.SetVector(property, value);
                    break;

                case OntologyNodeSide.Right:
                    _rightGO.GetComponent<Renderer>().material.SetVector(property, value);
                    break;

                case OntologyNodeSide.Full:
                    _fullGO.GetComponent<Renderer>().material.SetVector(property, value);
                    break;
            }
        }

        public void SetShaderProperty(string property, float value, OntologyNodeSide side = OntologyNodeSide.All)
        {
            switch (side)
            {
                case OntologyNodeSide.All:
                    if (_fullGO != null)
                        _fullGO.GetComponent<Renderer>().material.SetFloat(property, value);
                    if (_leftGO != null)
                    {
                        _leftGO.GetComponent<Renderer>().material.SetFloat(property, value);
                        _rightGO.GetComponent<Renderer>().material.SetFloat(property, value);
                    }
                    break;

                case OntologyNodeSide.Left:
                    _leftGO.GetComponent<Renderer>().material.SetFloat(property, value);
                    break;

                case OntologyNodeSide.Right:
                    _rightGO.GetComponent<Renderer>().material.SetFloat(property, value);
                    break;

                case OntologyNodeSide.Full:
                    _fullGO.GetComponent<Renderer>().material.SetFloat(property, value);
                    break;
            }
        }

        public void SetVisibility(bool visible, OntologyNodeSide side = OntologyNodeSide.All)
        {
            switch (side)
            {
                case OntologyNodeSide.All:
                    if (_fullGO != null)
                        _fullGO.SetActive(visible);
                    if (_leftGO != null)
                    {
                        _leftGO.SetActive(visible);
                        _rightGO.SetActive(visible);
                    }
                    break;

                case OntologyNodeSide.Left:
                    _leftGO.SetActive(visible);
                    break;

                case OntologyNodeSide.Right:
                    _rightGO.SetActive(visible);
                    break;

                case OntologyNodeSide.Full:
                    _fullGO.SetActive(visible);
                    break;
            }
        }

        private Vector3[] _originalVerticesFull;
        private Vector3[] _originalVerticesLeft;
        private bool _verticesTransformed;

        /// <summary>
        /// Atlas transforms must be applied to *all* live meshes
        /// </summary>

        public void ApplyAtlasTransform(Func<Vector3, Vector3> transformFunction)
        {
            if (!_verticesTransformed)
            {
                if (_fullGO != null)
                    _originalVerticesFull = _fullGO.GetComponent<MeshFilter>().mesh.vertices;
                if (_leftGO != null)
                    _originalVerticesLeft = _leftGO.GetComponent<MeshFilter>().mesh.vertices;
                _verticesTransformed = true;
            }

            if (FullGO != null)
            {
                Vector3[] verticesFullT = new Vector3[_originalVerticesFull.Length];
                for (var i = 0; i < _originalVerticesFull.Length; i++)
                    verticesFullT[i] = _fullGO.transform.InverseTransformPoint(transformFunction(_fullGO.transform.TransformPoint(_originalVerticesFull[i])));
                _fullGO.GetComponent<MeshFilter>().mesh.vertices = verticesFullT;
            }

            if (_leftGO != null)
            {
                Vector3[] verticesLeftT = new Vector3[_originalVerticesLeft.Length];
                for (var i = 0; i < _originalVerticesLeft.Length; i++)
                    verticesLeftT[i] = _leftGO.transform.InverseTransformPoint(transformFunction(_leftGO.transform.TransformPoint(_originalVerticesLeft[i])));
                _leftGO.GetComponent<MeshFilter>().mesh.vertices = verticesLeftT;
                _rightGO.GetComponent<MeshFilter>().mesh.vertices = verticesLeftT;
            }
        }

        public void ResetAtlasTransform()
        {
            if (_fullGO != null)
                _fullGO.GetComponent<MeshFilter>().mesh.vertices = _originalVerticesFull;
            if (_leftGO != null)
            {
                _leftGO.GetComponent<MeshFilter>().mesh.vertices = _originalVerticesLeft;
                _rightGO.GetComponent<MeshFilter>().mesh.vertices = _originalVerticesLeft;
            }
        }

        #region Private helpers
        private bool _fullLoading;
        private bool _sideLoading;

        private async void LoadFull()
        {
            if (_fullLoading) return; // duplicate call
            _fullLoading = true;

            var MeshTask = AddressablesRemoteLoader.LoadMeshPrefab(_id.ToString());
            await MeshTask;

            _fullGO = GameObject.Instantiate(MeshTask.Result, _parentGO.transform);
            _fullGO.name = "Full";
            
            Renderer rend = _fullGO.GetComponent<Renderer>();
            rend.material = _defaultMaterial;
            rend.material.SetColor("_Color", _defaultColor);

            _fullLoadedSource.SetResult(true);
        }

        private async void LoadSided(bool left, bool right)
        {
            if (_sideLoading) return; // duplicate call
            _sideLoading = true;

            var meshTaskL = AddressablesRemoteLoader.LoadMeshPrefab($"{_id}L");
            var meshTaskR = AddressablesRemoteLoader.LoadMeshPrefab($"{_id}R");
            await meshTaskL;

            _leftGO = GameObject.Instantiate(meshTaskL.Result, _parentGO.transform);
            _leftGO.name = "Left";

            Renderer rendL = _leftGO.GetComponent<Renderer>();
            rendL.material = _defaultMaterial;
            rendL.material.SetColor("_Color", _defaultColor);

            _leftGO.SetActive(left);

            await meshTaskR;

            // Reverse the scale to create the right gameobject
            _rightGO = GameObject.Instantiate(meshTaskR.Result, _parentGO.transform);
            _rightGO.name = "Right";

            Renderer rendR = _rightGO.GetComponent<Renderer>();
            rendR.material = _defaultMaterial;
            rendR.material.SetColor("_Color", _defaultColor);

            _rightGO.SetActive(right);

            _sideLoadedSource.SetResult(true);
        }
        #endregion
    }
}
