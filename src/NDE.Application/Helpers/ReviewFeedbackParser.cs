using System.Text.RegularExpressions;

namespace NDE.Application.Helpers;

public static class ReviewFeedbackParser
{
  private static readonly Regex MarkerByHash = new(
    @"<!--\s*.*?\|\s*Hash:\s*(?<hash>[A-Za-z0-9_\-:+/=]{6,})\s*-->",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private static readonly Regex AcceptedCheckbox = new(
    @"(?m)^\s*-\s*\[(?<state>[ xX])\]\s*Aceito\b.*$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private static readonly Regex RejectedCheckbox = new(
    @"(?m)^\s*-\s*\[(?<state>[ xX])\]\s*Rejeito\b.*$",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private static readonly Regex MotivoRegex = new(
    @"(?mi)^(?:#+|\-)?\s*Motivo:\s*(?<motivo>[\s\S]*?)(?:^###|^-\s*\[|<!--|$)",
    RegexOptions.Compiled | RegexOptions.CultureInvariant);

  private const int LOOKBACK = 1200;

  public static int IsAcceptedByHash(string content, string hash)
  {
    if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(hash))
      return -1;

    var match = MarkerByHash.Matches(content)
        .Cast<Match>()
        .Where(m => m.Success && string.Equals(
            m.Groups["hash"].Value.Trim(), hash.Trim(), StringComparison.OrdinalIgnoreCase))
        .LastOrDefault();

    if (match is null)
      return -1;

    int markerIndex = match.Index;

    int feedbackStart = content.LastIndexOf("### Feedback", markerIndex, StringComparison.OrdinalIgnoreCase);
    if (feedbackStart < 0)
      feedbackStart = Math.Max(0, markerIndex - LOOKBACK);

    if (feedbackStart >= markerIndex)
      return -1;

    string feedbackSection = content.Substring(feedbackStart, markerIndex - feedbackStart);

    var acceptedMatch = AcceptedCheckbox.Match(feedbackSection);
    var rejectedMatch = RejectedCheckbox.Match(feedbackSection);

    bool isAcceptedChecked = acceptedMatch.Success &&
                             acceptedMatch.Groups["state"].Value.Equals("x", StringComparison.OrdinalIgnoreCase);

    bool isRejectedChecked = rejectedMatch.Success &&
                             rejectedMatch.Groups["state"].Value.Equals("x", StringComparison.OrdinalIgnoreCase);

    if (!isAcceptedChecked && !isRejectedChecked)
      return 1;

    if (isAcceptedChecked && isRejectedChecked)
      return 1;

    return isAcceptedChecked ? 2 : 0;
  }

  public static string? GetReason(string content, string hash)
  {
    if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(hash))
      return null;

    var match = MarkerByHash.Matches(content)
        .Cast<Match>()
        .Where(m => m.Success && string.Equals(
            m.Groups["hash"].Value.Trim(), hash.Trim(), StringComparison.OrdinalIgnoreCase))
        .LastOrDefault();

    if (match is null)
      return null;

    int markerIndex = match.Index;

    int feedbackStart = content.LastIndexOf("### Feedback", markerIndex, StringComparison.OrdinalIgnoreCase);
    if (feedbackStart < 0)
      feedbackStart = Math.Max(0, markerIndex - LOOKBACK);

    if (feedbackStart >= markerIndex)
      return null;

    string feedbackBlock = content.Substring(feedbackStart, markerIndex - feedbackStart);

    var motivoMatch = MotivoRegex.Match(feedbackBlock);
    if (motivoMatch.Success)
    {
      var motivo = motivoMatch.Groups["motivo"].Value.Trim();
      return string.IsNullOrWhiteSpace(motivo) ? null : motivo;
    }

    return null;
  }

  public static bool IsCodeReviewComment(string content)
  {
    if (string.IsNullOrWhiteSpace(content))
      return false;

    return MarkerByHash.IsMatch(content);
  }
}