using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    /// <summary>
    /// Główny kontroler postaci w widoku trzecioosobowym dostarczany przez system Starter Assets.
    /// Zarządza wejściem od gracza, ruchem po terenie, skokami, grawitacją oraz współpracuje z mechanizmem animacji.
    /// Oblicza również pozycjonowanie kamery we współpracy z narzędziem Cinemachine.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    [RequireComponent(typeof(UnityEngine.InputSystem.PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player")]
        /// <summary> Prędkość poruszania się postaci podczas standardowego chodu (m/s). </summary>
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        /// <summary> Prędkość poruszania się postaci podczas wciśnięcia przycisku sprintu (m/s). </summary>
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        /// <summary> Wskaźnik wygładzania określający, jak szybko postać odwraca się twarzą w kierunku wektora poruszania. </summary>
        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        /// <summary> Mnożnik przyśpieszania i zwalniania używany przy zmianach wektora prędkości. </summary>
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        /// <summary> Referencja źródła dźwięku dla odtwarzania kroków. </summary>
        public AudioSource AudioFootsteps;
        
        /// <summary> Referencja źródła dźwięku dla odtwarzania upadków z wysokości (lądowania). </summary>
        public AudioSource LandingAudio;
        
        /// <summary> Referencja źródła dźwięku do dodatkowych odgłosów wyposażenia (Foley). </summary>
        public AudioSource AudioFoley;
        
        /// <summary> Plik dźwiękowy odtwarzany podczas lądowania. </summary>
        public AudioClip LandingAudioClip;
        
        /// <summary> Tablica plików dźwiękowych odtwarzanych z każdym krokiem. </summary>
        public AudioClip[] FootstepAudioClips;
        
        /// <summary> Głośność odtwarzanych dźwięków kroku. </summary>
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        [Space(10)]
        /// <summary> Docelowa wysokość skoku postaci (kalkulowana poprzez wyliczenie prędkości początkowej względem grawitacji). </summary>
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        /// <summary> Niestandardowa wartość przyspieszenia ziemskiego (grawitacji) nakładana na obiekt kontrolera (-9.81 jest domyślnie w Unity). </summary>
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        /// <summary> Ilość czasu (w sekundach), jaka musi minąć zanim gracz będzie mógł wykonać kolejny skok. </summary>
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        /// <summary> Czas opóźnienia przejścia w stan "opadania" po straceniu gruntu, pomagający w sprawnym schodzeniu po schodach bez migotania animacji. </summary>
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        /// <summary> Flaga diagnostyczna określająca, czy system rozpoznaje gracza jako "dotykającego gruntu". </summary>
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        /// <summary> Margines położenia strefy kolizyjnej sfery testującej grunt; modyfikowany w nierównym terenie. </summary>
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        /// <summary> Wielkość sfery wirtualnej rzucanej na ziemię by sprawdzić oparcie o grunt (powinna odpowiadać promieniowi kontrolera). </summary>
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        /// <summary> Zbiór warstw traktowanych przez gracza jako legalne powierzchnie podłoża (ziemia, schody, platformy). </summary>
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        /// <summary> Obiekt reprezentujący wektor (punkt) powiązany z kamerą, używany przez Cinemachine jako Follow Target. </summary>
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        /// <summary> Górny, maksymalny kąt obrócenia celownika kamery. </summary>
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        /// <summary> Dolny, maksymalny kąt obrócenia celownika kamery. </summary>
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        /// <summary> Dodatkowy modyfikator kąta obrotu nakładany na siłę do transformacji kamery (do ręcznego tuningu perspektywy). </summary>
        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        /// <summary> Przełącznik trwale zablokowujący odczyty ruszania kamerą, utrwalający obraz w punkcie 0,0,0. </summary>
        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // cinemachine
        /// <summary> Docelowy wyliczony kąt odchylenia bocznego obiektywu (skręt/yaw) wysyłany do Cinemachine. </summary>
        private float _cinemachineTargetYaw;
        
        /// <summary> Docelowy wyliczony kąt odchylenia góra-dół obiektywu (pitch) wysyłany do Cinemachine. </summary>
        private float _cinemachineTargetPitch;

        // player
        /// <summary> Faktycznie wyliczana w czasie rzeczywistym prędkość na podłożu interpolowana we wbudowanych krzywych. </summary>
        private float _speed;
        
        /// <summary> Stopień mieszania klatek z maszyny stanów animatora (tzw. Blend Tree). </summary>
        private float _animationBlend;
        
        /// <summary> Obliczony celowy obrót gracza po odczytaniu wektora wejścia (kierunku na analogu lub klawiaturze). </summary>
        private float _targetRotation = 0.0f;
        
        /// <summary> Zmienna używana do metody SmoothDamp opisującej łagodne ruszanie się modelu postaci po osi obrotu (Ref parameter). </summary>
        private float _rotationVelocity;
        
        /// <summary> Komponent prędkości wyznaczany tylko w orientacji pionowej, podatny na grawitację i skok. </summary>
        private float _verticalVelocity;
        
        /// <summary> Prędkość końcowa na jaką postać gracza nie może zrzucić więcej energii w locie. </summary>
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        /// <summary> Aktualny timer zliczający opóźnienia do zresetowania bloku skoku. </summary>
        private float _jumpTimeoutDelta;
        
        /// <summary> Aktualny timer zliczający opóźnienia wejścia w stan upadku swobodnego. </summary>
        private float _fallTimeoutDelta;

        // animation IDs
        /// <summary> Optymalny hash identyfikatora parametru 'Speed' z głównego animatora. </summary>
        private int _animIDSpeed;
        
        /// <summary> Optymalny hash identyfikatora parametru 'Grounded' z głównego animatora. </summary>
        private int _animIDGrounded;
        
        /// <summary> Optymalny hash identyfikatora parametru 'Jump' z głównego animatora. </summary>
        private int _animIDJump;
        
        /// <summary> Optymalny hash identyfikatora parametru 'FreeFall' z głównego animatora. </summary>
        private int _animIDFreeFall;
        
        /// <summary> Optymalny hash identyfikatora parametru 'MotionSpeed' z głównego animatora. </summary>
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM 
        /// <summary> Główny uchwyt komponentu podłączonego do Input System na potrzeby identyfikacji myszy i klawiatury. </summary>
        private UnityEngine.InputSystem.PlayerInput _playerInput;
