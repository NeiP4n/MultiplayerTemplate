namespace Sources.Code.Core.Singletones
{
    public class SingletonClass<T>
        where T : SingletonClass<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                _instance ??= new T();
                return _instance;
            }
        }
    }
}