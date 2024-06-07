# EntityLite integration with MassTransit

Execute the following script in your SQL Server database to use EntityLite Transactional Outbox:

```SQL
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

```

## Saga Persistence

Comming soon.
