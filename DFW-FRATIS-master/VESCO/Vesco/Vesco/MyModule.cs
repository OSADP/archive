using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ninject.Modules;
using PAI.CTIP.Optimization.Common;
using PAI.CTIP.Optimization.Function;
using PAI.CTIP.Optimization.Geography;
using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Reporting.Services;
using PAI.CTIP.Optimization.Services;
using PAI.CTIP.Optimization.Common;
using PAI.CTIP.Optimization.Function;
using PAI.CTIP.Optimization.Geography;
using PAI.CTIP.Optimization.Model;
using PAI.CTIP.Optimization.Services;
using PAI.Core;

namespace Vesco
{
    class MyModule : NinjectModule
    {
        public override void Load()
        {

            Bind<IDrayageOptimizer>().To<DrayageOptimizer>().InSingletonScope();
            Bind<IPheromoneMatrix>().To<PheromoneMatrix>().InSingletonScope()
                .WithConstructorArgument("initialPheromoneValue", 0.0)
                .WithConstructorArgument("rho", 0.5)
                .WithConstructorArgument("q", 1000.0);
            Bind<IRouteExitFunction>().To<RouteExitFunction>().InSingletonScope();
            Bind<IRouteService>().To<RouteService>().InSingletonScope();
            Bind<IRouteStopDelayService>().To<RouteStopDelayService>().InSingletonScope();
            Bind<IRouteStopService>().To<RouteStopService>().InSingletonScope();
            Bind<IStatisticsService>().To<StatisticsService>().InSingletonScope();
            Bind<IObjectiveFunction>().To<DistanceObjectiveFunction>().InSingletonScope();
            Bind<IRandomNumberGenerator>().To<RandomNumberGenerator>().InSingletonScope();
            Bind<IProbabilityMatrix>().To<ProbabilityMatrix>().InSingletonScope();
            Bind<INodeFactory>().To<NodeFactory>().InSingletonScope();

            Bind<ILogger>().To<NullLogger>().InSingletonScope();
            Bind<INodeService>()
                .To<NodeService>()
                .InSingletonScope()
                .WithConstructorArgument("configuration", new OptimizerConfiguration());
            Bind<IDistanceService>().To<DistanceService>().InSingletonScope();
            Bind<ITravelTimeEstimator>().To<TravelTimeEstimator>().InSingletonScope();

            Bind<IReportingService>().To<ReportingService>().InSingletonScope();    // todo verify scope
            Bind<IRouteSanitizer>().To<RouteSanitizer>().InSingletonScope();    // todo verify scope
        }
    }
}
