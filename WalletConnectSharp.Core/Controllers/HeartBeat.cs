using System;
using System.Threading;
using System.Threading.Tasks;
using WalletConnectSharp.Core.Interfaces;
using WalletConnectSharp.Core.Models.Heartbeat;
using WalletConnectSharp.Events;
using WalletConnectSharp.Events.Model;

namespace WalletConnectSharp.Core.Controllers
{
    public class HeartBeat : IHeartBeat
    {
        public static readonly object PULSE_OBJECT = new object();
        
        public EventDelegator Events { get; }
        
        public CancellationToken HeartBeatCancellationToken { get; private set; }
        
        public int Interval { get; }
        
        public readonly Guid contextGuid = Guid.NewGuid();

        public string Name
        {
            get
            {
                return $"heartbeat-{contextGuid}";
            }
        }

        public string Context
        {
            get
            {
                return Name;
            }
        }

        public HeartBeat(HeartBeatOptions opts = null)
        {
            if (opts == null)
            {
                opts = new HeartBeatOptions()
                {
                    Interval = 5000 // 5 seconds
                };
            }
            
            Events = new EventDelegator(this);

            Interval = opts.Interval;
        }

        public Task Init()
        {
            HeartBeatCancellationToken = new CancellationToken();

            Task.Run(async () =>
            {
                while (!HeartBeatCancellationToken.IsCancellationRequested)
                {
                    Pulse();

                    await Task.Delay(Interval, HeartBeatCancellationToken);
                }
            }, HeartBeatCancellationToken);

            return Task.CompletedTask;
        }

        private void Pulse()
        {
            Events.Trigger(HeartbeatEvents.Pulse, PULSE_OBJECT);
        }
    }
}