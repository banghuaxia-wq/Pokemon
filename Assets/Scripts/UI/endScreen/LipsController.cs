using UnityEngine;

public class LipsController : MonoBehaviour
{
    public Collider2D character1Face;
    public Collider2D character2Face;
    public GameObject character1KissEffect;
    public GameObject character2KissEffect;
    public Transform lipsBody; // The stretchable part
    public Transform lipsEnd;  // The lips tip
    public float maxRadius = 3f;
    public float minRadius = 0.2f;

    public GameObject faceCover;
    public GameObject heartHat;

    void Start()
    {
        faceCover.SetActive(false);
    }
    void Update()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector3 anchor = transform.position;
        Vector3 dir = (mouseWorld - anchor).normalized;
        float dist = Mathf.Clamp(Vector3.Distance(mouseWorld, anchor), minRadius, maxRadius);

        // Set lips body position, scale, and rotation
        lipsBody.position = anchor + dir * dist / 2f; // Centered between anchor and tip
        lipsBody.right = dir; // Orient the body
        
        var sr = lipsBody.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.size = new Vector2(dist, sr.size.y);
        }
        
        // Set lips end position and rotation
        lipsEnd.position = anchor + dir * dist;
        lipsEnd.right = dir;

        // Check for collision with character faces
        bool kissing1 = false, kissing2 = false;
        Collider2D lipsTipCollider = lipsEnd.GetComponent<Collider2D>();
        if (lipsTipCollider != null && character1Face != null)
        {
            kissing1 = lipsTipCollider.IsTouching(character1Face);
        }
        if (lipsTipCollider != null && character2Face != null)
        {
            kissing2 = lipsTipCollider.IsTouching(character2Face);
        }

        if (character1KissEffect != null)
            character1KissEffect.SetActive(kissing1);
        if (character2KissEffect != null)
            character2KissEffect.SetActive(kissing2);
        if (faceCover != null)
            faceCover.SetActive(kissing1 || kissing2);
        if (heartHat != null)
            heartHat.SetActive(kissing1 || kissing2);
    }
}