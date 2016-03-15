// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApprovalSubscriber.cs" company="New World Systems Corp.">
//   Copyright © New World Systems Corp. All rights reserved.
// </copyright>
// <summary>
//   Defines the ApprovalSubscriber type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Reflection;
using log4net;
using MassTransit;
using MassTransit.Log4NetIntegration;
using MassTransit.Saga;
using Company.Server.Suite.Approval.Sagas;
using MassTransit.NHibernateIntegration.Saga;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Company.Server.Suite.Approval.SubscriberHost
{
	internal class ApprovalSubscriber
	{
		private const string QueueName = "rabbitmq://localhost/approval.subscriber";

		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private IServiceBus _bus;

		public void Start()
		{
			_log.Info("Initializing the service bus.");
			_bus = ServiceBusFactory.New(b =>
			{
				b.UseRabbitMq();
				b.UseControlBus();
				b.ReceiveFrom(QueueName);
				b.SetConcurrentConsumerLimit(1);
				b.UseLog4Net();

				_log.Info(string.Format("Subscribed on queue: {0}", QueueName));

				var sagaRepository = new NHibernateSagaRepository<ApprovalProcessSaga>(ApprovalSubscriberService.CreateSessionFactory());

				b.Subscribe(sub => sub.Saga<ApprovalProcessSaga>(sagaRepository).Permanent());
			});

			_log.Info("Subscriber started...");
		}

		public void Stop()
		{
			_log.Info("Subscriber stop requested...");
			_bus.Dispose();
		}
	}
}