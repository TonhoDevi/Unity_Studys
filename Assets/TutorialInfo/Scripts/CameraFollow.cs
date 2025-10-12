using UnityEngine;

public class CameraTrueOrbit : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    public Vector3 targetOffset = new Vector3(0f, 1.5f, 0f);
    
    [Header("Distância")]
    public float distance = 8f;
    public float minDistance = 3f;
    public float maxDistance = 15f;
    
    [Header("Ângulos Iniciais")]
    public float startAngleHorizontal = 0f; // 0 = atrás, 90 = direita, 180 = frente, 270 = esquerda
    public float startAngleVertical = 30f; // Inclinação (0 = nível, 90 = direto de cima)
    
    [Header("Controles")]
    public float orbitSpeed = 3f;
    public float zoomSpeed = 3f;
    
    [Header("Limites")]
    public float minVerticalAngle = 5f;
    public float maxVerticalAngle = 80f;
    
    private float horizontalAngle; // Rotação ao redor do eixo Y (órbita horizontal)
    private float verticalAngle;   // Inclinação (quanto acima/abaixo do player)
    private float currentDistance;
    
    void Start()
    {
        horizontalAngle = startAngleHorizontal;
        verticalAngle = startAngleVertical;
        currentDistance = distance;
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    
    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogWarning("Sem target!");
            return;
        }
        
        HandleInput();
        UpdateCameraPosition();
    }
    
    void HandleInput()
    {
        // Esconde/mostra cursor
        if (Input.GetMouseButtonDown(1))
        {
            Cursor.visible = false;
        }
        if (Input.GetMouseButtonUp(1))
        {
            Cursor.visible = true;
        }
        
        // Orbita apenas com botão direito pressionado
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            // Rotação horizontal (ao redor do player)
            horizontalAngle += mouseX * orbitSpeed;
            
            // Rotação vertical (altura da câmera)
            verticalAngle -= mouseY * orbitSpeed;
            verticalAngle = Mathf.Clamp(verticalAngle, minVerticalAngle, maxVerticalAngle);
        }
        
        // Zoom com scroll
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentDistance -= scroll * zoomSpeed;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
        }
        
        // Reset com R
        if (Input.GetKeyDown(KeyCode.R))
        {
            horizontalAngle = startAngleHorizontal;
            verticalAngle = startAngleVertical;
            currentDistance = distance;
        }
    }
    
    void UpdateCameraPosition()
    {
        // Ponto focal (centro da órbita)
        Vector3 focusPoint = target.position + targetOffset;
        
        // Converte ângulos em radianos
        float horizontalRad = horizontalAngle * Mathf.Deg2Rad;
        float verticalRad = verticalAngle * Mathf.Deg2Rad;
        
        // Calcula posição usando trigonometria esférica
        // X e Z definem a posição no círculo horizontal
        // Y define a altura
        float horizontalDistance = currentDistance * Mathf.Cos(verticalRad);
        
        Vector3 position;
        position.x = focusPoint.x + horizontalDistance * Mathf.Sin(horizontalRad);
        position.y = focusPoint.y + currentDistance * Mathf.Sin(verticalRad);
        position.z = focusPoint.z + horizontalDistance * Mathf.Cos(horizontalRad);
        
        // Aplica posição
        transform.position = position;
        
        // Sempre olha para o ponto focal
        transform.LookAt(focusPoint);
    }
    
    void OnDrawGizmos()
    {
        if (target == null || !Application.isPlaying) return;
        
        Vector3 focusPoint = target.position + targetOffset;
        
        // Desenha o ponto focal
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(focusPoint, 0.3f);
        
        // Desenha a órbita horizontal
        Gizmos.color = Color.cyan;
        float horizontalDist = currentDistance * Mathf.Cos(verticalAngle * Mathf.Deg2Rad);
        DrawCircle(focusPoint, horizontalDist, 32);
        
        // Linha da câmera até o focal
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, focusPoint);
    }
    
    void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                0,
                Mathf.Sin(angle) * radius
            );
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}