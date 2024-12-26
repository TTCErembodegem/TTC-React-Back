using Serilog;

namespace Ttc.Model.Core;

public class TtcLogger
{
    private readonly ILogger _logger;

    public TtcLogger(Serilog.ILogger logger)
    {
        _logger = logger;
    }

    public void Information(string log)
    {
        _logger.Information(log);
    }

    public void Error(string log)
    {
        _logger.Error(log);
    }
}