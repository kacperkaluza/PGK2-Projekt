using UnityEngine;

/// <summary>
/// Skrypt pomocniczy podczepiony pod gracza, umozliwiający przepychanie obiektów środowiska wyposażonych w komponent RigidBody.
/// Ignoruje obiekty na warstwach kinetycznych i oblicza poprawny kąt oraz wektor wypchnięcia, chroniąc przed błędami kolizji z podłożem.
/// </summary>
public class BasicRigidBodyPush : MonoBehaviour
{
    /// <summary> Maska warstw decydująca, obiekty jakich warstw mogą zostać popchnięte przez ten skrypt. </summary>
    public LayerMask pushLayers;
    
    /// <summary> Flaga włączająca/wyłączająca zachowanie przepychania z poziomu edytora. </summary>
    public bool canPush;
    
    /// <summary> Siła oddziaływania na inne obiekty fizyczne. </summary>
    [Range(0.5f, 5f)] public float strength = 1.1f;

    /// <summary>
    /// Metoda silnika Unity wywoływana automatycznie w momencie zajścia fizycznego kontaktu gracza wyposażonego
    /// w komponent CharacterController z innym colliderem w otoczeniu.
    /// </summary>
    /// <param name="hit">Parametr zawierający obszerne statystyki opisujące zjawisko kolizji (punkt styku, normalna, wektor ruchu).</param>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (canPush) PushRigidBodies(hit);
    }

    /// <summary>
    /// Logika analizująca zaistniałą kolizję, weryfikująca czy dany obiekt ma właściwy RigidBody oraz warstwę i
    /// nadająca mu impuls (siłę) fizyczną odpychającą go od postaci gracza na płaszczyźnie poziomej.
    /// </summary>
    /// <param name="hit">Referencja do parametrów kolizji.</param>
    private void PushRigidBodies(ControllerColliderHit hit)
    {
        // make sure we hit a non kinematic rigidbody
        Rigidbody body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic) return;

        // make sure we only push desired layer(s)
        var bodyLayerMask = 1 << body.gameObject.layer;
        if ((bodyLayerMask & pushLayers.value) == 0) return;

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3f) return;

        // Calculate push direction from move direction, horizontal motion only
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

        // Apply the push and take strength into account
        body.AddForce(pushDir * strength, ForceMode.Impulse);
    }
}