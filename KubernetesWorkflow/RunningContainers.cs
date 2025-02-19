﻿namespace KubernetesWorkflow
{
    public class RunningContainers
    {
        public RunningContainers(StartupConfig startupConfig, RunningPod runningPod, RunningContainer[] containers)
        {
            StartupConfig = startupConfig;
            RunningPod = runningPod;
            Containers = containers;
        }

        public StartupConfig StartupConfig { get; }
        public RunningPod RunningPod { get; }
        public RunningContainer[] Containers { get; }

        public string Describe()
        {
            return string.Join(",", Containers.Select(c => c.GetName()));
        }
    }

    public class RunningContainer
    {
        public RunningContainer(RunningPod pod, ContainerRecipe recipe, Port[] servicePorts)
        {
            Pod = pod;
            Recipe = recipe;
            ServicePorts = servicePorts;
        }

        public string GetName()
        {
            return $"<{Recipe.Name}>";
        }

        public RunningPod Pod { get; }
        public ContainerRecipe Recipe { get; }
        public Port[] ServicePorts { get; }
    }
}
