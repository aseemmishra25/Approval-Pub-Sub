// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApprovalSubscriberService.cs" company="New World Systems Corp.">
//   Copyright © New World Systems Corp. All rights reserved.
// </copyright>
// <summary>
//   Defines the ApprovalSubscriberService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Configuration;
using System.Data.SqlClient;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using MassTransit.NHibernateIntegration;
using NHibernate;
using Topshelf;

namespace Company.Server.Suite.Approval.SubscriberHost
{
	internal static class ApprovalSubscriberService
	{
		[STAThread]
		public static void Main()
		{
			HostFactory.Run(
				x =>
				{
					x.Service<ApprovalSubscriber>(
						s =>
						{
							s.ConstructUsing(name => new ApprovalSubscriber());
							s.WhenStarted(sub => sub.Start());
							s.WhenStopped(sub => sub.Stop());
						});

					x.RunAsNetworkService();
					x.SetDescription("Subscriber for Logos Approval");
					x.SetDisplayName("New World Logos Approval Service");
					x.SetServiceName("nwLogosApproval");
				});
		}

		public static ISessionFactory CreateSessionFactory()
		{
			SqlServerSessionFactoryProvider provider;

			try
			{
				var csb = new SqlConnectionStringBuilder();

                csb.ConnectionString = ConfigurationManager.ConnectionStrings["LogosRuntimeServices"].ConnectionString;				
				provider = new SqlServerSessionFactoryProvider(csb.ConnectionString, typeof(ApprovalProcessSagaMap));
			}
			catch (FluentConfigurationException ex)
			{
				string message = ex.InnerException.Message;

				throw;
			}

			return provider.GetSessionFactory();
		}
	}
}