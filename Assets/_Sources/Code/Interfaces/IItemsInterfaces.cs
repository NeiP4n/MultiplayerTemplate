
using Sources.Code.Interfaces;

namespace Game.Interfaces
{
    public interface IDamageableZone
    {
        float DamageAmount { get; }
        void ApplyDamage(IPlayerHealth target);
    }
}