using System;
using System.Threading.Tasks;

namespace WalletConnectSharp.Sign.Models.Engine
{
    public interface IAcknowledgement
    {
        Task Acknowledged();

        public static IAcknowledgement FromAction(Action ack)
        {
            return new ActionAcknowledgement(ack);
        }

        public static IAcknowledgement FromTask(Task task)
        {
            return new TaskAcknowledgement(task);
        }

        public class ActionAcknowledgement : IAcknowledgement
        {
            private Action _action;

            public ActionAcknowledgement(Action action)
            {
                this._action = action;
            }
            
            public Task Acknowledged()
            {
                _action();
                return Task.CompletedTask;
            }
        }
        
        public class TaskAcknowledgement : IAcknowledgement
        {
            private Task _task;

            public TaskAcknowledgement(Task task)
            {
                this._task = task;
            }
            
            public async Task Acknowledged()
            {
                await _task;
            }
        }
    }
}