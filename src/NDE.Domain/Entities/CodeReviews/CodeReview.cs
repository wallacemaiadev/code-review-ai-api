using System.Security.Cryptography;
using System.Text;

using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class CodeReview : Entity<Guid>
{
  public Guid PullRequestId { get; private set; }
  public string FilePath { get; private set; } = string.Empty;
  public string FileLink { get; private set; } = string.Empty;
  public string Diff { get; private set; } = string.Empty;
  public string Language { get; private set; } = string.Empty;
  public string DiffHash { get; private set; } = string.Empty;
  public int VerdictId { get; private set; }
  public string Suggestion { get; private set; } = string.Empty;
  public int TokensConsumed { get; private set; }
  public string Feedback { get; private set; } = string.Empty;
  public bool Closed { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }

  public PullRequest PullRequest { get; private set; } = default!;
  public List<Modification> Modifications { get; set; } = new();

  protected CodeReview() : base(Guid.Empty) { }

  public CodeReview(Guid pullRequestId, string filePath, string diff, string language) : base(Guid.NewGuid())
  {
    PullRequestId = pullRequestId;
    FilePath = filePath;
    VerdictId = Verdict.Pending.Id;
    TokensConsumed = 0;
    Closed = false;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
    SetDiff(diff);
  }

  public bool SetDiff(string diff)
  {
    if (string.IsNullOrWhiteSpace(diff))
      throw new ArgumentNullException("Diff não pode ser nulo ou vazio.", nameof(diff));

    var hash = GenerateHash(diff);

    if (!CompareDiff(hash))
      return false;

    Diff = NormalizeDiff(diff);
    DiffHash = hash;
    Updated();

    return true;
  }

  public void AddModifications(Modification code)
  {
    if (code == null)
      throw new ArgumentNullException("A modificação não pode ser nulo.", nameof(code));

    Modifications ??= new List<Modification>();
    Modifications.Add(code);
  }

  public bool SetFeedback(string feedback)
  {
    if (string.IsNullOrWhiteSpace(feedback))
      throw new ArgumentNullException("Feedback não pode ser nulo ou vazio.", nameof(feedback));

    if (Feedback != feedback)
    {
      Feedback = feedback ?? string.Empty;
      Updated();
      return true;
    }

    return false;
  }

  public bool SetVerdict(Verdict verdict)
  {
    if (verdict == null)
      throw new ArgumentNullException("O veredito não pode ser nulo ou vazio.", nameof(verdict));

    if (VerdictId != verdict.Id)
    {
      VerdictId = verdict.Id;
      Updated();
      return true;
    }

    return false;
  }

  public bool SetSuggestion(string suggestion, int tokens)
  {
    if (string.IsNullOrWhiteSpace(suggestion))
      throw new ArgumentNullException("A sugestão não pode ser nulo ou vazio.", nameof(suggestion));

    if (tokens < 0)
      tokens = 0;

    if (Suggestion != suggestion)
    {
      TokensConsumed = tokens;
      Suggestion = suggestion;

      Updated();
      return true;
    }

    return false;
  }

  public bool SetFileLink(int pullRequestId, string repositoryUrl, string filePath)
  {
    if (pullRequestId <= 0)
      throw new ArgumentNullException("O Id do Pull Request está inválido.", nameof(pullRequestId));

    if (string.IsNullOrEmpty(repositoryUrl))
      throw new ArgumentNullException("A URL do repositório não pode ser nulo ou vazio.", nameof(repositoryUrl));

    if (string.IsNullOrEmpty(filePath))
      throw new ArgumentNullException("O caminho do arquivo não pode ser nulo ou vazio.", nameof(repositoryUrl));

    var fileLink = $"{repositoryUrl}/pullrequest/{pullRequestId}?_a=files&path=/{filePath}";

    if (FileLink != fileLink)
    {
      FileLink = fileLink;
      Updated();
      return true;
    }

    return false;
  }

  public void SetClosed(bool closed = true)
  {
    if (Closed != closed)
    {
      Closed = closed;
      Updated();
    }
  }

  public string GenerateHash(string diff)
  {
    if (string.IsNullOrWhiteSpace(diff))
      throw new ArgumentNullException("Diff não pode ser nulo ou vazio.", nameof(diff));

    var normalized = NormalizeDiff(diff);
    var hash = ComputeSha512Hex(normalized);

    return hash;
  }

  public bool CompareDiff(string diff)
  {
    if (string.IsNullOrWhiteSpace(diff))
      throw new ArgumentNullException("Diff não pode ser nulo ou vazio.", nameof(diff));

    if (string.IsNullOrWhiteSpace(DiffHash))
      return true;

    var normalized = NormalizeDiff(diff);
    var newHash = ComputeSha512Hex(normalized);

    return !string.Equals(DiffHash, newHash, StringComparison.OrdinalIgnoreCase);
  }

  public string NormalizeDiff(string diff)
  {
    if (string.IsNullOrWhiteSpace(diff))
      throw new ArgumentNullException("Diff não pode ser nulo ou vazio.", nameof(diff));

    return diff.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
  }

  public string ComputeSha512Hex(string diff)
  {
    if (string.IsNullOrWhiteSpace(diff))
      throw new ArgumentNullException("Diff não pode ser nulo ou vazio.", nameof(diff));

    var bytes = Encoding.UTF8.GetBytes(diff);
    var hash = SHA512.HashData(bytes);
    return Convert.ToHexString(hash).ToLowerInvariant();
  }

  public void Updated() => UpdatedAt = DateTime.UtcNow;
}
