using Microsoft.Extensions.Logging;

namespace Inkify
{
	internal static class MinimumLogLevelFilter
	{
		public static LogLevel Level { get; set; } = LogLevel.None;
		internal static bool Filter(LogLevel level) => level >= Level;
	}
}