using UnityEngine;

public class RoadScroller : MonoBehaviour
{
    public float speed = 5f; // velocidad de movimiento
    public float height = 10f; // altura del sprite

    void Update()
    {
        // mover hacia abajo
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // loop infinito: cuando baja demasiado, lo subimos arriba
        if (transform.position.y <= -height)
        {
            transform.position += new Vector3(0, height * 2, 0);
        }
    }
}
