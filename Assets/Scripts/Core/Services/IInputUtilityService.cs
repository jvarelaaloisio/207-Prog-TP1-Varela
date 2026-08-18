namespace Core.Services
{
    /// <summary /> Used to track last device used
    public interface IInputUtilityService
    {
        /// <summary /> If the last input was from mouse or keyboard
        bool IsUsingMouse { get; }
    }
}