﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using Newtonsoft.Json;
using NServiceBus;
using NServiceBus.Features;
using NServiceBus.Logging;
using NServiceBus.MessageInterfaces;
using NServiceBus.ObjectBuilder;
using NServiceBus.Settings;

namespace Aggregates
{
    public class DurableConsumer : Feature
    {
        public DurableConsumer()
        {
            RegisterStartupTask<ConsumerRunner>();
            DependsOn<EventStore>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<NServiceBusDispatcher>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent<DurableSubscriber>(DependencyLifecycle.SingleInstance);
        }
    }

    public class VolatileConsumer : Feature
    {
        public VolatileConsumer()
        {
            RegisterStartupTask<ConsumerRunner>();
            DependsOn<EventStore>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<NServiceBusDispatcher>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent<VolatileSubscriber>(DependencyLifecycle.SingleInstance);
        }
    }

    internal class ConsumerRunner : FeatureStartupTask
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ConsumerRunner));
        private readonly IBuilder _builder;
        private readonly ReadOnlySettings _settings;
        private readonly Configure _configure;

        public ConsumerRunner(IBuilder builder, ReadOnlySettings settings, Configure configure)
        {
            _builder = builder;
            _settings = settings;
            _configure = configure;
        }

        protected override void OnStart()
        {
            Logger.Debug("Starting event consumer");
            _builder.Build<IEventSubscriber>().SubscribeToAll(_settings.EndpointName(), _builder.Build<IDispatcher>());
        }
    }
}