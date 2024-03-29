﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Quadrant : MonoBehaviour {
    [Header("Seed Settings")]
    [SerializeField] string seed = "";
    [SerializeField] bool useRandomSeed = true;

    [Header("Hole Settings")]
    [SerializeField] int width = 6;
    [SerializeField] int height = 4;
    [SerializeField] [Range(0, 100)] int holeFillPercent = 20;

    public float UnitSize { get { return unitSize; } }
    const float unitSize = 0.5f;

    Square[,] hole;
    System.Random rng;

    Dictionary<Vector3, Square> squares;

    private void Start() {
        if (useRandomSeed) {
            seed = Guid.NewGuid().ToString();
        }
        rng = new System.Random(seed.GetHashCode());

        hole = new Square[(int)(width / unitSize), (int)(height / unitSize)];
        squares = new Dictionary<Vector3, Square>();

        int _maxVertexIndex = GenerateHole();

        // DrawDebugLines();
        GenerateMesh(_maxVertexIndex);
    }

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
        for(int x = 0 ; x < hole.GetLength(0) ; x++) {
            for(int y = 0 ; y < hole.GetLength(1) ; y++) {
                if(!(x == 0)) { // left edge
                    hole[x,y].left = hole[x-1,y];
                } else {
                    hole[x,y].left = new Square();
                }
                if(!(y == 0)) { // top edge
                    hole[x,y].above = hole[x,y-1];
                } else {
                    hole[x,y].above = new Square();
                }
                if(!(x == hole.GetLength(0) - 1)) { // right edge
                    hole[x,y].right = hole[x+1,y];
                } else {
                    hole[x,y].right = new Square();
                }
                if(!(y == hole.GetLength(1) - 1)) { // bottom edge
                    hole[x,y].below = hole[x,y+1];
                } else {
                    hole[x,y].below = new Square();
                }
            }
        }
    }

    private void CreateShape() {
        // Generate a Basic Shape
        for (int x = 0; x < hole.GetLength(0); x++) {
            for (int y = 0; y < hole.GetLength(1); y++) {
                if (hole[x, y].IsAtEdge()) {
                    int _next = rng.Next(100);
                    hole[x, y].filled = _next > holeFillPercent ? 0 : 1;
                }
            }
        }

        do
        {
            // Make sure (1) everything is connected
            FixConnections();

            // and (2) the amount of squares % 5 == 0
            int _extraSquares = GetExtraSquares();
            while(_extraSquares > 0) {
                _extraSquares += RemoveExtraSquares();
            }
        } while(FindSubHoles().Count > 1);
    }

    private List<List<Square>> FindSubHoles() {
        List<List<Square>> _subHoles = new List<List<Square>>();
        Dictionary<Square, int> _visited = new Dictionary<Square, int>();

        for(int x = 0 ; x < hole.GetLength(0) ; x++) {
            for(int y = 0 ; y < hole.GetLength(1) ; y++) {
                Square _curr = hole[x,y];
                if(_curr.filled == 1) continue;
                if(!_visited.ContainsKey(_curr)) {
                    List<Square> _subHole = new List<Square>();
                    FindSubHolesRecursive(_curr, ref _subHole, ref _visited);
                    _subHoles.Add(_subHole);
                }
            }
        }

        return _subHoles;
    }

    private void FindSubHolesRecursive(Square _curr, ref List<Square> _subHole, ref Dictionary<Square, int> _visited) {
        if(_curr.filled == 1) return;
        if(_visited.ContainsKey(_curr)) return;

        _visited.Add(_curr, 1);
        _subHole.Add(_curr);

        FindSubHolesRecursive(_curr.left, ref _subHole, ref _visited);
        FindSubHolesRecursive(_curr.above, ref _subHole, ref _visited);
        FindSubHolesRecursive(_curr.right, ref _subHole, ref _visited);
        FindSubHolesRecursive(_curr.below, ref _subHole, ref _visited);
    }

    private void FixConnections() {
        List<List<Square>> _subHoles = FindSubHoles();

        // get the index of the longest
        int _idxLongest = 0;
        for(int i = 0 ; i < _subHoles.Count ; i++) {
            if(_subHoles[i].Count > _subHoles[_idxLongest].Count) _idxLongest = i;
        }

        // fill out the little subHoles
        for(int x = 0 ; x < hole.GetLength(0) ; x++) {
            for(int y = 0 ; y < hole.GetLength(1) ; y++) {
                for (int i = 0; i < _subHoles.Count ; i++) {
                    if(i == _idxLongest) continue;
                    for(int j = 0 ; j < _subHoles[i].Count ; j++) {
                        if(hole[x,y] == _subHoles[i][j]) {
                            hole[x,y].filled = 1;
                        }
                    }
                }
            }
        }
    }

    private int RemoveExtraSquares() {
        // start at random x position so that we're not always eating from the top left corner
        for (int x = rng.Next(hole.GetLength(0)); x < hole.GetLength(0); x++) {
            for (int y = 0; y < hole.GetLength(1); y++) {
                if(hole[x,y].filled == 1) continue;
                if (hole[x, y].IsAtEdge()) {
                    hole[x, y].filled = 1;
                    return -1;
                }
            }
        }
        return 0;
    }

    private int GetExtraSquares() {
        int _activeSquares = 0;
        for(int x = 0 ; x < hole.GetLength(0) ; x++) {
            for(int y = 0 ; y < hole.GetLength(1) ; y++) {
                if(hole[x,y].filled == 0) _activeSquares++;
            }
        }

        return _activeSquares % 5;
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
        _mesh.name = $"Mesh {gameObject.name}";

        _mesh.vertices = _vertices;
        _mesh.triangles = _triangles.ToArray();

        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = _mesh;

        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();

        GetComponent<MeshCollider>().sharedMesh = _mesh;
    }

    public bool IsInHole(Vector3 _position) {
        return (_position.x >= transform.position.x - (width / 2) &&
                _position.x <= transform.position.x + (width / 2) &&
                _position.y <= transform.position.y + (height / 2) &&
                _position.y >= transform.position.y - (height / 2));
    }

    public Square GetNearestSquare(Vector3 _position) {
        Square nearest = hole[0,0];

        // todo this is ridiculously inefficient to compute on every piece placement
        //  probably better to have hole be a tree instead and search that way
        for(int x = 0 ; x < hole.GetLength(0) ; x++) {
            for(int y = 0 ; y < hole.GetLength(1) ; y++) {
                float dist_to_nearest = Vector3.Distance(_position, nearest.GetMiddlePosition());
                if(Vector3.Distance(_position, hole[x,y].GetMiddlePosition()) < dist_to_nearest) {
                    nearest = hole[x,y];
                }
            }
        }

        return nearest;
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

        public Vector3 GetMiddlePosition()
        {
            float x = ((topLeft.position.x * bottomRight.position.y - topLeft.position.y * bottomRight.position.y) * (bottomLeft.position.x - topRight.position.x) - 
                (topLeft.position.x - bottomRight.position.x) * (bottomLeft.position.x * topRight.position.y - bottomLeft.position.y * topRight.position.x)) /
                ((topLeft.position.x - bottomRight.position.x) * (bottomLeft.position.y - topRight.position.y) - (topLeft.position.y - bottomRight.position.y) * (bottomLeft.position.y - topRight.position.x));
            float y = ((topLeft.position.x * bottomRight.position.y - topLeft.position.y * bottomRight.position.y) * (bottomLeft.position.y - topRight.position.y) - 
                (topLeft.position.y - bottomRight.position.y) * (bottomLeft.position.x * topRight.position.y - bottomLeft.position.y * topRight.position.x)) /
                ((topLeft.position.x - bottomRight.position.x) * (bottomLeft.position.y - topRight.position.y) - (topLeft.position.y - bottomRight.position.y) * (bottomLeft.position.y - topRight.position.x));

            return new Vector3(x, y, -5);
        }

        public Square() {
            filled = 1;
        }

        public bool IsFilled() {
            return filled != 0;
        }

        public bool IsAtEdge() {
            return (left.filled == 1 || above.filled == 1 || right.filled == 1 || below.filled == 1);
        }
    }

    private void OnDrawGizmos() {
        if(hole == null) return;
        for(int x = 0 ; x < hole.GetLength(0) ; x++) {
            for(int y = 0 ; y < hole.GetLength(1) ; y++) {
                if(!hole[x,y].IsFilled()) {
                    Handles.Label(hole[x,y].GetMiddlePosition(), $"({x},{y})");
                }
            }
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
