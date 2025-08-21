using ScreenManager.Interfaces;

namespace ScreenManager.Core
{
    public interface IScreenWithTypedContext<TContext> : IScreen
    {
        void Initialize(TContext context);
    }
}
