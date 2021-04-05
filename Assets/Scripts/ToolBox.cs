using System.Collections.Generic;
using UnityEngine;

public class ToolBox : MonoBehaviour {
    [SerializeField][Range(1,6)] int numPieces = 3;
    
    Vector2 size;
    (Vector3,Piece)[] toolboxPieces;
    System.Random rng;

    List<(PieceShape shape, int frequency)> pieceFrequencyDistribution = new List<(PieceShape, int)> {
        ( PieceShape.P , 22 ),   // 22%
        ( PieceShape.F , 37 ),   // 15%
        ( PieceShape.Y , 52 ),   // 15%
        ( PieceShape.L , 60 ),   // 8%
        ( PieceShape.N , 68 ),   // 8%
        ( PieceShape.T , 75 ),   // 7%
        ( PieceShape.U , 80 ),   // 5%
        ( PieceShape.V , 85 ),   // 5%
        ( PieceShape.W , 90 ),   // 5%
        ( PieceShape.Z , 95 ),   // 5%
        ( PieceShape.X , 98 ),   // 3%
        ( PieceShape.I , 100 ),  // 2%
    };
    enum PieceShape { F, I, L, N, P, T, U, V, W, X, Y, Z };

    private void Awake() {
        size = (Vector2)GetComponent<MeshRenderer>().bounds.size;

        rng = new System.Random(Time.time.GetHashCode());

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
        int _next = rng.Next(0, 100);
        Debug.Log($"Next: {_next}");
        PieceShape _shape = GetPieceShape(_next);
        Debug.Log($"Shape: {_shape}");
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
}
