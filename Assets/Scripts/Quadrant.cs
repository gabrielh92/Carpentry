using System;
using System.Collections.Generic;
using UnityEngine;

public class Quadrant : MonoBehaviour {
    [Header("Seed Settings")]
    [SerializeField] string seed = "";
    [SerializeField] bool useRandomSeed = true;

    [Header("Hole Settings")]
    [SerializeField] int width = 6;
    [SerializeField] int height = 4;
    [SerializeField] [Range(0, 100)] int holeFillPercent = 20;

    Square[,] hole;
    const float unitSize = 0.5f;

    private void Start() {
        if (useRandomSeed) {
            seed = DateTime.Now.ToString();
        }
        System.Random rng = new System.Random(seed.GetHashCode());

        hole = new Square[(int)(width / unitSize), (int)(height / unitSize)];

        int _maxVertexIndex = GenerateHole();

        DrawDebugLines();
        GenerateMesh(_maxVertexIndex);
    }

    // [0] -> Empty Space
    // [1] -> Filled Space
    private int GenerateHole() {
        int _idx = InitializeSquares();
        CreateShape();
        return _idx;
    }

    private int InitializeSquares() {
        float left = -(width / 2);
        float top = (height / 2);
        int _idx = 0;
        Dictionary<Vector3, Node> nodes = new Dictionary<Vector3, Node>();

        for (int x = 0; x < hole.GetLength(0); x++) {
            for (int y = 0; y < hole.GetLength(1); y++) {
                Node _topLeft = new Node(new Vector3(left + (x * unitSize), top - (y * unitSize), 0));
                Node _topRight = new Node(new Vector3(left + (x * unitSize) + unitSize, top - (y * unitSize), 0));
                Node _bottomRight = new Node(new Vector3(left + (x * unitSize) + unitSize, top - (y * unitSize) - unitSize, 0));
                Node _bottomLeft = new Node(new Vector3(left + (x * unitSize), top - (y * unitSize) - unitSize, 0));

                if (!nodes.ContainsKey(_topLeft.position)) {
                    nodes.Add(_topLeft.position, _topLeft);
                    _topLeft.vertexIndex = _idx;
                    _idx++;
                } else {
                    _topLeft = nodes[_topLeft.position];
                }

                if (!nodes.ContainsKey(_topRight.position)) {
                    nodes.Add(_topRight.position, _topRight);
                    _topRight.vertexIndex = _idx;
                    _idx++;
                } else {
                    _topRight = nodes[_topRight.position];
                }

                if (!nodes.ContainsKey(_bottomRight.position)) {
                    nodes.Add(_bottomRight.position, _bottomRight);
                    _bottomRight.vertexIndex = _idx;
                    _idx++;
                } else {
                    _bottomRight = nodes[_bottomRight.position];
                }

                if (!nodes.ContainsKey(_bottomLeft.position)) {
                    nodes.Add(_bottomLeft.position, _bottomLeft);
                    _bottomLeft.vertexIndex = _idx;
                    _idx++;
                } else {
                    _bottomLeft = nodes[_bottomLeft.position];
                }

                Square _sq = new Square(_topLeft, _topRight, _bottomRight, _bottomLeft);
                hole[x, y] = _sq;
            }
        }

        UpdateNeighbourSquares();
        return _idx;
    }

    private void UpdateNeighbourSquares() {
        // TODO - implement
    }

    private void CreateShape() {
        // TODO - implement
    }

    private void GenerateMesh(int _totalVertices) {
        Vector3[] _vertices = new Vector3[_totalVertices + 1];
        List<int> _triangles = new List<int>();

        for(int x = 0 ; x < hole.GetLength(0) ; x++) {
            for(int y = 0 ; y < hole.GetLength(1) ; y++) {
                if(hole[x,y].filled == 0) {
                    Node _topLeft = hole[x, y].topLeft;
                    Node _topRight = hole[x, y].topRight;
                    Node _bottomRight = hole[x, y].bottomRight;
                    Node _bottomLeft = hole[x, y].bottomLeft;

                    _vertices[_topLeft.vertexIndex] =  _topLeft.position;
                    _vertices[_topRight.vertexIndex] = _topRight.position;
                    _vertices[_bottomRight.vertexIndex] = _bottomRight.position;
                    _vertices[_bottomLeft.vertexIndex] =_bottomLeft.position;

                    _triangles.Add(_topLeft.vertexIndex);
                    _triangles.Add(_topRight.vertexIndex);
                    _triangles.Add(_bottomRight.vertexIndex);

                    _triangles.Add(_topLeft.vertexIndex);
                    _triangles.Add(_bottomRight.vertexIndex);
                    _triangles.Add(_bottomLeft.vertexIndex);
                }
            }
        }

        Mesh _mesh = new Mesh();

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles.ToArray();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = _mesh;

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
    }

    public bool IsInHole(Vector3 _position) {
        return (_position.x >= transform.position.x - (width / 2) &&
                _position.x <= transform.position.x + (width / 2) &&
                _position.y <= transform.position.y + (height / 2) &&
                _position.y >= transform.position.y - (height / 2));
    }

    public class Node {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _position) {
            position = _position;
        }
    }

    public class Square {
        public int filled;
        public Node topLeft, topRight, bottomRight, bottomLeft;

        public Square left, above, right, below;

        public Square(Node _topLeft, Node _topRight, Node _bottomRight, Node _bottomLeft) {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            filled = 0;
        }

        public bool IsAtEdge() {
            return left == null || above == null || right == null || below == null;
        }
    }

    private void DrawDebugLines() {
        float left, right, top, bottom;
        left = transform.position.x - (width / 2);
        right = transform.position.x + (width / 2);
        top = transform.position.y + (height / 2);
        bottom = transform.position.y - (height / 2);

        Debug.DrawLine(new Vector3(left, top, -5), new Vector3(right, top, -5), Color.red, Mathf.Infinity);
        Debug.DrawLine(new Vector3(left, bottom, -5), new Vector3(right, bottom, -5), Color.blue, Mathf.Infinity);
        Debug.DrawLine(new Vector3(left, top, -5), new Vector3(left, bottom, -5), Color.yellow, Mathf.Infinity);
        Debug.DrawLine(new Vector3(right, top, -5), new Vector3(right, bottom, -5), Color.green, Mathf.Infinity);

        for (float i = left; i < right; i += unitSize) {
            Debug.DrawLine(new Vector3(i, top, -5), new Vector3(i, bottom, -5), Color.white, Mathf.Infinity);
        }

        for (float i = bottom; i < top; i += unitSize) {
            Debug.DrawLine(new Vector3(left, i, -5), new Vector3(right, i, -5), Color.white, Mathf.Infinity);
        }
    }
}
