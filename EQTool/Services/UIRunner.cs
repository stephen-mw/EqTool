using EQTool.Models;
using EQTool.ViewModels;
using EQToolShared.Enums;
using System;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace EQTool.Services
{
    public class UIRunner : IDisposable
    {
        private readonly SpellWindowViewModel spellWindowViewModel;
        private readonly LoggingService loggingService;
        private readonly ActivePlayer activePlayer;
        private DispatcherTimer timer;

        public UIRunner(SpellWindowViewModel spellWindowViewModel, LoggingService loggingService, ActivePlayer activePlayer)
        {
            this.activePlayer = activePlayer;
            this.loggingService = loggingService;
            this.spellWindowViewModel = spellWindowViewModel;
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += UITimer_Elapsed;
            timer.Start();
        }

        private DateTime? LastUIRun = null;
        private DateTime? LastBoatUpdate = null;
        private void UITimer_Elapsed(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            var dt_ms = 0.0;
            if (LastUIRun.HasValue)
            {
                dt_ms = (now - LastUIRun.Value).TotalMilliseconds;
            }
            loggingService.Log($"[UIRunner] Tick - Elapsed Time (ms): {dt_ms}", EventType.Debug, activePlayer?.Player?.Server);
            LastUIRun = now;
            spellWindowViewModel.UpdateTriggers(dt_ms);
            if (!LastBoatUpdate.HasValue || (LastBoatUpdate.HasValue && (now - LastBoatUpdate.Value).TotalMinutes > 5))
            {
                _ = Task.Factory.StartNew(() =>
                {
                    spellWindowViewModel.UpdateAPITimers();
                });
                LastBoatUpdate = now;
            }
        }

        public void Dispose()
        {
            timer?.Stop();
            timer = null;
        }
    }
}
