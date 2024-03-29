﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Piece : MonoBehaviour {
    [SerializeField] Sprite pieceSprite;

    private bool isSelected = false;
    private bool isPlaced = false;
    private ToolBox toolBox;
    private Quadrant[] quadrants;

    private void Awake() {
        if(pieceSprite) {
            Mesh _mesh = SpriteToMesh(pieceSprite);
            GetComponent<MeshFilter>().mesh = _mesh;
            GetComponent<MeshCollider>().sharedMesh = _mesh;
        }
    }

    private void Start() {
        toolBox = FindObjectOfType<ToolBox>();
        quadrants = FindObjectsOfType<Quadrant>();
    }

    private void OnCollisionEnter(Collision _other) {
        if(_other.gameObject.GetComponent<Quadrant>()) {
            Debug.Log($"Collided with {_other.gameObject.name}");
        }
        else {
            Debug.Log($"Sad {_other.gameObject.name}");
        }
    }

    private void Update() {
        if(isPlaced) return;

        if(isSelected) {
            if (Input.GetMouseButtonDown(0)) {
                TryToPlace(); // returns a bool if it was placed but we don' need it yet
                return;
            }
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.F)) {
                Flip();
            }
            if (Input.GetKeyDown(KeyCode.Z)) {
                Rotate(false);
            }
            if (Input.GetKeyDown(KeyCode.X)) {
                Rotate(true);
            }

            // Move the piece under the mouse pointer
            Vector3 _position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(_position.x, _position.y, -6);

            // this snaps movement to a grid-like pattern, might go for it but it needs to align with the quadrant grids
            // transform.position = new Vector3((float)(Math.Round(2 * _position.x, MidpointRounding.AwayFromZero) / 2),
            //                                  (float)(Math.Round(2 * _position.y, MidpointRounding.AwayFromZero) / 2),
            //                                  -6);
        } else {
            if (Input.GetMouseButtonDown(0)) {
                Select();
            }
        }
    }

    private void Select() {
        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit)) {
            if(hit.transform == transform) {
                isSelected = true;
            }
        }

    }

    private bool TryToPlace() {
        isSelected = false;
        if(toolBox.IsInside(transform.position)) {
            transform.position = toolBox.GetPositionForPiece(this.GetInstanceID());
            return true;
        } 
        
        foreach (Quadrant q in quadrants) {
            if(q.IsInHole(transform.position)) {
                var nearestSquare = q.GetNearestSquare(transform.position);
                if(!nearestSquare.IsFilled()) {
                    toolBox.PopPiece(this.GetInstanceID());
                    var placementPosition = nearestSquare.GetMiddlePosition(); 
                    transform.position = new Vector3(placementPosition.x, placementPosition.y, -6);
                    isPlaced = true;
                    return true;
                }
            }
        }

        return false;
    }

    private void Flip() {
        Mesh _mesh = GetComponent<MeshFilter>().mesh;
        _mesh.triangles = _mesh.triangles.Reverse().ToArray();
        _mesh.RecalculateNormals();
        transform.Rotate(0, 180, 0);
    }

    private void Rotate(bool _cw) {
        transform.Rotate(0, 0, (_cw) ? 90 : -90);
    }

    private Mesh SpriteToMesh(Sprite _sprite) {
        Mesh _mesh = new Mesh();
        _mesh.name = $"Piece {pieceSprite.name}";
        _mesh.vertices = Array.ConvertAll(_sprite.vertices, i => (Vector3)i);
        _mesh.uv = _sprite.uv;
        _mesh.triangles = Array.ConvertAll(_sprite.triangles, i => (int)i);
        _mesh.RecalculateBounds();
        _mesh.RecalculateNormals();
        return _mesh;
    }
}
