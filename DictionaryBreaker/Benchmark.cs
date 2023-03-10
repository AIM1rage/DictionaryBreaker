using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Validators;

namespace DictionaryBreaker;

public class Tests
{
    [SetUp]
    public void Setup() { }

    [Test]
    public void Test()
    {
        var config = new ManualConfig()
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .AddValidator(JitOptimizationsValidator.DontFailOnError)
            .AddLogger(ConsoleLogger.Default)
            .AddColumnProvider(DefaultColumnProviders.Instance);
        BenchmarkRunner.Run<DictionaryFillingBenchmark>(config);
    }
}

public partial class DictionaryFillingBenchmark
{
    private const int RepeatsCount = 50000;
    private readonly Random _random = new Random();

    [Benchmark(Description = "KeyGeneration")]
    public int GenerateKey()
    {
        var key = 0;
        for (var i = 0; i < RepeatsCount / 2; i++)
            key = s_primes[_random.Next(s_primes.Length - 1)] * i;
        return key;
    }

    [Benchmark(Description = "FillingDictionaryWithMagicNumbers")]
    public Dictionary<int, int> DictionaryWithPrimes()
    {
        var dictionary = new Dictionary<int, int>();
        for (var i = 0; i < RepeatsCount / 2; i++)
        {
            var key = s_primes[_random.Next(s_primes.Length - 1)] * i;
            dictionary[key] = i;
            dictionary[unchecked(int.MaxValue + 1 + key)] = i;
        }

        return dictionary;
    }

    [Benchmark(Description = "FillingDictionaryWithConsecutiveNumbers")]
    public Dictionary<int, int> DictionaryWithConsecutiveNumbers()
    {
        var start = _random.Next();
        var dictionary = new Dictionary<int, int>();
        for (var i = 0; i < RepeatsCount; i++)
        {
            dictionary[start] = start;
            start = unchecked(start + 1);
        }

        return dictionary;
    }

    [Benchmark(Description = "FillingDictionaryWithPrimeNumbers")]
    public Dictionary<int, int> DictionaryWithPrimeNumbers()
    {
        var dictionary = new Dictionary<int, int>();
        var prime = 2;
        for (var i = 0; i < RepeatsCount; i++)
        {
            dictionary[prime] = i;
            //Метод GetPrime() используется внутри Dictionary.
            //Понятно, что большая часть вычислений происходит внутри этого метода.
            //В .NET 4.8 для вычисления хэша наивно используется побитовое И на int.MaxValue
            prime = HashHelper.GetPrime(prime + 1);
        }

        return dictionary;
    }
}