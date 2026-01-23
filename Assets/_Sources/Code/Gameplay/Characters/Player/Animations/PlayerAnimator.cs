using UnityEngine;
using MonsterLove.StateMachine;
using Sources.Characters;
using Sources.Code.Interfaces;

namespace Game.States
{
    public class PlayerAnimator : MonoBehaviour
    {
        public enum States { Idle, Move, Run, Jumped }

        [SerializeField] private Animator animator;

        private StateMachine<States> fsm;
        private float jumpTimer = 0f;
        private const float jumpMinDuration = 0.25f;

        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int IsJumping = Animator.StringToHash("isJumping");

        private IInputManager _input;
        private GroundMover _mover;
        private bool _initialized = false;

        public void Construct(IInputManager input, GroundMover mover)
        {
            _input = input;
            _mover = mover;
            _initialized = true;
        }

        void Start()
        {
            fsm = new StateMachine<States>(this);
            fsm.ChangeState(States.Idle);
        }

        public void HandleJump()
        {
            if (fsm != null && fsm.State != States.Jumped)
            {
                fsm.ChangeState(States.Jumped);
                Debug.Log("HandleJump: FSM changed to Jumped");
            }
        }

        public void UpdateState()
        {
            if (!_initialized || fsm == null) return;

            fsm.Driver.Update.Invoke();

            if (fsm.State == States.Jumped) return;

            Vector2 moveInput = new(_input.Horizontal, _input.Vertical);
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            bool isRunning = _input.SprintPressed;

            if (!isMoving && fsm.State != States.Idle)
                fsm.ChangeState(States.Idle);
            else if (isMoving && !isRunning && fsm.State != States.Move)
                fsm.ChangeState(States.Move);
            else if (isMoving && isRunning && fsm.State != States.Run)
                fsm.ChangeState(States.Run);

            if (_input.ConsumeJump() && _mover.IsGrounded)
                fsm.ChangeState(States.Jumped);
        }

        #region FSM States

        void Idle_Enter() => animator.SetFloat(Speed, 0);
        void Move_Enter() => animator.SetFloat(Speed, 7);
        void Run_Enter() => animator.SetFloat(Speed, 10);

        void Jumped_Enter()
        {
            animator.SetTrigger(IsJumping);
            jumpTimer = 0f;
            Debug.Log("Jumped_Enter: isJumping = " + animator.GetBool(IsJumping));
        }

        void Jumped_Update()
        {
            jumpTimer += Time.deltaTime;

            if (!_mover.IsGrounded || jumpTimer < jumpMinDuration)
                return;

            Vector2 moveInput = new(_input.Horizontal, _input.Vertical);
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            bool isRunning = _input.SprintPressed;

            if (!isMoving) fsm.ChangeState(States.Idle);
            else if (!isRunning) fsm.ChangeState(States.Move);
            else fsm.ChangeState(States.Run);
        }

        void Jumped_Exit()
        {
        }

        #endregion
    }
}
