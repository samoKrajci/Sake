using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace GameLibrary
{
    public class Clock
    {
        public int _tps;
        private System.Timers.Timer _mainTimer;
        private bool mainTick;
        private bool halfTick;
        private double interval;
        private bool lastTickMain;

        public Clock(int tps)
        {
            this._tps = tps;
            this.interval = (double)1000 / (double)tps;
            this.mainTick = false;
            this.halfTick = false;

            this.lastTickMain = true;

            this._mainTimer = new System.Timers.Timer(interval / (double)2);
            this._mainTimer.Elapsed += OnTimer;
            this._mainTimer.AutoReset = true;
        }
        public void Start()
        {
            this._mainTimer.Enabled = true;
        }

        private void OnTimer(Object source, ElapsedEventArgs e)
        {
            if (!this.lastTickMain)
            {
                this.mainTick = true;
            }
            this.halfTick = true;
            this.lastTickMain = !this.lastTickMain;
        }
        public bool IsMainTick()
        {
            if (this.mainTick)
            {
                this.mainTick = false;
                return true;
            }
            return false;
        }
        public bool IsHalfTick()
        {
            if (this.halfTick)
            {
                this.halfTick = false;
                return true;
            }
            return false;
        }

    }
}
