namespace API.DIService
{
    public sealed class MsDiService
    {
        #region Private Static Members

        private static readonly object MsDiLocker = new object();
        private static MsDiService _instance;
        private IHost _host = null;
        private bool _hasRegister = false;

        #endregion


        #region Constructor

        private MsDiService()
        {
        }

        #endregion


        #region Public Static Methods

        public static MsDiService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (MsDiLocker)
                    {
                        _instance = new MsDiService();
                    }
                }

                return _instance;
            }
        }

        #endregion


        #region Public Methods

        public void Register(Action<IServiceCollection> diRegister)
        {
            if (_host == null || !_hasRegister)
            {
                _host = Host.CreateDefaultBuilder()
                         .ConfigureServices((context, services) =>
                         {
                             diRegister(services);
                         }
                    ).Build();

                _hasRegister = true;
            }
        }

        /// <summary>
        /// Resolve to a Singleton class
        /// </summary>
        /// <typeparam name="T">Type of class</typeparam>
        /// <returns>Resolved class</returns>
        public T GetService<T>()
        {
            return _host.Services.GetService<T>();
        }

        /// <summary>
        /// Create a new instance of a class
        /// </summary>
        /// <typeparam name="T">Interface of the class</typeparam>
        /// <typeparam name="C">Concrete type of the class</typeparam>
        /// <returns>Interface of the class</returns>
        public T CreateService<T, C>()
            where T : class
            where C : class
        {
            return ActivatorUtilities.CreateInstance<C>(_host.Services) as T;
        }

        #endregion
    }
}