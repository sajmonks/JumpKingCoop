using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingCoop.Networking.Utils
{
    public class TimedTask<T>
    {
        public T State { get; private set; }

        public float TimeoutTime { get; private set; }

        public bool Started { get; private set; }

        private DateTime startTime = DateTime.Now;

        private Func<T> functionOnTimeout;

        public TimedTask(T defaultState)
        {
            State = defaultState;
            Started = false;
        }

        public void Start(float timeoutTime, Func<T> onTimeout)
        {
            TimeoutTime = timeoutTime;
            functionOnTimeout = onTimeout;
            Started = true;
        }

        public void Stop(T result)
        {
            State = result;
            Started = false;
        }

        public void Update()
        {
            if(Started)
            {
                var timeDifference = DateTime.Now - startTime;
                if(timeDifference.TotalSeconds >= TimeoutTime)
                {
                    State = functionOnTimeout.Invoke();
                    Started = false;
                }
            }
        }
    }
}
