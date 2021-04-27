using System;
using System.Collections.Generic;
using UnityEngine;

public class ToolBox : MonoBehaviour {
    [SerializeField][Range(1,6)] int numPieces = 3;
    [SerializeField][Range(0,255)] int boundClickCoyoteOffset = 0;
    
    Vector2 size;
    (Vector3,Piece)[] toolboxPieces;
    System.Random rng;

    List<(PieceShape shape, int frequency)> pieceFrequencyDistribution = new List<(PieceShape, int)> {
        ( PieceShape.P , 21 ),   // 22%
        ( PieceShape.F , 36 ),   // 15%
        ( PieceShape.Y , 51 ),   // 15%
        ( PieceShape.L , 59 ),   // 8%
        ( PieceShape.N , 67 ),   // 8%
        ( PieceShape.T , 74 ),   // 7%
        ( PieceShape.U , 79 ),   // 5%
        ( PieceShape.V , 84 ),   // 5%
        ( PieceShape.W , 89 ),   // 5%
        ( PieceShape.Z , 94 ),   // 5%
        ( PieceShape.X , 97 ),   // 3%
        ( PieceShape.I , 99 ),  // 2%
    };
    enum PieceShape { F, I, L, N, P, T, U, V, W, X, Y, Z };

    private void Awake() {
        size = (Vector2)GetComponent<MeshRenderer>().bounds.size;

        string _seed = Guid.NewGuid().ToString();
        rng = new System.Random(_seed.GetHashCode());

        toolboxPieces = new (Vector3, Piece)[numPieces];
        SetPieceLocations();

        for(int i = 0 ; i < toolboxPieces.Length ; i++) {
            Vector3 _position = toolboxPieces[i].Item1;
            toolboxPieces[i].Item2 = InstantiatePiece(_position);
        }
    }

    private void Update() {

    }

    private Piece InstantiatePiece(Vector3 _position) {
        int _next = rng.Next(100); // rng is non-inclusive
        PieceShape _shape = GetPieceShape(_next);
        GameObject _prefab =  LoadPiecePrefab(_shape);
        return GameObject.Instantiate(_prefab, _position, Quaternion.identity).GetComponent<Piece>();
    }

    private GameObject LoadPiecePrefab(PieceShape _shape) {
        return (GameObject) Resources.Load($"Pieces/Piece {_shape}");
    }

    private PieceShape GetPieceShape(int _frequencyIdx) {
        for (int i = 0; i < pieceFrequencyDistribution.Count; i++) {
            int _f = pieceFrequencyDistribution[i].frequency;
            int _prevF = (i == 0) ? -1 : pieceFrequencyDistribution[i - 1].frequency;
            if (_frequencyIdx <= _f && _frequencyIdx > _prevF) {
                return pieceFrequencyDistribution[i].shape;
            }
        }
        Debug.LogWarning("Unable to find a matching frequency. Defaulting to shape P");
        return PieceShape.P; // use the most frequent as default
    }

    private void SetPieceLocations() {
        toolboxPieces[0] = (new Vector3(transform.position.x, transform.position.y + size.y / 3, -6), null);
        toolboxPieces[1] = (new Vector3(transform.position.x, transform.position.y, -6), null);
        toolboxPieces[2] = (new Vector3(transform.position.x, transform.position.y - size.y / 3, -6), null);
    }

    public Vector3 GetPositionForPiece(int _pieceID) {
        foreach((Vector3, Piece) _piece in toolboxPieces) {
            if(_pieceID == _piece.Item2.GetInstanceID()) {
                Debug.Log("Found!");
                return _piece.Item1;
            }
        }
        Debug.LogError("Didn't find Position for piece for some reason.");
        return Vector3.zero;
    }

    public void PopPiece(int _pieceID) {
        for(int i = 0 ; i < toolboxPieces.Length ; i++) {
            if(_pieceID == toolboxPieces[i].Item2.GetInstanceID()) {
                Vector3 _pos = toolboxPieces[i].Item1;
                toolboxPieces[i].Item2 = InstantiatePiece(_pos);
            }
        }
    }

    public bool IsInside(Vector3 _position) {
        return (_position.x > (transform.position.x - (size.x/2) - boundClickCoyoteOffset) &&
                _position.x < (transform.position.x + (size.x/2) + boundClickCoyoteOffset) &&
                _position.y > (transform.position.y - (size.y/2) - boundClickCoyoteOffset) &&
                _position.y < (transform.position.y + (size.y/2) + boundClickCoyoteOffset));
    }
}
