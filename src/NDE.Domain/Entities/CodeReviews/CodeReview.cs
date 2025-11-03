using System.Security.Cryptography;
using System.Text;

using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class CodeReview : Entity<Guid>
{
  public int PullRequestId { get; private set; }
  public AzurePullRequest AzurePullRequest { get; private set; } = default!;
  public string FilePath { get; private set; } = string.Empty;
  public string Diff { get; private set; } = string.Empty;
  public string Language { get; private set; } = string.Empty;
  public string Fingerprint { get; private set; } = string.Empty;
  public string DiffHash { get; private set; } = string.Empty;
  public int VerdictId { get; private set; } = Verdict.Pending.Id;
  public string Suggestion { get; private set; } = string.Empty;
  public int TokensConsumed { get; private set; }
  public float[] Vector { get; private set; } = Array.Empty<float>();
  public string SummaryReview { get; private set; } = string.Empty;
  public string Feedback { get; private set; } = string.Empty;
  public bool Closed { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }

  protected CodeReview() : base(Guid.Empty) { }

  public CodeReview(
      int pullRequestId,
      string filePath,
      string diff
  ) : base(Guid.NewGuid())
  {
    PullRequestId = pullRequestId;
    FilePath = filePath;
    VerdictId = Verdict.Pending.Id;
    TokensConsumed = 0;
    Closed = false;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
    UpdateDiff(diff);
  }

  public bool UpdateDiff(string newDiff)
  {
    var normalized = NormalizeText(newDiff);
    var newHash = ComputeSha512Hex(normalized);

    if (!string.IsNullOrEmpty(DiffHash) && string.Equals(DiffHash, newHash, StringComparison.OrdinalIgnoreCase))
      return false;

    Diff = normalized;
    DiffHash = newHash;
    Fingerprint = ComputeFingerprint();
    Updated();
    return true;
  }

  public void SetDiffHash(string hash)
  {
    if (string.IsNullOrWhiteSpace(hash))
      return;

    DiffHash = hash;
    Updated();
  }

  public void SetVector(float[]? vector)
  {
    Vector = vector ?? Array.Empty<float>();
    Updated();
  }

  public void SetFeedback(string feedback)
  {
    Feedback = feedback ?? string.Empty;
    Updated();
  }

  public void SetVerdict(int verdictId)
  {
    VerdictId = verdictId;
    Updated();
  }

  public void SetSuggestion(string? suggestion)
  {
    Suggestion = suggestion ?? string.Empty;
    Updated();
  }

  public void SetLanguage(string? language)
  {
    Language = language ?? string.Empty;
    Updated();
  }

  public void SetTokens(int tokens)
  {
    if (tokens < 0)
      tokens = 0;

    TokensConsumed = tokens;
    Updated();
  }

  public void SetSummaryReview(string summaryReview)
  {
    SummaryReview = summaryReview ?? string.Empty;
    Updated();
  }

  public void Updated() => UpdatedAt = DateTime.UtcNow;

  public void SetClosed(bool closed = true)
  {
    Closed = closed;
    Updated();
  }

  private string ComputeFingerprint()
  {
    var sb = new StringBuilder(256)
        .Append(Id).Append('|')
        .Append(PullRequestId).Append('|')
        .Append(NormalizePath(FilePath)).Append('|')
        .Append(Diff);

    return ComputeSha256Hex(sb.ToString());
  }

  private static string ComputeSha256Hex(string input)
  {
    var bytes = Encoding.UTF8.GetBytes(input);
    var hash = SHA256.HashData(bytes);
    return Convert.ToHexString(hash).ToLowerInvariant();
  }

  private static string ComputeSha512Hex(string input)
  {
    var bytes = Encoding.UTF8.GetBytes(input);
    var hash = SHA512.HashData(bytes);
    return Convert.ToHexString(hash).ToLowerInvariant();
  }

  private static string NormalizeText(string? text)
      => string.IsNullOrEmpty(text) ? string.Empty
         : text.Replace("\r\n", "\n").Replace("\r", "\n").Trim();

  private static string NormalizePath(string? path)
      => string.IsNullOrEmpty(path) ? string.Empty
         : path.Replace('\\', '/').ToLowerInvariant().Trim();
}