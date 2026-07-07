using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    /// <summary>
    /// Klasa służąca jako pomost pomiędzy nowym systemem wejścia (Input System) a kontrolerem postaci gracza (ThirdPersonController).
    /// Zbiera dane wejściowe z przypisanych akcji (poruszanie, celowanie, skok, sprint) i udostępnia je jako zmienne publiczne.
    /// Zawiera również logikę odpowiedzialną za ukrywanie i blokowanie kursora myszy.
    /// </summary>
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        /// <summary> Wektor 2D reprezentujący kierunek poruszania się postaci. </summary>
        public Vector2 move;
        
        /// <summary> Wektor 2D reprezentujący kierunek poruszania myszą (obrót kamery). </summary>
        public Vector2 look;
        
        /// <summary> Flaga określająca, czy klawisz skoku jest obecnie wciśnięty. </summary>
        public bool jump;
        
        /// <summary> Flaga określająca, czy klawisz sprintu jest obecnie wciśnięty. </summary>
        public bool sprint;

        [Header("Movement Settings")]
        /// <summary> Zmienna przełączająca rodzaj wejścia na obsługę analogową (np. gałki z kontrolera, uwzględniające stopniowanie wychylenia). </summary>
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        /// <summary> Flaga ustawiająca, czy po aktywacji okna gry kursor powinien zostać zablokowany. </summary>
        public bool cursorLocked = true;
        
        /// <summary> Flaga określająca, czy ruch kursora myszy ma wpływać na obrót kamery/rozglądanie się. </summary>
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// Wywołanie zdarzenia (Callback) z Input System przy wykryciu wejścia dla ruchu (Move).
        /// </summary>
        /// <param name="value">Zawartość otrzymanej wartości wejścia, zrzutowana na Vector2.</param>
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        /// <summary>
        /// Wywołanie zdarzenia (Callback) z Input System przy wykryciu wejścia dla obrotu/celowania (Look).
        /// </summary>
        /// <param name="value">Zawartość otrzymanej wartości wejścia, zrzutowana na Vector2.</param>
        public void OnLook(InputValue value)
        {
            if(cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        /// <summary>
        /// Wywołanie zdarzenia (Callback) z Input System przy wykryciu naciśnięcia klawisza skoku.
        /// </summary>
        /// <param name="value">Zawartość otrzymanej wartości wejścia opisującej stan przycisku.</param>
        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        /// <summary>
        /// Wywołanie zdarzenia (Callback) z Input System przy wykryciu naciśnięcia klawisza sprintu.
        /// </summary>
        /// <param name="value">Zawartość otrzymanej wartości wejścia opisującej stan przycisku.</param>
        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
#endif

        /// <summary>
        /// Nadpisuje wewnętrzną wartość wektora poruszania z zachowaniem nowej wartości wejścia.
        /// </summary>
        /// <param name="newMoveDirection">Nowy wektor ruchu.</param>
        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        } 

        /// <summary>
        /// Nadpisuje wewnętrzną wartość wektora rozglądania z zachowaniem nowej wartości wejścia.
        /// </summary>
        /// <param name="newLookDirection">Nowy wektor obrotu.</param>
        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        /// <summary>
        /// Aktualizuje stan wciśnięcia przycisku skoku.
        /// </summary>
        /// <param name="newJumpState">Nowy stan przycisku skoku (prawda lub fałsz).</param>
        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        /// <summary>
        /// Aktualizuje stan wciśnięcia przycisku sprintu.
        /// </summary>
        /// <param name="newSprintState">Nowy stan przycisku sprintu (prawda lub fałsz).</param>
        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        /// <summary>
        /// Metoda automatyczna wywoływana przez Unity za każdym razem, gdy aplikacja zyskuje lub traci focus (aktywację okna).
        /// </summary>
        /// <param name="hasFocus">Stan (true - aktywna, false - zminimalizowana).</param>
        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        /// <summary>
        /// Ustawia ukrycie oraz fizyczne związanie (zamknięcie wewnątrz okna) kursora, odpowiednio do przekazanego parametru.
        /// </summary>
        /// <param name="newState">Jeżeli prawda, kursor jest ukrywany i centrowany, jeśli fałsz, kursor zostaje uwolniony.</param>
        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}