using NDE.Application.ViewModels.Response.LLMService;

namespace NDE.Application.Interfaces;

public interface ILLMService
{
  int GetTokensConsumed();
  Task<LLMResponse> SendMessage(string instructions, string content);
}
