using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Backend.Application.Common.Interfaces;

namespace Backend.Infrastructure.Services;

public class VirtualClockService : IVirtualClockService, IDisposable
{
    private readonly Timer _timer;
    private DateTime _baseDate;
    private TimeSpan _currentTime;
    private readonly object _lock = new object();

    public DateTime CurrentTime => _baseDate.Date.Add(_currentTime);
    public event Action TimeAdvanced = delegate { };

    public VirtualClockService()
    {
        _timer = new Timer(AdvanceTime, null, Timeout.Infinite, 1000); // 1 saniye aralık
        _baseDate = DateTime.Now;
        _currentTime = DateTime.Now.TimeOfDay;
        Start();
    }

    public void SetInitialTime(DateTime setTime)
    {
        Stop(); // Önce mevcut timer'ı durdur
        lock (_lock)
        {
            _baseDate = setTime;    
            _currentTime = setTime.TimeOfDay;
        }
        Start(); // Yeni ayarla ve başlat
    }

    public void Start()
    {
        _timer.Change(0, 1000); // 1 saniyede bir tetikle
    }

    public void Stop()
    {
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void AdvanceTime(object? state)
    {
        lock (_lock)
        {
            _currentTime = _currentTime.Add(TimeSpan.FromSeconds(1));

            // 24 saat dolunca tarihi ilerlet
            if (_currentTime.TotalHours >= 24)
            {
                _baseDate = _baseDate.AddDays(1);
                _currentTime = TimeSpan.Zero;
            }
        }

        TimeAdvanced?.Invoke();
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}
