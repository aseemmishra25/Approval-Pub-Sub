// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApprovalPublisher.cs" company="New World Systems Corp.">
//   Copyright © New World Systems Corp. All rights reserved.
// </copyright>
// <summary>
//   Defines the ApprovalPublisher type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Web.Configuration;
using MassTransit;
using Company.Server.Suite.Approval.Messages;

namespace Company.Server.Suite.Approval
{
	public class ApprovalPublisher : IDisposable
	{
		private const string ChannelName = "approval.request";

		private static readonly string ChannelEndpointAddress = ConstructChannelEndpointAddress();

		private static readonly string ChannelUsername = "guest";

		private static readonly string ChannelPassword = "guest";

		private const string QUEUE_NAME = "rabbitmq://localhost/approval.request";

		private readonly Lazy<IServiceBus> _bus = new Lazy<IServiceBus>(CreateBus, true);

		//private IServiceBus _bus;

		//private IServiceBus Bus
		//{
		//	get
		//	{
		//		return _bus ?? (_bus = CreateBus());
		//	}
		//}

		public IServiceBus Bus
		{
			get
			{
				return _bus.Value;
			}
		}

		private static string ConstructChannelEndpointAddress()
		{
			var rabbitMqHostName = WebConfigurationManager.AppSettings["RabbitMqHostName"] ?? "localhost";

			return new UriBuilder("rabbitmq", rabbitMqHostName, -1, ChannelName).ToString();
		}
		/// <summary>
		/// Send an approval request message to the approval service.
		/// </summary>
		/// <param name="processId">The process code identifying the approval configuration to use.</param>
		/// <param name="orgStructureId">The organization structure (or department) ID.</param>
		/// <param name="recordId">The record ID related to the notification.</param>
		/// <param name="recordNumber">The user-friendly identifier for the record.</param>
		/// <param name="description">A short string describing the record.</param>
		/// <param name="userId">The user id of the requestor.</param>
		/// <param name="sourceUri">The absolute URL to the process page (typically the list page).</param>
		/// <param name="entryUri">The absolute URL to the record entry URL.</param>
		/// <param name="approvalsUri">The absolute URL to the record's approval URL.</param>
		/// <returns>The correlation ID of the process instance.</returns>
		public Guid RequestApproval(int processId, int orgStructureId, int recordId, string recordNumber, string description, int userId, Uri sourceUri, Uri entryUri, Uri approvalsUri)
		{
			var message = new RequestMessage
			{
				CorrelationId = Guid.NewGuid(),
				ProcessId = processId,
				UserId = userId,
				OrgStructureId = orgStructureId,
				RecordId = recordId,
				RecordNumber = recordNumber,
				Description = description,
				SourceUri = sourceUri,
				EntryUri = entryUri,
				ApprovalsUri = approvalsUri
			};

			Bus.Publish(message);

			return message.CorrelationId;
		}

		/// <summary>
		/// Sends a request to the approval service to continue an approval request.
		/// </summary>
		/// <param name="instanceId">The record identifier of the process to be approved.</param>
		/// <param name="userId">The user id of the requestor.</param>
		/// <param name="reason">The reason for the cancellation request.</param>
		public void ResubmitApprovalRequest(Guid instanceId, int userId, string reason = null)
		{
			var message = new ResubmitMessage
			{
				CorrelationId = instanceId,
				UserId = userId,
				Comment = reason ?? string.Empty
			};

			Bus.Publish(message);
		}

		/// <summary>
		/// Sends a request to the approval service to cancel an existing approval request.
		/// </summary>
		/// <param name="instanceId">The record identifier of the process to be approved.</param>
		/// <param name="userId">The user id of the requestor.</param>
		/// <param name="reason">The reason for the cancellation request.</param>
		public void CancelApprovalRequest(Guid instanceId, int userId, string reason = null)
		{
			var message = new CancelMessage
			{
				CorrelationId = instanceId,
				UserId = userId,
				Reason = reason ?? string.Empty
			};

			Bus.Publish(message);
		}

		/// <summary>
		/// Send a message to the approval service to approve a request.
		/// </summary>
		/// <param name="instanceId">The record identifier of the process instance to be approved.</param>
		/// <param name="userId">The user id of the requestor.</param>
		/// <param name="comment">Any user entered comment.</param>
		public void ApproveRequest(Guid instanceId, int userId, string comment = null)
		{
			var message = new ApproveMessage
			{
				CorrelationId = instanceId,
				UserId = userId,
				Comment = comment ?? string.Empty
			};

			Bus.Publish(message);
			
		}

		/// <summary>
		/// Send a message to the approval service to return a request for more information.
		/// </summary>
		/// <param name="instanceId">The record identifier of the process to be approved.</param>
		/// <param name="userId">The user id of the requestor.</param>
		/// <param name="reason">The reason the request was returned.</param>
		public void ReturnRequest(Guid instanceId, int userId, string reason = null)
		{
			var message = new ReturnMessage
			{
				CorrelationId = instanceId,
				UserId = userId,
				Reason = reason ?? string.Empty
			};

			Bus.Publish(message);
		}

		/// <summary>
		/// Send a message to the approval service to reject a request.
		/// </summary>
		/// <param name="instanceId">The record identifier of the process to be approved.</param>
		/// <param name="userId">The user id of the requestor.</param>
		/// <param name="reason">The reason the request was rejected.</param>
		public void RejectRequest(Guid instanceId, int userId, string reason = null)
		{
			var message = new RejectMessage
			{
				CorrelationId = instanceId,
				UserId = userId,
				Reason = reason ?? string.Empty
			};

			Bus.Publish(message);
		}

		/// <summary>
		/// Send a message from the approval service to notify of request status changes.
		/// </summary>
		/// <param name="instanceId">The record identifier of the process to be approved.</param>
		/// <param name="userId">The user id of the requestor.</param>
		/// <param name="status">The updated status of the request.</param>
		public void RequestStatusUpdated(Guid instanceId, int userId, int status)
		{
			var message = new RequestStatusUpdatedMessage
			{
				CorrelationId = instanceId,
				UserId = userId,
				RequestStatus = status
			};

			Bus.Publish(message);
		}

		/// <summary>
		/// Cleans up the resource for class instance.
		/// </summary>
		public void Dispose()
		{
			//Bus.Dispose();
			//_bus = null;

			if (_bus.IsValueCreated)
			{
				Bus.Dispose();
			}
		}

		//private IServiceBus CreateBus()
		//{
		//	return ServiceBusFactory.New(busConfig =>
		//	{
		//		busConfig.SetDefaultRetryLimit(0);
		//		busConfig.UseRabbitMq();
		//		busConfig.UseControlBus();
		//		busConfig.ReceiveFrom(QUEUE_NAME);
		//	});
		//}

		private static IServiceBus CreateBus()
		{
			return ServiceBusFactory.New(busConfig =>
			{
				busConfig.UseRabbitMq(transport => transport.ConfigureHost(new Uri(ChannelEndpointAddress), host =>
				{
					host.SetUsername(ChannelUsername);
					host.SetPassword(ChannelPassword);
				}));

				busConfig.UseControlBus();
				busConfig.ReceiveFrom(ChannelEndpointAddress);
			});
		}
	}
}