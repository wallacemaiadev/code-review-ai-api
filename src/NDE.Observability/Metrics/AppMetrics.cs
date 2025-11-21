using System.Diagnostics.Metrics;

namespace NDE.Observability.Metrics
{
  public static class AppMetrics
  {
    private static Meter? _meter;

    private static Counter<int>? _pullRequestCreatedCounter;
    private static Counter<int>? _pullRequestUpdatedCounter;


    private static Counter<int>? _projectCreatedCounter;

    private static Counter<int>? _repositoryCreatedCounter;

    private static Counter<int>? _teamStandardCreatedCounter;


    private static Counter<int>? _similarFoundCounter;

    private static Counter<int>? _buildPipelineCounter;


    private static Histogram<double>? _generateEmbeddingDuration;
    private static Histogram<double>? _llmRequestDuration;

    public static void Init(string name, string version)
    {
      _meter = new Meter(name, version);

      _projectCreatedCounter = _meter.CreateCounter<int>(
        "project_created_total",
        description: "Total de projetos registrados no sistema.");

      _repositoryCreatedCounter = _meter.CreateCounter<int>(
        "repository_created_total",
        description: "Total de repositórios registrados no sistema.");

      _teamStandardCreatedCounter = _meter.CreateCounter<int>(
        "teamstandard_created_total",
        description: "Total de Team Standards criados.");

      _pullRequestCreatedCounter = _meter.CreateCounter<int>(
        "pullrequest_created_total",
        description: "Total de Pull Requests criados.");

      _pullRequestUpdatedCounter = _meter.CreateCounter<int>(
        "pullrequest_updated_total",
        description: "Total de Pull Requests atualizados.");

      _generateEmbeddingDuration = _meter.CreateHistogram<double>(
        "generate_embedding_duration_seconds",
        unit: "s",
        description: "Duração (em segundos) para gerar embeddings a partir do diff.");

      _llmRequestDuration = _meter.CreateHistogram<double>(
        "llm_request_duration_seconds",
        unit: "s",
        description: "Duração (em segundos) da requisição ao modelo LLM durante a análise de diffs.");

      _similarFoundCounter = _meter.CreateCounter<int>(
        "get_similar_found_total",
        description: "Total de diffs similares encontrados pelo sistema.");

      _buildPipelineCounter = _meter.CreateCounter<int>(
        "build_pipeline_total",
        description: "Total de execuções de pipeline registradas.");
    }

    public static void ObservePullRequestCreated(int pullRequestId, Guid repositoryId, Guid projectId, string repositoryName)
    {
      _pullRequestCreatedCounter?.Add(1,
      [
        new("pullrequest_id", pullRequestId),
        new("repository_name", repositoryName),
        new("project_id", projectId),
        new("repository_id", repositoryId),
      ]);
    }

    public static void ObservePullRequestUpdated(int pullRequestId, string title, string projectName, string repositoryName, Guid projectId, Guid repositoryId)
    {
      _pullRequestUpdatedCounter?.Add(1,
      [
        new("pullrequest_id", pullRequestId),
        new("pullrequest_title", title),
        new("project_id", projectId),
        new("repository_id", repositoryId),
        new("project_name", projectName),
        new("repository_name", repositoryName)
      ]);
    }

    public static void ObserveSimilarFound(int pullRequestId, string projectName, string repositoryName, List<string> payloads, int quantity)
    {
      _similarFoundCounter?.Add(quantity,
      [
        new("pullrequest_id", pullRequestId),
        new("project_name", projectName),
        new("repository_name", repositoryName),
        new("payloads", payloads)
      ]);
    }

    public static void ObserveProjectCreated(Guid projectId, string projectName)
    {
      _projectCreatedCounter?.Add(1,
      [
        new("project_id", projectId),
        new("project_name", projectName)
      ]);
    }

    public static void ObserveRepositoryCreated(Guid repositoryId, string repositoryName, string projectName)
    {
      _repositoryCreatedCounter?.Add(1,
      [
        new("repository_id", repositoryId),
        new("repository_name", repositoryName),
        new("project_name", projectName)
      ]);
    }

    public static void ObserveTeamStandardCreated(Guid repositoryId, string repositoryName, string projectName)
    {
      _teamStandardCreatedCounter?.Add(1,
      [
        new("repository_id", repositoryId),
        new("repository_name", repositoryName),
        new("project_name", projectName)
      ]);
    }

    public static void ObserveGenerateEmbeddingDuration(TimeSpan duration)
    {
      _generateEmbeddingDuration?.Record(duration.TotalSeconds);
    }

    public static void ObserveLlmRequestDuration(TimeSpan duration)
    {
      _llmRequestDuration?.Record(duration.TotalSeconds);
    }


    public static void ObserveBuildPipeline(
      int buildId,
      string buildNumber,
      DateTime queueTime,
      DateTime startTime,
      DateTime finishTime,
      string sourceBranch,
      string repositoryName,
      string projectName,
      Guid repositoryId,
      Guid projectId,
      string status,
      string result)
    {
      _buildPipelineCounter?.Add(1,
      [
        new("build_id", buildId),
        new("build_number", buildNumber),
        new("queue_time", queueTime),
        new("start_time", startTime),
        new("finish_time", finishTime),
        new("sourceBranch", sourceBranch),
        new("repository_name", repositoryName),
        new("project_name", projectName),
        new("repository_id", repositoryId),
        new("project_id", projectId),
        new("status", status),
        new("result", result)
      ]);
    }
  }
}
