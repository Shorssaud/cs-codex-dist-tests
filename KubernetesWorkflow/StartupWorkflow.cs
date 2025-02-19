﻿using System.IO;

namespace KubernetesWorkflow
{
    public class StartupWorkflow
    {
        private readonly WorkflowNumberSource numberSource;
        private readonly K8sCluster cluster;
        private readonly KnownK8sPods knownK8SPods;
        private readonly RecipeComponentFactory componentFactory = new RecipeComponentFactory();

        internal StartupWorkflow(WorkflowNumberSource numberSource, K8sCluster cluster, KnownK8sPods knownK8SPods)
        {
            this.numberSource = numberSource;
            this.cluster = cluster;
            this.knownK8SPods = knownK8SPods;
        }

        public RunningContainers Start(int numberOfContainers, Location location, ContainerRecipeFactory recipeFactory, StartupConfig startupConfig)
        {
            return K8s(controller =>
            {
                var recipes = CreateRecipes(numberOfContainers, recipeFactory, startupConfig);

                var runningPod = controller.BringOnline(recipes, location);

                return new RunningContainers(startupConfig, runningPod, CreateContainers(runningPod, recipes));
            });
        }

        public void Stop(RunningContainers runningContainers)
        {
            K8s(controller =>
            {
                controller.Stop(runningContainers.RunningPod);
            });
        }

        public void DownloadContainerLog(RunningContainer container, ILogHandler logHandler)
        {
            K8s(controller =>
            {
                controller.DownloadPodLog(container.Pod, container.Recipe, logHandler);
            });
        }

        public string ExecuteCommand(RunningContainer container, string command, params string[] args)
        {
            return K8s(controller =>
            {
                return controller.ExecuteCommand(container.Pod, container.Recipe.Name, command, args);
            });
        }

        public void DeleteAllResources()
        {
            K8s(controller =>
            {
                controller.DeleteAllResources();
            });
        }

        private static RunningContainer[] CreateContainers(RunningPod runningPod, ContainerRecipe[] recipes)
        {
            return recipes.Select(r => new RunningContainer(runningPod, r, runningPod.GetServicePortsForContainerRecipe(r))).ToArray();
        }

        private ContainerRecipe[] CreateRecipes(int numberOfContainers, ContainerRecipeFactory recipeFactory, StartupConfig startupConfig)
        {
            var result = new List<ContainerRecipe>();
            for (var i = 0; i < numberOfContainers; i++)
            {
                result.Add(recipeFactory.CreateRecipe(i ,numberSource.GetContainerNumber(), componentFactory, startupConfig));
            }

            return result.ToArray();
        }

        private void K8s(Action<K8sController> action)
        {
            var controller = new K8sController(cluster, knownK8SPods, numberSource);
            action(controller);
            controller.Dispose();
        }

        private T K8s<T>(Func<K8sController, T> action)
        {
            var controller = new K8sController(cluster, knownK8SPods, numberSource);
            var result = action(controller);
            controller.Dispose();
            return result;
        }
    }

    public interface ILogHandler
    {
        void Log(Stream log);
    }

    public abstract class LogHandler : ILogHandler
    {
        public void Log(Stream log)
        {
            using var reader = new StreamReader(log);
            var line = reader.ReadLine();
            while (line != null)
            {
                ProcessLine(line);
                line = reader.ReadLine();
            }
        }

        protected abstract void ProcessLine(string line);
    }
}
