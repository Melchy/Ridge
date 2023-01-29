using NUnit.Framework;

[assembly: Parallelizable(ParallelScope.Children)]
[assembly: FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
