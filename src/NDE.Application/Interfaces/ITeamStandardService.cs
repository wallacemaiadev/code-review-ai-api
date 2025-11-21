namespace NDE.Application.Interfaces;

public interface ITeamStandardService
{
  Task<bool> SaveTeamStandardAsync(string teamStandard, Guid repositoryId);
  Task<string?> GetTeamStandardAsync(Guid repositoryId);
}
