using System.Diagnostics.Metrics;

namespace NDE.Observability.Metrics
{
  public static class AppMetrics
  {
    private static Meter? _meter;

    private static Counter<int>? _projectCreatedCounter;
    private static Counter<int>? _repositoryCreatedCounter;
    private static Counter<int>? _teamStandardCreatedCounter;
    private static Counter<int>? _pullRequestCreatedCounter;
    private static Counter<int>? _pullRequestUpdatedCounter;
    private static Counter<int>? _pullRequestClosedCounter;
    private static Histogram<double>? _getSimilarDiffDuration;
    private static Counter<int>? _similarFoundCounter;
    private static Counter<int>? _codeReviewVectorCreatedDiffCounter;
    private static Counter<int>? _codeReviewVectorUpdatedDiffCounter;
    private static Counter<int>? _buildPipelineCounter;

    public static void Init(string name, string version)
    {
      _meter = new Meter(name, version);

      _projectCreatedCounter = _meter.CreateCounter<int>(
        "nde_project_created_total",
        description: "Total de projetos criados.");

      _repositoryCreatedCounter = _meter.CreateCounter<int>(
          "nde_repository_created_total",
          description: "Total de repositórios criados.");

      _teamStandardCreatedCounter = _meter.CreateCounter<int>(
          "nde_teamstandard_created_total",
          description: "Total de Team Standards (padrões de projeto da equipe) criados.");

      _pullRequestCreatedCounter = _meter.CreateCounter<int>(
        "nde_pullrequest_created_total",
        description: "Total de Pull Requests criados.");

      _pullRequestUpdatedCounter = _meter.CreateCounter<int>(
        "nde_pullrequest_updated_total",
        description: "Total de Pull Requests atualizados.");

      _pullRequestClosedCounter = _meter.CreateCounter<int>(
        "nde_pullrequest_closed_total",
        description: "Total de Pull Requests fechados.");

      _getSimilarDiffDuration = _meter.CreateHistogram<double>(
        "nde_get_similar_diff_duration_seconds",
        unit: "s",
        description: "Duração (em segundos) da operação de busca de diffs similares.");

      _similarFoundCounter = _meter.CreateCounter<int>(
        "nde_get_similar_found_total",
        description: "Total de diffs similares encontrados.");

      _codeReviewVectorCreatedDiffCounter = _meter.CreateCounter<int>(
        "nde_codereview_created_total",
        description: "Total de vetores de code reviews criados.");

      _codeReviewVectorUpdatedDiffCounter = _meter.CreateCounter<int>(
      "nde_codereview_updated_total",
      description: "Total de vetores de code reviews atualizados.");

      _buildPipelineCounter = _meter.CreateCounter<int>(
        "nde_build_pipeline_total",
        description: "Total de builds de pipeline registrados.");
    }

    public static void RecordPullRequestCreated(int pullRequestId, string title, string projectName, string repositoryName)
    {
      _pullRequestCreatedCounter?.Add(1,
      [
        new("pullrequest_id", pullRequestId),
        new("pullrequest_title", title),
        new("project_name", projectName),
        new("repository_name", repositoryName),
      ]);
    }

    public static void RecordPullRequestUpdated(int pullRequestId, string title, string projectName, string repositoryName)
    {
      _pullRequestUpdatedCounter?.Add(1,
      [
        new("pullrequest_id", pullRequestId),
        new("pullrequest_title", title),
        new("project_name", projectName),
        new("repository_name", repositoryName)
      ]);
    }

    public static void RecordPullRequestClosed(int pullRequestId, string title, string projectName, string repositoryName)
    {
      _pullRequestClosedCounter?.Add(1,
      [
        new("pullrequest_id", pullRequestId),
        new("pullrequest_title", title),
        new("project_name", projectName),
        new("repository_name", repositoryName)
      ]);
    }

    public static void RecordGetSimilarDiffDuration(TimeSpan duration, string projectName, string repositoryName)
    {
      _getSimilarDiffDuration?.Record(duration.TotalSeconds,
      [
        new("project_name", projectName),
        new("repository_name", repositoryName)
      ]);
    }

    public static void RecordSimilarFound(int pullRequestId, string projectName, string repositoryName, List<string> payloads, int quantity)
    {
      _similarFoundCounter?.Add(quantity,
      [
        new("pullrequest_id", pullRequestId),
        new("project_name", projectName),
        new("repository_name", repositoryName),
        new("payloads", payloads)
      ]);
    }

    public static void RecordProjectCreated(Guid projectId, string projectName)
    {
      _projectCreatedCounter?.Add(1,
      [
        new("project_id", projectId),
        new("project_name", projectName)
      ]);
    }

    public static void RecordRepositoryCreated(Guid repositoryId, string repositoryName, string projectName)
    {
      _repositoryCreatedCounter?.Add(1,
      [
        new("repository_id", repositoryId),
        new("repository_name", repositoryName),
        new("project_name", projectName)
      ]);
    }

    public static void RecordTeamStandardCreated(Guid repositoryId, string repositoryName, string projectName)
    {
      _teamStandardCreatedCounter?.Add(1,
      [
        new("repository_id", repositoryId),
        new("repository_name", repositoryName),
        new("project_name", projectName)
      ]);
    }

    public static void RecordCodeReviewVectorCreated(int pullRequestId, string repositoryName, string projectName, string verdict)
    {
      _codeReviewVectorCreatedDiffCounter?.Add(1,
      [
        new("pullrequest_id", pullRequestId),
        new("repository_name", repositoryName),
        new("project_name", projectName),
        new("verdict", verdict)
      ]);
    }

    public static void RecordCodeReviewVectorUpdated(int pullRequestId, string repositoryName, string projectName, string verdict)
    {
      _codeReviewVectorUpdatedDiffCounter?.Add(1,
      [
        new("pullrequest_id", pullRequestId),
        new("repository_name", repositoryName),
        new("project_name", projectName),
        new("verdict", verdict)
      ]);
    }

    public static void RecordBuildPipeline(
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
