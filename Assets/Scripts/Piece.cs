using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
    private bool isSelected = false;
    private bool isPlaced = false;
    private ToolBox toolBox;

    private void Start() {
        toolBox = FindObjectOfType<ToolBox>();
    }

    private void Update() {
        if(isPlaced) return;

        if(isSelected) {
            if (Input.GetMouseButtonDown(0)) {
                Place();
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

            Vector3 _position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(_position.x, _position.y, -6);
        } else {
            if (Input.GetMouseButtonDown(0)) {
                Select();
            }
        }
    }

    private void Select() {
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        // Casts the ray and get the first game object hit
        if(hit.transform == transform) {
            isSelected = true;
        }
    }

    private void Place() {
        isSelected = false;
        if(toolBox.IsInside(transform.position)) {
            transform.position = toolBox.GetPositionForPiece(this.GetInstanceID());
        } else {
            toolBox.PopPiece(this.GetInstanceID());
            isPlaced = true;
        }
    }

    private void Flip() {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    private void Rotate(bool _cw) {
        transform.Rotate(0, 0, (_cw) ? 90 : -90);
    }
}
