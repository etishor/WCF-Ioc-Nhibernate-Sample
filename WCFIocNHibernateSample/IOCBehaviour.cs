using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace WCFIocNHibernateSample
{
    /// <summary>
    /// Behavior that allows the service instance to be created by a IOC container
    /// </summary>
    /// <typeparam name="T">Type of the service</typeparam>
    public class IOCBehaviour<T> : IServiceBehavior, IInstanceProvider
    {
        private readonly IContainer container;

        public IOCBehaviour(IContainer container)
        {
            this.container = container;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, 
            Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {}

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers)
            {
                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;
                if (channelDispatcher != null)
                {
                    foreach (EndpointDispatcher endpoint in channelDispatcher.Endpoints)
                    {
                        // set the intance that will create service instances
                        endpoint.DispatchRuntime.InstanceProvider = this;
                        // attach a message inspector to manage the nhibernate session binding to the session context
                        endpoint.DispatchRuntime.MessageInspectors.Add((IDispatchMessageInspector)container.Resolve<IDispatchMessageInspector>());
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {}

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            // provide instance using the container
            return container.Resolve<T>();
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return container.Resolve<T>();
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            IDisposable disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }
    }
}
