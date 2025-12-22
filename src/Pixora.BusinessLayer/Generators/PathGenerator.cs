using Pixora.BusinessLayer.Generators.Interfaces;

namespace Pixora.BusinessLayer.Generators;

public class PathGenerator(TimeProvider timeProvider) : IPathGenerator
{
    public string CreatePath(string fileName)
    {
        var now = timeProvider.GetUtcNow();
        return Path.Combine(now.Year.ToString("0000"), now.Month.ToString("00"), now.Day.ToString("00"), fileName);
    }
}