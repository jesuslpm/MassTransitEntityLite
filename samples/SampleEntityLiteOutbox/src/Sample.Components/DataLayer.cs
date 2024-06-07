
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
// using Microsoft.SqlServer.Types;
using System.Runtime.Serialization;

using System.ComponentModel;
using inercya.EntityLite;	
using inercya.EntityLite.Extensions;	

namespace Sample.Components
{
	[Serializable]
	[DataContract]
	[SqlEntity(BaseTableName="registrations")]
	public partial class Registration
	{
		private Guid _registrationId;
		[DataMember]
		[SqlField(DbType.Guid, 16, IsKey=true, ColumnName ="registration_id", BaseColumnName ="registration_id", BaseTableName = "registrations" )]		
		public Guid RegistrationId 
		{ 
		    get { return _registrationId; } 
			set 
			{
			    _registrationId = value;
			}
        }

		private DateTime _registrationDate;
		[DataMember]
		[SqlField(DbType.DateTime2, 8, Scale=7, ColumnName ="registration_date", BaseColumnName ="registration_date", BaseTableName = "registrations" )]		
		public DateTime RegistrationDate 
		{ 
		    get { return _registrationDate; } 
			set 
			{
			    _registrationDate = value;
			}
        }

		private String _memberId;
		[DataMember]
		[SqlField(DbType.String, 64, ColumnName ="member_id", BaseColumnName ="member_id", BaseTableName = "registrations" )]		
		public String MemberId 
		{ 
		    get { return _memberId; } 
			set 
			{
			    _memberId = value;
			}
        }

		private String _eventId;
		[DataMember]
		[SqlField(DbType.String, 64, ColumnName ="event_id", BaseColumnName ="event_id", BaseTableName = "registrations" )]		
		public String EventId 
		{ 
		    get { return _eventId; } 
			set 
			{
			    _eventId = value;
			}
        }

		private Decimal _payment;
		[DataMember]
		[SqlField(DbType.Decimal, 17, Precision = 18, Scale=2, ColumnName ="payment", BaseColumnName ="payment", BaseTableName = "registrations" )]		
		public Decimal Payment 
		{ 
		    get { return _payment; } 
			set 
			{
			    _payment = value;
			}
        }

		private String _currentState;
		[DataMember]
		[SqlField(DbType.String, 64, ColumnName ="current_state", BaseColumnName ="current_state", BaseTableName = "registrations" )]		
		public String CurrentState 
		{ 
		    get { return _currentState; } 
			set 
			{
			    _currentState = value;
			}
        }


	}

	public partial class RegistrationRepository : Repository<Registration> 
	{
		public RegistrationRepository(DataService DataService) : base(DataService)
		{
		}

		public new RegistrationDataService  DataService  
		{
			get { return (RegistrationDataService) base.DataService; }
			set { base.DataService = value; }
		}

		public Registration Get(string projectionName, Guid registrationId)
		{
			return ((IRepository<Registration>)this).Get(projectionName, registrationId, FetchMode.UseIdentityMap);
		}

		public Registration Get(string projectionName, Guid registrationId, FetchMode fetchMode = FetchMode.UseIdentityMap)
		{
			return ((IRepository<Registration>)this).Get(projectionName, registrationId, fetchMode);
		}

		public Registration Get(Projection projection, Guid registrationId)
		{
			return ((IRepository<Registration>)this).Get(projection, registrationId, FetchMode.UseIdentityMap);
		}

		public Registration Get(Projection projection, Guid registrationId, FetchMode fetchMode = FetchMode.UseIdentityMap)
		{
			return ((IRepository<Registration>)this).Get(projection, registrationId, fetchMode);
		}

		public Registration Get(string projectionName, Guid registrationId, params string[] fields)
		{
			return ((IRepository<Registration>)this).Get(projectionName, registrationId, fields);
		}

		public Registration Get(Projection projection, Guid registrationId, params string[] fields)
		{
			return ((IRepository<Registration>)this).Get(projection, registrationId, fields);
		}

		public bool Delete(Guid registrationId)
		{
			var entity = new Registration { RegistrationId = registrationId };
			return this.Delete(entity);
		}

				// asyncrhonous methods

		public System.Threading.Tasks.Task<Registration> GetAsync(string projectionName, Guid registrationId)
		{
			return ((IRepository<Registration>)this).GetAsync(projectionName, registrationId, FetchMode.UseIdentityMap);
		}

		public System.Threading.Tasks.Task<Registration> GetAsync(string projectionName, Guid registrationId, FetchMode fetchMode = FetchMode.UseIdentityMap)
		{
			return ((IRepository<Registration>)this).GetAsync(projectionName, registrationId, fetchMode);
		}

		public System.Threading.Tasks.Task<Registration> GetAsync(Projection projection, Guid registrationId)
		{
			return ((IRepository<Registration>)this).GetAsync(projection, registrationId, FetchMode.UseIdentityMap);
		}

		public System.Threading.Tasks.Task<Registration> GetAsync(Projection projection, Guid registrationId, FetchMode fetchMode = FetchMode.UseIdentityMap)
		{
			return ((IRepository<Registration>)this).GetAsync(projection, registrationId, fetchMode);
		}

		public System.Threading.Tasks.Task<Registration> GetAsync(string projectionName, Guid registrationId, params string[] fields)
		{
			return ((IRepository<Registration>)this).GetAsync(projectionName, registrationId, fields);
		}

		public System.Threading.Tasks.Task<Registration> GetAsync(Projection projection, Guid registrationId, params string[] fields)
		{
			return ((IRepository<Registration>)this).GetAsync(projection, registrationId, fields);
		}

		public System.Threading.Tasks.Task<bool> DeleteAsync(Guid registrationId)
		{
			var entity = new Registration { RegistrationId = registrationId };
			return this.DeleteAsync(entity);
		}
			}
	// [Obsolete("Use nameof instead")]
	public static partial class RegistrationFields
	{
		public const string RegistrationId = "RegistrationId";
		public const string RegistrationDate = "RegistrationDate";
		public const string MemberId = "MemberId";
		public const string EventId = "EventId";
		public const string Payment = "Payment";
		public const string CurrentState = "CurrentState";
	}

	public static partial class RegistrationProjections
	{
		public const string BaseTable = "BaseTable";
	}
}

namespace Sample.Components
{
	public partial class RegistrationDataService : DataService
	{
		partial void OnCreated();

		private void Init()
		{
			EntityNameToEntityViewTransform = TextTransform.ToUnderscoreLowerCaseNamingConvention;
			EntityLiteProvider.DefaultSchema = "dbo";
			AuditDateTimeKind = DateTimeKind.Utc;
			SequenceSuffix = "_seq";
			OnCreated();
		}

        public RegistrationDataService() : base("Default")
        {
			Init();
        }

        public RegistrationDataService(string connectionStringName) : base(connectionStringName)
        {
			Init();
        }

        public RegistrationDataService(string connectionString, string providerName) : base(connectionString, providerName)
        {
			Init();
        }

		private Sample.Components.RegistrationRepository _RegistrationRepository;
		public Sample.Components.RegistrationRepository RegistrationRepository
		{
			get 
			{
				if ( _RegistrationRepository == null)
				{
					_RegistrationRepository = new Sample.Components.RegistrationRepository(this);
				}
				return _RegistrationRepository;
			}
		}
	}
}
