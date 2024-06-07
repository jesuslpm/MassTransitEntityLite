# Transactional Outbox Sample

This sample shows how to configure and use MassTransit's new transactional outbox integrated with EntityLite using a SQL Server database, including:

- An API controller that uses a domain registration service to register a member for an event. The controller has no knowledge of MassTransit, and the registration service uses `IPublishEndpoint` to publish a domain event which is written to the transactional outbox.
- Adds the transactional outbox delivery service as a hosted service, which delivers outbox message to the transport.
- Two separate services that consumes the domain event by itself, one of them uses transactional outbox (and inbox, for idempotent message delivery), it updates the database and publish another domain event which is written to the transactional outbox as well.

## Database setup

Execute the following script in SQL Sever Management Studio:

```SQL
CREATE DATABASE SampleOutboxEntityLite
GO
USE SampleOutboxEntityLite
GO
CREATE SCHEMA mt
GO

CREATE SEQUENCE mt.outbox_messages_sequence_number_seq
AS bigint
START WITH 1
INCREMENT BY 1

CREATE TABLE mt.outbox_messages
(
	sequence_number bigint NOT NULL CONSTRAINT pk_outbox_messages PRIMARY KEY CONSTRAINT df_outbox_messages_sequence_number DEFAULT NEXT VALUE FOR  mt.outbox_messages_sequence_number_seq,
	message_id uniqueidentifier NOT NULL,
	conversation_id uniqueidentifier,
	correlation_id uniqueidentifier,
	initiator_id uniqueidentifier,
	request_id uniqueidentifier,
	source_address nvarchar(256),
	destination_address nvarchar(256),
	response_address nvarchar(256),
	fault_address nvarchar(256),
	expiration_time datetime,
	enqueue_time datetime,
	sent_time datetime NOT NULL,
	inbox_message_id uniqueidentifier,
	inbox_consumer_id uniqueidentifier,
	outbox_id uniqueidentifier,
	headers nvarchar(max),
	properties nvarchar(max),
	content_type nvarchar(256) NOT NULL,
	message_type nvarchar(4000) NOT NULL,
	body nvarchar(max)
);

CREATE INDEX ix_outbox_messages_expiration_time ON mt.outbox_messages(expiration_time);
CREATE INDEX ix_outbox_messages_enqueue_time ON mt.outbox_messages(enqueue_time);
CREATE UNIQUE INDEX ux_outbox_messages_inbox_message_id ON mt.outbox_messages(inbox_message_id, inbox_consumer_id, sequence_number);
CREATE UNIQUE INDEX ix_outbox_messages_outbox_id ON mt.outbox_messages(outbox_id, sequence_number);



GO

CREATE SEQUENCE mt.inbox_states_id_seq 
AS bigint
START WITH 1
INCREMENT BY 1;


CREATE TABLE mt.inbox_states
(
	id bigint NOT NULL CONSTRAINT pk_inbox_states PRIMARY KEY CONSTRAINT df_inbox_states_id DEFAULT NEXT VALUE FOR mt.inbox_states_id_seq,
	message_id uniqueidentifier NOT NULL,
	consumer_id uniqueidentifier NOT NULL,
	lock_id uniqueidentifier NOT NULL,
	entity_row_version int NOT NULL,
	received datetime NOT NULL,
	receive_count int NOT NULL,
	expiration_time datetime,
	consumed datetime,
	delivered datetime,
	last_sequence_number bigint,
	CONSTRAINT uk_inbox_states_message_id_consumer_id UNIQUE(message_id, consumer_id)
);

CREATE INDEX ix_inbox_states_delivered ON mt.inbox_states(delivered);

GO

CREATE TABLE mt.outbox_states
(
	outbox_id uniqueidentifier NOT NULL CONSTRAINT pk_outbox_states PRIMARY KEY,
	lock_id uniqueidentifier NOT NULL,
	entity_row_version int NOT NULL,
	created datetime NOT NULL,
	delivered datetime,
	last_sequence_number bigint
)

GO


CREATE TABLE [dbo].[registrations](
	[registration_id] [uniqueidentifier] NOT NULL CONSTRAINT pk_registrations PRIMARY KEY,
	[registration_date] [datetime2](7) NOT NULL,
	[member_id] [nvarchar](64) NOT NULL,
	[event_id] [nvarchar](64) NOT NULL,
	[payment] [decimal](18, 2) NOT NULL,
	[current_state] nvarchar(64) NOT NULL,
	CONSTRAINT uk_registrations_member_id_event_id UNIQUE(member_id, event_id)
)

GO


```

Check the connection string in `appsettings.json` files

# Run RabbitMQ

Ejecute the following command:

```
docker run -d --hostname rabbit --name rabbit -p 15672:15672 -p 5672:5672 -e RABBITMQ_DEFAULT_USER=guest -e RABBITMQ_DEFAULT_PASS=guest rabbitmq:3-management-alpine
```

After the container is stopped, you can restart it from Docker Desktop or executing the following command:

```
docker start rabbit
```

# Run in Visual Studio:

Just click start button, both Api and Worker will start.




