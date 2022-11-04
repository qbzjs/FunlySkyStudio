using System.Threading;
using System;
using System.Diagnostics;
using System.Timers;

namespace BudEngine.NetEngine.src.Util {
    public class Timer : System.Timers.Timer {

        private Action _timeEvent;

        public void SetTimer (Action onTimedEvent, int interval) {
            interval = interval == 0 ? 1 : interval;
            
            this.Stop();
            this.Elapsed -= OnElapsedEvent;
            this._timeEvent = onTimedEvent;
            this.Interval = interval;
            this.Elapsed += OnElapsedEvent;
            this.AutoReset = true;
            this.Start();
        }

        public void SetTimeout (Action onTimedEvent, int interval) {
            interval = interval == 0 ? 1 : interval;
            
            this.Elapsed -= OnElapsedEvent;
            this._timeEvent = onTimedEvent;
            this.Interval = interval;
            this.Elapsed += OnElapsedEvent;
            this.AutoReset = false;
            this.Start ();
        }

        private void OnElapsedEvent (object sender, EventArgs e) {
            // int threadCount = BudEngine.NetEngine.src.Sdk.GetThreadCount();
            // UnityEngine.Debug.Log("###websocket new timer 线程ID:" + Thread.CurrentThread.ManagedThreadId + "  threadCount:"+threadCount);
            this._timeEvent ();
        }
    }

    public class TimeEventArgs : EventArgs {
        public TimeEventArgs () {

        }

        public string Seq { get; set; }
    }
}