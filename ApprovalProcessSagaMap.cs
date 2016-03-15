using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentNHibernate.Mapping;
using MassTransit.NHibernateIntegration;
using MassTransit.Util;
using Company.Server.Suite.Approval.Data;

namespace Company.Server.Suite.Approval.SubscriberHost
{
	[UsedImplicitly]
	public class ApprovalProcessSagaMap : SagaStateMachineClassMapping<Approval.Sagas.ApprovalProcessSaga>
	{
		public ApprovalProcessSagaMap()
		{
			Property(x => x.ProcessId);
			Property(x => x.OrgStructureId);
			Property(x => x.RequestOwnerId);
			Property(x => x.IsSequential);
			Property(x => x.EntryUri);
			Property(x => x.SourceUri);
			Property(x => x.ApprovalsUri);
			Property(x => x.RecordId);
			Property(x => x.RecordNumber);
			Property(x => x.RecordDescription);
			Property(x => x.CurrentLevelId);
		}
	}
}
