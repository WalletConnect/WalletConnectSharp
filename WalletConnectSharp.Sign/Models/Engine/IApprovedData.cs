using System;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public interface IApprovedData : IAcknowledgement
    {
        string Topic { get; }
        
        public static IApprovedData FromAction(string topic, Action ack)
        {
            return new ActionApprovedData(topic, ack);
        }

        public static IApprovedData FromTask(string topic, Task task)
        {
            return new TaskApprovedData(topic, task);
        }

        public class ActionApprovedData : IApprovedData
        {
            private Action _action;

            public ActionApprovedData(string topic, Action action)
            {
                this._action = action;
                Topic = topic;
            }
            
            public Task Acknowledged()
            {
                _action();
                return Task.CompletedTask;
            }

            public string Topic { get; }
        }
        
        public class TaskApprovedData : IApprovedData
        {
            private Task _task;

            public TaskApprovedData(string topic, Task task)
            {
                this._task = task;
                Topic = topic;
            }
            
            public async Task Acknowledged()
            {
                await _task;
            }

            public string Topic { get; }
        }
    }
}