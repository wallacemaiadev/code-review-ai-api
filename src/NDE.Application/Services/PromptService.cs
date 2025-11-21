using System.Text;

using NDE.Application.Interfaces;

namespace NDE.Application.Services;

public class PromptService : IPromptService
{
  private readonly string _promptConfigPath;

  public PromptService(string promptConfigPath)
  {
    _promptConfigPath = promptConfigPath;
  }

  public async Task<string> BuildInstructions(List<string> changedFiles, string? previousSuggestion, string? prompt = null)
  {
    var sections = new List<string>();

    if (string.IsNullOrEmpty(prompt))
    {
      sections.Add($"{await GetDefaultPrompt()}\n\n");
    }
    else
    {
      sections.Add($"{prompt}\n\n");
    }

    sections.Add("\n---\n");

    if (!string.IsNullOrEmpty(previousSuggestion))
    {
      sections.Add(previousSuggestion);
      sections.Add("\n---\n");
    }

    if (changedFiles.Count > 0)
    {
      sections.Add("### Arquivos Alterados neste PR:\n");
      sections.Add(string.Join("\n", changedFiles.Select(f => $"- {f}")));
      sections.Add("\n---\n");
    }

    sections.Add(await GetFeedbackPrompt());

    return string.Join("\n", sections);
  }

  private async Task<string> GetDefaultPrompt()
  {
    try
    {
      var inputFile = Path.Combine(_promptConfigPath, "input.md");
      var content = await File.ReadAllTextAsync(path: inputFile, encoding: Encoding.UTF8);

      if (string.IsNullOrWhiteSpace(content))
        throw new Exception("Arquivo de prompt padrão está vazio");

      return content;
    }
    catch (Exception ex)
    {
      var msg = $"Falha ao ler arquivo de prompt padrão: {ex.Message}";
      Console.WriteLine(msg);
      throw new TypeLoadException(msg);
    }
  }

  private async Task<string> GetFeedbackPrompt()
  {
    try
    {
      var outputFile = Path.Combine(_promptConfigPath, "output.md");
      var content = await File.ReadAllTextAsync(path: outputFile, encoding: Encoding.UTF8);

      if (string.IsNullOrWhiteSpace(content))
        throw new Exception("Arquivo de feedback prompt está vazio");

      return content;
    }
    catch (Exception ex)
    {
      var msg = $"Falha ao ler arquivo de feedback prompt: {ex.Message}";
      Console.WriteLine(msg);
      throw new TypeLoadException(msg);
    }
  }
}