#endif
        /// <summary> Referencja przypisanego animatora szkieletowego u gracza. </summary>
        private Animator _animator;
        
        /// <summary> Referencja komponentu Unity o nazwie CharacterController służącego za kolizje sfery chodzącej. </summary>
        private CharacterController _controller;
        
        /// <summary> Skrypt odczytujący wektory wejścia przycisków od gracza (od StarterAssets). </summary>
        private StarterAssetsInputs _input;
        
        /// <summary> Odnośnik na docelowy transformator aktywnej, renderującej z przodu scenę, kamery widokowej. </summary>
        private GameObject _mainCamera;
        
        /// <summary> Referencja do własnego skryptu PlayerStats służąca do wymiany np. mnożnika prędkości z gładkim skryptem ruchu. </summary>
        private PlayerStats _playerStats;

        /// <summary> Ułamek błędu (Epsilon) minimalizujący wpadnięcia logiczne float'a z kontrolerów, ucinający małe zjawiska dryftu wektora. </summary>
        private const float _threshold = 0.01f;

        /// <summary> Zmienna flagująca (True/False) powiązana z bytem komponentu Animatora. Pozwala skryptowi uniknąć rzucania wyjść w przypadku braku struktury 3D. </summary>
        private bool _hasAnimator;

        /// <summary>
        /// Prosta asercja do detekcji obsługi kontroli myszą/klawiaturą. Mnoży wartości z delty czasu.
        /// </summary>
        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
				return false;
#endif
            }
        }

        /// <summary>
        /// Inicjalizacja uruchamiana przed pierwszą klatką. Głównie po to by zainicjować MainCamera.
        /// </summary>
        private void Awake()
        {
            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
        }

        /// <summary>
        /// Powiązanie poszczególnych komponentów systemowych, reset timerów oraz zaczytanie identyfikatorów z maszyn Animator'a.
        /// </summary>
        private void Start()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _animator = GetComponent<Animator>();
            _hasAnimator = _animator != null;
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
            _playerStats = GetComponent<PlayerStats>();
#if ENABLE_INPUT_SYSTEM 
            _playerInput = GetComponent<UnityEngine.InputSystem.PlayerInput>();
