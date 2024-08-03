```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.22631.3958/23H2/2023Update/SunValley3)
Intel Core i7-10870H CPU 2.20GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 8.0.302
  [Host]     : .NET 8.0.6 (8.0.624.26715), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.6 (8.0.624.26715), X64 RyuJIT AVX2


```
| Method | Mean     | Error     | StdDev    |
|------- |---------:|----------:|----------:|
| Linq   | 4.125 ms | 0.0814 ms | 0.1588 ms |
| NoLinq | 6.624 ms | 0.1322 ms | 0.2019 ms |
