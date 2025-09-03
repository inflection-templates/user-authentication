# Deployments

There could be multiple types of deployments implemented for this API service.

For example, we can deploy the service on a local machine, on a virtual machine, or on a cloud platform like Azure, AWS, or Google Cloud. The service can be directly run on virtual machine through NGINX or it can be run in a containerized environment using Docker or Kubernetes.

This document will provides a brief guidance on how to deploy the API service on a following platforms.

1. [Local machine using Docker](./local-machine-docker-deployment.md)
2. [Virtual Machine using NGINX](virtual-machine-nginx-deployment.md)
3. Managed containerized services such as
   1. [Azure Container Apps](azure-container-apps-deployment.md)
   2. [AWS Elastic Container Service (ECS Fargate)](aws-ecs-fargate-deployment.md)
4. In a Kubernetes cluster
   1. [Locally K3S](local-k3s-deployment.md)
   2. [Azure Kubernetes Service (AKS)](azure-aks-deployment.md)
   3. [AWS Elastic Kubernetes Service (EKS)](aws-eks-deployment.md)
   4. [Google Kubernetes Engine (GKE)](google-gke-deployment.md)

For CI/CD, GitHub Actions has been used. For other source control systems, please refer to their respective documentations.

For each of the above deployment types, the workflow files are located in `.github/workflows` folder. The workflow files are named as `deploy-<environment>-<deployment-type>.yml`.
