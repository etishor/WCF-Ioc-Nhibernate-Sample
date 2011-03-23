using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using NHibernate;

namespace WCFIocNHibernateSample
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerCall)]
    [ServiceContract]
    public class SampleService
    {
        private readonly ISessionFactory sessionFactory;

        public SampleService(ISessionFactory sessionFactory)
        {
            this.sessionFactory = sessionFactory;
        }

        [OperationContract]
        public Guid DoWork()
        {
            Person p = new Person();
            p.Name = "name";
            sessionFactory.GetCurrentSession().Save(p);
            return p.Id;
        }

        [OperationContract]
        public void Change(Guid id, string name)
        {
            Person p = sessionFactory.GetCurrentSession().Load<Person>(id);
            p.Name = name;
        }
    }
}
