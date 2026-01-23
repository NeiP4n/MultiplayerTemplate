using System.Threading;
using Sources.Code.Core.Singletones;

namespace Sources.Code.Gameplay
{
    public class GameTokenProvider : SingletonClass<GameTokenProvider>
    {
        public CancellationToken Token { get; private set; }
        
        public void Init(CancellationToken gameToken)
        {
            Token = gameToken;
        }
    }
}