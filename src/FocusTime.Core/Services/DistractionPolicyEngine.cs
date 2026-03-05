using FocusTime.Core.Models;

namespace FocusTime.Core.Services;

/// <summary>
/// Manages distraction policies: blocklist, timeout, allowlist
/// </summary>
public class DistractionPolicyEngine
{
    private Settings _settings;
    private Dictionary<string, int> _domainSecondsInCurrentWork = new Dictionary<string, int>();

    public DistractionPolicyEngine(Settings settings)
    {
        _settings = settings;
        // Clean up expired timeouts on init
        CleanupExpiredTimeouts();
    }

    /// <summary>
    /// Clean up expired timeouts from settings
    /// </summary>
    private void CleanupExpiredTimeouts()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _settings.DomainTimeouts
            .Where(kvp => kvp.Value <= now)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _settings.DomainTimeouts.Remove(key);
        }
    }

    /// <summary>
    /// Check if a domain is currently blocked (in timeout)
    /// </summary>
    public bool IsDomainBlocked(string domain, out DateTime? timeoutUntil)
    {
        timeoutUntil = null;
        if (_settings.DomainTimeouts.ContainsKey(domain))
        {
            var until = _settings.DomainTimeouts[domain];
            if (DateTime.UtcNow < until)
            {
                timeoutUntil = until;
                return true;
            }
            else
            {
                _settings.DomainTimeouts.Remove(domain);
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
            // Apply timeout (store in UTC)
            _settings.DomainTimeouts[domain] = DateTime.UtcNow.AddMinutes(_settings.DomainTimeoutMinutes);
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
    /// Check if an app process is in the allowlist (case-insensitive, handles paths)
    /// </summary>
    public bool IsAppAllowed(string processName)
    {
        if (string.IsNullOrEmpty(processName))
            return false;

        // Extract filename from full path if needed
        string filename = System.IO.Path.GetFileName(processName);
        
        return _settings.AppAllowlistProcessNames
            .Any(allowed => filename.Contains(allowed, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if an app is neutral (system utility, not focused but not distracted)
    /// </summary>
    public bool IsAppNeutral(string processName)
    {
        if (string.IsNullOrEmpty(processName))
            return false;

        string filename = System.IO.Path.GetFileName(processName);
        
        return _settings.AppNeutralProcessNames
            .Any(neutral => filename.Contains(neutral, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if a domain is in the allowlist (work-related)
    /// </summary>
    public bool IsDomainAllowed(string domain)
    {
        if (string.IsNullOrEmpty(domain))
            return false;

        return _settings.DomainAllowlist
            .Any(allowed => domain.Contains(allowed, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if a domain is neutral (empty, local, transitioning)
    /// </summary>
    public bool IsDomainNeutral(string domain)
    {
        return string.IsNullOrEmpty(domain) || 
               domain == "local" || 
               domain == "about:blank" ||
               domain.StartsWith("file://");
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
