using System;
using System.Collections.Generic;
using MSFramework.Domain;
using MSFramework.Domain.Event;

namespace ServicePlan.Domain.AggregateRoot
{
	/// <summary>
	/// 创建服务计划事件
	/// </summary>
	public class CreateServicePlanEvent : AggregateEventBase
	{
		public Product Product { get; }

		public User User { get; }

		public string Name { get; }

		public DateTime BeginTime { get; }

		public DateTime EndTime { get; }

		public CreateServicePlanEvent(Product product, User user, string name, DateTime beginTime, DateTime endTime)
		{
			Product = product;
			User = user;
			Name = name;
			BeginTime = beginTime;
			EndTime = endTime;
		}
	}

	/// <summary>
	/// 创建路演计划事件
	/// </summary>
	public class CreateRoadShowPlanEvent : AggregateEventBase
	{
		public Guid OwnerId { get; }
         
		public Client Client { get; }
         
		public List<ClientUser> ClientUsers { get; }
         
		public string Address { get; }
         
		public DateTime BeginTime { get; }
         
		public DateTime EndTime { get; }

		public string Name { get; }

		public User User { get; }

		public User Creator { get; }

		public CreateRoadShowPlanEvent(Client client, List<ClientUser> clientUsers, User user, User creator, string name, string address,
			DateTime beginTime, DateTime endTime, Guid ownerId)
		{
			Client = client;
			ClientUsers = clientUsers;
			User = user;
			Creator = creator;
			Name = name;
			Address = address;
			BeginTime = beginTime;
			EndTime = endTime;
			OwnerId = ownerId;
		}
	}

	/// <summary>
	/// 提交服务计划事件
	/// </summary>
	public class SubmitPlanEvent : AggregateEventBase
	{
	}

	/// <summary>
	/// 完成服务计划事件
	/// </summary>
	public class CompletePlanEvent : AggregateEventBase
	{
		public string Subject { get; }

		public string IndustryId { get; }

		public CompletePlanEvent(string subject, string industryId)
		{
			Subject = subject;
			IndustryId = industryId;
		}
	}

	/// <summary>
	/// 上传附件事件
	/// </summary>
	public class UploadAttachmentsEvent : AggregateEventBase
	{
		public List<Attachment> Attachments { get; }

		public UploadAttachmentsEvent(List<Attachment> attachments)
		{
			Attachments = attachments;
		}
	}

	public class QcVerifySuccessEvent : AggregateEventBase
	{
		public User User { get; }

		public QcVerifySuccessEvent(User user)
		{
			User = user;
		}
	}

	public class QcVerifyFailedEvent : AggregateEventBase
	{
		public User User { get; }

		public QcVerifyFailedEvent(User user)
		{
			User = user;
		}
	}
	
	public class AuditSuccessEvent : AggregateEventBase
	{
		public User User { get; }

		public AuditSuccessEvent(User user)
		{
			User = user;
		}
	}

	public class AuditFailedEvent : AggregateEventBase
	{
		public User User { get; }

		public AuditFailedEvent(User user)
		{
			User = user;
		}
	}

	public class SetTitleAndAbstractEvent : AggregateEventBase
	{
		public string Title { get; }

		public string Abstract { get; }

		public SetTitleAndAbstractEvent(string title, string @abstract)
		{
			Title = title;
			Abstract = @abstract;
		}
	}

	public class SubmitAuditEvent : AggregateEventBase
	{
		public User User { get; }

		public SubmitAuditEvent(User user)
		{
			User = user;
		}
	}

	public class SendEmailEvent : AggregateEventBase
	{
		public List<Guid> ClientUsers { get; }

		public SendEmailEvent(List<Guid> clientUsers, DateTime creationTime)
		{
			ClientUsers = clientUsers;
			CreationTime = creationTime;
		}
	}

	public class SetEmailSentSuccessEvent : AggregateEventBase
	{
		public Guid Identity { get; }

		public DateTime ResponseTime { get; }

		public SetEmailSentSuccessEvent(Guid identity, DateTime responseTime)
		{
			Identity = identity;
			ResponseTime = responseTime;
		}
	}
	
	public class SetEmailSentFailedEvent : AggregateEventBase
	{
		public Guid Identity { get; }

		public DateTime ResponseTime { get; }

		public SetEmailSentFailedEvent(Guid identity, DateTime responseTime)
		{
			Identity = identity;
			ResponseTime = responseTime;
		}
	}
}