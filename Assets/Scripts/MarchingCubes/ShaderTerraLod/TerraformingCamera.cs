using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerraformingCamera : MonoBehaviour
{
    [SerializeField] private bool useVisual;

    [SerializeField] private GameObject visualIndicator;
    [Range(1,5)]
    public float BrushSize = 2f;
    [Range(0.01f, 1)]
    public float BrushStrength = 1f;

    Vector3 _hitPoint;
    Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (useVisual)
        {
            float scaleFactor = BrushSize * 1.5f;
            visualIndicator.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            BrushSize = Mathf.Min(5, BrushSize + 1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            BrushSize = Mathf.Max(1, BrushSize - 1);
        }
    }

    private void LateUpdate()
    {
        if(!useVisual)
        {
            if (Input.GetMouseButton(0))
            {
                Terraform(true);
            }
            else if (Input.GetMouseButton(1))
            {
                Terraform(false);
            }
        }
        else
        {
            bool hasHit = false;
            RaycastHit hit;
            if (Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition),
                out hit, 1000, 7))
            {
                hasHit = true;
                visualIndicator.transform.position = hit.point;
                visualIndicator.SetActive(true);
            }
            else
            {
                hasHit = false;
                visualIndicator.SetActive(false);
            }

            if (Input.GetMouseButton(0))
            {
                if (!hasHit) return;
                Terraform(true, hit);
            }
            else if (Input.GetMouseButton(1))
            {
                if (!hasHit) return;
                Terraform(false, hit);
            }
        }
    }

    private void Terraform(bool add)
    {
        RaycastHit hit;

        if (
            Physics.Raycast(_cam.ScreenPointToRay(Input.mousePosition),
            out hit, 1000)
        )
        {
            Chunk hitChunk = hit.collider.gameObject.GetComponent<Chunk>();

            _hitPoint = hit.point;

            hitChunk.EditWeights(_hitPoint, BrushSize, add, BrushStrength);
        }
    }

    private void Terraform(bool add, RaycastHit hit)
    {
        Chunk hitChunk = hit.collider.gameObject.GetComponent<Chunk>();

        _hitPoint = hit.point;

        hitChunk.EditWeights(_hitPoint, BrushSize, add, BrushStrength);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_hitPoint, BrushSize);
    }
}
