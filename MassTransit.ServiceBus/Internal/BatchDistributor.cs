/// Copyright 2007-2008 The Apache Software Foundation.
/// 
/// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
/// this file except in compliance with the License. You may obtain a copy of the 
/// License at 
/// 
///   http://www.apache.org/licenses/LICENSE-2.0 
/// 
/// Unless required by applicable law or agreed to in writing, software distributed 
/// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
/// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
/// specific language governing permissions and limitations under the License.
namespace MassTransit.ServiceBus.Internal
{
	using System;
	using System.Collections.Generic;

	/// <summary>
	/// A batch distributor is subscribed to a message type so that it can dispatch batches of messages
	/// to new consumers for each batch
	/// </summary>
	/// <typeparam name="TMessage">The type of message that is being batched</typeparam>
	/// <typeparam name="TBatchId">The type for the batch id</typeparam>
	public class BatchDistributor<TMessage, TBatchId> :
		Consumes<TMessage>.Selected,
		Produces<Batch<TMessage, TBatchId>>
		where TMessage : class, BatchedBy<TBatchId>
	{
		private readonly Dictionary<TBatchId, Batch<TMessage, TBatchId>> _batches = new Dictionary<TBatchId, Batch<TMessage, TBatchId>>();

		private readonly MessageDispatcher<Batch<TMessage, TBatchId>> _messageDispatcher = new MessageDispatcher<Batch<TMessage, TBatchId>>();

		private readonly object _lockContext = new object();

		private readonly TimeSpan _timeout;
		private readonly IServiceBus _bus;

		public BatchDistributor(IServiceBus bus)
		{
			_bus = bus;

			_timeout = GetMessageTimeout(TimeSpan.FromMinutes(30));
		}

		public bool Active
		{
			get { return _messageDispatcher.Active; }
		}

		public bool Accept(TMessage message)
		{
			lock (_lockContext)
				if (_batches.ContainsKey(message.BatchId))
					return true;

			Batch<TMessage, TBatchId> batch = new Batch<TMessage, TBatchId>(_bus, message.BatchId, message.BatchLength, _timeout);

			if (_messageDispatcher.Accept(batch))
				return true;

			return false;
		}

		public void Consume(TMessage message)
		{
			TBatchId batchId = message.BatchId;

			Batch<TMessage, TBatchId> batch;

			bool invokeHandler = false;

			lock (_lockContext)
			{
				if (!_batches.ContainsKey(batchId))
				{
					batch = new Batch<TMessage, TBatchId>(_bus, batchId, message.BatchLength, _timeout);

					_batches.Add(batchId, batch);

					invokeHandler = true;
				}
				else
				{
					batch = _batches[batchId];
				}
			}

			// push this message to the context, releasing the enumerator
			batch.Consume(message);

			if (invokeHandler)
			{
				_messageDispatcher.Consume(batch);
			}
		}

		private static TimeSpan GetMessageTimeout(TimeSpan defaultValue)
		{
			TimeSpan value = defaultValue;

			object[] attributes = typeof (TMessage).GetCustomAttributes(typeof (TimeoutAttribute), true);
			foreach (TimeoutAttribute timeout in attributes)
			{
				value = timeout.Timeout;
			}

			return value;
		}

		public void Attach(Consumes<Batch<TMessage, TBatchId>>.All consumer)
		{
			_messageDispatcher.Attach(consumer);
		}

		public void Detach(Consumes<Batch<TMessage, TBatchId>>.All consumer)
		{
			_messageDispatcher.Detach(consumer);
		}
	}
}