#else
			Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        /// <summary>
        /// Odświeżanie główne: aktualizuje test gruntu, parametry skoku oraz przemieszcza ciało gracza za pomocą Move().
        /// </summary>
        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);

            JumpAndGravity();
            GroundedCheck();
            Move();
        }

        /// <summary>
        /// Odświeżanie fazy Late (po uprzednim przesunięciu postaci) by na bieżąco nadążyć obrotem głowy i kamery w oknie.
        /// </summary>
        private void LateUpdate()
        {
            CameraRotation();
        }

        /// <summary>
        /// Generuje hashe stringów powiązanych z właściwościami animatora i zapisuje w celu znacznej poprawy optymalizacji w pętlach klatek.
        /// </summary>
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        /// <summary>
        /// Funkcja bazująca na teście sfery za pomocą fizyki układu z wirtualnym promienistym offsetem. Weryfikuje bliskość kolizji do powierzchni, by orzec o wejściu gracza na podłoże.
        /// </summary>
        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        /// <summary>
        /// Logika kontrolująca ruch osi myszy i limitująca obroty dla wydelegowanego elementu Cinemachine. 
        /// Odbija też input w sposób bezbłędny dla stałych rotacji 360-stopniowych.
        /// </summary>
        private void CameraRotation()
        {
            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        /// <summary>
        /// Rdzeń pętli obliczającej nową pozycję na bazie prędkości interpolowanych LERP'em, a następnie 
        /// nakładający na nie wektory obrotu po odczycie kąta kamery i żądania klawiatury by skierować ruch obiektu po mapie.
        /// Metoda na nowo zintegrowana ze skryptem `PlayerStats` wspierając dodatkowe bonusy (Mnożnik Prędkości Gracza).
        /// </summary>
        private void Move()
        {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            float currentMoveSpeed = MoveSpeed;
            float currentSprintSpeed = SprintSpeed;
            if (_playerStats != null)
            {
                currentMoveSpeed *= _playerStats.moveSpeedMultiplier;
                currentSprintSpeed *= _playerStats.moveSpeedMultiplier;
            }
            float targetSpeed = _input.sprint ? currentSprintSpeed : currentMoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            float speedOffset = 0.1f;
            float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset ||
                currentHorizontalSpeed > targetSpeed + speedOffset)
            {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            }
            else
            {
                _speed = targetSpeed;
            }

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity, RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // move the player
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        /// <summary>
        /// Moduł fizyki lotu, który manipuluje rzutowaniem wskaźników grawitacyjnych dla wyznaczenia naturalnego upadku, wstrzymywania i wybijania ze skoku.
        /// Obsługuje timery blokujące i integruje się z maszynami animacyjnymi dla poszczególnych faz "FreeFall".
        /// </summary>
        private void JumpAndGravity()
        {
            if (Grounded)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                // Jump
                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                    }
                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                    }
                }

                // if we are not grounded, do not jump
                _input.jump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        /// <summary>
        /// Prosta funkcja normalizująca licznik stopniowy w pętli 360 za pomocą systemowych clampów.
        /// </summary>
        /// <param name="lfAngle">Wyjściowy kąt do oceny normy.</param>
        /// <param name="lfMin">Limit dla wartości dolnych.</param>
        /// <param name="lfMax">Limit dla wartości górnych.</param>
        /// <returns>Bezpiecznie sklasyfikowany Float.</returns>
        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        /// <summary>
        /// Opcjonalne rysowanie z perspektywy edytora Unity reprezentacji testu sfery z `GroundedCheck`.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        /// <summary>
        /// Wywołanie Eventu (AnimationEvent) umieszczanego jako flaga na ścieżce animacji. 
        /// Aktywuje odtwarzanie źródła z krokiem (Footstep/Foley) tylko podczas adekwatnej klatki.
        /// </summary>
        /// <param name="animationEvent">Struktura danych przekazana z Eventu ze skryptu bazowego animacji.</param>
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {

                if (AudioFootsteps != null)
                    AudioFootsteps.Play();
                if (AudioFoley != null)
                    AudioFoley.Play();
            }
        }

        /// <summary>
        /// Wywołanie Eventu (AnimationEvent) po pomyślnym lądowaniu wygenerowanym po klatkach "Jump/Freefall".
        /// </summary>
        /// <param name="animationEvent">Struktura danych przekazana z okna integracyjnego animacji.</param>
        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (LandingAudio != null)
                    LandingAudio.Play();

            }
        }
    }
}