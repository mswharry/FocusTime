using FocusTime.Core.Models;

namespace FocusTime.Core.Services;

/// <summary>
/// Manages distraction policies: blocklist, timeout, allowlist
/// </summary>
public class DistractionPolicyEngine
{
    private Settings _settings;
    private Dictionary<string, DateTime> _domainTimeouts = new Dictionary<string, DateTime>();
    private Dictionary<string, int> _domainSecondsInCurrentWork = new Dictionary<string, int>();

    public DistractionPolicyEngine(Settings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Check if a domain is currently blocked (in timeout)
    /// </summary>
    public bool IsDomainBlocked(string domain, out DateTime? timeoutUntil)
    {
        timeoutUntil = null;
        if (_domainTimeouts.ContainsKey(domain))
        {
            var until = _domainTimeouts[domain];
            if (DateTime.Now < until)
            {
                timeoutUntil = until;
                return true;
            }
            else
            {
                _domainTimeouts.Remove(domain);
            }
        }
        return false;
    }

    /// <summary>
    /// Track domain usage during Work phase
    /// Returns true if domain should be blocked (exceeded threshold)
    /// </summary>
    public bool TrackDomainUsage(string domain, bool isWorkPhase)
    {
        if (!isWorkPhase || !_settings.DomainBlocklist.Contains(domain))
            return false;

        // Check if already timed out
        if (IsDomainBlocked(domain, out _))
            return true;

        // Track seconds
        if (!_domainSecondsInCurrentWork.ContainsKey(domain))
            _domainSecondsInCurrentWork[domain] = 0;

        _domainSecondsInCurrentWork[domain]++;

        // Check if exceeded threshold
        if (_domainSecondsInCurrentWork[domain] > _settings.BlockedAllowedSecondsInWork)
        {
            // Apply timeout
            _domainTimeouts[domain] = DateTime.Now.AddMinutes(_settings.DomainTimeoutMinutes);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Reset domain tracking for new Work segment
    /// </summary>
    public void ResetWorkSegment()
    {
        _domainSecondsInCurrentWork.Clear();
    }

    /// <summary>
    /// Check if an app process is in the allowlist
    /// </summary>
    public bool IsAppAllowed(string processName)
    {
        return _settings.AppAllowlistProcessNames.Contains(processName);
    }

    /// <summary>
    /// Compute break bank: plannedBreakMinutes - (distractedMinutes / 2)
    /// </summary>
    public int ComputeBreakBank(int plannedBreakMinutes, int distractedMinutesInPrevWork)
    {
        int discount = distractedMinutesInPrevWork / 2;
        return Math.Max(0, plannedBreakMinutes - discount);
    }
}
