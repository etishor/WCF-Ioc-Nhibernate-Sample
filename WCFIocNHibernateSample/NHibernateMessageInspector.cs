using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;
using NHibernate;
using System.ServiceModel;
using NHibernate.Context;

namespace WCFIocNHibernateSample
{
    /// <summary>
    /// Mesage inspector to handle binding/unbinding of the session to the current session context
    /// </summary>
    public class NHibernateMessageInspector : IDispatchMessageInspector
    {
        private readonly ISessionFactory factory;

        public NHibernateMessageInspector(ISessionFactory factory)
        {
            this.factory = factory;
        }

        public object AfterReceiveRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel, InstanceContext instanceContext)
        {
            try
            {
                // open a new session and bind it the the CurrentSessionContext
                CurrentSessionContext.Bind(factory.OpenSession());
                // start a transaction
                factory.GetCurrentSession().BeginTransaction();
                Console.WriteLine("session opened and transaction started...");
            }
            catch (Exception x)
            {
                // if we throw anything other than FaultException service crashes.
                throw new FaultException(x.Message);
            }

            return request.Headers.Action;
        }

        public void BeforeSendReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            try
            {
                // unbind the session from the context
                ISession session = CurrentSessionContext.Unbind(factory);
                try
                {
                    // decide whether to commit or rollback
                    if (reply.IsFault)
                    {
                        session.Transaction.Rollback();
                        Console.WriteLine("Transaction rolled back");
                    }
                    else
                    {
                        session.Transaction.Commit();
                        Console.WriteLine("Transaction commited");
                    }
                }
                finally
                {
                    // cleanup
                    session.Transaction.Dispose();
                    session.Dispose();
                    Console.WriteLine("Session closed");
                }
            }
            catch (Exception x)
            {
                reply = System.ServiceModel.Channels.Message.CreateMessage(reply.Version, new FaultCode("Error"), x.Message, reply.Headers.Action);
                // rethrowing crashes the service.
            }
        }

    }
}
