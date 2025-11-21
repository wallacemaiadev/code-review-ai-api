namespace NDE.Application.Interfaces;

public interface IPromptService
{
  Task<string> BuildInstructions(List<string> changedFiles, string? previousSuggestion = null, string? prompt = null);
}