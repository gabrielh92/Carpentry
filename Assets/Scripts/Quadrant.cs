using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
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

        GenerateHoleShape();

        DrawDebugLines();
        GenerateMesh();
    }

    // [0] -> Empty Space
    // [1] -> Filled Space
    private void GenerateHoleShape() {
        float left, top;
        left = transform.position.x - (width / 2);
        top = transform.position.y + (height / 2) - unitSize;

        int _idx = 0;
        Dictionary<Vector3, Node> nodes = new Dictionary<Vector3, Node>();

        for (int x = 0; x < hole.GetLength(0); x++) {
            for (int y = 0; y < hole.GetLength(1); y++) {
                Node _topLeft = new Node(new Vector3(left + (x * unitSize),
                                                     top - (y * unitSize),
                                                     transform.position.z));
                Node _topRight = new Node(new Vector3(left + (x * unitSize) + unitSize,
                                                     top - (y * unitSize),
                                                     transform.position.z));
                Node _bottomRight = new Node(new Vector3(left + (x * unitSize) + unitSize,
                                                         top - (y * unitSize) + unitSize,
                                                         transform.position.z));
                Node _bottomLeft = new Node(new Vector3(left + (x * unitSize),
                                                         top - (y * unitSize) + unitSize,
                                                         transform.position.z));

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

                // draws x's in every cell
                Debug.DrawLine(_topLeft.position, _bottomRight.position, Color.white, Mathf.Infinity);
                Debug.DrawLine(_topRight.position, _bottomLeft.position, Color.white, Mathf.Infinity);

                Square _sq = new Square(_topLeft, _topRight, _bottomRight, _bottomLeft);
                hole[x, y] = _sq;
            }
        }

        UpdateNeighbourSquares();

        hole[0, 0].filled = 1; // TODO - remove
    }

    private void UpdateNeighbourSquares() {
        // TODO - implement
    }

    private void GenerateMesh() {
        List<Vector3> _vertices = new List<Vector3>();
        List<int> _triangles = new List<int>();

        // TODO - remove
        float left, right, top, bottom;
        left = -(width / 2);
        right = (width / 2);
        top = (height / 2);
        bottom = -(height / 2);

        _vertices.Add(new Vector3(left, top, -5));
        _vertices.Add(new Vector3(right, top, -5));
        _vertices.Add(new Vector3(right, bottom, -5));
        _vertices.Add(new Vector3(left, bottom, -5));

        _triangles.Add(0);
        _triangles.Add(1);
        _triangles.Add(2);

        _triangles.Add(0);
        _triangles.Add(2);
        _triangles.Add(3);

        // TODO - generate mesh from nodes in squares

        Mesh _mesh = new Mesh();

        _mesh.vertices = _vertices.ToArray();
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
