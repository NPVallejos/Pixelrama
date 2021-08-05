public class CharacterInput
{
    private float DASH_THRESHOLD = 0.2f;

    private class InputState
    {
        public float Horizontal;
        public float Vertical;
        public float Dash;
        public bool Jump;
        public bool Fire;
        public bool Restart;
        public bool SupaJump;
    }

    public float Horizontal { get { return m_currentState.Horizontal; } }
    public float Vertical { get { return m_currentState.Vertical; } }

    public float Dash { get { return m_currentState.Dash; }}

    public bool DashButtonDown { get { return (m_previousState.Dash < DASH_THRESHOLD && m_currentState.Dash > DASH_THRESHOLD);  } }

    public bool DashButtonUp { get { return (m_previousState.Dash > DASH_THRESHOLD && m_currentState.Dash < DASH_THRESHOLD); } }

    public bool JumpDown { get { return !m_previousState.Jump && m_currentState.Jump; } }

    public bool JumpUp { get { return m_previousState.Jump && !m_currentState.Jump; }}

    public bool Jump { get { return m_currentState.Jump; } }

    public bool FireDown { get { return !m_previousState.Fire && m_currentState.Fire; } }

    public bool Fire { get { return m_currentState.Fire; } }

    public bool Restart { get { return m_currentState.Restart; } }

    public bool RestartDown { get { return !m_previousState.Restart && m_currentState.Restart; } }

    public bool SupaJumpDown { get { return !m_previousState.SupaJump && m_currentState.SupaJump; } }

    public bool SupaJumpUp { get { return m_previousState.SupaJump && !m_currentState.SupaJump; }}

    public bool SupaJump { get { return m_currentState.SupaJump; } }

    // The current state of the input that is updated on the fly via the Update loop.
    private InputState m_currentState;

    // Store the previous state of the input used on the last FixedUpdate loop,
    // so that we can replicate the difference between GetButton and GetButtonDown.
    private InputState m_previousState;

    // Have we been updated since the last FixedUpdate call? If we haven't been updated,
    // we don't reset. Otherwise, FixedUpdate being called twice in a row will cause
    // JumpDown/FireDown to falsely report being reset.
    private bool m_updatedSinceLastReset;

    public CharacterInput()
    {
        m_currentState = new InputState();
        m_previousState = new InputState();
    }

    public void OnUpdate(float horizontal, float vertical, float dash, bool jump, bool fire, bool restart, bool supaJump)
    {
        // We always take their most up to date horizontal and vertical input. This way we
        // can ignore tiny bursts of accidental press, plus there's some smoothing provided
        // by Unity anyways.
        m_currentState.Horizontal = horizontal;
        m_currentState.Vertical = vertical;
        m_currentState.Dash = dash;

        // However, for button presses we want to catch even single-frame presses between
        // fixed updates. This means that we can only copy across their 'true' status, and not
        // false ones. This means that a single frame press of the button will result in that
        // button reporting 'true' until the end of the next FixedUpdate clearing it. This prevents
        // the loss of very quick button presses which can be very important for jump and fire.
        if (jump)
            m_currentState.Jump = true;

        if (fire)
            m_currentState.Fire = true;

        if (restart)
            m_currentState.Restart = true;

        if (supaJump)
            m_currentState.SupaJump = true;

        m_updatedSinceLastReset = true;
    }

    public void ResetAfterFixedUpdate()
    {
        // Don't reset unless we've actually recieved a new set of input from the Update() loop.
        if (!m_updatedSinceLastReset)
            return;

        // Swap the current with the previous and then we'll reset the old
        // previous.
        InputState temp = m_previousState;
        m_previousState = m_currentState;
        m_currentState = temp;

        // We reset the state of single frame events only (that aren't set continuously) as the
        // continious ones will be set from scratch on the next Update() anyways.
        m_currentState.Jump = false;
        m_currentState.Fire = false;
        m_currentState.Restart = false;
        m_currentState.SupaJump = false;
        m_updatedSinceLastReset = false;
    }
}
