using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WalletConnectSharp.Common.Model;
using WalletConnectSharp.Events.Model;
using Xunit;

namespace WalletConnectSharp.Events.Tests
{
    public class EventDelegatorTests
    {
        interface ITest
        {
            public int test1 { get; }
        }

        public class TestEventData : ITest
        {
            public int test1 { get; set; }
            public string test2;
        }

        public class TestGenericData<T>
        {
            public T data;
        }
        
        [Fact]
        public async void AsyncEventsPropagate()
        {
            var events = new EventDelegator();

            TaskCompletionSource<string> eventCallbackTask = new TaskCompletionSource<string>();

            string eventId = Guid.NewGuid().ToString();
            
            events.ListenFor<string>(eventId, delegate(object sender, GenericEvent<string> @event)
            {
                eventCallbackTask.SetResult(@event.EventData);
            });
            var eventData = Guid.NewGuid().ToString();

            await Task.Run(delegate
            {
                Thread.Sleep(500);
                events.Trigger(eventId, eventData);
            });

            Assert.Equal(eventData, (await eventCallbackTask.Task));
        }

        [Fact]
        public void ListenForOnce()
        {
            EventDelegator events = new EventDelegator();

            TestEventData result1 = null;
            
            events.ListenForOnce<TestEventData>("abc", delegate(object sender, GenericEvent<TestEventData> @event)
            {
                result1 = @event.EventData;
            });
            
            var testData1 = new TestEventData()
            {
                test1 = 11,
                test2 = "abccc"
            };

            events.Trigger("abc", testData1);
            
            Assert.StrictEqual(testData1, result1);

            result1 = null;

            events.Trigger("abc", testData1);
            
            Assert.Null(result1);
        }

        [Fact]
        public void RemoveListener()
        {
            EventDelegator events = new EventDelegator();

            events.ListenFor<TestEventData>("abc", Callback);
            
            events.RemoveListener<TestEventData>("abc", Callback);

            events.Trigger("abc", new TestEventData());
        }

        private void Callback(object sender, GenericEvent<TestEventData> e)
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void InheritanceEventsPropagate()
        {
            EventDelegator events = new EventDelegator();

            TestEventData result1 = null;
            ITest result2 = null;
            TestGenericData<TestEventData> result3 = null;
            ITest result4 = null;

            events.ListenFor<TestEventData>("abc", delegate(object sender, GenericEvent<TestEventData> @event)
            {
                result1 = @event.EventData;
            });

            events.ListenFor<ITest>("abc", delegate(object sender, GenericEvent<ITest> @event)
            {
                result2 = @event.EventData;
            });


            events.ListenFor<TestGenericData<TestEventData>>("xyz",
                delegate(object sender, GenericEvent<TestGenericData<TestEventData>> @event)
                {
                    result3 = @event.EventData;
                });
            
            events.ListenFor<ITest>("xyz", delegate(object sender, GenericEvent<ITest> @event)
            {
                result4 = @event.EventData;
            });

            var testData1 = new TestEventData()
            {
                test1 = 11,
                test2 = "abccc"
            };

            var testData2 = new TestGenericData<TestEventData>()
            {
                data = testData1
            };

            events.Trigger("abc", testData1);
            events.Trigger("xyz", testData2);
            
            Assert.StrictEqual(testData1, result1);
            Assert.StrictEqual(testData1, result2);
            Assert.StrictEqual(testData2, result3);
            Assert.Null(result4);
        }

        [Fact]
        public void ListenAndDeserializeJson()
        {
            EventDelegator events = new EventDelegator();

            TestEventData result1 = null;
            
            events.ListenForAndDeserialize<TestEventData>("abc", delegate(object sender, GenericEvent<TestEventData> @event)
            {
                result1 = @event.EventData;
            });
            
            var testData1 = new TestEventData()
            {
                test1 = 11,
                test2 = "abccc"
            };

            var json = JsonConvert.SerializeObject(testData1);

            events.Trigger("abc", json);
            
            Assert.Equal(result1.test1, testData1.test1);
            Assert.Equal(result1.test2, testData1.test2);
        }
    }
}