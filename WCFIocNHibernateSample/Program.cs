using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.Loquacious;
using NHibernate.ByteCode.LinFu;
using NHibernate.Context;
using System.ServiceModel.Dispatcher;
using NHibernate.Tool.hbm2ddl;

namespace WCFIocNHibernateSample
{
    public interface IContainer
    {
        object Resolve<T>();
    }

    public class FakeContainer : IContainer
    {
        private readonly ISessionFactory sessionFactory;
        public FakeContainer(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        // don't do this, use a real container
        public object Resolve<T>()
        {
            Type t = typeof(T);
            if (t == typeof(SampleService))
            {
                return new SampleService(sessionFactory);
            }
            if (t == typeof(IDispatchMessageInspector))
            {
                return new NHibernateMessageInspector(sessionFactory);
            }
            throw new InvalidOperationException("use a real container");
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            FakeContainer container = new FakeContainer(BuildSessionFactory());

            using (ServiceHost host = new ServiceHost(typeof(SampleService)))
            {
                // add a behavior that allows the service to be created by the container with it's dependencies injected
                host.Description.Behaviors.Add(new IOCBehaviour<SampleService>(container));

                host.Open();

                Console.WriteLine("Waiting...");
                Console.ReadKey();
            }
            
        }

        // configure nhibernate
        static ISessionFactory BuildSessionFactory()
        {
            Configuration cfg = new Configuration();

            cfg.Proxy(p => p.ProxyFactoryFactory<ProxyFactoryFactory>());
            
            // this is important - specify the desired session context storage
            cfg.CurrentSessionContext<WcfOperationSessionContext>();

            cfg.DataBaseIntegration(db =>
            {
                db.Dialect<NHibernate.Dialect.MsSql2008Dialect>();
                db.Driver<NHibernate.Driver.SqlClientDriver>();
                db.ConnectionString = @"Data Source=.\sqlexpress;Initial Catalog=sample;Integrated Security=True;Pooling=False";
            });

            cfg.AddAssembly(typeof(Program).Assembly);

            // remove this in production
            SchemaUpdate updater = new SchemaUpdate(cfg);
            updater.Execute(true, true);

            // do actual configuration
            return cfg.BuildSessionFactory();
        }
    }
}
