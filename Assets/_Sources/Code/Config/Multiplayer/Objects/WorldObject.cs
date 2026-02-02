using UnityEngine;
using TriInspector;
using Sources.Code.Configs.Multiplayer.Global;

namespace Sources.Code.Configs.Multiplayer.Objects
{
    [DeclareBoxGroup("Identity")]
    public sealed class WorldObject : MonoBehaviour
    {
        [Group("Identity"), Required]
        [SerializeField] private GlobalIdentifiableObject globalId;

        public int Id => globalId != null ? globalId.Id : 0;
        public GlobalIdCategory Category =>
            globalId != null ? globalId.Category : GlobalIdCategory.None;

        private void Awake()
        {
            if (globalId == null)
                Debug.LogError("Missing GlobalIdentifiableObject!", this);
        }
    }
}
